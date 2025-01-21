using RWCustom;
using UnityEngine;

namespace RideableLizards.LizardThings;

public class LizardHeart : CosmeticSprite
{
    public static void LoadAtlases()
    {
        Futile.atlasManager.LoadAtlas("Atlases/RideableLizards_Heart");
    }

    public LizardGraphics LizGraphics;
    public Color HeartColor;
    private int lifeTime;

    public LizardHeart(LizardGraphics lizardGraphics)
    {
        LizGraphics = lizardGraphics;
        this.pos = lizardGraphics.head.pos;
        vel = Custom.rotateVectorDeg(new Vector2(0f, Random.Range(5f, 10f)), Random.Range(-30, 31));

        Color.RGBToHSV(lizardGraphics.effectColor, out var H, out var S, out var V);
        HeartColor = Color.HSVToRGB(H + Random.Range(-0.05f, 0.05f), S, V);

        lifeTime = 5 * 40; //40 ticks is one second in RW
    }

    public override void Update(bool eu)
    {
        if (lifeTime <= 0 || this.vel is { x: <= 0.1f, y: <= 0.1f }) this.Destroy();
        lifeTime--;
        this.vel *= 0.9f;

        base.Update(eu);
    }

    public override void InitiateSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam)
    {
        sLeaser.sprites = new FSprite[1];
        sLeaser.sprites[0] = new FSprite("RideableLizards_Heart");
        var spr = sLeaser.sprites[0];
        spr.color = HeartColor;
        this.AddToContainer(sLeaser, rCam, null);
    }

    public override void DrawSprites(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, float timeStacker, Vector2 camPos)
    {
        var spr = sLeaser.sprites[0];
        spr.SetPosition(Vector2.Lerp(this.lastPos, this.pos, timeStacker) - camPos);

        //spr.rotation = 0f;
        //spr.anchorY = 0.5f;

        base.DrawSprites(sLeaser, rCam, timeStacker, camPos);
    }

    public override void AddToContainer(RoomCamera.SpriteLeaser sLeaser, RoomCamera rCam, FContainer newContatiner)
    {
        if (newContatiner == null)
        {
            newContatiner = rCam.ReturnFContainer("Midground");
        }

        foreach (var spr in sLeaser.sprites)
        {
            spr.RemoveFromContainer();
            newContatiner.AddChild(spr);
        }
    }
}