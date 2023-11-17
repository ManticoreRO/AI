using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class SkillHasSpalsh : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var skill = c.CurrentActiveSkill;

            bool conditional = (skill.SkillTarget == Enums.SkillTargets.PBAOE_ALLY) ||
                               (skill.SkillTarget == Enums.SkillTargets.PBAOE_ALL_NON_ALLY) ||
                               (skill.SkillTarget == Enums.SkillTargets.PBAOE_ENEMY) ||
                               (skill.SkillTarget == Enums.SkillTargets.SPLASH_ALLY) ||
                               (skill.SkillTarget == Enums.SkillTargets.SPLASH_ALL_NON_ALLY) ||
                               (skill.SkillTarget == Enums.SkillTargets.SPLASH_ENEMY);                               

            return (conditional) ? 10 : -10;
        }
    }
}
