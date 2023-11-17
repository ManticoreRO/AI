using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.AI.Components;
using System;
using UnityEngine.AI;
using System.Linq;
using UnityEngine.UI.Extensions;
using Photon.Pun;

namespace JRPG
{
    public class MapAIManager : MonoBehaviour
    {
        #region [Context and Instance]
        public List<MapAIContext> ListOfAIs = new List<MapAIContext>();
        public static MapAIManager Instance;

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
        }

        
        #endregion


        internal Dictionary<Transform, MapObject> MapObjectsStorage = new Dictionary<Transform, MapObject>();
        internal HashSet<Transform> MapObjectsLookUp = new HashSet<Transform>();

        /// <summary>
        /// Range at which the AI sees things
        /// </summary>
        public float AIRange = 30f;
        /// <summary>
        /// The time between recalculating the path to a target enemy to follow
        /// </summary>
        public float FollowEnemyRefreshTime = 1f;
        public float CampFatigueRecoverTime = 1f;
        [Space(2)]
        [Header("Battle win chances")]
        public float ChanceToWinWhenStronger = 0.75f;
        public float ChanceToWinWhenSamePower = 0.5f;
        public float ChanceToWinWhenWeaker = 0.25f;
        [Space(2)]
        [Header("Fatigue consumption")]
        public float FatigueAfterWinWhenStronger = 0.25f;
        public float FatigueAfterWinWhenSamePower = 0.5f;
        public float FatigueAfterWinWhenWeaker = 0.75f;
        [Space(2)]
        public float RandomChanceForAction = 0.25f;
    

        #region [Start/Pause]
        
        public void StartAiWithDelay()
        {
            if (_startAiDelay != null)
            {
                StopCoroutine(_startAiDelay);
            }

            _startAiDelay = StartCoroutine(StartAiDelay());
        }
        private Coroutine _startAiDelay = null;
        public IEnumerator StartAiDelay()
        {
            yield return new WaitForSeconds(10f);
            StartAIs();
        }
        /// <summary>
        /// Executes all the active AIs
        /// </summary>
        public void StartAIs()
        {
            for (int i = 0; i < ListOfAIs.Count; i++)
            {
                ListOfAIs[i].aiController.gameObject.SetActive(true);
                // enable AI
                ListOfAIs[i].aiController.GetComponent<MapAIController>().enabled = true;
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().enabled = true;
                ListOfAIs[i].aiController.GetComponentInChildren<SphereCollider>().enabled = true;
                ListOfAIs[i].aiController.GetComponentInChildren<AICollider>().enabled = true;
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Start();
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Execute();
            }
        }

        public void PauseAIs()
        {
            for (int i = 0; i < ListOfAIs.Count; i++)
            {
                ListOfAIs[i].aiController.StopCoroutines();
                StopAllCoroutines();
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Pause();
                ListOfAIs[i].aiController.Context.IsPaused = true;
                //ListOfAIs[i].aiController.GetComponent<NavMeshAgent>().isStopped = true;
                ListOfAIs[i].aiController.enabled = false;
            }
        }

        public void StopAIs()
        {
            for (int i = 0; i < ListOfAIs.Count; i++)
            {
                Debug.Log("MapAI: Stipping AI " + ListOfAIs[i].aiController.gameObject.name);
                ListOfAIs[i].aiController.StopCoroutines();                
                ListOfAIs[i].aiController.enabled = false;
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Stop();
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().enabled = false;
                ListOfAIs[i].aiController.GetComponentInChildren<SphereCollider>().enabled = false;
                ListOfAIs[i].aiController.GetComponentInChildren<AICollider>().enabled = false;
                ListOfAIs[i].aiController.gameObject.SetActive(false);
            }
            StopAllCoroutines();
        }

        public void ResumeAIs()
        {
            for (int i = 0; i < ListOfAIs.Count; i++)
            {
                ListOfAIs[i].aiController.gameObject.SetActive(true);
                // enable AI
                ListOfAIs[i].aiController.enabled = true;
                ListOfAIs[i].aiController.Context.IsPaused = false;
                ListOfAIs[i].aiController.GetComponent<MapAIController>().enabled = true;
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().enabled = true;
                ListOfAIs[i].aiController.GetComponentInChildren<SphereCollider>().enabled = true;
                ListOfAIs[i].aiController.GetComponentInChildren<AICollider>().enabled = true;
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Start();
                ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Execute();

                //ListOfAIs[i].aiController.enabled = true;
                //ListOfAIs[i].aiController.Context.IsPaused = false;
                //ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Resume();
                //ListOfAIs[i].aiController.GetComponent<NavMeshAgent>().isStopped = false;
            }
        }
        #endregion

        #region [Initializers]
        ///// <summary>
        ///// Creates the storage of all map objects with their positions
        ///// </summary>
        //public void CreateObjectsStorage()
        //{            
        //    for (int i = 0; i < MapObjectManager.Instance.MapObjects.Count; i++)
        //    {
        //        var mapO = MapObjectManager.Instance.MapObjects[MapObjectManager.Instance.MapObjects.Keys.ElementAt(i)];
        //        if (mapO.gameObject != null)
        //        {
        //            MapObjectsStorage.Add(mapO.transform, mapO);
        //            MapObjectsLookUp.Add(mapO.transform);
        //        }
        //    }
        //}

        /// <summary>
        /// Refresh the list of enemy heroes for each ai
        /// </summary>
        public void RefreshHeroesListsInContexts()
        {
            for (int i = 0; i < ListOfAIs.Count; i++)
            {
                ListOfAIs[i].EnemyHeroes.Clear();
                for (int j = 0; j < HeroManager.Instance.DebugHeroes.Count; j++)
                {
                    if (HeroManager.Instance.DebugHeroes[j].GetComponent<RTSPlayerController>() != ListOfAIs[i].AIHero)
                    {
                        ListOfAIs[i].EnemyHeroes.Add(HeroManager.Instance.DebugHeroes[i].GetComponent<RTSPlayerController>());
                    }
                }
            }
        }

        /// <summary>
        /// Adds an AI context for a player controller
        /// </summary>
        /// <param name="player"></param>
        public void CreateContextForPlayer(RTSPlayerController player)
        {
            // we don't add if we have it already
            for (int i = 0; i < ListOfAIs.Count; i++)
            {
                if (ListOfAIs[i].AIHero == player)
                {
                    return;
                }
            }
            // if here, add it
            Debug.Log("MapAI: INITIALIZING");
            ListOfAIs.Add(new MapAIContext(player));
            player.GetComponent<MapAIController>().Context = ListOfAIs.Last();
        }

        /// <summary>
        /// Remove an AI context for a player controller
        /// </summary>
        /// <param name="player"></param>
        public void RemoveContextForPlayer(RTSPlayerController player)
        {
            object mylock = new object();

            lock (mylock)
            { for (int i = 0; i < ListOfAIs.Count; i++)
                {
                    if (ListOfAIs[i].AIHero == player)
                    {
                        // first we stop the AI if running
                        if (ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].state == UtilityAIClientState.Running)
                        {
                            ListOfAIs[i].aiController.GetComponent<UtilityAIComponent>().clients[0].Stop();
                        }
                        ListOfAIs.RemoveAt(i);
                        break;
                    }
                }
            }
        }

        #endregion

        #region [Utility]
        /// <summary>
        /// Get the object that is nearest on the path and not consider the power level
        /// </summary>
        /// <param name="aiController"></param>
        /// <returns></returns>
        public MapObjectHandle GetClosestObject(MapAIController aiController)
        {
            object mylock = new object();

            float minDist = float.MaxValue;
            MapObjectHandle selectedObject = null;

            lock (mylock)
            {
                for (int i = 0; i < aiController.Context.FoundObjects.Count; i++)
                {
                    var dist = GetPathDistanceToMapObject(aiController.Context, aiController.Context.FoundObjects.ElementAt(i));
                    bool isNotVisited = aiController.Context.FoundObjects.ElementAt(i).GetComponent<MapObjectHandle>().ClickInteractable;
                    bool isDangerous = aiController.Context.LostEncounters.Contains(aiController.Context.FoundObjects.ElementAt(i).GetComponent<MapObjectHandle>());
                    if (dist < minDist && isNotVisited && !isDangerous)
                    {
                        selectedObject = aiController.Context.FoundObjects.ElementAt(i).GetComponent<MapObjectHandle>();
                        minDist = dist;
                    }
                }
            }

            return selectedObject;
        }

        /// <summary>
        /// Get the object that is nearest on the path of a specific type
        /// </summary>
        /// <param name="aiController"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public MapObjectHandle GetClosestObject(MapAIController aiController, MapObjectTypes objectType)
        {
            object mylock = new object();

            float minDist = float.MaxValue;
            MapObjectHandle selectedObject = null;

            lock (mylock)
            {

                for (int i = 0; i < aiController.Context.FoundObjects.Count; i++)
                {
                    var objMapOHandle = aiController.Context.FoundObjects.ElementAt(i).GetComponent<MapObjectHandle>();
                    if (!aiController.Context.FoundObjects.ElementAt(i).gameObject.activeSelf || !objMapOHandle.ClickInteractable) //if object is disabled or spent
                    {
                        continue;
                    }
                    var ambushParams =  aiController.Context.FoundObjects.ElementAt(i).GetComponent<AmbushParams>();
                    var dist = GetPathDistanceToMapObject(aiController.Context, aiController.Context.FoundObjects.ElementAt(i));
                    var objType = objMapOHandle.MapObjectType;
                    //bool isNotVisited = aiController.Context.FoundObjects.ElementAt(i).GetComponent<MapObjectHandle>().ClickInteractable && aiController.Context.FoundObjects.ElementAt(i).GetComponent<PhotonView>().ViewID > 0;
                    bool randChance = UnityEngine.Random.value <= RandomChanceForAction;
                    bool isMarkedAsDangerous = aiController.Context.LostEncounters.Contains(objMapOHandle);

                    // we need to check if we have an encounter and the power of that encounter
                    bool encounterManageable = true;
                    if (objType == MapObjectTypes.Encounter)
                    {
                        float chance = GetEncounterChances(aiController.Context, ambushParams);
                        if (chance <= ChanceToWinWhenWeaker && randChance)
                        {
                            encounterManageable = false;
                        }
                    }
                    else //if chest there may be an ambush
                    if (objType == MapObjectTypes.Chest)
                    {
                        if (objMapOHandle.HasAmbush)
                        {
                            float chance = GetEncounterChances(aiController.Context, ambushParams);
                            if (chance <= ChanceToWinWhenWeaker && randChance)
                            {
                                encounterManageable = false;
                            }
                        }
                    }
                    else //if chest there may be an ambush
                    if (objType == MapObjectTypes.Grave)
                    {
                        if (objMapOHandle.HasAmbush)
                        {
                            float chance = GetEncounterChances(aiController.Context, ambushParams);
                            if (chance <= ChanceToWinWhenWeaker && randChance)
                            {
                                encounterManageable = false;
                            }
                        }
                    }


                    if (dist < minDist && objType == objectType /*&& isNotVisited */&& encounterManageable && !isMarkedAsDangerous)
                    {
                        selectedObject = objMapOHandle;
                        minDist = dist;
                    }
                }

            }

            return selectedObject;
        }

        /// <summary>
        /// Get all objects of a specific type in range
        /// </summary>
        /// <param name="aiController"></param>
        /// <param name="objectType"></param>
        /// <returns></returns>
        public HashSet<Transform> GetObjectsOfType(MapAIController aiController, MapObjectTypes objectType)
        {
            HashSet<Transform> result = new HashSet<Transform>();

            for (int i = 0; i < aiController.Context.FoundObjects.Count; i++)
            {
                var mapO = aiController.Context.FoundObjects.ElementAt(i).GetComponent<MapObjectHandle>();
                if (mapO.MapObjectType == objectType && !aiController.Context.LostEncounters.Contains(mapO) && mapO.ClickInteractable)
                {
                    result.Add(mapO.transform);
                }
            }

            return result;
        }

        /// <summary>
        /// Get the path distance to a specific player. Returns -1 if no path was found or the player is not on the navmesh 
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public float GetPathDistanceToPlayer(MapAIContext context, RTSPlayerController player)
        {
            NavMeshPath navPath = new NavMeshPath();
            float dist = -1;
            if (context.AIHero.agent.CalculatePath(player.transform.position, navPath))
            {
                if (navPath.status == NavMeshPathStatus.PathComplete)
                {
                    dist = context.AIHero.agent.remainingDistance;
                }
            }

            return dist;
        }

        /// <summary>
        /// Get the path distance to a specific map object. Returns -1 if no path was found or the map object is not on the navmesh 
        /// </summary>
        /// <param name="mapObject"></param>
        /// <returns></returns>
        public float GetPathDistanceToMapObject(MapAIContext context, Transform mapObject)
        {
            NavMeshPath navPath = new NavMeshPath();
            float dist = -1;
            var pathResult = context.AIHero.agent.CalculatePath(mapObject.position, navPath);
            if (pathResult)
            {
                if (navPath.status == NavMeshPathStatus.PathComplete)
                {
                    dist = 0;
                    for (int i = 1; i < navPath.corners.Length; i++)
                    {
                        dist += Vector3.Distance(navPath.corners[i - 1], navPath.corners[i]);
                    }
                }
            }

            return dist;
        }

        /// <summary>
        /// Get hero power depending on army and buffs (todo: add power level, returns attack power for now)
        /// </summary>
        /// <param name="actor"></param>
        /// <param name="heroId"></param>
        /// <returns></returns>
        public float GetHeroPower(int actor, int heroId)
        {
            var hero = ResourcesLoader.Instance.UnitsDict[heroId];
            
            return hero.UnitBasePower;
        }
        
        /// <summary>
        /// Get encounter power (TODO: returns only the attack power of the unit/hero encounter. Calculate power level when data available
        /// </summary>
        /// <param name="encounterId"></param>
        /// <returns></returns>
        public float GetEncounterPower(MapAIContext context, AmbushParams ambushParams)
        {
            float result = -1f;
            Unit unit = null;

            if (ambushParams.IsHero)
            {
                unit = ResourcesLoader.Instance.UnitsDict[ambushParams.NpcHeroId];
                
                result = unit.UnitBasePower;
            }
            else
            if (ambushParams.NpcUnitId != -1)
            {
                StatsParams unitsInAmbush = MenuManager.GetBaseUnitStats(ambushParams.NpcUnitId, Enums.UnitTypes.UNIT);
                int numberOfUnitsInAmbush = UnityEngine.Random.Range(3, 8);
                int unitsPower = numberOfUnitsInAmbush * unitsInAmbush.UnitBasePower;
                               
                unit = ResourcesLoader.Instance.UnitsDict[ambushParams.NpcUnitId];
                result = unitsPower * numberOfUnitsInAmbush;
            }
            else
            {
                Debug.LogError("MapAI: Unit encounter power is 0 for some reason!" + ambushParams.gameObject.name);
                result = 0f;
            }

            return result;
        }

        /// <summary>
        /// Return the chance to win an encounter
        /// </summary>
        /// <param name="context">Current AI context</param>
        /// <param name="ambushParams">Encounter</param>
        /// <returns></returns>
        public float GetEncounterChances(MapAIContext context, AmbushParams ambushParams)
        {
            float myHeroPower = GetHeroPower(Const.AI_ACTOR, context.AIHero.GetComponent<MapObjectUnit>().CharacterStats.UnitID);
            float encounterPower = ambushParams.GetComponent<MapObjectHandle>().HasAmbush ? GetEncounterPower(context, ambushParams) : 0;
            //List<int> myHeroArmy = context.AIHero.GetComponent<MapObjectUnit>().CharacterStats.PresetList[0];
            float armyPower = 0;
            //for (int i = 0; i < myHeroArmy.Count; i++)
            //{
            //    armyPower += MenuManager.GetBaseUnitStats(myHeroArmy[i], Enums.UnitTypes.UNIT).UnitBasePower;
            //}
            armyPower += myHeroPower;

            if (context.LostEncounters.Contains(ambushParams.GetComponent<MapObjectHandle>()))
            {
                return 0f;
            }

            if (myHeroPower > encounterPower)
            {
                return ChanceToWinWhenStronger;                
            }
            else
                if (myHeroPower == encounterPower)
            {
                return ChanceToWinWhenSamePower;
               
            }
            else
            {
                return ChanceToWinWhenWeaker;                
            }
        }
        #endregion

        #region [MapObjects & Actions]
        public bool SimulateBattle(MapAIContext context, MapObjectHandle mapObject)
        {
            var ambushParams = mapObject.transform.GetComponent<AmbushParams>();
            
            Debug.Log("MapAI: Got ambushed, in battle!");
            float myHeroPower = GetHeroPower(Const.AI_ACTOR, context.AIHero.GetComponent<MapObjectUnit>().CharacterStats.UnitID);
            float encounterPower = ambushParams.GetComponent<MapObjectHandle>().HasAmbush ? GetEncounterPower(context, ambushParams) : 0;;

            float chanceToWin = 0f;
            float fatigue = 0f;

            // first we stop the ai from moving
            context.ResetActions();
            
            // we pause the ai so it doesn't move until we finish the encounter
            context.IsPaused = true;

            if (myHeroPower > encounterPower)
            {
                chanceToWin = ChanceToWinWhenStronger;
                fatigue = FatigueAfterWinWhenStronger;
            }
            else
                if (myHeroPower == encounterPower)
            {
                chanceToWin = ChanceToWinWhenSamePower;
                fatigue = FatigueAfterWinWhenSamePower;
            }
            else
            {
                chanceToWin = ChanceToWinWhenWeaker;
                fatigue = FatigueAfterWinWhenWeaker;
            }

            bool battleWon = UnityEngine.Random.value <= chanceToWin;

            if (battleWon)
            {
                Debug.Log("MapAI: We WON the encounter!");
                if (mapObject.MapObjectType != MapObjectTypes.Chest && mapObject.MapObjectType != MapObjectTypes.Grave)
                {
                    mapObject.FinishEncounter(false, Const.AI_ACTOR, mapObject.GetComponent<PhotonView>().ViewID);
                }
                //mapObject.gameObject.SetActive(false);
                // check fatigue
                context.Rested -= fatigue + .25f;
                if (context.Rested <= 0f)
                {
                    context.Rested = 0f;
                }
            }
            else
            {
                Debug.Log("MapAI: We lost the encounter!");
                if (!context.LostEncounters.Contains(mapObject))
                {
                    context.LostEncounters.Add(mapObject);
                }
                context.Rested = 0f;
                context.IsGoingForMapObject = false;               
            }
            context.IsPaused = false;

            return battleWon;
        }

        public void Camp(MapAIContext context)
        {
            Debug.Log("MapAI: Need to camp! Camping...");
            UnitAnims.SetAnim(context.aiController.GetComponent<Animator>(), UnitAnims.AnimParams.RunForwardBool, false);
            context.ResetActions();
            context.IsCamping = true;
            //call ai camping
            context.aiController.AiCamping();
        }

        /// <summary>
        /// simulate the AI opening the chest
        /// </summary>
        /// <param name="context"></param>
        /// <param name="chestObject"></param>
        public void GetChest(MapAIContext context, MapObjectChest chestObject)
        {
            var ambushParams = chestObject.GetComponent<AmbushParams>();
            
            if (chestObject.HasAmbush)
            {
                if (SimulateBattle(context, chestObject))
                {
                    chestObject.GetComponent<PhotonView>().RPC("RPC_OpenmapChest", RpcTarget.All, Const.AI_ACTOR, chestObject.GetComponent<PhotonView>().ViewID);
                    if (context.FoundObjects.Contains(chestObject.transform))
                    {
                        context.FoundObjects.Remove(chestObject.transform);
                    }
                }
            }     
            else
            {
                chestObject.GetComponent<PhotonView>().RPC("RPC_OpenmapChest", RpcTarget.All, Const.AI_ACTOR, chestObject.GetComponent<PhotonView>().ViewID);
                if (context.FoundObjects.Contains(chestObject.transform))
                {
                    context.FoundObjects.Remove(chestObject.transform);
                }
            }
        }

        public void GetBuff(MapAIContext context, MapObjectBuff buffObject)
        {
            Debug.Log("MapAI: Applying buff to ai: " + buffObject.MapObjectBuffType);
            buffObject.GetComponent<PhotonView>().RPC("RPC_ApplyBuff", RpcTarget.All, Const.AI_ACTOR, context.AIHero.GetComponent<MapObjectUnit>().CharacterStats.UnitID, buffObject.BuffPower, (int)buffObject.MapObjectBuffType, buffObject.GetComponent<PhotonView>().ViewID);
                       
            // since we are buffed, we empty the lostencounters list so the ai can try again
            context.LostEncounters.Clear();
            // and remove it from found objects
            if (context.FoundObjects.Contains(buffObject.transform))
            {
                context.FoundObjects.Remove(buffObject.transform);
            }
        }

        public void GetGrave(MapAIContext context, MapObjectGrave graveObject)
        {
            var ambushParams = graveObject.GetComponent<AmbushParams>();
            //TODO: when battle is decided, do battle first, then open chest
            if (graveObject.HasAmbush && graveObject.GetComponent<PhotonView>().ViewID >= 1)
            {

                if (SimulateBattle(context, graveObject))
                {
                    graveObject.GetComponent<PhotonView>().RPC("RPC_ReadGrave", RpcTarget.All, Const.AI_ACTOR, graveObject.GetComponent<PhotonView>().ViewID);
                    if (context.FoundObjects.Contains(graveObject.transform))
                    {
                        context.FoundObjects.Remove(graveObject.transform);
                    }
                }

            }
            else
            {
                if (graveObject.GetComponent<PhotonView>().ViewID >= 1)
                {
                    graveObject.GetComponent<PhotonView>().RPC("RPC_ReadGrave", RpcTarget.All, Const.AI_ACTOR, graveObject.GetComponent<PhotonView>().ViewID);
                    if (context.FoundObjects.Contains(graveObject.transform))
                    {
                        context.FoundObjects.Remove(graveObject.transform);
                    }
                }
            }
        }

        public void TeleportToPlayer()
        {
            //Debug.Log("MapAI: Teleporting to " + HeroManager.Instance.DebugHeroes[0].name);
            PauseAIs();
            ListOfAIs[0].aiController.Agent.isStopped = true;
            ListOfAIs[0].aiController.Agent.enabled = false;          
            ListOfAIs[0].aiController.transform.position = HeroManager.Instance.DebugHeroes[0].transform.position;
            PlayerUIManager.Instance.Button_OnSimulateBattle();
        }
        #endregion

    }
}