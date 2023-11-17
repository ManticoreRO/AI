using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.AI.Components;
using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace JRPG
{
    public enum BattleAIPhase
    {
        InitializePhase,
        SkillPhase ,
        MoveForSkill ,
        MoveForAttack ,
        MovePhase ,
        AttackPhase ,
        BasicActions ,
        EndTurn      
    }

    public class AIManager : MonoBehaviour, IContextProvider
    {        
        public AIContext _context;
        public bool IsExecuting = false;
        public static AIManager Instance;
        /// <summary>
        /// Keeps track of all friendlies that have an enemy in the adjacent tile
        /// - int: player actor
        /// - BattleController - our troop in danger
        /// - List - enemies attacking our troop
        /// </summary>
        public Dictionary<int, Dictionary<BattleController, List<BattleController>>> AIFriendlyTroopsInDanger = new Dictionary<int, Dictionary<BattleController, List<BattleController>>>();
        private BattleAIPhase _previousAIPhase;
        public BattleAIPhase CurrentAIPhase;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            //_context = new AIContext(BattleManager.Instance.TroopsInBattle[BattleManager.Instance.CurrentTroop]);
            GetComponent<UtilityAIComponent>().enabled = true;
        }

        /// <summary>
        /// Initializing the new ai for the current troop
        /// </summary>
        public void SetAIState(bool state, bool startInStandBy = false)
        {
            if (state)
            {

                if (BattleManager.Instance.TroopsInBattle != null && BattleManager.Instance.CurrentTroop >= 0)
                {
                    _context = new AIContext(BattleManager.Instance.TroopsInBattle[BattleManager.Instance.CurrentTroop]);
                    _previousAIPhase = BattleAIPhase.InitializePhase;
                    CurrentAIPhase = BattleAIPhase.InitializePhase;
                    if (startInStandBy)
                    {
                        return;
                    }

                    if (BattleManager.Instance.TroopsInBattle[BattleManager.Instance.CurrentTroop].isDisabled) //if this unit is incapacitated give next turn
                    {
                        BattleUIManager.Instance.SaveLogEntry(Enums.InfoMessageSituations.DISABLED, null, null, null, 0, false);
                        SetAIPhase(BattleAIPhase.EndTurn);
                        return;
                    }
                    IsExecuting = true;
                    GetComponent<UtilityAIComponent>().clients[(int)BattleAIPhase.InitializePhase].Start();
                    GetComponent<UtilityAIComponent>().clients[(int)BattleAIPhase.InitializePhase].Execute();                    
                }
            }
            else
            {
                if (CurrentAIPhase != BattleAIPhase.EndTurn) GetComponent<UtilityAIComponent>().clients[(int)CurrentAIPhase].Stop();
                IsExecuting = false;
            }
        }

        /// <summary>
        /// set new AI phase and make sure is not the same
        /// </summary>
        /// <param name="newPhase"></param>
        public void SetAIPhase(BattleAIPhase newPhase)
        {
            if (CurrentAIPhase != newPhase)
            {
                var tList = new List<BattleController>() { _context.CurrentUnit };
               
                Debug.Log("BattleAI: Setting new phase to " + newPhase);
                _previousAIPhase = CurrentAIPhase;
                CurrentAIPhase = newPhase;
                // if we are at end of turn, let's request it
                if (CurrentAIPhase == BattleAIPhase.EndTurn || _context.CurrentUnit.IsDead)
                {
                    _context.IsDone = true;
                    GetComponent<UtilityAIComponent>().clients[(int)_previousAIPhase].Stop();
                    SetAIState(false);
                    BattleManager.Instance.AISwitchTurn();
                }
                else
                {
                    BattleManager.BattleQueue.AddToQueue(_context.CurrentUnit, new BattleQueueParams(0, BattleManager.BattleAction.AIDummyAction, _context.CurrentUnit, tList));
                    BattleManager.Instance.PlayIfEmptyQueue();
                }
                //ExecuteCurrentPhase();
            }
        }

        /// <summary>
        /// Checks if we are at new AI phase
        /// </summary>
        /// <returns></returns>
        public bool IsNewPhase()
        {
            Debug.Log("<color=magenta> _previousAIPhase = " + _previousAIPhase + "   CurrentAIPhase = " + CurrentAIPhase + "</color>");
            return (_previousAIPhase != CurrentAIPhase);
        }

        /// <summary>
        /// Execute the current AI phase
        /// </summary>
        public void ExecuteCurrentPhase([CallerMemberName] string callerName = "")
        {
            //todo: DANIEL, check why the context is null sometimes
            if (_context == null)
            {
                Debug.Log("Context null?! - Skip from caller " + callerName);
            }
            ExecutePhase();
        }

        private void ExecutePhase()
        {
            //GetComponent<UtilityAIComponent>().clients[(int)_previousAIPhase].Stop();
            //GetComponent<UtilityAIComponent>().clients[(int)CurrentAIPhase].Start();
            if (GetComponent<UtilityAIComponent>().clients.Length > (int)CurrentAIPhase)
            {
                GetComponent<UtilityAIComponent>().clients[(int)CurrentAIPhase].Execute();
                if (CurrentAIPhase != BattleAIPhase.EndTurn)
                {
                    InitializeAIContextAfterPhase(_context);
                }
            }
        }
        /// <summary>
        /// Initializes the ai context again after a phase
        /// </summary>
        /// <param name="c"></param>
        public void InitializeAIContextAfterPhase(IAIContext context)
        {
            if (BattleUIManager.Outcome != BattleUIManager.MatchOutcome.None)
            {
                return;
            }
            
            var c = context as AIContext;

            // if the unit is dead, the c.current.ontile is null and it triggers an exception
            if (c.CurrentUnit.IsDead)
            {
                return;
            }

            c.IsDone = false;
            c.SelectedEnemy = null;
            c.ProtectHero = false;

            c.AllEnemies = EncounterManager.Instance.GetAllEnemies();
            c.AllAllies = EncounterManager.Instance.GetAllAllies();
            //c.CurrentUnit = BattleManager.Instance.TroopsInBattle[BattleManager.Instance.CurrentTroop]; //re-init of current troop
            //if (c.CurrentUnit == null)
            //{
            //    Debug.LogError("cCurrentUnit is Null!");
            //}

            //if (c.CurrentUnit?.OnTile == null)
            //{
            //    Debug.LogError("c.CurrentUnit.OnTile is Null!");
            //}
            //if (c.CurrentUnit?.TroopStats == null)
            //{
            //    Debug.LogError("c.CurrentUnit.TroopStats Null!");
            //}
            BattleTile[,] tilesInRange = EncounterManager.Instance.GetTilesInRange((int)c.CurrentUnit.OnTile.TileCoordinates.x, (int)c.CurrentUnit.OnTile.TileCoordinates.y, c.CurrentUnit.TroopStats.Movement.StatValue);
            c.AllEnemiesInRange = EncounterManager.Instance.GetEnemiesInRange(tilesInRange, c.CurrentUnit.ownerActor);
            c.AllAlliesInRange = EncounterManager.Instance.GetFriendlyInRange(tilesInRange, c.CurrentUnit.ownerActor);

            c.AllAlliesInDanger = AIFriendlyInDanger(c.CurrentUnit);

            c.AllAdjacentEnemies = GetAdjacentEnemies();
            c.AllAdjacentAllies = GetAdjacentAllies();

            // build the list of all allies in range that are in danger aswell
            c.AllAlliesInRangeInDanger.Clear();
            if (c.AllAlliesInDanger != null)
            {
                foreach (var ally in c.AllAlliesInDanger)
                {
                    if (c.AllAlliesInRange.Contains(ally))
                    {
                        c.AllAlliesInRangeInDanger.Add(ally);
                    }
                }
            }
        }

        /// <summary>
        /// Update the dictionary with all firendly troops of AI currently having enemies around
        /// </summary>
        /// <param name="battleTile"></param>
        public void UpdateAIFriendlyTroopsInDanger(BattleTile battleTile)
        {
            // only for AI also exclude traps
            if (battleTile.OccupiedBy.OwnerActor != Const.AI_ACTOR || !battleTile.OccupiedBy.isAI || battleTile.OccupiedBy.OwnerActor == Const.NEUTRAL_ACTOR) return;
            // check if there is an enemy on an adjacent tile and update the dictionary
            BattleTile[,] adjacent = EncounterManager.Instance.GetTilesInRange((int)battleTile.TileCoordinates.x, (int)battleTile.TileCoordinates.y, 1);
            List<BattleController> enemies = EncounterManager.Instance.GetEnemiesInRange(adjacent, battleTile.OccupiedBy.OwnerActor);

            if (!AIFriendlyTroopsInDanger.ContainsKey(battleTile.OccupiedBy.OwnerActor))
            {
                AIFriendlyTroopsInDanger.Add(battleTile.OccupiedBy.OwnerActor, new Dictionary<BattleController, List<BattleController>>());
            }

            if (enemies.Count > 0)
            {
                if (!AIFriendlyTroopsInDanger[battleTile.OccupiedBy.OwnerActor].ContainsKey(battleTile.OccupiedBy))
                {
                    AIFriendlyTroopsInDanger[battleTile.OccupiedBy.OwnerActor].Add(battleTile.OccupiedBy, enemies);
                }
                else
                {
                    AIFriendlyTroopsInDanger[battleTile.OccupiedBy.OwnerActor][battleTile.OccupiedBy] = enemies;
                }
            }
            // sort now by hp
            SortFriendlyInDangerByEnemyHP(battleTile.OccupiedBy.OwnerActor);
        }

        /// <summary>
        /// Check if a troop is attacked by any other troop and returns a list of all that attacks it
        /// </summary>
        /// <param name="troop"></param>
        /// <returns></returns>
        public List<BattleController> AIFriendlyInDanger(BattleController troop)
        {
            if (AIFriendlyTroopsInDanger.ContainsKey(troop.OwnerActor))
            {
                if (AIFriendlyTroopsInDanger[troop.ownerActor].ContainsKey(troop))
                return AIFriendlyTroopsInDanger[troop.OwnerActor][troop];
            }

            // return nothing otherwise
            return null;
        }

        /// <summary>
        /// Get all the adjacent enemies sorted by hp of the current selected troop
        /// </summary>
        /// <returns></returns>
        public List<BattleController> GetAdjacentEnemies()
        {
            BattleController current = BattleManager.Instance.TroopsInBattle[BattleManager.Instance.CurrentTroop];
            BattleTile[,] tilesInRange = EncounterManager.Instance.GetTilesInRange((int)current.OnTile.TileCoordinates.x, (int)current.OnTile.TileCoordinates.y, 1);
            var result = EncounterManager.Instance.GetEnemiesInRange(tilesInRange, current.ownerActor);
            result.Sort((o1, o2) => o1.TroopStats.HitPoints.StatValue.CompareTo(o2.TroopStats.HitPoints.StatValue));
            return result;
        }

        /// <summary>
        /// Get all the adjacent allies sorted by hp
        /// </summary>
        /// <returns></returns>
        public List<BattleController> GetAdjacentAllies()
        {
            BattleController current = BattleManager.Instance.TroopsInBattle[BattleManager.Instance.CurrentTroop];
            BattleTile[,] tilesInRange = EncounterManager.Instance.GetTilesInRange((int)current.OnTile.TileCoordinates.x, (int)current.OnTile.TileCoordinates.y, 1);
            var result = EncounterManager.Instance.GetFriendlyInRange(tilesInRange, current.ownerActor);
            result.Sort((o1, o2) => SortListByRemainingHP(o1.TroopStats, o2.TroopStats));
            return result;
        }

        // Helper function to sort by remaining health
        public int SortListByRemainingHP(StatsParams o1, StatsParams o2)
        {
            float diff1 = o1.HitPointsPool.StatValue - o1.HitPoints.StatValue;
            float diff2 = o2.HitPointsPool.StatValue - o2.HitPoints.StatValue;

            return diff1.CompareTo(diff2);
        }

        /// <summary>
        /// Get all the adjacent enemies sorted by hp around a specific target
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public List<BattleController> GetAdjacentEnemies(BattleController target)
        {
            BattleController current = target;
            BattleTile[,] tilesInRange = EncounterManager.Instance.GetTilesInRange((int)current.OnTile.TileCoordinates.x, (int)current.OnTile.TileCoordinates.y, 1);
            var result = EncounterManager.Instance.GetEnemiesInRange(tilesInRange, current.ownerActor);
            result.Sort((o1, o2) => o1.TroopStats.HitPoints.StatValue.CompareTo(o2.TroopStats.HitPoints.StatValue));
            return result;
        }

        // Helper function for sorting
        public void SortFriendlyInDangerByEnemyHP(int actor)
        {
            if (!AIFriendlyTroopsInDanger.ContainsKey(actor)) return;

            List<BattleController> keyList = new List<BattleController>(AIFriendlyTroopsInDanger[actor].Keys);
            for (int i = 0; i < keyList.Count; i++)
            {
                var friendly = keyList[i];
                if (AIFriendlyTroopsInDanger[actor][friendly].Count > 0)
                {  
                    AIFriendlyTroopsInDanger[actor][friendly].Sort((o1, o2) => o1.TroopStats.HitPoints.StatValue.CompareTo(o2.TroopStats.HitPoints.StatValue));
                }
            }
        }

        /// <summary>
        /// Is the enemy hero in our range? If yes, return it
        /// </summary>
        public BattleController GetEnemyHeroInRange(BattleController troop)
        {
            var tilesInRange = EncounterManager.Instance.GetTilesInTroopRangeVector(troop, (int)troop.TroopStats.Movement.StatValue);

            for (int i = 0; i < tilesInRange.Count; i++)
            {
                var tile = tilesInRange.ElementAt(i).Value;
                if (!EncounterManager.Instance.IsTileOccupied(tile) && tile.OccupiedBy != null)
                {
                    if (tile.OccupiedBy.OwnerActor != troop.OwnerActor && tile.OccupiedBy.OwnerActor != Const.NEUTRAL_ACTOR)
                    {
                        return tile.OccupiedBy;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Is the enemy hero in the specific range? If yes, return it.
        /// </summary>
        /// <param name="troop"></param>
        /// <param name="range">The range we check inside</param>
        /// <returns></returns>
        public BattleController GetEnemyHeroInRange(BattleController troop, int range)
        {
            var tilesInRange = EncounterManager.Instance.GetTilesInTroopRangeVector(troop, range);

            if (tilesInRange != null)
            {
                for (int i = 0; i < tilesInRange.Count; i++)
                {
                    var tile = tilesInRange.ElementAt(i).Value;
                    if (EncounterManager.Instance.IsTileOccupied(tile) && tile.OccupiedBy != null)
                    {
                        if (tile.OccupiedBy.OwnerActor != troop.OwnerActor && tile.OccupiedBy.OwnerActor != Const.NEUTRAL_ACTOR)
                        {
                            return tile.OccupiedBy;
                        }
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Is your own hero in range? If yes, return it
        /// </summary>
        /// <param name="troop"></param>
        /// <returns></returns>
        public BattleController GetOwnHeroInRange(BattleController troop)
        {
            var tilesInRange = EncounterManager.Instance.GetTilesInTroopRangeVector(troop, (int)troop.TroopStats.Movement.StatValue);

            for (int i = 0; i < tilesInRange.Count; i++)
            {
                var tile = tilesInRange.ElementAt(i).Value;
                if (EncounterManager.Instance.IsTileOccupied(tile) && tile.OccupiedBy != null)
                {
                    if (tile.OccupiedBy.OwnerActor == EncounterManager.Instance.GetOwnHeroActor())
                    {
                        return tile.OccupiedBy;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Is the own hero in the specific range? If yes, return it.
        /// </summary>
        /// <param name="troop"></param>
        /// <param name="range">The range we check inside</param>
        /// <returns></returns>
        public BattleController GetOwnHeroInRange(BattleController troop, int range)
        {
            var tilesInRange = EncounterManager.Instance.GetTilesInTroopRangeVector(troop, range);

            for (int i = 0; i < tilesInRange.Count; i++)
            {
                var tile = tilesInRange.ElementAt(i).Value;
                if (EncounterManager.Instance.IsTileOccupied(tile) && tile.OccupiedBy != null)
                {
                    if (tile.OccupiedBy.OwnerActor == EncounterManager.Instance.GetOwnHeroActor())
                    {
                        return tile.OccupiedBy;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the enemy hero controller for the specified troop
        /// </summary>
        /// <param name="troop"></param>
        /// <returns></returns>
        public BattleController GetEnemyHero(BattleController troop)
        {
            for (int i = 0; i < BattleManager.Instance.Heroes.Count; i++)
            {
                if (BattleManager.Instance.Heroes[i].OwnerActor != troop.OwnerActor)
                    return BattleManager.Instance.Heroes[i];
            }

            // we should't get here
            return null;
        }

        /// <summary>
        /// Return the hero controller that commands the specified troop
        /// </summary>
        /// <param name="troop"></param>
        /// <returns></returns>
        public BattleController GetOwnHero(BattleController troop)
        {
            for (int i = 0; i < BattleManager.Instance.Heroes.Count; i++)
            {
                if (BattleManager.Instance.Heroes[i].OwnerActor == troop.OwnerActor)
                    return BattleManager.Instance.Heroes[i];
            }

            // we should't get here
            return null;
        }

        /// <summary>
        /// Get a possible closest position to move towards the closest enemy
        /// </summary>
        /// <returns></returns>
        public BattleTile GetPossibleMovePosition()
        {
            BattleController enemyToMoveTo = null;

            var enemies = _context.AllEnemies;
            
            float dist = float.MaxValue;
            foreach (var enemy in enemies)
            {
                if(enemy.IsDead) continue;
                var dist1 = Vector3.Distance(enemy.transform.position, _context.CurrentUnit.transform.position);
                if (dist1 < dist )
                {                   
                    enemyToMoveTo = enemy;  
                    dist = dist1;
                }
            }

            BattleTile positionAroundEnemy = null;

            // Early out if for some reason we didnt find a target
            if (enemyToMoveTo == null)
                return null;

            Debug.Log("<color=magenta>AI =======> Moving toward position: " + enemyToMoveTo.TroopStats.UnitName + "</color>");

            positionAroundEnemy = EncounterManager.Instance.GetFreeAdjacentTile(enemyToMoveTo.OnTile);
            

            return positionAroundEnemy;
        }

        /// <summary>
        /// Get a possible closest position to move towards the closest enemy for a ranged troop
        /// </summary>
        /// <returns></returns>
        public BattleTile GetPossibleMovePositionForRanged(bool isForSkill = false)
        {
            BattleController enemyToMoveTo = null;

            var enemies = _context.AllEnemies;
            var currentSkill = _context.CurrentActiveSkill;
            
            Dictionary<Vector3, BattleTile> tilesInEnemyRangePlusOne = new Dictionary<Vector3, BattleTile>();
            Dictionary<Vector3, BattleTile> tilesInEnemyRange = new Dictionary<Vector3, BattleTile>();
            Dictionary<Vector3, BattleTile> availableTiles = new Dictionary<Vector3, BattleTile>();
            List<BattleController> dangerousEnemies = new List<BattleController>();

            // we first see which enemies can attack our ranged unit
            for (int i = 0; i < enemies.Count; i++)
            {
                if (!enemies[i].IsDead)
                {
                    enemyToMoveTo = enemies[i];
                    // Get tiles in enemy movement range
                    tilesInEnemyRange = (!isForSkill) ? EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo) :
                                                               EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, currentSkill.ActionBaseRange);
                    // if first enemy, we initialize the available tiles 
                    if (i == 0)
                    {
                        foreach (var kvp in tilesInEnemyRange)
                        {
                            availableTiles.Add(kvp.Key, kvp.Value);
                        }
                    }
                    // Check if we are in that range
                    if (tilesInEnemyRange.ContainsValue(_context.CurrentUnit.OnTile))
                    {
                        dangerousEnemies.Add(enemyToMoveTo);
                        // create a list of unavailable tiles
                        // what is the length of the safe area for our archer
                        var safeAreaModifier = _context.CurrentUnit.TroopStats.AttackRange.StatValue - enemyToMoveTo.TroopStats.Movement.StatValue;
                        if (safeAreaModifier < 0) safeAreaModifier = 0;
                        tilesInEnemyRangePlusOne = (!isForSkill) ? EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, safeAreaModifier) :
                                                               EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, currentSkill.ActionBaseRange);
                        availableTiles = EncounterManager.Instance.InverseTileAreaIntersection(tilesInEnemyRangePlusOne, availableTiles);
                    }
                }
            }
            // if the list is empty, we will use the allenemies list
            if (dangerousEnemies.Count == 0)
            {
                return null; // in this situation we can move like a melee towards the enemy
            }
           
            // remove from available tiles all tiles that are occupied
            List<BattleTile> freeTiles = new List<BattleTile>();
            foreach (var tile in availableTiles)
            {
                if (!EncounterManager.Instance.IsTileOccupied(tile.Value))
                {
                    freeTiles.Add(tile.Value);
                }
            }

            if (freeTiles.Count == 0) return null;

            return freeTiles[UnityEngine.Random.Range(0, freeTiles.Count)];


            //// select the best enemy to attack so the ranged is safe from all other enemies
            //for (int i = 0; i < enemies.Count; i++)
            //{
            //    enemyToMoveTo = enemies[i];

            //    tilesInEnemyRangePlusOne = (!isForSkill) ? EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, 2) :
            //                                               EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, currentSkill.ActionBaseRange);
            //    tilesInEnemyRange = (!isForSkill) ? EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, 1) :
            //                                        EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, currentSkill.ActionBaseRange - 1);

            //    var availableTilesForCurrentEnemy = EncounterManager.Instance.InverseTileAreaIntersection(tilesInEnemyRangePlusOne, tilesInEnemyRange);
            //    availableTiles = EncounterManager.Instance.InverseTileAreaIntersection(availableTilesForCurrentEnemy, availableTiles);
            //}
        }
       
        /// <summary>
        /// Get a possible closest position to move towards a specific enemy for a ranged troop taking into account if the ranged can move to that position
        /// </summary>
        /// <param name="enemy">The target we need to get to</param>
        /// <param name="caller">The troop we want to move there</param>
        /// <returns></returns>
        public BattleTile GetPossibleMovePositionForRanged(BattleController enemy, BattleController caller)
        {
            BattleController enemyToMoveTo = enemy;

            // check the range of the enemy and select a tile outside it
            var tilesInEnemyRangePlusOne = EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, 3);
            var tilesInEnemyRange = EncounterManager.Instance.GetTilesInTroopRangeVector(enemyToMoveTo, 1);
            var availableTiles = EncounterManager.Instance.InverseTileAreaIntersection(tilesInEnemyRangePlusOne, tilesInEnemyRange);

            // remove from available tiles all tiles that are occupied
            List<BattleTile> freeTiles = new List<BattleTile>();
            if (availableTiles != null && availableTiles.Count > 0)
            {               
                foreach (var tile in availableTiles)
                {
                    if (!EncounterManager.Instance.IsTileOccupied(tile.Value))
                    {
                        freeTiles.Add(tile.Value);
                    }
                }
            }

            // if we didnt find any tiles to move in range for some reason, we move like a melee
            if (freeTiles.Count == 0)
            {
                return GetPossibleMovePosition();
            }

            return freeTiles[UnityEngine.Random.Range(0, freeTiles.Count)];
        }

        public IAIContext GetContext(Guid aiId)
        {
            return _context;
        }

        public bool IsAIDone(IAIContext context)
        {
            var c = (AIContext)context;
            return c.CurrentUnit.IsDead || (c.CurrentUnit.HasAttacked && c.CurrentUnit.TemporaryMovementRange <= 0 && c.CurrentActiveSkill == null);
        }
       
        // coroutine for starting attack after moving
        public void GetToTargetAndAttack(IAIContext context)
        {
            var c = (AIContext)context;
            
            if (c.SelectedEnemy != null)
            {              
                BattleManager.Instance.Attack(c.CurrentUnit, c.SelectedEnemy);
                c.IsAttacking = true;                
            }            
        }

        // end turn
        public void EndTurnForAI(IAIContext context)    
        {
            return;

            var c = (AIContext)context;
            c.IsDone = true;
            Debug.Log("<color=cyan> Ai IsDone</color>");
            BattleManager.Instance.InvokeSwitchTurn();
        }

    }
}