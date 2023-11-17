using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using System;

namespace JRPG
{
    public class MapAIContext : IAIContext
    {
        // The maximum range the AI searches for mapobjects or enemies
        public float AI_Range;

        public SphereCollider RangeCollider;
        public bool IsFollowingEnemy;
        public bool IsGoingForMapObject;
        public bool IsCamping;
        public bool IsDoneAction;
        public MapAIController aiController;
        public Vector3 LastKnownPlayerPosition;
        /// <summary>
        /// this is always 1
        /// </summary>
        public readonly float FullyRested = 1f;
        /// <summary>
        /// rested value between 0.1f and 1f
        /// </summary>
        public float Rested
        {
            get => _rested;
            set
            {
                if (value <= 0)
                {
                    _rested = 0.1f;
                }
                else if(value > 1)
                {
                    _rested = 1;
                }
                else
                {
                    _rested = value;
                }
            }}
        private float _rested = 1;
        
        public bool IsPaused;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hero">The hero the AI will control</param>
        public MapAIContext(RTSPlayerController hero)
        {
            this.AIHero = hero;            
            this.EnemiesInRange = new List<RTSPlayerController>();
            this.FoundObjects = new HashSet<Transform>();
            this.SelectedTarget = null;
            this.SelectedEnemy = null;
            this.IsFollowingEnemy = false;
            this.IsGoingForMapObject = false;
            this.IsCamping = false;
            this.IsDoneAction = true;
            this.aiController = AIHero.gameObject.GetComponent<MapAIController>();
            this.EnemyHeroes = new List<RTSPlayerController>();
            this.IsPaused = false;
            this.LostEncounters = new List<MapObjectHandle>();

        }
       
        /// <summary>
        /// Reset the bools that decide the actions
        /// </summary>
        public void ResetActions()
        {
            IsFollowingEnemy = false;
            //aiController.AiFollowEnemyPlayer(cancelFollow: true);
            IsCamping = false;
            //aiController.AiCamping(cancelRest: true);
            IsGoingForMapObject = false;
        }

        /// <summary>
        /// Selected target to move at
        /// </summary>
        public Transform SelectedTarget
        {
            get;
            set;
        }

        /// <summary>
        /// If we have an enemy selected
        /// This will enable the AI to follow or wait for the player to leave the battle
        /// </summary>
        public Transform SelectedEnemy
        {
            get;
            set;
        }
        /// <summary>
        /// All map objects in range
        /// </summary>
        public HashSet<Transform> FoundObjects
        {
            get;
            set;
        }

        /// <summary>
        /// A list of lost encounters so we can avoid them until buffed
        /// </summary>
        public List<MapObjectHandle> LostEncounters
        {
            get;
            set;
        }

        /// <summary>
        /// The hero the AI is using
        /// </summary>
        public RTSPlayerController AIHero
        {
            get;
            private set;
        }

        /// <summary>
        /// The enemy/player heroes
        /// </summary>
        public List<RTSPlayerController> EnemyHeroes
        {
            get;
            set;
        }

        /// <summary>
        /// If any enemy hero in range, it is stored here
        /// </summary>
        public List<RTSPlayerController> EnemiesInRange
        {
            get;
            set;
        }
    }
}