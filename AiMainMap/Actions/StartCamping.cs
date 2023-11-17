using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class StartCamping : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            MapAIManager.Instance.Camp((MapAIContext)context);
        }
    }
}