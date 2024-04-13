using RideableLizards.LizardThings;
using RideableLizards.PlayerThings;

namespace RideableLizards;

public static class Hooks
{
    public static void Apply()
    {
        On.LizardAI.Update += LizardMovement.Update;
        On.FriendTracker.RunSpeed += LizardMovement.RunSpeed;

        On.Player.Grabability += LizardGrabability.PlayerOnGrabability;
        On.Player.IsCreatureLegalToHoldWithoutStun += LizardGrabability.PlayerOnIsCreatureLegalToHoldWithoutStun;
        On.Creature.Grab += LizardGrabability.CreatureOnGrab;
    }
}