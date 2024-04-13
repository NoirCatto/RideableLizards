using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardMovement
{
    public static void Update(On.LizardAI.orig_Update orig, LizardAI ai)
    {
        if (ai.lizard.TryGetLizardData(out var lizardData) && lizardData.Rider != null)
        {
            ai.behavior = LizardAI.Behavior.FollowFriend;

            var destPos = lizardData.Rider.abstractCreature.pos;
            destPos.x += (int)(lizardData.Rider.input[0].analogueDir.x * 5f);
            destPos.y += (int)(lizardData.Rider.input[0].analogueDir.y * 5f);

            ai.friendTracker.friendDest = destPos;

        }

        orig(ai);
    }

    public static float RunSpeed(On.FriendTracker.orig_RunSpeed orig, FriendTracker self)
    {
        var result = orig(self);

        if (self.creature is Lizard liz && liz.TryGetLizardData(out var lizardData) && lizardData.Rider != null)
        {
            result *= 2f;
        }

        return result;
    }
}