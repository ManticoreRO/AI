using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SelectMapObjectAsTarget : ActionBase
    {
        [ApexSerialization(defaultValue = true)]
        bool SelectClosest;
        [ApexSerialization(defaultValue = false)]
        bool SelectClosestSpecific;
        [ApexSerialization(defaultValue = MapObjectTypes.AI_MARKERS)]
        MapObjectTypes SpecificType;
        
        public override void Execute(IAIContext context)
        {
            var c = (MapAIContext)context;
            
            if (SelectClosest)
            {
                var ai = c.aiController;
                var closest = MapAIManager.Instance.GetClosestObject(ai);
                if (closest == null)
                {
                    c.SelectedTarget = c.SelectedEnemy;
                    Debug.Log("MapAI: NO TARGET found close to us! Going for the enemy instead!");
                    c.IsFollowingEnemy = true;
                    ai.AiFollowEnemyPlayer();
                    return;
                }
                c.SelectedTarget = closest.transform;
                c.ResetActions();
                c.IsGoingForMapObject = true;
                c.IsDoneAction = false;
                ai.AiGoToMapObject();
                return;
            }

            if (SelectClosestSpecific)
            {
                var ai = c.aiController;
               
                var closest = MapAIManager.Instance.GetClosestObject(ai, SpecificType);
                if (closest == null)
                {
                    Debug.Log("MapAI: closest is null = " + c.SelectedTarget.name);
                    c.IsGoingForMapObject = false;
                    c.IsFollowingEnemy = true;
                    c.IsDoneAction = true;
                    c.IsPaused = false;
                    c.SelectedTarget = c.SelectedEnemy;
                    ai.AiFollowEnemyPlayer();
                    return;                    
                }

                c.SelectedTarget = closest.transform;

                Debug.Log("MapAI: Selecting closest specific object! name = " + c.SelectedTarget.name);
                c.ResetActions();
                c.IsGoingForMapObject = true;
                c.IsDoneAction = false;
                ai.AiGoToMapObject();
                return;
            }

            Debug.LogError("MapAI: I haven't selected any map object to move at!");
            return;
        }
    }
}