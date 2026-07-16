using Insight.Services.Ai.Gemini.AgentServices;
using Insight.Services.Ai.Gemini.Interfaces;
using Insight.Services.Ai.Gemini.Streaming.Types;
using Insight.Services.Ai.Gemini.Types;
using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insight.Services.Core.Tests.Streaming;

[TestClass]
public class GeminiAgentContentAccumulationTests
{
    private Mock<IGeminiApiClient> _apiClientMock = null!;
    private Mock<IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>> _metadataProviderMock = null!;
    private Mock<IEventBus> _eventBusMock = null!;
    private ILoggerFactory _loggerFactory = null!;

    [TestInitialize]
    public void Setup()
    {
        _apiClientMock = new Mock<IGeminiApiClient>();
        _apiClientMock.Setup(c => c.AgentExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _metadataProviderMock = new Mock<IAgentMetadataProvider<AgentDefinitionDto, SkillDto, WorkflowDto>>();
        _eventBusMock = new Mock<IEventBus>();
        _loggerFactory = NullLoggerFactory.Instance;
    }

    // Simulates a multi-step interaction where a tool-call step (e.g. google_search)
    // completes *after* the text-producing model_output step. Regression test for the
    // content accumulator being wiped by that later, textless step.
    [TestMethod]
    public async Task CreateBlogPostStreamedAsync_WhenToolCallStepFollowsTextStep_ShouldNotLoseAccumulatedContent()
    {
        var events = new List<GeminiStreamEvent>
        {
            new()
            {
                EventType = "interaction.created",
                Interaction = new InteractionData { Id = "interaction-1" }
            },
            new()
            {
                EventType = "step.start",
                Step = new StepData { Id = "step-0", Type = "model_output", Index = 0 }
            },
            new()
            {
                EventType = "step.delta",
                Step = new StepData
                {
                    Id = "step-0",
                    Content = new List<ContentData> { new() { Type = "text", Text = "Hello World" } }
                }
            },
            new()
            {
                EventType = "step.stop",
                Step = new StepData { Id = "step-0", Type = "model_output" }
            },
            new()
            {
                EventType = "step.start",
                Step = new StepData { Id = "step-1", Type = "tool_call", Index = 1 }
            },
            new()
            {
                EventType = "step.stop",
                Step = new StepData { Id = "step-1", Type = "tool_call" }
            },
            new()
            {
                EventType = "interaction.completed",
                Interaction = new InteractionData { Id = "interaction-1", Status = "succeeded" }
            }
        };

        _apiClientMock
            .Setup(c => c.StreamAgentInteractionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(events));

        var agent = new GeminiAgent(_apiClientMock.Object, _metadataProviderMock.Object, _loggerFactory);

        var result = await agent.CreateBlogPostStreamedAsync("topic", "audience", "style", _eventBusMock.Object);

        Assert.AreEqual("Hello World", result.Content);
    }

    [TestMethod]
    public async Task CreateBlogPostStreamedAsync_WithMultipleTextSteps_ShouldConcatenateContent()
    {
        var events = new List<GeminiStreamEvent>
        {
            new() { EventType = "interaction.created", Interaction = new InteractionData { Id = "interaction-1" } },
            new() { EventType = "step.start", Step = new StepData { Id = "step-0", Type = "model_output", Index = 0 } },
            new()
            {
                EventType = "step.delta",
                Step = new StepData
                {
                    Id = "step-0",
                    Content = new List<ContentData> { new() { Type = "text", Text = "Part one. " } }
                }
            },
            new() { EventType = "step.stop", Step = new StepData { Id = "step-0", Type = "model_output" } },
            new() { EventType = "step.start", Step = new StepData { Id = "step-1", Type = "model_output", Index = 1 } },
            new()
            {
                EventType = "step.delta",
                Step = new StepData
                {
                    Id = "step-1",
                    Content = new List<ContentData> { new() { Type = "text", Text = "Part two." } }
                }
            },
            new() { EventType = "step.stop", Step = new StepData { Id = "step-1", Type = "model_output" } },
            new() { EventType = "interaction.completed", Interaction = new InteractionData { Id = "interaction-1", Status = "succeeded" } }
        };

        _apiClientMock
            .Setup(c => c.StreamAgentInteractionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(ToAsyncEnumerable(events));

        var agent = new GeminiAgent(_apiClientMock.Object, _metadataProviderMock.Object, _loggerFactory);

        var result = await agent.CreateBlogPostStreamedAsync("topic", "audience", "style", _eventBusMock.Object);

        Assert.AreEqual("Part one. Part two.", result.Content);
    }

    private static async IAsyncEnumerable<GeminiStreamEvent> ToAsyncEnumerable(IEnumerable<GeminiStreamEvent> events)
    {
        foreach (var e in events)
        {
            yield return e;
        }
        await Task.CompletedTask;
    }
}
