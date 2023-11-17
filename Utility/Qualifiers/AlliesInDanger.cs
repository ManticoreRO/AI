using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class AlliesInDanger : QualifierBase
    {        
        [ApexSerialization(defaultValue = 10)]
        public int score;

        public override float Score(IAIContext context)
        {
            var c = context as AIContext;

            if (c.AllAlliesInRangeInDanger.Count > 0)
            {
                // sort the list of allies in danger by hitpoints
                c.AllAlliesInRangeInDanger.Sort((o1, o2) => o1.TroopStats.HitPoints.StatValue.CompareTo(o2.TroopStats.HitPoints.StatValue));
                // find all enemies around it
                BattleController selected = c.AllAlliesInRangeInDanger[0];
                List<BattleController> targets = AIManager.Instance.GetAdjacentEnemies(selected);
                // select the lowest hp enemy
                c.SelectedEnemy = targets[0];

                return score;
            }
            else
                return -1;
        }
    }
}