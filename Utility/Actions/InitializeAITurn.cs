using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class InitializeAITurn : ActionBase<AIContext>
    {
        public override void Execute(AIContext context)
        {
            Debug.Log("======> AI: executing initializations!");
            var c = context as AIContext;
            c.IsDone = false;
            c.SelectedEnemy = null;
            c.PositionToMove = null;

            c.AllEnemies = EncounterManager.Instance.GetAllEnemies();
            c.AllAllies = EncounterManager.Instance.GetAllAllies();

            BattleTile[,] tilesInRange = EncounterManager.Instance.GetTilesInRange((int)c.CurrentUnit.OnTile.TileCoordinates.x, (int)c.CurrentUnit.OnTile.TileCoordinates.y, c.CurrentUnit.TroopStats.Movement.StatValue);
            c.AllEnemiesInRange = EncounterManager.Instance.GetEnemiesInRange(tilesInRange, c.CurrentUnit.ownerActor);
            c.AllAlliesInRange = EncounterManager.Instance.GetFriendlyInRange(tilesInRange, c.CurrentUnit.ownerActor);

            c.AllAlliesInDanger = AIManager.Instance.AIFriendlyInDanger(c.CurrentUnit);

            c.AllAdjacentEnemies = AIManager.Instance.GetAdjacentEnemies();
            c.AllAdjacentAllies = AIManager.Instance.GetAdjacentAllies();

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

            // get current usable skill if there is one
            List<Skill> Skills = c.CurrentUnit.ActiveSkills;

            c.CurrentActiveSkill = null;
            // we need to check which skill is available 
            for (int i = 0; i < Skills.Count; i++)
            {
                if (!Skills[i].SkillParams.IsLocked && Skills[i].SkillCooldown <= c.CurrentUnit.ActiveSkills[i].SkillParams.TurnsSinceLastSkillUse)
                {
                    c.CurrentActiveSkill = Skills[i];
                    break;
                }
            }

            // setting up the enemy and own hero
            for (int i = 0; i < BattleManager.Instance.Heroes.Count; i++)
            {
                if (BattleManager.Instance.Heroes[i].OwnerActor != c.CurrentUnit.OwnerActor)
                {
                    c.EnemyHero = BattleManager.Instance.Heroes[i];
                }
                else
                {
                    c.MyHero = BattleManager.Instance.Heroes[i];
                }
            }
        }
    }
}
