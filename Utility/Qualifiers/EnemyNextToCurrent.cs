using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace JRPG
{
    public class EnemyNextToCurrent : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = context as AIContext;
            if (c.CurrentUnit.IsDead) return -1;
            c.AllAdjacentEnemies = AIManager.Instance.GetAdjacentEnemies();
            var result = (c.AllAdjacentEnemies.Count > 0 && !c.CurrentUnit.HasAttacked) ? desiredScore : -1;
            // before scoring, we initialize the selected target, just in case the score wins and if we do have enemies around
            if (c.AllAdjacentEnemies.Count > 0)
            {
                // select the lowest hp enemy
                c.SelectedEnemy = c.AllAdjacentEnemies[0];
            }
            Debug.Log("=========> AI: <color=magenta>Checking if we have an adjacent enemy. Score = " + result + "</color>");
            return (c.AllAdjacentEnemies.Count > 0 && !c.CurrentUnit.HasAttacked) ? desiredScore : -1;
        }
    }
}
