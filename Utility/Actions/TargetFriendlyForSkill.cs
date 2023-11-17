using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    [FriendlyName("Target Ally for Skill", "AI targets an ally unit for skill usage if the skill is targeted.")]
    public class TargetFriendlyForSkill : ActionBase
    {
        [ApexSerialization(defaultValue = false)]
        bool forAdjacent;
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            if (forAdjacent)
            {
                BattleManager.TargetUnits.Add(c.AllAdjacentAllies[0]);
            }
            else
            {
                BattleManager.TargetUnits.Add(c.AllAlliesInRange[0]);
            }
        }
    }
}