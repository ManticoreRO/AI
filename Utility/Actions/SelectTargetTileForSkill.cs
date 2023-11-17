using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SelectTargetTileForSkill : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;
            var range = c.CurrentActiveSkill.ActionBaseValue;
            var tilesInRange = EncounterManager.Instance.GetTilesInRange((int)c.CurrentUnit.OnTile.TileCoordinates.x, (int)c.CurrentUnit.OnTile.TileCoordinates.y, range);

            BattleController targetPosition;

            Debug.Log("============> AI: checking for a tile to use the skill on!");
            // set up the enemy target. If no troops on the battlefield left, we go for the hero
            if (c.AllEnemies.Count > 0)
            {
                //todo:Daniel Check all enemies when they die so u do not blink near them as well
                targetPosition = c.AllEnemies[Random.Range(0, c.AllEnemies.Count)];
            }
            else
            {
                targetPosition = c.EnemyHero;
            }
            // we need an empty walkable tile to be able to move
            var targetTilePosition = EncounterManager.Instance.GetFreeAdjacentTile(targetPosition.OnTile);

            List<BattleTile> listOfTilesInRange = new List<BattleTile>();

            // transform our matrix to a list
            for (int i = 0; i < tilesInRange.GetLength(0); i++)
            {
                for (int j = 0; j < tilesInRange.GetLength(1); j++)
                {
                    listOfTilesInRange.Add(tilesInRange[i, j]);
                }
            }

            if (targetTilePosition != null)
            {
                EncounterManager.Instance.SetupBoardForExtendedMovement();
                BattleManager.Instance.RecalculatePath(c.CurrentUnit.OnTile, targetTilePosition);


                int index = BattleManager.Instance.Path.Count;
                // check the path and close it when it goes out of the range
                for (int i = 0; i < BattleManager.Instance.Path.Count; i++)
                {
                    if (!listOfTilesInRange.Contains(BattleManager.Instance.Path[i]))
                    {
                        index = i;
                        break;
                    }
                }

                targetTilePosition = BattleManager.Instance.Path[index-1];
            }
            else // if we didn't find any free tile, then we just teleport somewhere in the range randomly
            {
                Debug.LogError("=========> AI: the target tile for targetable skill is null! We shouldn't be here");
                targetTilePosition = listOfTilesInRange[Random.Range(0, listOfTilesInRange.Count)];               
            }

            BattleManager.TargetTiles.Add(targetTilePosition);
            Debug.Log("============> AI: found tile: " + targetTilePosition);
            // we reset the Astar path algorhytm and show my movement range
            EncounterManager.Instance.ShowMovementRange(c.CurrentUnit.OnTile, c.CurrentUnit.TemporaryMovementRange, c.CurrentUnit);
        }
    }
}