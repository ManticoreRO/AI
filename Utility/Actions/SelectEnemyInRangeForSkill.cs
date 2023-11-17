using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SelectEnemyInRangeForSkill : ActionBase
    {
        [ApexSerialization(defaultValue = false)]
        bool RandomEnemy;
        [ApexSerialization(defaultValue = false)]
        bool IsSkillTargettingEnemy;

        public override void Execute(IAIContext context)
        {
            var c = context as AIContext;
            BattleController selectedEnemy;

            Debug.Log("=======> AI: Selecting an enemy in range for targeted skill!");
            var tilesInSkillRange = EncounterManager.Instance.GetTilesInRange((int)c.CurrentUnit.OnTile.TileCoordinates.x, (int)c.CurrentUnit.OnTile.TileCoordinates.y, c.CurrentActiveSkill.ActionBaseRange);
            var allEnemiesInSkillRange = EncounterManager.Instance.GetEnemiesInRange(tilesInSkillRange, c.CurrentUnit.OwnerActor);
            if (allEnemiesInSkillRange.Count > 0)
            {
                if (RandomEnemy)
                {
                    selectedEnemy = allEnemiesInSkillRange[Random.Range(0, allEnemiesInSkillRange.Count)];
                }
                else
                {
                    selectedEnemy = allEnemiesInSkillRange[0];
                }


                // we check if the skill needs a target tile or a target enemy and select accordingly
                if (!IsSkillTargettingEnemy)
                {
                    var targetTile = EncounterManager.Instance.GetFreeAdjacentTile(selectedEnemy.OnTile);
                    c.CurrentUnit.TargetTile = targetTile;
                    BattleManager.TargetTiles.Add(targetTile);
                }
                else
                {
                    c.CurrentUnit.TargetUnit = selectedEnemy;
                    BattleManager.TargetUnits.Add(selectedEnemy);
                }
            }
        }
    }
}