using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class CanAttackEnemyHero : QualifierBase
    {
        [ApexSerialization]
        bool onlyAdjacent;
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var enemyHero = (!onlyAdjacent) ? AIManager.Instance.GetEnemyHeroInRange(c.CurrentUnit) : AIManager.Instance.GetEnemyHeroInRange(c.CurrentUnit,1);
            float retVal = -1;

            // if we have enemy in range
            if (enemyHero != null)
            {
                var tilesAroundTarget = EncounterManager.Instance.GetFreeAdjacentTile(enemyHero.OnTile);
                // and if we have free tiles around it, score
                if (tilesAroundTarget != null && !onlyAdjacent)
                {
                    c.PositionToMove = tilesAroundTarget;
                    c.SelectedEnemy = enemyHero;
                    retVal = desiredScore;
                }
                else
                if (onlyAdjacent)
                {
                    c.SelectedEnemy = enemyHero;
                    retVal = desiredScore;
                }
            }

            Debug.Log("===========> AI: testing if we can attack enemt hero! Score = " + retVal);
            return retVal;
        }
    }
}