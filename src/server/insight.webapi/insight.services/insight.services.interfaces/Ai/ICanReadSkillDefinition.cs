namespace Insight.Services.Interfaces.Ai;

public interface ICanReadSkillDefinition<TSkillDefinition> where TSkillDefinition : class
{
    IDictionary<string, TSkillDefinition> LoadSkills();
    TSkillDefinition GetSkill(string skillName);
}


