using Insight.Services.Ai.Gemini.Streaming.Types;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Ai.Gemini.Streaming;

public class GeminiEventTransformer
{
    private readonly ILogger<GeminiEventTransformer> _logger;
    private string _currentInteractionId = string.Empty;
    private int _currentStepIndex = 0;
    private string _accumulatedContent = string.Empty;
    private DateTime _stepStartTime = DateTime.UtcNow;
    private UsageData? _lastUsageData;

    public GeminiEventTransformer(ILogger<GeminiEventTransformer> logger)
    {
        _logger = logger;
    }

    public List<AgentStatusEvent> Transform(GeminiStreamEvent geminiEvent)
    {
        ArgumentNullException.ThrowIfNull(geminiEvent);

        var events = new List<AgentStatusEvent>();

        if (geminiEvent.EventType == null)
        {
            _logger.LogWarning("Received Gemini stream event with null EventType");
            return events;
        }

        switch (geminiEvent.EventType)
        {
            case "interaction.created":
                events.AddRange(HandleInteractionCreated(geminiEvent));
                break;
            case "step.start":
                events.AddRange(HandleStepStart(geminiEvent));
                break;
            case "step.delta":
                events.AddRange(HandleStepDelta(geminiEvent));
                break;
            case "step.stop":
                events.AddRange(HandleStepStop(geminiEvent));
                break;
            case "interaction.completed":
                events.AddRange(HandleInteractionCompleted(geminiEvent));
                break;
            default:
                _logger.LogDebug("Received unknown Gemini event type: {EventType}", geminiEvent.EventType);
                break;
        }

        return events;
    }

    private List<AgentStatusEvent> HandleInteractionCreated(GeminiStreamEvent geminiEvent)
    {
        if (geminiEvent.Interaction?.Id == null)
        {
            _logger.LogWarning("interaction.created event missing interaction.id");
            return [];
        }

        _currentInteractionId = geminiEvent.Interaction.Id;
        _currentStepIndex = 0;
        _accumulatedContent = string.Empty;

        return
        [
            new AgentStatusEvent
            {
                InteractionId = _currentInteractionId,
                EventType = AgentEventType.Interacting,
                Status = "Agent started processing request",
                Timestamp = DateTime.UtcNow
            }
        ];
    }

    private List<AgentStatusEvent> HandleStepStart(GeminiStreamEvent geminiEvent)
    {
        if (geminiEvent.Step?.Id == null)
        {
            _logger.LogWarning("step.start event missing step.id");
            return [];
        }

        _currentStepIndex = geminiEvent.Step.Index ?? 0;
        _accumulatedContent = string.Empty;
        _stepStartTime = DateTime.UtcNow;

        var stepType = geminiEvent.Step.Type ?? "unknown";

        return
        [
            new AgentStatusEvent
            {
                InteractionId = _currentInteractionId,
                EventType = AgentEventType.StepStarted,
                Status = $"Step {_currentStepIndex} started ({stepType})",
                Data = new Dictionary<string, object>
                {
                    { "step_id", geminiEvent.Step.Id },
                    { "step_type", stepType },
                    { "step_index", _currentStepIndex }
                },
                Timestamp = DateTime.UtcNow
            }
        ];
    }

    private List<AgentStatusEvent> HandleStepDelta(GeminiStreamEvent geminiEvent)
    {
        if (geminiEvent.Step == null)
        {
            _logger.LogWarning("step.delta event missing step data");
            return [];
        }

        // Accumulate text content
        if (geminiEvent.Step.Content != null)
        {
            foreach (var content in geminiEvent.Step.Content)
            {
                if (content.Type == "text" && !string.IsNullOrEmpty(content.Text))
                {
                    _accumulatedContent += content.Text;
                }
            }
        }

        var progress = CalculateProgress(_accumulatedContent);

        var result = new List<AgentStatusEvent>
        {
            new AgentStatusEvent
            {
                InteractionId = _currentInteractionId,
                EventType = AgentEventType.StepProgressing,
                Status = "Step in progress",
                Progress = progress,
                Data = new Dictionary<string, object>
                {
                    { "step_index", _currentStepIndex },
                    { "accumulated_content", _accumulatedContent }
                },
                Timestamp = DateTime.UtcNow
            }
        };

        // Check for function calls in step.delta
        if (geminiEvent.Step.FunctionCalls != null && geminiEvent.Step.FunctionCalls.Count > 0)
        {
            foreach (var funcCall in geminiEvent.Step.FunctionCalls)
            {
                if (!string.IsNullOrEmpty(funcCall.Name))
                {
                    result.Add(new AgentStatusEvent
                    {
                        InteractionId = _currentInteractionId,
                        EventType = AgentEventType.FunctionCalled,
                        Status = $"Function call: {funcCall.Name}",
                        Data = new Dictionary<string, object>
                        {
                            { "function_name", funcCall.Name },
                            { "function_id", funcCall.Id ?? string.Empty },
                            { "function_args", funcCall.Args ?? new Dictionary<string, object>() }
                        },
                        Timestamp = DateTime.UtcNow
                    });
                }
            }
        }

        return result;
    }

    private List<AgentStatusEvent> HandleStepStop(GeminiStreamEvent geminiEvent)
    {
        if (geminiEvent.Step == null)
        {
            _logger.LogWarning("step.stop event missing step data");
            return [];
        }

        var progress = CalculateProgress(_accumulatedContent);

        return
        [
            new AgentStatusEvent
            {
                InteractionId = _currentInteractionId,
                EventType = AgentEventType.StepCompleted,
                Status = $"Step {_currentStepIndex} completed",
                Progress = progress,
                Data = new Dictionary<string, object>
                {
                    { "step_id", geminiEvent.Step.Id ?? string.Empty },
                    { "step_type", geminiEvent.Step.Type ?? string.Empty },
                    { "step_index", _currentStepIndex },
                    { "final_content", _accumulatedContent }
                },
                Timestamp = DateTime.UtcNow
            }
        ];
    }

    private List<AgentStatusEvent> HandleInteractionCompleted(GeminiStreamEvent geminiEvent)
    {
        if (geminiEvent.Interaction == null)
        {
            _logger.LogWarning("interaction.completed event missing interaction data");
            return [];
        }

        _lastUsageData = geminiEvent.Interaction.Usage ?? geminiEvent.Usage;

        var progress = CalculateProgress(_accumulatedContent, _lastUsageData);

        return
        [
            new AgentStatusEvent
            {
                InteractionId = _currentInteractionId,
                EventType = AgentEventType.InteractionComplete,
                Status = "Agent completed processing",
                Progress = progress,
                Data = new Dictionary<string, object>
                {
                    { "interaction_status", geminiEvent.Interaction.Status ?? string.Empty },
                    { "final_content", _accumulatedContent }
                },
                Timestamp = DateTime.UtcNow
            }
        ];
    }

    private ProgressData CalculateProgress(string content, UsageData? usage = null)
    {
        return new ProgressData
        {
            CurrentStep = _currentStepIndex,
            WordCount = CountWords(content),
            CharacterCount = content.Length,
            ParagraphCount = CountParagraphs(content),
            ElapsedTime = DateTime.UtcNow - _stepStartTime,
            TotalInputTokens = usage?.TotalInputTokens,
            TotalOutputTokens = usage?.TotalOutputTokens,
            TotalCachedTokens = usage?.TotalCachedTokens,
            TotalThoughtTokens = usage?.TotalThoughtTokens
        };
    }

    private static int CountWords(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
    }

    private static int CountParagraphs(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0;

        return text.Split(new[] { "\r\n\r\n", "\n\n" }, StringSplitOptions.None).Length;
    }
}
