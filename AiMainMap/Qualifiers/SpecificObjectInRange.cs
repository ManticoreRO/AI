using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SpecificObjectInRange : QualifierBase
    {
        [ApexSerialization]
        float SuccessScore;
        [ApexSerialization(defaultValue = MapObjectTypes.AI_MARKERS)]
        MapObjectTypes TypeOfObject;

        public override float Score(IAIContext context)
        {
            var c = (MapAIContext)context;
            var result = MapAIManager.Instance.GetObjectsOfType(c.aiController, TypeOfObject);
            float score = ((result.Count > 0 && !c.IsCamping) ? SuccessScore : -10);
            // randomize it a bit

            score += Random.Range(0, 10);

            Debug.Log("MapAI: Testing if SPECIFIC object in sight! -" + TypeOfObject + " score given = " + score);
            return score;
        }
    }
}