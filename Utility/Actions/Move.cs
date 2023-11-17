using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    [FriendlyName("Move To Position","Move the unit to a position on the grid without attacking.")]
    public class Move : ActionBase
    {
        [ApexSerialization]
        bool IsRanged;
        [ApexSerialization]
        bool isForSkill;
        public override void Execute(IAIContext context)
        {
            var c = context as AIContext;

            
            // we do not execute if the troop is already moving or moved already
            if (c.CurrentUnit.IsMoving || c.CurrentUnit.TemporaryMovementRange <= 0 || c.CurrentUnit.HasMoved || c.CurrentUnit.IsDead) return;

            //Debug.Log("=========> AI: Moving unit on the board towards player troops or hero!");
            c.SelectedEnemy = null;
          
            if (c.ProtectHero && c.EnemiesAttackingHero.Count > 0)
            {
                // if here then we have enemies attacking our own hero
                //Debug.Log("========>AI: Our hero under attack, moving to help");
                c.EnemiesAttackingHero.Sort((o1, o2) => o1.TroopStats.HitPoints.StatValue.CompareTo(o2.TroopStats.HitPoints.StatValue));
                if (!IsRanged)
                {
                    c.PositionToMove = EncounterManager.Instance.GetFreeAdjacentTile(c.EnemiesAttackingHero[0].OnTile);
                }
                else // if ranged we move at a safe distance
                {
                    c.PositionToMove = EncounterManager.Instance.GetFreeTileInRange(c.EnemiesAttackingHero[0], 2, (int)c.CurrentUnit.TroopStats.Movement.StatValue);

                    // if we cannot move like a ranged unit, move as melee
                    if (c.PositionToMove == null)
                    {
                        c.PositionToMove = AIManager.Instance.GetPossibleMovePosition();
                    }

                    c.SelectedEnemy = c.EnemiesAttackingHero[0];
                    c.IsAttacking = true;
                    // now we test if any enemy is in the attack range
                    //var tilesInTroopRange = EncounterManager.Instance.GetTilesInRange(c.CurrentUnit.OnTile.TileCenter, c.CurrentUnit.TroopStats.AttackRange.StatValue);
                    //for (int i = 0; i < c.AllEnemies.Count; i++)
                    //{
                    //    if (tilesInTroopRange.ContainsValue(c.AllEnemies[i].OnTile))
                    //    {
                    //        //if we got something, then we set up the attack
                    //        c.SelectedEnemy = c.AllEnemies[i];
                    //        c.IsAttacking = true;
                    //        break;
                    //    }
                    //}
                    if (!c.IsAttacking || c.SelectedEnemy == null) c.CurrentUnit.HasAttacked = true;

                }
            }
            else
            {
                if (!IsRanged)
                {
                    c.PositionToMove = AIManager.Instance.GetPossibleMovePosition();
                }
                else // if ranged, try to move to a position where it is safe to shoot (AKA not in movement range of the atacking enemy
                {
                    c.SelectedEnemy = null;

                    c.PositionToMove = AIManager.Instance.GetPossibleMovePositionForRanged();
                    // now we test if any enemy is in the attack range
                    Debug.Log("============> AI: troop range is " + c.CurrentUnit.TroopStats.AttackRange.StatValue);
                    // if we cannot move like a ranged unit, move as melee
                    if (c.PositionToMove == null)
                    {
                        c.PositionToMove = AIManager.Instance.GetPossibleMovePosition();
                    }
                    var tilesInTroopRange = EncounterManager.Instance.GetTilesInRange(c.CurrentUnit.OnTile.TileCenter, c.CurrentUnit.TroopStats.AttackRange.StatValue);
                    
                    for (int i = 0; i < c.AllEnemies.Count; i++)
                    {
                        if (tilesInTroopRange.ContainsValue(c.AllEnemies[i].OnTile))
                        {
                            //if we got something, then we set up the attack
                            c.SelectedEnemy = c.AllEnemies[i];
                            c.IsAttacking = true;
                            Debug.Log("============> AI: Got Target " + c.SelectedEnemy.TroopStats.UnitName);
                            break;
                        }
                    }

                    // if last enemy, gang him
                    //if (c.AllEnemies.Count == 1)
                    //{
                    //    c.SelectedEnemy = c.AllEnemies[0];
                    //    c.IsAttacking = true;
                    //}
                    // if not found target, we set it as attacked already
                    if (!c.IsAttacking || c.SelectedEnemy == null) c.CurrentUnit.HasAttacked = true;
                }
            }

            
            if (c.PositionToMove != null && !c.CurrentUnit.IsMoving)
            {                
                var actor = EncounterManager.Instance.GetTroopIdByController(c.CurrentUnit);
              
                Debug.Log("<color=magenta>AI =======> Moving toward position: " + c.PositionToMove.transform.position + "</color>");
                BattleManager.Instance.MoveUnitTowardsPosition(c.PositionToMove, actor);
                // we have to make some checks and invalidate some actions if they need not be done after this step
                if (!IsRanged) c.CurrentUnit.HasAttacked = true;
                if (IsRanged && c.CurrentUnit.HasMoved && c.CurrentUnit.TemporaryMovementRange > 0) c.CurrentUnit.TemporaryMovementRange = 0;
                if (isForSkill && c.CurrentUnit.HasMoved && c.CurrentUnit.TemporaryMovementRange <= 0) 
                {
                    c.CurrentActiveSkill = null;
                }
            }   
            else
            {
                if (c.PositionToMove == null)
                {
                    Debug.LogError("=======> AI: Cannot move, I might be blocked!");
                    
                }
            }
        }        
    }
}