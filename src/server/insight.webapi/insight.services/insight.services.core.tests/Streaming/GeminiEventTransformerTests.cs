using Insight.Services.Ai.Gemini.Streaming;
using Insight.Services.Ai.Gemini.Streaming.Types;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Insight.Services.Core.Tests.Streaming;

[TestClass]
public class GeminiEventTransformerTests
{
    private Mock<ILogger<GeminiEventTransformer>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<GeminiEventTransformer>>();
    }

    [TestMethod]
    public void Transform_InteractionCreated_ShouldEmitInteractingEvent()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);
        var geminiEvent = new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData
            {
                Id = "interaction-123",
                Status = "active"
            }
        };

        var events = transformer.Transform(geminiEvent);

        Assert.AreEqual(1, events.Count);
        Assert.AreEqual(AgentEventType.Interacting, events[0].EventType);
        Assert.AreEqual("interaction-123", events[0].InteractionId);
        Assert.IsTrue(events[0].Status.Contains("started"));
    }

    [TestMethod]
    public void Transform_StepStart_ShouldEmitStepStartedEvent()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        // First create interaction
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });

        var geminiEvent = new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData
            {
                Id = "step-1",
                Type = "model_output",
                Index = 0
            }
        };

        var events = transformer.Transform(geminiEvent);

        Assert.AreEqual(1, events.Count);
        Assert.AreEqual(AgentEventType.StepStarted, events[0].EventType);
        Assert.IsTrue(events[0].Data!.ContainsKey("step_id"));
    }

    [TestMethod]
    public void Transform_StepDelta_ShouldAccumulateContent()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        // Setup
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData { Id = "step-1", Type = "model_output", Index = 0 }
        });

        // First delta
        var delta1 = new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-1",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "Hello " }
                }
            }
        };

        var events1 = transformer.Transform(delta1);
        Assert.AreEqual(1, events1.Count);
        Assert.AreEqual(AgentEventType.StepProgressing, events1[0].EventType);
        Assert.AreEqual(1, events1[0].Progress!.WordCount);

        // Second delta
        var delta2 = new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-1",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "World" }
                }
            }
        };

        var events2 = transformer.Transform(delta2);
        Assert.AreEqual(1, events2.Count);
        Assert.AreEqual(2, events2[0].Progress!.WordCount); // "Hello World"
    }

    [TestMethod]
    public void Transform_StepDelta_WithFunctionCall_ShouldEmitFunctionCalledEvent()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData { Id = "step-1", Index = 0 }
        });

        var deltaWithFunction = new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-1",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "Calling function..." }
                },
                FunctionCalls = new List<FunctionCallData>
                {
                    new FunctionCallData
                    {
                        Name = "google_search",
                        Id = "call-123",
                        Args = new Dictionary<string, object> { { "query", "test" } }
                    }
                }
            }
        };

        var events = transformer.Transform(deltaWithFunction);

        Assert.AreEqual(2, events.Count); // StepProgressing + FunctionCalled
        Assert.AreEqual(AgentEventType.StepProgressing, events[0].EventType);
        Assert.AreEqual(AgentEventType.FunctionCalled, events[1].EventType);
        Assert.AreEqual("google_search", events[1].Data!["function_name"]);
    }

    [TestMethod]
    public void Transform_StepStop_ShouldEmitStepCompletedEvent()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData { Id = "step-1", Index = 0 }
        });
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-1",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "Final content" }
                }
            }
        });

        var stopEvent = new GeminiStreamEvent
        {
            EventType = "step.stop",
            Step = new StepData { Id = "step-1", Type = "model_output" }
        };

        var events = transformer.Transform(stopEvent);

        Assert.AreEqual(1, events.Count);
        Assert.AreEqual(AgentEventType.StepCompleted, events[0].EventType);
        Assert.IsTrue(events[0].Data!.ContainsKey("final_content"));
    }

    [TestMethod]
    public void Transform_InteractionCompleted_ShouldEmitCompleteEventWithUsage()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });

        var completedEvent = new GeminiStreamEvent
        {
            EventType = "interaction.completed",
            Interaction = new InteractionData
            {
                Id = "interaction-123",
                Status = "succeeded",
                Usage = new UsageData
                {
                    TotalInputTokens = 100,
                    TotalOutputTokens = 200,
                    TotalCachedTokens = 50
                }
            }
        };

        var events = transformer.Transform(completedEvent);

        Assert.AreEqual(1, events.Count);
        Assert.AreEqual(AgentEventType.InteractionComplete, events[0].EventType);
        Assert.AreEqual(100, events[0].Progress!.TotalInputTokens);
        Assert.AreEqual(200, events[0].Progress!.TotalOutputTokens);
        Assert.AreEqual(50, events[0].Progress!.TotalCachedTokens);
    }

    [TestMethod]
    public void Transform_WithNullEventType_ShouldReturnEmpty()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);
        var geminiEvent = new GeminiStreamEvent
        {
            EventType = null
        };

        var events = transformer.Transform(geminiEvent);

        Assert.AreEqual(0, events.Count);
    }

    [TestMethod]
    public void Transform_WithUnknownEventType_ShouldReturnEmpty()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);
        var geminiEvent = new GeminiStreamEvent
        {
            EventType = "unknown.event",
            Interaction = new InteractionData { Id = "interaction-123" }
        };

        var events = transformer.Transform(geminiEvent);

        Assert.AreEqual(0, events.Count);
    }

    [TestMethod]
    public void Transform_CalculateProgress_WordCount()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData { Id = "step-1", Index = 0 }
        });

        var deltaEvent = new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-1",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "This is a test content" }
                }
            }
        };

        var events = transformer.Transform(deltaEvent);
        var progress = events[0].Progress!;

        Assert.AreEqual(5, progress.WordCount); // "This is a test content"
        Assert.AreEqual(22, progress.CharacterCount); // includes spaces
    }

    [TestMethod]
    public void Transform_CalculateProgress_ParagraphCount()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData { Id = "step-1", Index = 0 }
        });

        var deltaEvent = new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-1",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "Paragraph 1\n\nParagraph 2\n\nParagraph 3" }
                }
            }
        };

        var events = transformer.Transform(deltaEvent);
        var progress = events[0].Progress!;

        Assert.AreEqual(3, progress.ParagraphCount);
    }

    [TestMethod]
    public void Transform_MultipleSteps_ShouldTrackCurrentStepIndex()
    {
        var transformer = new GeminiEventTransformer(_loggerMock.Object);

        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "interaction.created",
            Interaction = new InteractionData { Id = "interaction-123" }
        });

        // Step 0
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData { Id = "step-0", Index = 0 }
        });

        var events0 = transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-0",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "Step 0 content" }
                }
            }
        });

        Assert.AreEqual(0, events0[0].Progress!.CurrentStep);

        // Step 1
        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.stop",
            Step = new StepData { Id = "step-0", Type = "model_output" }
        });

        transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.start",
            Step = new StepData { Id = "step-1", Index = 1 }
        });

        var events1 = transformer.Transform(new GeminiStreamEvent
        {
            EventType = "step.delta",
            Step = new StepData
            {
                Id = "step-1",
                Content = new List<ContentData>
                {
                    new ContentData { Type = "text", Text = "Step 1 content" }
                }
            }
        });

        Assert.AreEqual(1, events1[0].Progress!.CurrentStep);
    }
}
