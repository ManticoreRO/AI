using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class NoEnemyInRange : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            float retVal = desiredScore;
            var c = (AIContext)context;

            // we need to guard for movement already made so we don't enter into an infinite loop
            if (c.CurrentUnit.HasMoved || c.CurrentUnit.TemporaryMovementRange <= 0)
            {
                Debug.Log("BattleAI: AI has already moved! Fail score set.");
                return -1;
            }

            if (c.AllEnemies.Count != 0)
            {
                if (c.AllEnemiesInRange.Count > 0)
                {
                    retVal = -1;
                }
            }
            else
            {
                var enemyHero = AIManager.Instance.GetEnemyHeroInRange(c.CurrentUnit);
                if (enemyHero != null)
                {
                    retVal = -1;
                }
            }

            Debug.Log("=========> AI: <color=blue>Checking if we do not have an enemy in range. Score = " + retVal + "</color>");
            return retVal;
        }
    }
}