using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{   
    public class ChangeAIPhase : ActionBase
    {
        [ApexSerialization]
        private BattleAIPhase newAIPhase;
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            //if (!c.CurrentUnit.IsMoving && !c.IsAttacking)
            //{
                AIManager.Instance.SetAIPhase(newAIPhase);
            //}
        }
    }
}