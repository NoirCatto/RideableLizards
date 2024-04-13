using System.Linq;
using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardBehaviors
{
    public static float FriendTrackerOnUtility(On.FriendTracker.orig_Utility orig, FriendTracker self)
    {
        var result = orig(self);

        if (self.creature is Lizard liz && liz.TryGetLizardData(out var lizardData))
        {
            if (lizardData.Rider != null)
            {
                var friendBonus =  Mathf.Pow( Mathf.Clamp01(liz.LikeOfPlayer(lizardData.Rider)), 2.5f);
                result += result * friendBonus;
            }
        }

        return result;
    }

    public static void LizardOnAttemptBite(On.Lizard.orig_AttemptBite orig, Lizard self, Creature creature)
    {
        if (creature is Player player && self.LikesPlayer(player))
            return;

        orig(self, creature);
    }
}