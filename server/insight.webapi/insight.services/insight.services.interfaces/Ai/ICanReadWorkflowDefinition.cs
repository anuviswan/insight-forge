namespace Insight.Services.Interfaces.Ai;

public interface ICanReadWorkflowDefinition<TWorkflowDefinition> where TWorkflowDefinition : class
{
    IDictionary<string, TWorkflowDefinition> Load(string workflowFolder);
    TWorkflowDefinition GetWorkflow(string workflowName);
}


