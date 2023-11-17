using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SkillIsTargetedOnTiles : QualifierBase
    {
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            bool resultTest = c.CurrentActiveSkill.SkillTarget == Enums.SkillTargets.TARGET_TILE;

            Debug.Log("==========> AI: checking if skills requires a tile as target! result = " + resultTest);
            return (resultTest) ? 10 : -10;
        }
    }
}