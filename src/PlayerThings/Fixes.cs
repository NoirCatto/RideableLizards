using System.Linq;
using RideableLizards.LizardThings;
using UnityEngine;

namespace RideableLizards.PlayerThings;

public static class Fixes
{
    public static void AbstractCreatureOnIsEnteringDen(On.AbstractCreature.orig_IsEnteringDen orig, AbstractCreature self, WorldCoordinate den)
    {
        if (self.realizedCreature is Player player && player.grasps.Any(x => x.grabbed is Lizard liz && liz.LikesPlayer(player)))
            return; //Fix player killing a lizard upon entering creature den

        orig(self,den);
    }

    //Stop Saint from shooting their tongue while on a lizard
    public static void TongueOnShoot(On.Player.Tongue.orig_Shoot orig, Player.Tongue self, Vector2 dir)
    {
        if (self.player.TryGetPlayerData(out var data) && data.RidingALizard)
        {
            if (self.Attached)
                self.Release();
            return;
        }

        orig(self, dir);
    }
}