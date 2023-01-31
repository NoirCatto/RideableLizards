using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using UnityEngine;

namespace RideableLizards;

public partial class RideableLizards
{
    private void LizardOnAct(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;
            c.GotoNext(i => i.MatchCallOrCallvirt<Creature>("LoseAllGrasps"));
            c.GotoPrev(i => i.MatchLdflda<Creature>("inputWithoutDiagonals"));
            c.GotoNext(i => i.MatchBrfalse(out label));
            c.GotoPrev(MoveType.Before,i => i.MatchLdarg(0));

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Lizard self) => self.abstractCreature.world.game.rainWorld.safariMode);
            c.Emit(OpCodes.Brfalse, label);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
            throw;
        }
    }
    
    private void LizardOnDamageAttackClosestChunk(On.Lizard.orig_DamageAttackClosestChunk orig, Lizard self, Creature target)
    {
        if (target is Player p && LikesPlayer(p, self))
        {
            return;
        }
        orig(self,target);
    }

    private static readonly List<Lizard> WasGrabbedByPlayer = new List<Lizard>();
    private void LizardOnUpdate(On.Lizard.orig_Update orig, Lizard self, bool eu)
    {
        if (!self.State.dead && self.grabbedBy.Count > 0 && self.grabbedBy.Any(x => x.grabber is Player))
        {
            var players = self.grabbedBy.Select(x => x.grabber).Where(x => x is Player).Cast<Player>().ToArray();
            
            if (!WasGrabbedByPlayer.Contains(self))
                WasGrabbedByPlayer.Add(self);
            self.abstractCreature.controlled = true;

            self.SafariControlInputUpdate(players.First().playerState.playerNumber); //Controls
            
            if ((self.inputWithDiagonals != null) && self.inputWithDiagonals.Value.y == -1 && self.inputWithDiagonals.Value.pckp) 
            {
                players.First().ThrowObject(0, players.First().evenUpdate); //Release lizard
            }
            if ((self.lastInputWithDiagonals != null && self.inputWithDiagonals != null) && self.inputWithDiagonals.Value.pckp && !self.lastInputWithDiagonals.Value.pckp) 
            {
                self.LoseAllGrasps(); //Release grabbed critter
            }
            if ((self.inputWithDiagonals != null) && self.inputWithDiagonals.Value.jmp)
            {
                players.First().tongue?.Release();
            }
                
        }
        else
        {
            if (WasGrabbedByPlayer.Contains(self))
            {
                self.abstractCreature.controlled = false; //We don't want to run this every update, otherwise it breaks Safari mode.
                WasGrabbedByPlayer.Remove(self);
            }
               
        }
        
        orig(self, eu);

    }
    
    private bool AbstractCreatureOnWantToStayInDenUntilEndOfCycle(On.AbstractCreature.orig_WantToStayInDenUntilEndOfCycle orig, AbstractCreature self)
    {
        if (self.realizedCreature is Lizard liz) //Fix lizard killing a player upon cycle end upon entering a creature den
        {
            return SlugDeets.All(x => x.Value.GrabbedLiz != liz) && orig(self);
        }
        return orig(self);
    }
    
    private void AbstractCreatureOnIsEnteringDen(On.AbstractCreature.orig_IsEnteringDen orig, AbstractCreature self, WorldCoordinate den)
    {
        if (self.realizedCreature is Player pl) //Fix player killing a lizard upon entering creature den
        {
            if (SlugDeets.ContainsKey(pl))
            {
                return;
            }
        }
        orig(self, den);
    }
    
}