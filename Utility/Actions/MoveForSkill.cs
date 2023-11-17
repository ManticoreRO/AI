using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class MoveForSkill : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = context as AIContext;


            // we do not execute if the troop is already moving or moved already
            if (c.CurrentUnit.IsMoving || c.CurrentUnit.TemporaryMovementRange <= 0 || c.CurrentUnit.HasMoved || c.CurrentUnit.IsDead) return;
            
            c.SelectedEnemy = null;

            c.PositionToMove = AIManager.Instance.GetPossibleMovePositionForRanged(true);
            // now we test if any enemy is in the attack range
            Debug.Log("============> AI: skill range is " + c.CurrentActiveSkill.ActionBaseRange);

            if (c.PositionToMove != null && !c.CurrentUnit.IsMoving)
            {
                var actor = EncounterManager.Instance.GetTroopIdByController(c.CurrentUnit);
                if (actor == -1)
                {
                    Debug.LogError("ID -1!!!");
                }
                Debug.Log("<color=magenta>AI =======> Moving toward position for skill: " + c.PositionToMove.transform.position + "</color>");
                BattleManager.Instance.MoveUnitTowardsPosition(c.PositionToMove, actor);
                // we have to make some checks and invalidate some actions if they need not be done after this step
                
                if (c.CurrentUnit.HasMoved && c.CurrentUnit.TemporaryMovementRange > 0) c.CurrentUnit.TemporaryMovementRange = 0;

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