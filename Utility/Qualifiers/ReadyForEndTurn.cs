using Apex.AI;
using Apex.Serialization;
using UnityEngine;

namespace JRPG
{
    public class ReadyForEndTurn : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            Debug.Log("<color=magenta> AI Stats: hasMoved = " + c.CurrentUnit.HasMoved + "    movesRemaining = " + c.CurrentUnit.TemporaryMovementRange + "   hasAttacked = " + c.CurrentUnit.HasAttacked + "   skillAvailable = " + (c.CurrentActiveSkill != null) + "</color>");
            bool allPhasesDone = AIManager.Instance.IsAIDone(c);

            if (allPhasesDone)
            {
                c.IsDone = true;
            }
            var result = allPhasesDone ? desiredScore : -40;

            Debug.Log("=========> AI: <color=blue>Checking if we have to end turn. Score = " + result + "</color>");
            return result;
        }
    }
}