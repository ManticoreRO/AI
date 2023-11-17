using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class EnemyHeroInRange : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (MapAIContext)context;

            for (int i = 0; i < c.EnemyHeroes.Count; i++)
            {
                if (Vector3.Distance(c.aiController.transform.position, c.EnemyHeroes[i].transform.position) <= MapAIManager.Instance.AIRange)
                {
                    return 50;
                }
            }
            return -10;
        }       
    }
}