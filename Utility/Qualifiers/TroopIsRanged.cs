using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class TroopIsRanged : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            
            var c = context as AIContext;

            if (c.CurrentUnit.TroopStats.AttackRange.StatValue > 1)
            {
                // select a target in range with lowest hp if we have any
                if (c.AllEnemiesInRange.Count > 0)
                {
                    c.SelectedEnemy = c.AllEnemiesInRange[0];
                }
            }

            Debug.Log("===========> AI: testing if archer! Score = " + ((c.CurrentUnit.TroopStats.AttackRange.StatValue > 1) ? desiredScore : -10));
            return (c.CurrentUnit.TroopStats.AttackRange.StatValue > 1) ? desiredScore : -10;
        }
    }
}
