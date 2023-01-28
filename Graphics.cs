using System.Linq;
using UnityEngine;

namespace RideableLizards;

public partial class RideableLizards
{
    private void ShortcutGraphicsOnDraw(On.ShortcutGraphics.orig_Draw orig, ShortcutGraphics self, float timeStacker, Vector2 camPos)
    {
        orig(self, timeStacker, camPos);
        
        if (!SlugDeets.Any())
            return;

        for (var k = 0; k < self.entranceSprites.GetLength(0); k++)
        {
            if (self.entranceSprites[k, 0] != null)
            {
                if (self.room.shortcuts[k].shortCutType == ShortcutData.Type.CreatureHole)
                {
                    self.entranceSprites[k, 0].isVisible = true;
                }

                if (self.entranceSprites[k, 0].element.name == "ShortcutDoubleArrow")
                {
                    self.entranceSprites[k, 0].isVisible = true;
                }

                if (self.entranceSprites[k, 0].element.name == "ShortcutTransportArrow")
                {
                    self.entranceSprites[k, 0].isVisible = true;
                }
            }
        }
    }
}