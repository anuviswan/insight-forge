namespace Insight.Services.Interfaces.Ai.Events;

public enum AgentEventType
{
    Interacting,       // interaction.created - agent started processing
    StepStarted,       // step.start - a step began
    StepProgressing,   // step.delta - content is streaming
    StepCompleted,     // step.stop - step finished
    InteractionComplete, // interaction.completed - all steps done
    FunctionCalled,    // function call detected in step.delta
    FunctionCompleted, // function execution result received
    Error              // error occurred
}
