using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SkillIsTargetedOnAdjacent : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            if (c.CurrentActiveSkill != null)
            {
                if (c.CurrentActiveSkill.ActionBaseRange == 1 && c.AllEnemiesInRange.Count > 0)
                {
                    return desiredScore;
                }
            }
            return -10;
        }
    }
}