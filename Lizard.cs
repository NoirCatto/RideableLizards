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
    // private void LizardAIOnAggressiveBehavior(ILContext il)
    // {
    //     try
    //     {
    //         var c = new ILCursor(il);
    //         var label = il.DefineLabel();
    //         // c.GotoNext(i => i.MatchCallvirt(typeof(LizardAI).GetMethod("AttemptBite")));
    //         c.GotoNext(i => i.MatchCallOrCallvirt<Lizard>("AttemptBite"));
    //         c.GotoPrev(MoveType.Before, i => i.MatchLdsfld<ModManager>("MMF"));
    //         //c.RemoveRange(2); //alt way
    //         c.Emit(OpCodes.Ldarg_0);
    //         c.Emit(OpCodes.Ldarg_1);
    //         c.EmitDelegate((LizardAI self, Tracker.CreatureRepresentation target) =>
    //         {
    //             Logger.LogDebug("Rideable1");
    //             if (target.representedCreature.realizedCreature is Player p)
    //             { Logger.LogDebug("Rideable2");
    //                 if (self.LikeOfPlayer(self.tracker.RepresentationForCreature(p.abstractCreature, false)) > 0.5f)
    //                 {
    //                     Logger.LogDebug("Rideable3");
    //                     return true;
    //                 }
    //             }
    //             return false;
    //         });
    //         c.Emit(OpCodes.Brfalse, label); //if above return false jump to label
    //         
    //         c.Emit(OpCodes.Ldarg_0);
    //         c.EmitDelegate((LizardAI self) =>
    //         {
    //            self.lizard.AttemptBite(null);
    //         });
    //         c.Emit(OpCodes.Ret);
    //         c.MarkLabel(label);
    //
    //     }
    //     catch (Exception ex)
    //     {
    //         Logger.LogError(ex);
    //         throw;
    //     }
    // }
    
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
        if (target is Player p && self.AI.LikeOfPlayer(self.AI.tracker.RepresentationForCreature(p.abstractCreature, false)) > LizLikeThreshold)
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
    
    private void AbstractCreatureOnIsEnteringDen(On.AbstractCreature.orig_IsEnteringDen orig, AbstractCreature self, WorldCoordinate den)
    {
        if (self.realizedCreature is Player pl)
        {
            if (SlugDeets.ContainsKey(pl))
            {
                return;
            }
        }
        
        orig(self, den);
    }
}