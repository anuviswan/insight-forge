namespace Insight.Services.Interfaces.Ai;

public interface ICanReadSkillDefinition<TSkillDefinition> where TSkillDefinition : class
{
    IDictionary<string, TSkillDefinition> Load(string skillFolder);
    TSkillDefinition GetSkill(string skillName);
}


