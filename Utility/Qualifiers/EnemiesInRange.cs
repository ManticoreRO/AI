using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace JRPG
{
    public class EnemiesInRange : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        [ApexSerialization(defaultValue = false)]
        bool isForSkill;

        public override float Score(IAIContext context)
        {
            var c = context as AIContext;
            float retVal = -1;

            // we need to guard for movement already made so we don't enter into an infinite loop
            if (c.CurrentUnit.HasMoved || c.CurrentUnit.TemporaryMovementRange <= 0)
            {
                Debug.Log("BattleAI: AI has already moved! Fail score set.");
                return -1;
            }

            if (!isForSkill)
            {               
                // we check if the enemy has troops, if not, check the enemy hero
                if (c.AllEnemies.Count != 0)
                {
                    if (c.AllEnemiesInRange.Count > 0)
                    {
                        retVal = desiredScore;
                    }
                }
                else
                {
                    var enemyHero = AIManager.Instance.GetEnemyHeroInRange(c.CurrentUnit);
                    if (enemyHero != null)
                    {
                        retVal = desiredScore;
                    }
                }               
            }
            else 
            {
                var tilesInSkillRange = EncounterManager.Instance.GetTilesInRange((int)c.CurrentUnit.OnTile.TileCoordinates.x, (int)c.CurrentUnit.OnTile.TileCoordinates.y, c.CurrentActiveSkill.ActionBaseRange);
                var allEnemiesInSkillRange = EncounterManager.Instance.GetEnemiesInRange(tilesInSkillRange, c.CurrentUnit.OwnerActor);

                if (allEnemiesInSkillRange.Count > 0)
                {
                    retVal = desiredScore;
                }
            }
            Debug.Log("=========> AI: <color=blue>Checking if we have an enemy in range. Score = " + retVal + "</color>");

            return retVal;
        }
    }
}