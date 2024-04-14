using System.Linq;
using RWCustom;
using UnityEngine;

namespace RideableLizards.LizardThings;

public static class LizardShortcutGraphics
{
    public static void ShortcutGraphicsOnDraw(On.ShortcutGraphics.orig_Draw orig, ShortcutGraphics self, float timeStacker, Vector2 camPos)
    {
        orig(self, timeStacker, camPos);

        var show = self.room.abstractRoom.entities.Any(e => e is AbstractCreature crit && crit.TryGetLizardData(out var lizData) && lizData.Rider != null);
        if (ModManager.MSC && !show) return; //MSC disables their visibility by default, I think

        for (var i = 0; i < self.room.shortcuts.Length; i++)
        {
            var shortcut = self.room.shortcuts[i];
            if (shortcut.shortCutType == ShortcutData.Type.NPCTransportation)
            {
                self.entranceSprites[i, 0].isVisible = show;
            }
            if (shortcut.shortCutType == ShortcutData.Type.RegionTransportation)
            {
                self.entranceSprites[i, 0].isVisible = show;
                if (show) self.entranceSprites[i, 0].color = Color.Lerp(self.entranceSprites[i, 0].color, Color.cyan, 0.5f);
            }
        }
    }

    public static void ShortcutGraphicsOnGenerateSprites(On.ShortcutGraphics.orig_GenerateSprites orig, ShortcutGraphics self)
    {
        orig(self);
        if (ModManager.MSC) return; //Will be using MSC sprites instead

        for (var i = 0; i < self.room.shortcuts.Length; i++)
        {
            var shortcut = self.room.shortcuts[i];
            if (shortcut.shortCutType == ShortcutData.Type.NPCTransportation)
            {
                if (self.entranceSprites[i, 0] != null)
                    self.camera.ReturnFContainer("Shortcuts").RemoveChild(self.entranceSprites[i, 0]);
                self.entranceSprites[i, 0] = new FSprite("Kill_Standard_Lizard") { scale = 0.65f };
                self.entranceSpriteLocations[i] = self.room.MiddleOfTile(self.room.shortcuts[i].StartTile) + IntVector2.ToVector2(self.room.ShorcutEntranceHoleDirection(self.room.shortcuts[i].StartTile)) * 15f;
                self.camera.ReturnFContainer("Shortcuts").AddChild(self.entranceSprites[i, 0]);
            }
            else if (shortcut.shortCutType == ShortcutData.Type.RegionTransportation)
            {
                if (self.entranceSprites[i, 0] != null)
                    self.camera.ReturnFContainer("Shortcuts").RemoveChild(self.entranceSprites[i, 0]);
                self.entranceSprites[i, 0] = new FSprite("Kill_White_Lizard") { scale = 0.65f };
                self.entranceSpriteLocations[i] = self.room.MiddleOfTile(self.room.shortcuts[i].StartTile) + IntVector2.ToVector2(self.room.ShorcutEntranceHoleDirection(self.room.shortcuts[i].StartTile)) * 15f;
                self.camera.ReturnFContainer("Shortcuts").AddChild(self.entranceSprites[i, 0]);
            }
        }
    }
}