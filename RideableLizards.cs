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
        On.Creature.Grab += CreatureOnGrab;
        On.AbstractCreature.IsEnteringDen += AbstractCreatureOnIsEnteringDen;
        //IL.LizardAI.AggressiveBehavior += LizardAIOnAggressiveBehavior;
        IL.Lizard.Act += LizardOnAct;
        On.Lizard.DamageAttackClosestChunk += LizardOnDamageAttackClosestChunk;
        On.Lizard.Update += LizardOnUpdate;
        On.ShortcutGraphics.Draw += ShortcutGraphicsOnDraw;
        On.Player.IsObjectThrowable += PlayerOnIsObjectThrowable;
        On.Player.Update += PlayerOnUpdate;
        On.RainWorld.Update += RainWorld_Update;
        On.GameSession.ctor += GameSessionOnctor;
        
    } //todo: add drop button for when liz is holding prey

    private const float LizLikeThreshold = 0.5f;

    private void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        WasGrabbedByPlayer.Clear();
        SlugDeets.Clear();
    }

    private void RainWorld_Update(On.RainWorld.orig_Update orig, RainWorld self)
    {
        try
        {
            orig(self);
        }
        catch (Exception e)
        {
            Logger.LogError(e);
            throw;
        }
    }
    
}
