using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class TargetAndFollowEnemy : ActionBase
    {
        [ApexSerialization(defaultValue = false)]
        bool RandomHero;
        [ApexSerialization(defaultValue = false)]
        bool ForceSelect;
        public override void Execute(IAIContext context)
        {
            var c = (MapAIContext)context;

            // if camping, we bail
            if (c.IsCamping)
            {
                return;
            }
            // since this is real time, we don't want to select different enemies every time
            // unless we force it
            if (c.SelectedEnemy != null && !ForceSelect)
            {
                return;
            }

            ////select enemy
            //if (RandomHero)
            //{
                c.SelectedEnemy = c.EnemyHeroes[Random.Range(0, c.EnemyHeroes.Count)].transform;
            //}
            //else
            //{
            //    c.SelectedEnemy = c.EnemiesInRange[Random.Range(0, c.EnemiesInRange.Count)].transform;
            //}
            // early out if no enemies, though it should't get here
            if (c.SelectedEnemy == null)
            {
                Debug.LogError("MapAI: there is no enemy to follow!");
                return;
            }
            c.ResetActions();
            c.IsFollowingEnemy = true;
            c.SelectedTarget = c.SelectedEnemy;
            c.aiController.AiFollowEnemyPlayer();

        }
    }
}