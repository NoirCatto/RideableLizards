namespace RideableLizards.LizardThings;

public static class LizardLike
{
    private const float LikeThreshold = 0.5f;
    public static bool LikesPlayer(this Lizard liz, Player player)
    {
        return liz.AI.LikeOfPlayer(liz.AI.tracker.RepresentationForCreature(player.abstractCreature, false)) > LikeThreshold;
    }
}