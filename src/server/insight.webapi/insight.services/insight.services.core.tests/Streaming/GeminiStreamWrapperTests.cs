using Insight.Services.Ai.Gemini.Streaming;
using Insight.Services.Ai.Gemini.Streaming.Types;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Text;

namespace Insight.Services.Core.Tests.Streaming;

[TestClass]
public class GeminiStreamWrapperTests
{
    private Mock<ILogger<GeminiStreamWrapper>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GeminiStreamWrapper>>();
    }

    [TestMethod]
    public async Task StreamAsync_WithNullAgentId_ShouldThrow()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        try
        {
            await foreach (var evt in wrapper.StreamAsync(null!, "input"))
            {
            }
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task StreamAsync_WithEmptyInput_ShouldThrow()
    {
        var handler = new MockHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        try
        {
            await foreach (var evt in wrapper.StreamAsync("agent-123", ""))
            {
            }
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }

    [TestMethod]
    public async Task StreamAsync_WithValidStream_ShouldParseEvents()
    {
        var streamContent = """
            {"event_type": "interaction.created", "interaction": {"id": "interaction-123"}}
            {"event_type": "step.start", "step": {"id": "step-1", "index": 0}}
            {"event_type": "step.stop", "step": {"id": "step-1"}}
            """;

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var events = new List<GeminiStreamEvent>();
        await foreach (var evt in wrapper.StreamAsync("agent-123", "input"))
        {
            events.Add(evt);
        }

        Assert.AreEqual(3, events.Count);
        Assert.AreEqual("interaction.created", events[0].EventType);
        Assert.AreEqual("step.start", events[1].EventType);
        Assert.AreEqual("step.stop", events[2].EventType);
    }

    [TestMethod]
    public async Task StreamAsync_WithSSEFormat_ShouldParseEvents()
    {
        var streamContent = """
            data: {"event_type": "interaction.created", "interaction": {"id": "interaction-123"}}
            data: {"event_type": "step.start", "step": {"id": "step-1"}}
            """;

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var events = new List<GeminiStreamEvent>();
        await foreach (var evt in wrapper.StreamAsync("agent-123", "input"))
        {
            events.Add(evt);
        }

        Assert.AreEqual(2, events.Count);
        Assert.AreEqual("interaction.created", events[0].EventType);
        Assert.IsNotNull(events[0].Interaction);
        Assert.AreEqual("interaction-123", events[0].Interaction.Id);
    }

    [TestMethod]
    public async Task StreamAsync_WithEmptyLines_ShouldSkipThem()
    {
        var streamContent = """
            {"event_type": "interaction.created", "interaction": {"id": "interaction-123"}}


            {"event_type": "step.start", "step": {"id": "step-1"}}
            """;

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var events = new List<GeminiStreamEvent>();
        await foreach (var evt in wrapper.StreamAsync("agent-123", "input"))
        {
            events.Add(evt);
        }

        Assert.AreEqual(2, events.Count);
    }

    [TestMethod]
    public async Task StreamAsync_WithMalformedJson_ShouldSkipAndContinue()
    {
        var streamContent = """
            {"event_type": "interaction.created", "interaction": {"id": "interaction-123"}}
            {malformed json}
            {"event_type": "step.start", "step": {"id": "step-1"}}
            """;

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var events = new List<GeminiStreamEvent>();
        await foreach (var evt in wrapper.StreamAsync("agent-123", "input"))
        {
            events.Add(evt);
        }

        Assert.AreEqual(2, events.Count); // Skipped malformed line
        Assert.AreEqual("interaction.created", events[0].EventType);
        Assert.AreEqual("step.start", events[1].EventType);
    }

    [TestMethod]
    public async Task StreamAsync_WithCancellation_ShouldStop()
    {
        var streamContent = new string(Enumerable.Range(0, 100)
            .Select(_ => """{"event_type": "step.delta"}""")
            .Aggregate((a, b) => a + "\n" + b)
            .ToArray());

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var cts = new CancellationTokenSource();
        var events = new List<GeminiStreamEvent>();

        await foreach (var evt in wrapper.StreamAsync("agent-123", "input", cts.Token))
        {
            events.Add(evt);
            if (events.Count >= 5)
                cts.Cancel();
        }

        Assert.IsTrue(events.Count >= 5);
        Assert.IsTrue(events.Count <= 10); // Roughly 5, might be slightly more
    }

    [TestMethod]
    public async Task StreamAsync_WithComplexEvent_ShouldParseCorrectly()
    {
        var streamContent = """
            {"event_type": "step.delta", "step": {"id": "step-1", "index": 0, "type": "model_output", "content": [{"type": "text", "text": "Hello "}, {"type": "text", "text": "World"}], "function_calls": [{"name": "google_search", "id": "call-123", "args": {"query": "test"}}]}}
            """;

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var events = new List<GeminiStreamEvent>();
        await foreach (var e in wrapper.StreamAsync("agent-123", "input"))
        {
            events.Add(e);
        }

        Assert.AreEqual(1, events.Count);
        var evt = events[0];
        Assert.AreEqual("step.delta", evt.EventType);
        Assert.IsNotNull(evt.Step);
        Assert.AreEqual("step-1", evt.Step.Id);
        Assert.AreEqual(2, evt.Step.Content!.Count);
        Assert.AreEqual(1, evt.Step.FunctionCalls!.Count);
        Assert.AreEqual("google_search", evt.Step.FunctionCalls[0].Name);
    }

    [TestMethod]
    public async Task StreamAsync_WithCompleteNonIncrementalResponse_ShouldSynthesizeEquivalentEvents()
    {
        // Some agent responses aren't delivered incrementally: the whole completed
        // interaction arrives as a single line matching the non-streaming response
        // shape (id/status/steps) rather than a sequence of event_type-tagged frames.
        var streamContent = """
            {"id": "interaction-1", "status": "completed", "steps": [{"id": "s0", "type": "function_call", "name": "google_search"}, {"id": "s1", "type": "model_output", "content": [{"type": "text", "text": "Hello World"}]}], "usage": {"total_tokens": 100, "total_input_tokens": 60, "total_output_tokens": 40}}
            """;

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var events = new List<GeminiStreamEvent>();
        await foreach (var evt in wrapper.StreamAsync("agent-123", "input"))
        {
            events.Add(evt);
        }

        Assert.AreEqual("interaction.created", events[0].EventType);
        Assert.AreEqual("interaction.completed", events[^1].EventType);
        Assert.AreEqual(100, events[^1].Interaction!.Usage!.TotalTokens);

        var modelOutputStop = events.Single(e =>
            e.EventType == "step.stop" && e.Step?.Type == "model_output");
        Assert.AreEqual("Hello World", modelOutputStop.Step!.Content!.Single().Text);

        // The non-text step should produce no step.delta (nothing to accumulate).
        Assert.IsFalse(events.Any(e => e.EventType == "step.delta" && e.Step?.Type == "function_call"));
    }

    [TestMethod]
    public async Task StreamAsync_WithCompleteResponseButNoSteps_ShouldFallBackToWarning()
    {
        var streamContent = """
            {"id": "interaction-1", "status": "completed"}
            """;

        var handler = new MockHttpMessageHandler(streamContent);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://api.gemini.test/") };
        var wrapper = new GeminiStreamWrapper(httpClient, _loggerMock.Object);

        var events = new List<GeminiStreamEvent>();
        await foreach (var evt in wrapper.StreamAsync("agent-123", "input"))
        {
            events.Add(evt);
        }

        Assert.AreEqual(0, events.Count);
    }

    // Mock HttpMessageHandler for testing
    private class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly string _content;

        public MockHttpMessageHandler(string content = "")
        {
            _content = content;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StreamContent(
                    new MemoryStream(Encoding.UTF8.GetBytes(_content))
                )
            };

            response.Content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/event-stream");
            return await Task.FromResult(response);
        }
    }
}
