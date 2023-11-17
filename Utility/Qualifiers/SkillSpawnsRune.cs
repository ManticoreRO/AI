using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;
using static Enums;

namespace JRPG
{
    public class SkillSpawnsRune : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            if (c.CurrentActiveSkill == null) return -10;

            if (c.CurrentActiveSkill.Action == Actions.SPAWN && NetworkConnectionManager.Instance.AllUnitsByTypes[UnitTypes.RUNE].ContainsKey(c.CurrentActiveSkill.SpawnUnitID))
            {
                return desiredScore;
            }

            return -10;
        }
    }
}