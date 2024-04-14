using System.Linq;
using RideableLizards.PlayerThings;
using RWCustom;
using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardMovement
{
    public static void LizardOnAct(On.Lizard.orig_Act orig, Lizard self)
    {
        orig(self);

        if (!self.TryGetLizardData(out var lizardData))
            return;
        if (lizardData.Rider == null || !lizardData.Rider.TryGetPlayerData(out var playerData))
            return;

        if (playerData.JumpCounter > 0)
        {
            if (playerData.LastJumpCounter == 0)
            {
                //todo handle rocketjump sound start here?

                if (self.jumpModule == null) //Standard lizards get instant response
                {
                    Jump(lizardData);
                }
            }
            //after
        }
        else if (playerData.LastJumpCounter != 0)
        {
            if (self.jumpModule != null)
            {
                if (playerData.LastJumpCounter < 10) //Cyan lizards will have small delay, but should be good enough
                {
                    Jump(lizardData);
                }
                else
                {
                    //Rocket jump
                }
            }
            //todo end rocketjump here?
        }
    }

    public static void Jump(LizardData lizardData)
    {
        var self = (Lizard)lizardData.Owner.realizedCreature;
        var aimPos = lizardData.Rider.input[0].analogueDir;
        if (self.LegsGripping <= 0) return;

        lizardData.Jumping = true;
        for (var i = 0; i < self.bodyChunks.Length; i++)
        {
            self.bodyChunks[i].pos += aimPos * ((self.bodyChunks.Length - i + 1f) / self.bodyChunks.Length) * 5f;
            self.bodyChunks[i].vel += aimPos * ((self.bodyChunks.Length - i + 1f) / self.bodyChunks.Length) * 12.5f; //15f?
        }
    }

    public static void LizardGraphicsOnUpdate(On.LizardGraphics.orig_Update orig, LizardGraphics self)
    {
        orig(self);

        if (!self.lizard.TryGetLizardData(out var lizardData))
            return;
        if (lizardData.Rider == null || !lizardData.Rider.TryGetPlayerData(out var playerData))
            return;

        if (lizardData.Jumping)
        {
            self.legsGrabbing = 0;
            self.frontLegsGrabbing = 0;
            self.hindLegsGrabbing = 0;
            self.noGripCounter = LizardData.JumpLinger;
        }
    }

    public static void LizardAIOnUpdate(On.LizardAI.orig_Update orig, LizardAI ai)
    {
        if (ai.lizard.TryGetLizardData(out var lizardData) && lizardData.Rider != null && ai.lizard.room != null)
        {
            if (ai.creature.realizedCreature.room.Tiles != null && !ai.pathFinder.DoneMappingAccessibility) //Shamelessly stealing this from Rain Meadow
                ai.pathFinder.accessibilityStepsPerFrame = ai.creature.realizedCreature.room.Tiles.Length; //Still not perfect, but better than vanilla
            else ai.pathFinder.accessibilityStepsPerFrame = 10;

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
        var lizard = (Lizard)lizardData.Owner.realizedCreature;
        var lizardGraphics = (LizardGraphics)lizard.graphicsModule;
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

        //Help grabbing vertical poles
        if (rider.input[0].y >= 1 && rider.input[0].x == 0 && room.Tiles != null)
        {
            if (!lizardGraphics.limbs.All(x => room.GetTile(room.GetTilePosition(x.grabPos)).verticalBeam))
            {
                var verticalPoleTiles = room.Tiles.Cast<Room.Tile>().Where(x => x.verticalBeam &&
                    Custom.DistLess(room.GetWorldCoordinate(new IntVector2(x.X, x.Y)), lizard.abstractCreature.pos, 2f)).ToArray();
                if (verticalPoleTiles.Any())
                {
                    var chosenOne = verticalPoleTiles.OrderBy(x => Custom.Dist(new Vector2(x.X, x.Y), lizard.abstractCreature.pos.Tile.ToVector2())).First();

                    for (var i = 2; i > 0; i--)
                    {
                        var destPos = room.GetWorldCoordinate(new IntVector2(chosenOne.X, chosenOne.Y + i));
                        if (Accesible(destPos))
                            return destPos;
                    }
                }
            }
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