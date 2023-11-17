using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class LowestHPScore : QualifierBase
    {
        [ApexSerialization]
        bool isEnemy;
        [ApexSerialization]
        bool forAdjacent;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            float retValue = -10;

            if (isEnemy)
            {
                if (forAdjacent)
                {
                    if (c.AllAdjacentEnemies.Count > 0)
                    {
                        // select the lowest hp
                        c.SelectedEnemy = c.AllAdjacentEnemies[0];
                        // we still try to detect if we can kill it 
                        retValue = c.CurrentUnit.TroopStats.AttackPower.StatValue - c.SelectedEnemy.TroopStats.HitPoints.StatValue;
                    }
                }
                else
                {
                    if (c.AllEnemiesInRange.Count > 0)
                    {
                        c.SelectedEnemy = c.AllEnemiesInRange[0];
                        retValue = c.CurrentUnit.TroopStats.AttackPower.StatValue - c.SelectedEnemy.TroopStats.HitPoints.StatValue;
                    }
                }
            }
            else // if for our own units then we dont attack, we just update the skill target, since on allies we can use just skills
            {
                if (forAdjacent)
                {
                    if (c.AllAdjacentAllies.Count > 0)
                    {                        
                        retValue = c.AllAdjacentAllies[0].TroopStats.HitPointsPool.StatValue - c.AllAdjacentAllies[0].TroopStats.HitPoints.StatValue;
                    }
                }
                else
                {
                    // we sort the allies in range after hp difference
                    c.AllAlliesInRange.Sort((o1, o2) => AIManager.Instance.SortListByRemainingHP(o1.TroopStats, o2.TroopStats));                    
                    retValue = c.AllAlliesInRange[0].TroopStats.HitPointsPool.StatValue - c.AllAlliesInRange[0].TroopStats.HitPoints.StatValue;
                }
            }

            Debug.Log("===========> AI: testing if low hp enemies around! Score = " + retValue);
            return retValue * 10;
        }
    }
}