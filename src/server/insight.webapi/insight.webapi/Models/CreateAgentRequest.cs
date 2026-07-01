namespace Insight.WebApi.Models;

public class CreateAgentRequest
{
    public string AgentName { get; set; }
    public string SystemInstruction { get; set; }
    public string Input { get; set; }
}
