using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    [FriendlyName("Select enemy with lowest HP in range", "Returns the difference between current unit HP and the HP of the sent battle controller.")]
    public class LowestHP : OptionScorerBase<BattleController>
    {
        public override float Score(IAIContext context, BattleController bc)
        {
            var c = (AIContext)context;
                        
            return c.CurrentUnit.TroopStats.HitPoints.StatValue - bc.TroopStats.HitPoints.StatValue;
        }
    }
}