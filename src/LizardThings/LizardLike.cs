using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardLike
{
    public const float LikeThreshold = 0.5f;
    public static bool LikesPlayer(this Lizard liz, Player player)
    {
        return LikeOfPlayer(liz, player) > LikeThreshold;
    }
    public static float LikeOfPlayer(this Lizard liz, Player player)
    {
        return liz.AI.LikeOfPlayer(liz.AI.tracker.RepresentationForCreature(player.abstractCreature, false));
    }
    public static float LikeOfPlayerRaw(this Lizard liz, Player player)
    {
        return liz.State.socialMemory.GetLike(player.abstractCreature.ID);
    }
    public static float TempLikeOfPlayer(this Lizard liz, Player player)
    {
        return liz.State.socialMemory.GetTempLike(player.abstractCreature.ID);
    }

    //Heart particles
    public static void LizardAIOnGiftRecieved(On.LizardAI.orig_GiftRecieved orig, LizardAI self, SocialEventRecognizer.OwnedItemOnGround giftofferedtome)
    {
        orig(self, giftofferedtome);

        if (giftofferedtome.owner is Player player)
        {
            var likeOfPlayer = self.lizard.LikeOfPlayerRaw(player);
            
            if (likeOfPlayer > LikeThreshold && self.lizard.TempLikeOfPlayer(player) > LikeThreshold)
            {
                var howMany = Mathf.FloorToInt(likeOfPlayer * 10f) - 4;
                if (likeOfPlayer > 0.95f)
                    howMany = 10;
                for (var i = 0; i < howMany; i++)
                {
                    self.lizard.room?.AddObject(new LizardHeart(self.lizard.graphicsModule as LizardGraphics));
                }
            }
        }
    }
}