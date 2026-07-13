using System.Net;
using Insight.Services.Ai.Gemini.Options;
using Insight.Services.Ai.Gemini.Resilience;
using Insight.Services.Interfaces.Ai;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insight.Services.Core.Tests.Resilience;

[TestClass]
public class GeminiRetryPolicyTests
{
    private Mock<IStreamingResilienceMetrics> _metricsMock = null!;
    private GeminiRetryPolicy _policy = null!;

    [TestInitialize]
    public void Setup()
    {
        _metricsMock = new Mock<IStreamingResilienceMetrics>();

        var options = Microsoft.Extensions.Options.Options.Create(new StreamingErrorPolicyOptions
        {
            MaxRetries = 3,
            InitialRetryDelay = TimeSpan.FromMilliseconds(1),
            RetryBackoffMultiplier = 2.0
        });

        _policy = new GeminiRetryPolicy(options, _metricsMock.Object, Mock.Of<ILogger<GeminiRetryPolicy>>());
    }

    [TestMethod]
    public async Task ExecuteAsync_SucceedsFirstTry_ReturnsResultWithoutRetrying()
    {
        var callCount = 0;

        var result = await _policy.ExecuteAsync(() =>
        {
            callCount++;
            return Task.FromResult("ok");
        }, "TestOp");

        Assert.AreEqual("ok", result);
        Assert.AreEqual(1, callCount);
        _metricsMock.Verify(x => x.RecordRetryAttempt(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ExecuteAsync_TransientFailureThenSuccess_RetriesAndSucceeds()
    {
        var callCount = 0;

        var result = await _policy.ExecuteAsync(() =>
        {
            callCount++;
            if (callCount < 3)
                throw new HttpRequestException("boom", inner: null, statusCode: HttpStatusCode.ServiceUnavailable);
            return Task.FromResult("recovered");
        }, "TestOp");

        Assert.AreEqual("recovered", result);
        Assert.AreEqual(3, callCount);
        _metricsMock.Verify(x => x.RecordRetryAttempt("TestOp"), Times.Exactly(2));
        _metricsMock.Verify(x => x.RecordRetrySuccess("TestOp"), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_ExceedsMaxRetries_ThrowsAndStopsAtLimit()
    {
        var callCount = 0;

        try
        {
            await _policy.ExecuteAsync<string>(() =>
            {
                callCount++;
                throw new HttpRequestException("persistent failure", inner: null, statusCode: HttpStatusCode.ServiceUnavailable);
            }, "TestOp");
            Assert.Fail("Expected HttpRequestException");
        }
        catch (HttpRequestException)
        {
            // Expected
        }

        // Initial attempt + 3 retries = 4 total calls
        Assert.AreEqual(4, callCount);
        _metricsMock.Verify(x => x.RecordRetryAttempt("TestOp"), Times.Exactly(3));
        _metricsMock.Verify(x => x.RecordRetrySuccess(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ExecuteAsync_NonTransientStatusCode_DoesNotRetry()
    {
        var callCount = 0;

        try
        {
            await _policy.ExecuteAsync<string>(() =>
            {
                callCount++;
                throw new HttpRequestException("bad request", inner: null, statusCode: HttpStatusCode.BadRequest);
            }, "TestOp");
            Assert.Fail("Expected HttpRequestException");
        }
        catch (HttpRequestException)
        {
            // Expected
        }

        Assert.AreEqual(1, callCount);
        _metricsMock.Verify(x => x.RecordRetryAttempt(It.IsAny<string>()), Times.Never);
        _metricsMock.Verify(x => x.RecordError("TestOp", "BadRequest"), Times.Once);
    }

    [TestMethod]
    public async Task ExecuteAsync_NonHttpException_DoesNotRetry()
    {
        var callCount = 0;

        try
        {
            await _policy.ExecuteAsync<string>(() =>
            {
                callCount++;
                throw new InvalidOperationException("logic error");
            }, "TestOp");
            Assert.Fail("Expected InvalidOperationException");
        }
        catch (InvalidOperationException)
        {
            // Expected
        }

        Assert.AreEqual(1, callCount);
        _metricsMock.Verify(x => x.RecordRetryAttempt(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ExecuteAsync_CancellationAlreadyRequested_DoesNotRetryTransientFailure()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var callCount = 0;

        try
        {
            await _policy.ExecuteAsync<string>(() =>
            {
                callCount++;
                throw new HttpRequestException("boom", inner: null, statusCode: HttpStatusCode.ServiceUnavailable);
            }, "TestOp", cts.Token);
            Assert.Fail("Expected HttpRequestException");
        }
        catch (HttpRequestException)
        {
            // Expected
        }

        Assert.AreEqual(1, callCount);
        _metricsMock.Verify(x => x.RecordRetryAttempt(It.IsAny<string>()), Times.Never);
    }

    [TestMethod]
    public async Task ExecuteAsync_TooManyRequestsStatus_IsRetried()
    {
        var callCount = 0;

        var result = await _policy.ExecuteAsync(() =>
        {
            callCount++;
            if (callCount < 2)
                throw new HttpRequestException("rate limited", inner: null, statusCode: HttpStatusCode.TooManyRequests);
            return Task.FromResult("ok");
        }, "TestOp");

        Assert.AreEqual("ok", result);
        Assert.AreEqual(2, callCount);
    }
}
