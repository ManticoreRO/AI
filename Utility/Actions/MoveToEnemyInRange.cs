using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;
using Random = UnityEngine.Random;

namespace JRPG
{
    public class MoveToEnemyInRange : ActionBase
    {
        [ApexSerialization(defaultValue = false)]
        bool RandomEnemy;
        [ApexSerialization(defaultValue = false)]
        bool GoForTheEnemyHero;
        [ApexSerialization(defaultValue = false)]
        bool IsRanged;

        public override void Execute(IAIContext context)
        {

            var c = context as AIContext;

            // the ranged units move in  a specific way and do not need to use all their movement points
            if (IsRanged && c.CurrentUnit.HasMoved)
            {
                c.CurrentUnit.TemporaryMovementRange = 0;
            }
            // dont execute if troop is already moving
            if (c.CurrentUnit.IsMoving || c.CurrentUnit.TemporaryMovementRange <= 0 || c.CurrentUnit.HasMoved || c.CurrentUnit.IsDead) return;
            
            //Debug.Log("=======> AI: Moving to enemy in range!");
            if (RandomEnemy && !GoForTheEnemyHero && !c.ProtectHero)
            {
                if (c.AllEnemiesInRange.Count > 0)
                {
                    c.SelectedEnemy = c.AllEnemiesInRange[Random.Range(0, c.AllEnemiesInRange.Count)];
                }
                else // if we are here and there are no enemies on the map, then we got here because we have the enemy hero in range. even if we set the ai up not to go 
                     // for the enemy, he will do so
                {                   
                    c.SelectedEnemy = AIManager.Instance.GetEnemyHeroInRange(c.CurrentUnit);
                }
            }

            // if we force it to go to the enemy hero, we change the target here 
            // we use this only if we are sure the enemy hero is in range
            if (GoForTheEnemyHero)
            {
                c.SelectedEnemy = AIManager.Instance.GetEnemyHeroInRange(c.CurrentUnit);
            }
            else
            {
                if (c.ProtectHero && c.EnemiesAttackingHero.Count > 0)
                {
                    // if here then we have enemies attacking our own hero
                    c.EnemiesAttackingHero.Sort((o1, o2) => o1.TroopStats.HitPoints.StatValue.CompareTo(o2.TroopStats.HitPoints.StatValue));
                    c.SelectedEnemy = c.EnemiesAttackingHero[0];
                }
                else
                {
                    if (c.AllEnemiesInRange.Count > 0)
                    {
                        // select the lowest hp enemy
                        c.SelectedEnemy = c.AllEnemiesInRange[0];
                    }
                    else
                    {
                        Debug.Log("<color=red> There are no enemies in range even if we initially found some!</color>");
                    }
                }
            }

            if (!IsRanged)
            {
                if (c.SelectedEnemy != null)
                {
                    c.PositionToMove = EncounterManager.Instance.GetFreeAdjacentTile(c.SelectedEnemy.OnTile, c.CurrentUnit.OnTile);
                }               
            }
            else
            {
                if (c.SelectedEnemy != null)
                {
                    c.PositionToMove = AIManager.Instance.GetPossibleMovePositionForRanged(c.SelectedEnemy, c.CurrentUnit);
                    //Debug.Log("============> AI: <color=green>found position to move at for ranged: " + c.PositionToMove.TileCenter + "</color>");
                }
            }

            if (c.PositionToMove != null)
            {
                BattleManager.Instance.MoveUnitTowardsPosition(c.PositionToMove, EncounterManager.Instance.GetTroopIdByController(c.CurrentUnit));
            }
            else
            {
                Debug.LogError("=======> AI: There is no free adjacent tile for the selected enemy!");
                // we cancel his movement
                c.CurrentUnit.HasMoved = true;
                c.CurrentUnit.TemporaryMovementRange = 0;
            }
        }
    }
}