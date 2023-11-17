using UnityEngine;
using _DPS;

namespace JRPG
{
    public class AICollider : MonoBehaviour
    {
        MapAIController _aiController;
        public MapObjectHandle HandleRef;
        private void OnEnable()
        {
            _aiController = GetComponentInParent<MapAIController>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_aiController == null) return;
            if (!_aiController.enabled) return;
           var otherHandle = other.GetComponent<MapObjectHandle>();
            // check to see if it's a mapobject or a hero
            if (otherHandle != null)
            {
                //Debug.Log("MapAI: object in range " + other.name);
                if (MapObjectTypes.AI_MARKERS.Contains(otherHandle.MapObjectType))
                {
                    _aiController.AddVisibleMapObject(otherHandle.transform);
                }

                else if (otherHandle.MapObjectType == MapObjectTypes.Player)
                {
                    _aiController.AddVisibleEnemy(otherHandle.gameObject.GetComponent<RTSPlayerController>());
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_aiController == null) return;
            if (!_aiController.enabled) return;

            var otherHandle = other.GetComponent<MapObjectHandle>();
            // check to see if it's a mapobject or a hero
            if (otherHandle != null)
            {
                //Debug.Log("MapAI: object in range " + other.name);
                if (MapObjectTypes.AI_MARKERS.Contains(otherHandle.MapObjectType))
                {
                    _aiController.RemoveVisibleMapObject(otherHandle.transform);
                }

                else if (otherHandle.MapObjectType == MapObjectTypes.Player)
                {
                    _aiController.RemoveVisibleEnemy(otherHandle.gameObject.GetComponent<RTSPlayerController>());
                }
            }
        }
    }
}