using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using System.Diagnostics;

namespace JRPG
{
    public class AIContext : IAIContext
    {
        private BattleController currentUnit;
        private BattleController selectedEnemy;
        public BattleTile PositionToMove;

        // List of all enemies and allies
        public List<BattleController> AllEnemies = new List<BattleController>();
        public List<BattleController> AllAllies = new List<BattleController>();

        // List of all enemies and allies in range
        public List<BattleController> AllEnemiesInRange = new List<BattleController>();
        public List<BattleController> AllAlliesInRange = new List<BattleController>();

        // List of all allies in danger
        public List<BattleController> AllAlliesInDanger = new List<BattleController>();

        // List of all allies in range that are in danger
        public List<BattleController> AllAlliesInRangeInDanger = new List<BattleController>();

        // List of all enemies next to current
        public List<BattleController> AllAdjacentEnemies = new List<BattleController>();

        // List of all allies around the current
        public List<BattleController> AllAdjacentAllies = new List<BattleController>();

        // Reference for the enemy hero 
        public BattleController EnemyHero;
        // Reference for my hero
        public BattleController MyHero;

        // Enemies surrounding our hero
        public List<BattleController> EnemiesAttackingHero = new List<BattleController>();
       
        public bool IsMoving = false;
        public bool IsAttacking = false;
        public bool ProtectHero = false;
        public bool IsDone = false;
        // Current available skill
        public Skill CurrentActiveSkill = null;

        public BattleController CurrentUnit
        {
            get
            {
                return currentUnit;
            }
            set
            {
                currentUnit = value;                
                UnityEngine.Debug.Log("<color=green> Battle AI set to " + currentUnit.TroopStats.UnitName + "</color>");
            }
        }

        public BattleController SelectedEnemy
        {
            get
            {
                return selectedEnemy;
            }
            set
            {
                selectedEnemy = value;

                if (selectedEnemy != null)
                {
                    string caller = new StackFrame(1).GetMethod().Name;
                    UnityEngine.Debug.Log("<color=green> Battle AI selected the enemy as " + selectedEnemy.TroopStats.UnitName + "</color> from function " + caller);
                }
            }
        }
        public AIContext(BattleController currentUnit)
        {
            CurrentUnit = currentUnit;
        }

        
    }
}
