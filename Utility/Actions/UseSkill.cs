using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;

namespace JRPG
{
    public class UseSkill : ActionBase
    {
        public override void Execute(IAIContext context)
        {
            var c = (AIContext)context;

            Debug.Log("=========> AI: using current skill - " + c.CurrentActiveSkill.SkillName);
            BattleManager.Instance.UseSkill(c.CurrentActiveSkill, c.CurrentUnit, false);
            //BattleManager.Instance.UseSkill(c.CurrentActiveSkill, c.CurrentUnit, BattleManager.TargetUnits, BattleManager.TargetTiles);
            c.CurrentActiveSkill = null;

                
            //todo: make the Ai to wait before doing something else
            //if (c.CurrentActiveSkill.SkillTriggerEnd)
            //{
            //    AIManager.Instance.ForceEndTurnAfterMove(c);
            //}

        }
    }
}