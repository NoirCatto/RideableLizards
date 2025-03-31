using System.Linq;
using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardBehaviors
{
    public static LizardAI.Behavior LizardAIOnDetermineBehavior(On.LizardAI.orig_DetermineBehavior orig, LizardAI self)
    {
        var behaviour = orig(self);
        
        if (self.lizard.TryGetLizardData(out var data) && data.Rider != null &&
            behaviour == LizardAI.Behavior.ReturnPrey && self.friendTracker?.friend != null &&
            data.ReturnPreyDelay > 0)
        {
            behaviour = LizardAI.Behavior.FollowFriend; //Frick you, no eating, only walk
        }

        return behaviour;
    }


    public static float FriendTrackerOnUtility(On.FriendTracker.orig_Utility orig, FriendTracker self)
    {
        var result = orig(self);

        if (self.creature is Lizard liz && liz.TryGetLizardData(out var lizardData))
        {
            if (lizardData.Rider != null)
            {
                var friendBonus =  Mathf.Pow( Mathf.Clamp01(liz.LikeOfPlayer(lizardData.Rider)), 2.5f);
                result += result + (friendBonus * 0.55f);
            }
        }

        return result;
    }

    public static void LizardOnAttemptBite(On.Lizard.orig_AttemptBite orig, Lizard self, Creature creature)
    {
        if (creature is Player player && self.LikesPlayer(player))
            return;

        if (self.TryGetLizardData(out var data))
        {
            if (data.Rider != null && !data.Rider.input[0].pckp)
                return;
        }

        var graspNull = self.grasps[0] == null;

        orig(self, creature);

        if (graspNull && self.grasps[0] != null && data != null)
            data.BitCreature = true;
    }
}