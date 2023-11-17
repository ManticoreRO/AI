using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    [FriendlyName("Test My HP", "Returns the amount of damage i receive until my HP is 0.")]
    public class AmIGoingToDie : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var stats = c.CurrentUnit.TroopStats;

            return (stats.HitPointsPool.StatValue - stats.HitPoints.StatValue);
        }
    }
}