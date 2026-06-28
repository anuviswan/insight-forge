namespace Insight.Services.Interfaces.Ai;

public interface ICanReadWorkflowDefinition<TWorkflowDefinition> where TWorkflowDefinition : class
{
    IDictionary<string, TWorkflowDefinition> LoadWorkflow();
    TWorkflowDefinition GetWorkflow(string workflowName);
}


