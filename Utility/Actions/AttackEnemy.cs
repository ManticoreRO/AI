using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class AttackEnemy : ActionBase
    {
        [ApexSerialization]
        public bool RandomEnemy = false;
        [ApexSerialization]
        bool isArcher;

        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            // we don't attack more than once
            //if (c.IsAttacking) return;
            Debug.Log("=======> AI: Trying to attack enemy!");

            if (RandomEnemy)
            {
                if (!isArcher)
                {
                    c.AllAdjacentEnemies = AIManager.Instance.GetAdjacentEnemies(c.CurrentUnit);
                    if (c.AllAdjacentEnemies.Count > 0)
                    {
                        c.SelectedEnemy = c.AllAdjacentEnemies[Random.Range(0, c.AllAdjacentEnemies.Count)];
                    }
                }
                else if (c.SelectedEnemy == null)
                {
                    var tilesInAttackRange = EncounterManager.Instance.GetTilesInRange((int)c.CurrentUnit.OnTile.TileCenter.x, (int)c.CurrentUnit.OnTile.TileCenter.z, c.CurrentUnit.TroopStats.AttackRange.StatValue);
                    var targetsInRangedRange = EncounterManager.Instance.GetEnemiesInRange(tilesInAttackRange, c.CurrentUnit.OwnerActor);
                    if (targetsInRangedRange.Count > 0)
                    {
                        c.SelectedEnemy = targetsInRangedRange[Random.Range(0, targetsInRangedRange.Count)];
                    }
                }
            }
            
            if (c.SelectedEnemy != null)
            {
                string troopType = (c.CurrentUnit.TroopStats.AttackRange.StatValue > 1) ? "RANGED" : "MELEE";
                Debug.Log("=======> AI: " + troopType + " Attacking enemy " + c.SelectedEnemy.TroopStats.UnitName);
                BattleManager.Instance.Attack(c.CurrentUnit, c.SelectedEnemy);
                c.CurrentUnit.HasMoved = true;
                c.CurrentUnit.TemporaryMovementRange = 0;
                c.IsAttacking = true;
            }    
            else
            {
                c.CurrentUnit.HasAttacked = true;
            }
        }
    }
}