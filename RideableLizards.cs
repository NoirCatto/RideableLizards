using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using UnityEngine;
using RWCustom;
using BepInEx;
using MonoMod.Cil;
using Debug = UnityEngine.Debug;
#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RideableLizards;

[BepInPlugin("NoirCat.RideableLizards", "Rideable Lizards", "1.0.0")]	
public partial class RideableLizards : BaseUnityPlugin
{
    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
    }

    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);
        
        On.Player.Grabability += PlayerOnGrabability;
        On.Player.IsCreatureLegalToHoldWithoutStun += PlayerOnIsCreatureLegalToHoldWithoutStun;
        On.Creature.Grab += CreatureOnGrab;
        On.AbstractCreature.WantToStayInDenUntilEndOfCycle += AbstractCreatureOnWantToStayInDenUntilEndOfCycle;
        On.AbstractCreature.IsEnteringDen += AbstractCreatureOnIsEnteringDen;
        IL.Lizard.Act += LizardOnAct;
        On.Lizard.DamageAttackClosestChunk += LizardOnDamageAttackClosestChunk;
        On.Lizard.Update += LizardOnUpdate;
        On.ShortcutGraphics.Draw += ShortcutGraphicsOnDraw;
        On.Player.IsObjectThrowable += PlayerOnIsObjectThrowable;
        On.Player.Update += PlayerOnUpdate;
        On.RainWorldGame.ShutDownProcess += RainWorldGameOnShutDownProcess;
        On.GameSession.ctor += GameSessionOnctor;
        
    }

    private const float LizLikeThreshold = 0.5f;

    private void RainWorldGameOnShutDownProcess(On.RainWorldGame.orig_ShutDownProcess orig, RainWorldGame self)
    {
        orig(self);
        ClearMemory();
    }
    private void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        ClearMemory();
    }

    #region Helpers
    private void ClearMemory()
    {
        WasGrabbedByPlayer.Clear();
        SlugDeets.Clear();
    }

    private bool LikesPlayer(Player player, Lizard liz)
    {
        return liz.AI.LikeOfPlayer(liz.AI.tracker.RepresentationForCreature(player.abstractCreature, false)) > LizLikeThreshold; 
    }
    #endregion

}
