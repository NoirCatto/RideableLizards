using RideableLizards.LizardThings;
using RideableLizards.PlayerThings;
using UnityEngine;

namespace RideableLizards;

public static class Hooks
{
    public static void Apply()
    {
        On.Lizard.Update += LizardOnUpdate;
        On.Lizard.AttemptBite += LizardBehaviors.LizardOnAttemptBite;
        On.Lizard.Act += LizardMovement.LizardOnAct;
        On.Lizard.ActAnimation += LizardAttacks.LizardOnActAnimation;
        IL.Lizard.ActAnimation += LizardAttacks.LizardILActAnimation;
        On.Lizard.EnterAnimation += LizardAttacks.LizardOnEnterAnimation;
        IL.LizardTongue.LashOut += LizardAttacks.LizardTongueILLashOut;
        On.LizardGraphics.Update += LizardMovement.LizardGraphicsOnUpdate;
        On.LizardAI.Update += (orig, self) =>
        {
            LizardMovement.LizardAIOnUpdate(self);
            orig(self);
            LizardAttacks.LizardAIOnUpdate(self);
        };
        On.LizardAI.LizardSpitTracker.Update += LizardAttacks.LizardSpitTrackerOnUpdate;
        On.LizardAI.LizardSpitTracker.AimPos += LizardAttacks.LizardSpitTrackerOnAimPos;
        On.LizardAI.DetermineBehavior += LizardBehaviors.LizardAIOnDetermineBehavior;
        On.FriendTracker.RunSpeed += LizardMovement.FriendTrackerOnRunSpeed;
        On.FriendTracker.Utility += LizardBehaviors.FriendTrackerOnUtility;
        On.ShortcutGraphics.Draw += LizardShortcutGraphics.ShortcutGraphicsOnDraw;
        On.ShortcutGraphics.GenerateSprites += LizardShortcutGraphics.ShortcutGraphicsOnGenerateSprites;
        On.LizardAI.GiftRecieved += LizardLike.LizardAIOnGiftRecieved;

        On.Player.Update += PlayerOnUpdate;
        On.Player.Grabability += LizardGrabability.PlayerOnGrabability;
        On.Player.IsCreatureLegalToHoldWithoutStun += LizardGrabability.PlayerOnIsCreatureLegalToHoldWithoutStun;
        On.Player.IsObjectThrowable += LizardGrabability.PlayerOnIsObjectThrowable;
        On.Creature.Grab += LizardGrabability.CreatureOnGrab;
        On.Player.Tongue.Shoot += Fixes.TongueOnShoot;
        On.AbstractCreature.IsEnteringDen += Fixes.AbstractCreatureOnIsEnteringDen;
    }

    private static void LizardOnUpdate(On.Lizard.orig_Update orig, Lizard self, bool eu)
    {
        orig(self, eu);
        if (self.TryGetLizardData(out var lizardData))
            lizardData.Update(eu);
    }

    private static void PlayerOnUpdate(On.Player.orig_Update orig, Player self, bool eu)
    {
        orig(self, eu);
        if (self.TryGetPlayerData(out var playerData))
            playerData.Update(eu);
    }
}