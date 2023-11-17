using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using System.Linq;
using Apex.Serialization;

namespace JRPG
{
    public class NoObjectInRange : QualifierBase
    {
        [ApexSerialization(defaultValue = 20)] 
        private float _decidedScore;
        public override float Score(IAIContext context)
        {
            var c = (MapAIContext)context;
            HashSet<Transform> handle = new HashSet<Transform>();

            for (int i = 0; i < c.FoundObjects.Count; i++)
            {
                if (!c.FoundObjects.ElementAt(i).GetComponent<MapObjectHandle>().ClickInteractable)
                    handle.Add(c.FoundObjects.ElementAt(i));
            }

            foreach (var elem in handle)
            {
                c.FoundObjects.Remove(elem);
            }

            Debug.Log("MapAI: Testing if objects in sight! Objects = " + c.FoundObjects.Count);
            return (c.FoundObjects.Count <= 0 && c.EnemiesInRange.Count <= 0 && (!c.IsCamping)) ? _decidedScore : -10;
        }
    }
}