using System.Linq;
using RWCustom;
using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardMovement
{
    public static void LizardAIOnUpdate(On.LizardAI.orig_Update orig, LizardAI ai)
    {
        if (ai.lizard.TryGetLizardData(out var lizardData) && lizardData.Rider != null)
        {
            if (ai.creature.realizedCreature.room?.Tiles != null && !ai.pathFinder.DoneMappingAccessibility) //Shamelessly stealing this from Rain Meadow
                ai.pathFinder.accessibilityStepsPerFrame = ai.creature.realizedCreature.room.Tiles.Length; //Still not perfect, but better than vanilla
            else ai.pathFinder.accessibilityStepsPerFrame = 10;

            if (ai.lizard.room != null)
                ai.friendTracker.friendDest = GetDestination(lizardData);
        }

        orig(ai);
    }

    public static float FriendTrackerOnRunSpeed(On.FriendTracker.orig_RunSpeed orig, FriendTracker self)
    {
        var result = orig(self);

        if (self.creature is Lizard liz && liz.TryGetLizardData(out var lizardData) && lizardData.Rider != null)
        {
            if (lizardData.Rider.input[0].analogueDir.magnitude != 0)
            {
                result = Custom.LerpAndTick(((LizardAI)self.AI).runSpeed, lizardData.Rider.input[0].analogueDir.magnitude, 0.25f, 0.05f);
            }
        }

        return result;
    }

    public const int TilesAwayToCheck = 10;
    public static WorldCoordinate GetDestination(LizardData lizardData, WorldCoordinate? fromSlopeDest = null)
    {
        var lizard = lizardData.Owner.realizedCreature;
        var rider = lizardData.Rider;
        var room = lizard.room;

        bool Accesible(WorldCoordinate pos) => room.aimap.TileAccessibleToCreature(pos.Tile, lizard.Template);

        var aimPos = lizard.abstractCreature.pos;
        aimPos.x += rider.input[0].x;
        aimPos.y += rider.input[0].y;

        //Direct into shortcut
        var shortcutsCloseBy = room.shortcuts.Where(x => Custom.DistLess(x.destinationCoord, aimPos, 2f) ||
                        (x.shortCutType == ShortcutData.Type.RoomExit && Custom.DistLess(x.destinationCoord, aimPos, 4f))).ToArray();
        if (shortcutsCloseBy.Any())
        {
            var chosenOne = shortcutsCloseBy.OrderBy(x => Custom.Dist(x.destinationCoord.Tile.ToVector2(), aimPos.Tile.ToVector2())).First();
            return chosenOne.destinationCoord;
        }

        //Help with slope movement
        if (fromSlopeDest == null)
        {
            for (var i = 0; i < 3; i++)
            {
                var slopePos = WorldCoordinate.AddIntVector(lizardData.Owner.pos, new IntVector2(-i, 0));
                if (room.IdentifySlope(slopePos.Tile) != Room.SlopeDirection.Broken)
                {
                    return GetDestinationSlope(lizardData, slopePos);
                }
            }
        }

        //Try finding valid path
        for (var i = 2; i < TilesAwayToCheck; i++)
        {
            var destPos = fromSlopeDest ?? lizard.abstractCreature.pos;
            destPos.x += (int)(rider.input[0].analogueDir.x * i);
            destPos.y += (int)(rider.input[0].analogueDir.y * i);

            if (Accesible(destPos))
                return destPos;

            for (var j = 1; j < 5; j++)
            {
                if (Accesible(WorldCoordinate.AddIntVector(destPos, new IntVector2(0, -j))))
                    return WorldCoordinate.AddIntVector(destPos, new IntVector2(0, -j));

                if (Accesible(WorldCoordinate.AddIntVector(destPos, new IntVector2(0, j))))
                    return WorldCoordinate.AddIntVector(destPos, new IntVector2(0, j));
            }
        }

        var fallbackPos = lizard.abstractCreature.pos;
        fallbackPos.x += (int)(rider.input[0].analogueDir.x * 3f);
        fallbackPos.y += (int)(rider.input[0].analogueDir.y * 3f);
        return fallbackPos;
    }

    public static WorldCoordinate GetDestinationSlope(LizardData lizardData, WorldCoordinate slopePos)
    {
        var lizard = lizardData.Owner.realizedCreature;
        var rider = lizardData.Rider;
        var room = lizard.room;

        var destPos = slopePos;
        destPos.x += (int)(rider.input[0].analogueDir.x * 3);
        destPos.y += (int)(rider.input[0].analogueDir.y * 3);

        var dirVec = new Vector2(destPos.x - slopePos.x, destPos.y - slopePos.y).normalized;

        var nextDest = slopePos;
        var i = 1;
        while (room.IdentifySlope(nextDest.Tile) != Room.SlopeDirection.Broken && i < TilesAwayToCheck)
        {
            nextDest = WorldCoordinate.AddIntVector(nextDest, new IntVector2((int)dirVec.x, 0));
            if (room.IdentifySlope(nextDest.Tile) == Room.SlopeDirection.Broken)
                nextDest = WorldCoordinate.AddIntVector(nextDest, new IntVector2(0, (int)dirVec.y));
            if (room.IdentifySlope(nextDest.Tile) == Room.SlopeDirection.Broken)
                nextDest = WorldCoordinate.AddIntVector(nextDest, new IntVector2(0, (int)dirVec.y * -2));
            i++;
        }

        return GetDestination(lizardData, nextDest);
    }
}