using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RideableLizards.LizardThings;
using UnityEngine;

namespace RideableLizards.PlayerThings;

public static class PlayerCWT
{
    public static readonly ConditionalWeakTable<AbstractCreature, PlayerData> PlayerDeets = new ConditionalWeakTable<AbstractCreature, PlayerData>();

    public static bool TryGetPlayerData(this Player player, out PlayerData playerData) => TryGetPlayerData(player.abstractCreature, out playerData);
    public static bool TryGetPlayerData(this AbstractCreature crit, out PlayerData playerData)
    {
        if (crit.creatureTemplate.type == CreatureTemplate.Type.Slugcat)
        {
            playerData = PlayerDeets.GetValue(crit, _ => new PlayerData(crit));
            return true;
        }

        Log.Once($"Creature {crit.ToString()} is NOT a Slugcat!!!");
        playerData = null;
        return false;
    }
    
}

public class PlayerData(AbstractCreature owner)
{
    public readonly AbstractCreature Owner = owner;

    public bool RidingALizard => Owner.realizedCreature != null && Owner.realizedCreature.grasps.Any(x => x?.grabbed is Lizard liz && liz.LikesPlayer((Player)Owner.realizedCreature));
    public bool LastRidingALizard;
    public IEnumerable<float> BodyChunksMass = new List<float>();

    public void Update(bool eu)
    {
        var self = Owner.realizedCreature;
        if (RidingALizard && !LastRidingALizard)
        {
            //Debug.Log($"RIDING A LIZ: {self.firstChunk.mass}");
            BodyChunksMass = self.bodyChunks.Select(x => x.mass).ToArray();
            foreach (var chunk in self.bodyChunks)
            {
                chunk.mass *= 0.1f;
            }
        }
        else if (!RidingALizard && LastRidingALizard)
        {
            for (var i = 0; i < self.bodyChunks.Length; i++)
            {
                var savedMass = BodyChunksMass.ElementAtOrDefault(i);
                var diff = savedMass == 0 ? 0 : ((savedMass * 0.1f) - self.bodyChunks[i].mass); //In case our bodymass has changed, somehow (hi RotundWorld)
                self.bodyChunks[i].mass = savedMass - diff;
            }
            //Debug.Log($"GOT OFF A LIZ: {self.firstChunk.mass}");
        }

        LastRidingALizard = RidingALizard;
    }
}