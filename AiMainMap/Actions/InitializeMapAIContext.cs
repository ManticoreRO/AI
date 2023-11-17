using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class InitializeMapAIContext : ActionBase<MapAIContext>
    {
        public override void Execute(MapAIContext context)
        {
            var c = (MapAIContext)context;

            c.EnemiesInRange = c.aiController.AllEnemiesInRange;
            c.FoundObjects = c.aiController.AllObjectsInRange;

            if (c.EnemyHeroes.Count == 0)
            {
                for (int i = 0; i < HeroManager.Instance.DebugHeroes.Count; i++)
                {
                    if (HeroManager.Instance.DebugHeroes[i].GetComponent<RTSPlayerController>() != c.AIHero)
                    {
                        c.EnemyHeroes.Add(HeroManager.Instance.DebugHeroes[i].GetComponent<RTSPlayerController>());
                    }
                }

                Debug.Log("<color=cyan>Enemyheroes count = " + c.EnemyHeroes.Count + "</color>");
            }

            if (c.LastKnownPlayerPosition == Vector3.zero)
            {
                c.LastKnownPlayerPosition = c.EnemyHeroes[UnityEngine.Random.Range(0, c.EnemyHeroes.Count)].transform.position;
                Debug.Log("<color=cyan>lastPlayerPos Updated = " + c.LastKnownPlayerPosition + "</color>");
            }
        }
    }
}