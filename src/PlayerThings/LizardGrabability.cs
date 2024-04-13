using RideableLizards.LizardThings;

namespace RideableLizards.PlayerThings;

public static class LizardGrabability
{
    public static Player.ObjectGrabability PlayerOnGrabability(On.Player.orig_Grabability orig, Player self, PhysicalObject obj)
    {
        if (obj is Lizard liz && liz.LikesPlayer(self))
            return Player.ObjectGrabability.TwoHands;

        return orig(self, obj);
    }

    public static bool PlayerOnIsCreatureLegalToHoldWithoutStun(On.Player.orig_IsCreatureLegalToHoldWithoutStun orig, Player self, Creature grabcheck)
    {
        if (grabcheck is Lizard liz && liz.LikesPlayer(self))
            return true;

        return orig(self, grabcheck);
    }

    public static bool CreatureOnGrab(On.Creature.orig_Grab orig, Creature self, PhysicalObject obj, int graspused, int chunkgrabbed, Creature.Grasp.Shareability shareability, float dominance, bool overrideequallydominant, bool pacifying)
    {
        if (self is Player player && obj is Lizard liz && liz.LikesPlayer(player))
        {
            shareability = Creature.Grasp.Shareability.NonExclusive; //Other players able to grab lizard
            pacifying = false; //Don't stun the lizard on grab
        }

        return orig(self, obj, graspused, chunkgrabbed, shareability, dominance, overrideequallydominant, pacifying);
    }

}