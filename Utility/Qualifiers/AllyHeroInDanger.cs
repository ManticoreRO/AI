using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Apex.AI;
using System.Linq;
using Apex.Serialization;

namespace JRPG
{
    public class AllyHeroInDanger : QualifierBase
    {
        [ApexSerialization]
        float desiredScore;
        public override float Score(IAIContext context)
        {
            var c = (AIContext)context;
            var ourHero = AIManager.Instance.GetOwnHero(c.CurrentUnit);
            
            // bail out if there is no own hero
            if (ourHero == null)
            {
                return -10;
            }

            // get the tiles around our hero
            var adjacentTiles = EncounterManager.Instance.GetTilesInTroopRangeVector(ourHero, 1);

            // get the enemies around our hero
            c.EnemiesAttackingHero.Clear();

            for (int i = 0; i < adjacentTiles.Count; i++)
            {
                var tile = adjacentTiles.ElementAt(i).Value;    
                if (EncounterManager.Instance.IsTileOccupied(tile) && tile.OccupiedBy != null)
                {
                    if (tile.OccupiedBy.OwnerActor != c.CurrentUnit.OwnerActor && tile.OccupiedBy.OwnerActor != Const.NEUTRAL_ACTOR)
                    {
                        c.EnemiesAttackingHero.Add(tile.OccupiedBy);
                    }
                }
            }

            float result = -10;
            if (c.EnemiesAttackingHero.Count > 0 && c.AllAllies.Count > 0)
            {
                c.ProtectHero = true;
                // adding a random element so not all our units rush to save the hero
                result = (Random.value < 0.5f)?desiredScore:-10;
            }

            return result;
        }
    }
}