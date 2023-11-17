using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI.Components;
using Apex.AI;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.AI;

namespace JRPG
{
    public class MapAIController : MonoBehaviour, IContextProvider
    {
        public MapAIContext Context;
        public HashSet<Transform> AllObjectsInRange = new HashSet<Transform>();
        public List<RTSPlayerController> AllEnemiesInRange = new List<RTSPlayerController>();
        private Animator _heroAnimator;
        public IAIContext GetContext(Guid aiId)
        {
            return Context;
        }

        private NavMeshAgent _agent;

        public NavMeshAgent Agent => _agent;

        private void Awake()
        {
            _agent = GetComponent<NavMeshAgent>();
        }
        private void Start()
        {
            
        }

        private void OnEnable()
        {
            _heroAnimator = gameObject.GetComponent<Animator>();
        }
        #region [Collider]
    
        public void AddVisibleMapObject(Transform mapObject)
        {            
            if (!AllObjectsInRange.Contains(mapObject))
            {
                AllObjectsInRange.Add(mapObject);
            }
        }

        public void RemoveVisibleMapObject(Transform mapObject)
        {
            if (AllObjectsInRange.Contains(mapObject))
            {
                AllObjectsInRange.Remove(mapObject);
            }
        }

        public void AddVisibleEnemy(RTSPlayerController enemy)
        {
            if (!AllEnemiesInRange.Contains(enemy))
            {
                AllEnemiesInRange.Add(enemy);
            }
        }

        public void RemoveVisibleEnemy(RTSPlayerController enemy)
        {
            if (AllEnemiesInRange.Contains(enemy))
            {
                AllEnemiesInRange.Remove(enemy);
            }
        }

        public void ExecuteAI()
        {
            StopAllCoroutines();
            GetComponent<UtilityAIComponent>().clients[0].Execute();
        }

        #endregion

        #region [On Object Collisions]
        //private bool _checkedCollision;
        //private void OnCollisionStay(Collision collision)
        //{
        //    if (!_checkedCollision)
        //    {
                
        //        Debug.Log("MapAI: Collided with " + collision.collider.gameObject.name);
        //        // this will be called only for the enemy hero/s
        //        if (collision.collider.GetComponent<RTSPlayerController>() != null)
        //        {
        //            // initiate fight
        //            _checkedCollision = true;
        //            PlayerUIManager.Instance.Button_OnSimulateBattle();
        //            return;
        //        }
        //    }
        //}
        public void StopCoroutines()
        {
            StopAllCoroutines();
        }
        private void ExecuteAiActionOnTarget(Transform selectedtarget)
        {            
            var mapObjectHandle = selectedtarget.GetComponent<MapObjectHandle>();

            if (!selectedtarget.gameObject.activeSelf) return;
            if (mapObjectHandle == null) return;
            if (this.Context == null) return;
            if (Context.SelectedTarget == null) return;
            if (Context.SelectedTarget != selectedtarget.gameObject.transform) return;

            Debug.Log("MapAI: Collided with " + selectedtarget.gameObject.name + " with target being " + Context.SelectedTarget.name);
            
            switch (mapObjectHandle.MapObjectType)
            {
                case MapObjectTypes.Encounter:
                    {
                        MapAIManager.Instance.SimulateBattle(this.Context, mapObjectHandle);
                        break;
                    }
                case MapObjectTypes.Buff:
                    {
                        MapAIManager.Instance.GetBuff(this.Context, selectedtarget.GetComponent<MapObjectBuff>());
                        break;
                    }
                case MapObjectTypes.Chest:
                    {
                        MapAIManager.Instance.GetChest(this.Context, selectedtarget.GetComponent<MapObjectChest>());
                        break;
                    }
                case MapObjectTypes.Grave:
                    {
                        MapAIManager.Instance.GetGrave(this.Context, selectedtarget.GetComponent<MapObjectGrave>());
                        break;
                    }
                case MapObjectTypes.PlayerCastle:
                    {
                        //todo for enemy camp
                        break;
                    }
                case MapObjectTypes.Player:
                    {
                        //todo for enemy final battle
                        //stop ai
                        PlayerUIManager.Instance.Button_OnSimulateBattle();
                        //MapAIManager.Instance.StopAIs();
                        break;
                    }
            }
            // now execute the ai
            Context.IsDoneAction = true;
            this.ExecuteAI();
        }
        #endregion

        #region [Conditions]
        /// <summary>
        /// Returns if the ai needs rest. For now, it checks if the fatigue is lower than the max fatigue
        /// </summary>
        /// <returns></returns>
        public bool IsRestNeeded()
        {            
            return (Context.Rested < Context.FullyRested);
        }

        #endregion

        #region AiContext

        public void AiCamping(bool cancel = false)
        {
            if (cancel && _aiCampingIe != null)
            {
                StopCoroutine(_aiCampingIe);
            }

            _aiCampingIe = StartCoroutine(AiCampingIe());
        }

        private Coroutine _aiCampingIe = null;
        public IEnumerator AiCampingIe()
        {
            if (Context.IsPaused && !Context.IsCamping)
            {
                yield break;
            }

            _agent.isStopped = true;
            Context.IsPaused = true;
            Debug.Log("MapAI: Camping! current rest/HP level at " + Context.Rested.ToString("P2"));
            //do the task while this is not full
            while (Context.Rested < Context.FullyRested)
            {
                var timeToRefresh = Mathf.FloorToInt(Time.time) + MapAIManager.Instance.CampFatigueRecoverTime;
                while (Time.time < timeToRefresh)
                {
                    yield return null;
                }
                Context.Rested = Mathf.Clamp(Context.Rested + 0.10f, 0.1f, 1);
            }

            Debug.Log("MapAI: Finished camping!");
            Context.Rested = Context.FullyRested;
            Context.IsPaused = false;
            Context.ResetActions();
            ExecuteAI();

        }

        public void AiFollowEnemyPlayer(bool cancel = false)
        {
            if (cancel && _aiFollowEnemyPlayerIe != null)
            {
                StopCoroutine(_aiFollowEnemyPlayerIe);
            }

            _aiFollowEnemyPlayerIe = StartCoroutine(AiFollowEnemyPlayerIe());
        }
        private Coroutine _aiFollowEnemyPlayerIe = null;
        public IEnumerator AiFollowEnemyPlayerIe()
        {
            if (Context.IsPaused && !Context.IsFollowingEnemy)
            {
                yield break;
            }

            Debug.Log("Trying to follow Player");
            if(Context.SelectedEnemy != null/* && Context.LastKnownPlayerPosition != Context.SelectedEnemy.position*/)
            {
                _agent.isStopped = true;
                NavMeshPath newPath = new NavMeshPath();
                var result = _agent.CalculatePath(Context.SelectedEnemy.position, newPath);
                if (result)
                {
                            
                    Context.LastKnownPlayerPosition = Context.SelectedEnemy.position;
                    _agent.SetPath(newPath);  
                    _agent.isStopped = false;
                    UnitAnims.SetAnim(_heroAnimator, UnitAnims.AnimParams.RunForwardBool, true);
                    Debug.Log("<color=yellow>Execute Following Player</color>");
                }
                else
                {
                    yield return null;
                    Debug.LogError("No Path found it should go for last known position");
                    ExecuteAI();
                }
            }
            //else
            //{
            //    Debug.LogError("No Path found it should go for last known position");
            //    ExecuteAI();
            //    yield break;
            //}
            var timeToRefresh = Mathf.FloorToInt(Time.time) + MapAIManager.Instance.FollowEnemyRefreshTime;
            while (Time.time < timeToRefresh)
            {
                yield return null;
            }
            Debug.Log("<color=magenta>Following player execute</color>");
            ExecuteAI();
            
        }

        
        public void AiGoToMapObject(bool cancel = false)
        {
            if (cancel && _aiGoToMapObjectIe != null)
            {
                StopCoroutine(_aiGoToMapObjectIe);
            }

            _aiGoToMapObjectIe = StartCoroutine(AiGoToMapObjectIe());
        }
        private Coroutine _aiGoToMapObjectIe = null;
        public IEnumerator AiGoToMapObjectIe()
        {
            //Debug.LogFormat("Elapsed milliseconds: {0}", Context.IsDoneAction);
            Debug.LogFormat("GoingForObj IsCamping: {0} IsGoingForMapObject: {1} IsDoneAction: {2}", Context.IsCamping, Context.IsGoingForMapObject, Context.IsDoneAction);
            if (!Context.IsCamping && Context.IsGoingForMapObject && !Context.IsDoneAction)
            {

                Context.IsDoneAction = true;
                // stop the agent for a bit then calculate the path
                _agent.isStopped = true;
                NavMeshPath newPath = new NavMeshPath();
                if (Context.SelectedTarget == null)
                {
                    Debug.LogError(
                        "MapAI: For some reason the selected target is null so we restart the ai and try and find another one!");
                    Context.ResetActions();
                    ExecuteAI();
                    yield break;
                }

                var result = _agent.CalculatePath(Context.SelectedTarget.position, newPath);
                if (result)
                {
                    _agent.SetPath(newPath);
                    _agent.isStopped = false;
                    UnitAnims.SetAnim(_heroAnimator, UnitAnims.AnimParams.RunForwardBool, true);
                    Debug.LogFormat("<color=yellow>MapAI: executing path</color>");
                }
                else
                {
                    Debug.LogError("MapAI: Path not found to the enemy! Forcing ai to move to the enemy player!");
                    Context.IsGoingForMapObject = false;
                    Context.IsFollowingEnemy = true;
                    AiFollowEnemyPlayer();
                }
            }

        }

        #endregion

        private float _timeToRefresh = 1f;
        private bool _cameraSwitched = false;
        private void Update()
        {
            if (_agent == null)
            {
                return;
            }
            // for debug pourposes, we can set the camera to follow the AI hero
            // TODO: remove at release
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!_cameraSwitched)
                {
                    HeroManager.Instance.SwitchCameraToHero(Context.AIHero);
                }
                else
                {
                    HeroManager.Instance.SwitchCameraToHero(Context.EnemyHeroes[0]);
                }
                _cameraSwitched = !_cameraSwitched;
            }

            if (Context == null) return;

            //// if we are camping
            //if (Context.IsCamping)
            //{                
            //    // stop the agent and keep it stopped
            //    _agent.isStopped = true;
            //    Context.IsPaused = true;
            //    if (Time.time >= _timeToRefresh)
            //    {
            //        _timeToRefresh = Mathf.FloorToInt(Time.time) + MapAIManager.Instance.CampFatigueRecoverTime;
            //        // increase the fatigue with a quarter of the max fatigue every CampFatigueRecoverTime seconds
            //        Context.Fatigue += Context.MaxFatigue / 4;
            //        if (Context.Fatigue >= Context.MaxFatigue)
            //        {
            //            Debug.Log("MapAI: Finished camping!");
            //            Context.Fatigue = Context.MaxFatigue;
            //            Context.ResetActions();
            //            Context.IsPaused = false;
            //            ExecuteAI();
            //        }
            //    }
            //}

            if (Context.IsPaused) return;
            
            // animations
            if ((!_agent.isStopped && _agent.remainingDistance <= _agent.stoppingDistance) ||  // if is normal target should stop at 0.1f (account for special situations like camp
                (Context.SelectedEnemy == Context.SelectedTarget && _agent.remainingDistance <= 1f)) //is it's the player hero
            {
                UnitAnims.SetAnim(_heroAnimator, UnitAnims.AnimParams.RunForwardBool, false);
                _agent.isStopped = true;
                Debug.Log("AI arrived");
                if (Context.SelectedTarget == null)
                {
                    ExecuteAI();
                    return;
                }
                ExecuteAiActionOnTarget(Context.SelectedTarget);
                //ExecuteAI();
            }            
            
           
            //if (Context.IsFollowingEnemy)
            //{
            //    if (Time.time >= _timeToRefresh)                
            //    {                    
            //        _timeToRefresh = Mathf.FloorToInt(Time.time) + MapAIManager.Instance.FollowEnemyRefreshTime;
            //        // stop the agent for a bit then calculate the path
            //        _agent.isStopped = true;
            //        NavMeshPath newPath = new NavMeshPath();
            //        var result = _agent.CalculatePath(Context.SelectedEnemy.position, newPath);
            //        if (result)
            //        {
            //            _agent.SetPath(newPath);
            //            _agent.isStopped = false;
            //            UnitAnims.SetAnim(_heroAnimator, UnitAnims.AnimParams.RunForwardBool, true);
            //            Debug.LogFormat("<color=magenta>MapAI: executing path following enemy</color>");
            //        }
            //        else
            //        {
            //            Debug.LogError("MapAI: Path not found to the enemy!");
            //        }                   
            //        ExecuteAI();
            //    }
            //}

            // if we are going for another map object
/*            if (!Context.IsCamping && Context.IsGoingForMapObject && !Context.IsDoneAction)
            {
                Context.IsDoneAction = true;
                // stop the agent for a bit then calculate the path
                _agent.isStopped = true;
                NavMeshPath newPath = new NavMeshPath();
                if (Context.SelectedTarget == null)
                {
                    Debug.LogError("MapAI: For some reason the selected target is null so we restart the ai and try and find another one!");
                    Context.ResetActions();
                    ExecuteAI();
                    return;
                }
                var pos = Context.SelectedTarget.position;
                if (pos != null)
                {
                    var result = _agent.CalculatePath(Context.SelectedTarget.position, newPath);
                    if (result)
                    {
                        _agent.SetPath(newPath);
                        _agent.isStopped = false;
                        UnitAnims.SetAnim(_heroAnimator, UnitAnims.AnimParams.RunForwardBool, true);
                        Debug.LogFormat("<color=yellow>MapAI: executing path</color>");
                    }
                    else
                    {
                        Debug.LogError("MapAI: Path not found to the enemy! Forcing ai to move to the enemy player!");
                        Context.IsGoingForMapObject = false;
                        Context.IsFollowingEnemy = true;
                        AiFollowEnemyPlayer();
                    }
                }
                else
                {
                    Debug.LogError("MapAI: Invalid target");
                }
            }*/
        }
    }
}