using Insight.Services.Interfaces.Ai;
using Insight.Services.Interfaces.Ai.Events;
using Microsoft.Extensions.Logging;

namespace Insight.Services.Ai.Gemini.AgentServices;

public class ProgressMetricsService : IProgressMetricsService
{
    private readonly Dictionary<string, JobMetricsTracker> _jobMetrics = new();
    private readonly ILogger<ProgressMetricsService> _logger;
    private readonly object _lock = new();

    public ProgressMetricsService(ILogger<ProgressMetricsService> logger)
    {
        _logger = logger;
    }

    public void TrackEvent(string jobId, AgentStatusEvent @event)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));
        ArgumentNullException.ThrowIfNull(@event, nameof(@event));

        lock (_lock)
        {
            if (!_jobMetrics.TryGetValue(jobId, out var tracker))
            {
                tracker = new JobMetricsTracker(jobId, _logger);
                _jobMetrics[jobId] = tracker;
            }

            tracker.TrackEvent(@event);
        }
    }

    public JobProgressMetrics? GetProgress(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        lock (_lock)
        {
            if (_jobMetrics.TryGetValue(jobId, out var tracker))
            {
                return tracker.GetCurrentMetrics();
            }
        }

        return null;
    }

    public DetailedJobProgress? GetDetailedProgress(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        lock (_lock)
        {
            if (_jobMetrics.TryGetValue(jobId, out var tracker))
            {
                return tracker.GetDetailedProgress();
            }
        }

        return null;
    }

    public void ClearMetrics(string jobId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(jobId, nameof(jobId));

        lock (_lock)
        {
            if (_jobMetrics.Remove(jobId))
            {
                _logger.LogInformation("Cleared metrics for job {JobId}", jobId);
            }
        }
    }

    private class JobMetricsTracker
    {
        private readonly string _jobId;
        private readonly ILogger<ProgressMetricsService> _logger;
        private readonly List<StepMetrics> _stepHistory = new();
        private readonly List<string> _eventLog = new();

        private DateTime _startTime = DateTime.UtcNow;
        private DateTime _lastUpdated = DateTime.UtcNow;
        private string _currentStatus = "";
        private int _currentStep = 0;
        private int? _totalSteps;
        private int _totalWordCount = 0;
        private int _totalCharacterCount = 0;
        private int _totalParagraphCount = 0;
        private long? _totalInputTokens;
        private long? _totalOutputTokens;
        private long? _totalCachedTokens;
        private long? _totalThoughtTokens;
        private bool _isComplete = false;

        private StepMetrics? _currentStepMetrics;

        public JobMetricsTracker(string jobId, ILogger<ProgressMetricsService> logger)
        {
            _jobId = jobId;
            _logger = logger;
        }

        public void TrackEvent(AgentStatusEvent @event)
        {
            _lastUpdated = DateTime.UtcNow;
            _currentStatus = @event.Status;

            // Update progress if available
            if (@event.Progress != null)
            {
                _currentStep = @event.Progress.CurrentStep;
                _totalSteps = @event.Progress.TotalSteps;
                _totalWordCount = @event.Progress.WordCount;
                _totalCharacterCount = @event.Progress.CharacterCount;
                _totalParagraphCount = @event.Progress.ParagraphCount;

                if (@event.Progress.TotalInputTokens.HasValue)
                    _totalInputTokens = @event.Progress.TotalInputTokens;
                if (@event.Progress.TotalOutputTokens.HasValue)
                    _totalOutputTokens = @event.Progress.TotalOutputTokens;
                if (@event.Progress.TotalCachedTokens.HasValue)
                    _totalCachedTokens = @event.Progress.TotalCachedTokens;
                if (@event.Progress.TotalThoughtTokens.HasValue)
                    _totalThoughtTokens = @event.Progress.TotalThoughtTokens;
            }

            // Track events
            switch (@event.EventType)
            {
                case AgentEventType.StepStarted:
                    StartStep(@event);
                    _eventLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Step {_currentStep} started");
                    break;

                case AgentEventType.StepProgressing:
                    _eventLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Step {_currentStep} progressing");
                    break;

                case AgentEventType.StepCompleted:
                    EndStep(@event);
                    _eventLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Step {_currentStep} completed");
                    break;

                case AgentEventType.InteractionComplete:
                    _isComplete = true;
                    _eventLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Interaction complete");
                    break;

                case AgentEventType.FunctionCalled:
                    _eventLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Function called");
                    break;

                case AgentEventType.Error:
                    _eventLog.Add($"[{DateTime.UtcNow:HH:mm:ss}] Error: {@event.Error?.Message}");
                    break;
            }
        }

        private void StartStep(AgentStatusEvent @event)
        {
            if (_currentStepMetrics != null)
            {
                _currentStepMetrics.EndTime = DateTime.UtcNow;
                _stepHistory.Add(_currentStepMetrics);
            }

            _currentStepMetrics = new StepMetrics
            {
                StepIndex = _currentStep,
                StepType = @event.Data?.ContainsKey("step_type") == true
                    ? @event.Data["step_type"].ToString() ?? "unknown"
                    : "unknown",
                StartTime = DateTime.UtcNow,
                Status = @event.Status
            };
        }

        private void EndStep(AgentStatusEvent @event)
        {
            if (_currentStepMetrics != null)
            {
                _currentStepMetrics.EndTime = DateTime.UtcNow;
                _currentStepMetrics.Status = @event.Status;

                if (@event.Progress != null)
                {
                    _currentStepMetrics.WordCount = @event.Progress.WordCount;
                    _currentStepMetrics.CharacterCount = @event.Progress.CharacterCount;
                    _currentStepMetrics.ParagraphCount = @event.Progress.ParagraphCount;
                }

                _stepHistory.Add(_currentStepMetrics);
                _currentStepMetrics = null;
            }
        }

        public JobProgressMetrics GetCurrentMetrics()
        {
            return new JobProgressMetrics
            {
                JobId = _jobId,
                StartTime = _startTime,
                LastUpdated = _lastUpdated,
                CurrentStatus = _currentStatus,
                CurrentStep = _currentStep,
                TotalSteps = _totalSteps,
                TotalWordCount = _totalWordCount,
                TotalCharacterCount = _totalCharacterCount,
                TotalParagraphCount = _totalParagraphCount,
                ElapsedSeconds = (DateTime.UtcNow - _startTime).TotalSeconds,
                TotalInputTokens = _totalInputTokens,
                TotalOutputTokens = _totalOutputTokens,
                TotalCachedTokens = _totalCachedTokens,
                TotalThoughtTokens = _totalThoughtTokens,
                IsComplete = _isComplete
            };
        }

        public DetailedJobProgress GetDetailedProgress()
        {
            return new DetailedJobProgress
            {
                CurrentMetrics = GetCurrentMetrics(),
                StepHistory = new List<StepMetrics>(_stepHistory),
                Events = new List<string>(_eventLog)
            };
        }
    }
}
