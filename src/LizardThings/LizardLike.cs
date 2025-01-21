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

    //Heart particles
    public static void LizardAIOnGiftRecieved(On.LizardAI.orig_GiftRecieved orig, LizardAI self, SocialEventRecognizer.OwnedItemOnGround giftofferedtome)
    {
        orig(self, giftofferedtome);

        if (giftofferedtome.owner is Player player)
        {
            var likeOfPlayer = self.lizard.LikeOfPlayer(player);

            UnityEngine.Debug.Log($"Like: {likeOfPlayer}");

            if (likeOfPlayer > LikeThreshold)
            {
                for (var i = 0; i < likeOfPlayer * 10; i++)
                {
                    self.lizard.room?.AddObject(new LizardHeart(self.lizard.graphicsModule as LizardGraphics));
                }
            }
        }
    }
}