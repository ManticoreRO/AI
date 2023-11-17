using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    [FriendlyName("Target Myself for Skill", "AI targets the current troop it is controlling for skill usage if the skill is targeted.")]
    public class TargetMyselfForSkill : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            BattleManager.TargetUnits.Add(c.CurrentUnit);
        }
    }
}