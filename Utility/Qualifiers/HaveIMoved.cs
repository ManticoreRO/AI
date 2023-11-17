using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using Apex.Serialization;

namespace JRPG
{
    public class HaveIMoved : QualifierBase
    {
        [ApexSerialization]
        private float desiredScore;

        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;

            return (c.CurrentUnit.HasMoved && c.CurrentUnit.TemporaryMovementRange == 0) ? desiredScore : -10;
        }
    }
}