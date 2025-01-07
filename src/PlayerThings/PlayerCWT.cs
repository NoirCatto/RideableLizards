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

        playerData = null;
        return false;
    }
    
}

public class PlayerData(AbstractCreature owner)
{
    public readonly AbstractCreature Owner = owner;
    public Player Cat => Owner.realizedCreature as Player;

    public bool RidingALizard => Cat != null && Cat.grasps.Any(x => x?.grabbed is Lizard liz && liz.LikesPlayer(Cat));
    public bool LastRidingALizard;
    public IEnumerable<float> BodyChunksMass = new List<float>();
    public int JumpCounter;
    public int LastJumpCounter;
    public const int MaxJumpCounter = 15;

    private const float MassMod = 0.1f;

    public void Update(bool eu)
    {
        if (RidingALizard && !LastRidingALizard)
        {
            BodyChunksMass = Cat.bodyChunks.Select(x => x.mass).ToList();
            foreach (var chunk in Cat.bodyChunks)
            {
                chunk.mass *= MassMod;
            }
        }
        else if (!RidingALizard && LastRidingALizard)
        {
            for (var i = 0; i < Cat.bodyChunks.Length; i++)
            {
                var savedMass = BodyChunksMass.ElementAtOrDefault(i);
                var diff = savedMass == 0 ? 0 : ((savedMass * MassMod) - Cat.bodyChunks[i].mass); //In case our bodymass has changed, somehow (hi RotundWorld)
                Cat.bodyChunks[i].mass = savedMass - diff;
            }
        }

        LastRidingALizard = RidingALizard;

        if (Cat.input[0].jmp) JumpCounter++;
        else JumpCounter = 0;
        if (Cat.input[1].jmp) LastJumpCounter++;
        else LastJumpCounter = 0;
    }
}