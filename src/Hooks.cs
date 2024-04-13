using RideableLizards.LizardThings;
using RideableLizards.PlayerThings;

namespace RideableLizards;

public static class Hooks
{
    public static void Apply()
    {
        On.Lizard.Update += LizardOnUpdate;
        On.Lizard.AttemptBite += LizardBehaviors.LizardOnAttemptBite;
        On.LizardAI.Update += LizardMovement.LizardAIOnUpdate;
        On.FriendTracker.RunSpeed += LizardMovement.FriendTrackerOnRunSpeed;
        On.FriendTracker.Utility += LizardBehaviors.FriendTrackerOnUtility;

        On.Player.Update += PlayerOnUpdate;
        On.Player.Grabability += LizardGrabability.PlayerOnGrabability;
        On.Player.IsCreatureLegalToHoldWithoutStun += LizardGrabability.PlayerOnIsCreatureLegalToHoldWithoutStun;
        On.Creature.Grab += LizardGrabability.CreatureOnGrab;
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