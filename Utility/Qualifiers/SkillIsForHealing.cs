using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using _DPS;

namespace JRPG
{
    public class SkillIsForHealing : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var skill = c.CurrentActiveSkill;

            bool conditional = (skill.ActionAttribute.Contains(Enums.ActionAttributes.HITPOINTS | Enums.ActionAttributes.MAX_HITPOINTS)) &&
                               ((skill.Action == Enums.Actions.INCREASE) || (skill.Action == Enums.Actions.SHIELD));

            return (conditional) ? 10 : -10;
        }
    }
}