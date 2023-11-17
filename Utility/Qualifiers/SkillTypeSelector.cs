using Apex.AI;
using Apex.Serialization;
using static Enums;

namespace JRPG
{
    public class SkillTypeSelector : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        [ApexSerialization]
        Actions skillAction;
        [ApexSerialization]
        ActionAttributes actionAttributes;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            if (c.CurrentActiveSkill == null) return -10;

            return (c.CurrentActiveSkill.Action == skillAction && c.CurrentActiveSkill.ActionAttribute == actionAttributes)?desiredScore:-10;
        }
    }
}