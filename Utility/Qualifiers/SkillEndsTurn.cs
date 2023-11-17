using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class SkillEndsTurn : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            return (c.CurrentActiveSkill.SkillTriggerEnd)?30:-10;
        }
    }
}