using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class SkillNeedsRepositioning : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var skill = c.CurrentActiveSkill;

            bool conditional = (skill.SkillTarget == Enums.SkillTargets.ALL_ENEMY_UNITS_IN_LINE) ||
                               (skill.SkillTarget == Enums.SkillTargets.ALL_UNITS_IN_LINE) ||
                               (skill.Action == Enums.Actions.KNOCKDOWN) ||
                               (skill.Action == Enums.Actions.PUSH);
                               

            return (conditional)?10:-10;
        }
    }
}