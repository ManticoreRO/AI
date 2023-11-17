using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class SkillAvailable : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var result = (c.CurrentActiveSkill == null) ? -10 : desiredScore;
            if (c.CurrentActiveSkill != null)
            {
                Debug.Log("=========> AI: We have an active skill to use: " + c.CurrentActiveSkill.SkillName);
            }

            Debug.Log("=========> AI: <color=blue>Checking if we have an active skill to use. Score = " + result + "</color>");
            return result;
        }
    }
}