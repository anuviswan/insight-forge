using Insight.Services.Interfaces.Core;
using Insight.WebApi.Services;

namespace Insight.Services.Core.Tests;

[TestClass]
public class BlogJobResultStoreTests
{
    private BlogJobResultStore _store;

    [TestInitialize]
    public void Setup()
    {
        _store = new BlogJobResultStore();
    }

    [TestMethod]
    public void GetResult_WithNoStoredResult_ReturnsNull()
    {
        Assert.IsNull(_store.GetResult("unknown-job"));
    }

    [TestMethod]
    public void SetResult_ThenGetResult_ReturnsSuccessfulResult()
    {
        var entry = new BlogEntry { Content = "Generated content" };

        _store.SetResult("job-1", entry);
        var result = _store.GetResult("job-1");

        Assert.IsNotNull(result);
        Assert.IsTrue(result.IsSuccess);
        Assert.AreEqual("Generated content", result.Entry!.Content);
        Assert.IsNull(result.Error);
    }

    [TestMethod]
    public void SetError_ThenGetResult_ReturnsFailedResult()
    {
        _store.SetError("job-1", "something went wrong");
        var result = _store.GetResult("job-1");

        Assert.IsNotNull(result);
        Assert.IsFalse(result.IsSuccess);
        Assert.AreEqual("something went wrong", result.Error);
        Assert.IsNull(result.Entry);
    }

    [TestMethod]
    public void SetResult_OverwritesPreviousError()
    {
        _store.SetError("job-1", "first attempt failed");
        _store.SetResult("job-1", new BlogEntry { Content = "retry succeeded" });

        var result = _store.GetResult("job-1");

        Assert.IsTrue(result!.IsSuccess);
        Assert.AreEqual("retry succeeded", result.Entry!.Content);
    }

    [TestMethod]
    public void MultipleJobs_MaintainSeparateResults()
    {
        _store.SetResult("job-1", new BlogEntry { Content = "content 1" });
        _store.SetResult("job-2", new BlogEntry { Content = "content 2" });

        Assert.AreEqual("content 1", _store.GetResult("job-1")!.Entry!.Content);
        Assert.AreEqual("content 2", _store.GetResult("job-2")!.Entry!.Content);
    }

    [TestMethod]
    public void SetResult_WithNullEntry_ThrowsArgumentNullException()
    {
        try
        {
            _store.SetResult("job-1", null!);
            Assert.Fail("Expected ArgumentNullException");
        }
        catch (ArgumentNullException)
        {
            // Expected
        }
    }

    [TestMethod]
    public void SetResult_WithEmptyJobId_ThrowsArgumentException()
    {
        try
        {
            _store.SetResult("", new BlogEntry());
            Assert.Fail("Expected ArgumentException");
        }
        catch (ArgumentException)
        {
            // Expected
        }
    }
}
