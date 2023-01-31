using System;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using UnityEngine;

namespace RideableLizards;

public partial class RideableLizards
{
    private Player.ObjectGrabability PlayerOnGrabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Lizard liz)
        {
            if (LikesPlayer(self, liz)) //Only allowed to grab tamed lizards
            {
                return Player.ObjectGrabability.TwoHands;
            }
        }

        if (obj is Player p)
        {
            if (p.grasps.Any(x => x?.grabbed is Lizard lizz && !lizz.State.dead)) //If player holding a lizard, don't grab them
            {
                return Player.ObjectGrabability.CantGrab;
            }
        }

        return orig(self, obj);
    }
    
    private bool CreatureOnGrab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspused, int chunkgrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideequallydominant, bool pacifying)
    {
        if (self is Player pl && obj is Lizard liz)
        {
            shareability = Creature.Grasp.Shareability.NonExclusive; //Other players able to grab lizard
            pacifying = false; //Don't stun the lizard on grab
        }
        
        return orig(self, obj, graspused, chunkgrabbed, shareability, dominance, overrideequallydominant, pacifying);
    }
    
    private bool PlayerOnIsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabcheck) //Arti fix
    {
        return grabcheck is Lizard || orig(self, grabcheck);
    }
}