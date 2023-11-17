using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace JRPG
{
    public class IsSkillTargeted : QualifierBase
    {
        [ApexSerialization(defaultValue = 10)]
        float score;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            bool resultTest = c.CurrentActiveSkill.SkillTarget == Enums.SkillTargets.TARGET ||
                              c.CurrentActiveSkill.SkillTarget == Enums.SkillTargets.SPLASH_ENEMY ||
                              c.CurrentActiveSkill.SkillTarget == Enums.SkillTargets.SPLASH_ALLY ||
                              c.CurrentActiveSkill.SkillTarget == Enums.SkillTargets.TARGET_TILE;

            Debug.Log("==========> AI: checking if skills requires target! result = " + resultTest);
            return (resultTest) ? score : -10;
        }
    }
}