using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class NeedCamping : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (MapAIContext)context;
            var result = c.aiController.IsRestNeeded();

            Debug.Log("MapAI: need camping " + result);
            return (result) ? 40 : -10;
        }
    }
}