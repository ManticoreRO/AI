using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class CancelSkill : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            // we only cancel the skill if we already moved
            if (c.CurrentUnit.HasMoved || c.CurrentUnit.TemporaryMovementRange <= 0)
            {
                c.CurrentActiveSkill = null;
            }
        }
    }
}