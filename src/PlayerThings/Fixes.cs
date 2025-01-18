using System.Linq;
using RideableLizards.LizardThings;

namespace RideableLizards.PlayerThings;

public static class Fixes
{
    public static void AbstractCreatureOnIsEnteringDen(On.AbstractCreature.orig_IsEnteringDen orig, AbstractCreature self, WorldCoordinate den)
    {
        if (self.realizedCreature is Player player && player.grasps.Any(x => x.grabbed is Lizard liz && liz.LikesPlayer(player)))
            return; //Fix player killing a lizard upon entering creature den

        orig(self,den);
    }

}