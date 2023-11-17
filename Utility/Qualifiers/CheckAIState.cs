using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class CheckAIState : QualifierBase
    {
        [ApexSerialization]
        BattleAIPhase checkForPhase;
        [ApexSerialization(defaultValue = 0)]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            if (AIManager.Instance.CurrentAIPhase == checkForPhase)
            {
                return desiredScore;
            }
            else
            {
                return -30;
            }
        }
    }
}
