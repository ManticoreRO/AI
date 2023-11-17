using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SkillIsTargetedOnEnemies : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            bool resultTest = c.CurrentActiveSkill.SkillTarget == Enums.SkillTargets.TARGET && c.CurrentActiveSkill.SkillTrigger == Enums.SkillTriggers.ON_PLAYER_INPUT;

            Debug.Log("==========> AI: checking if skills requires an enemy as target and input! result = " + resultTest);
            return (resultTest) ? 10 : -10;
        }
    }
}