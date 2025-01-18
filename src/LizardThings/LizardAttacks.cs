using System;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MoreSlugcats;
using RWCustom;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RideableLizards.LizardThings;

public static class LizardAttacks
{
    public static void LizardAIOnUpdate(LizardAI self)
    {
        if (!self.lizard.TryGetLizardData(out var data)) return;
        if (data.Rider == null) return;

        if (data.Rider.input[0].pckp && !data.BitCreature)
        {
            if (self.lizard.grasps[0] != null)
            {
                self.lizard.ReleaseGrasp(0);
                data.BitCreature = true;
            }

            self.lizard.biteDelay = 0;
            self.lizard.JawOpen += 0.2f;

            if (self.casualAggressionTarget != null && self.casualAggressionTarget.VisualContact &&
                Custom.DistLess(self.casualAggressionTarget.representedCreature.realizedCreature.mainBodyChunk.pos, self.lizard.mainBodyChunk.pos, self.lizard.lizardParams.attemptBiteRadius))
            {
                self.lizard.AttemptBite(self.casualAggressionTarget.representedCreature.realizedCreature);
            }
        }

        if (data.Rider.input[0].thrw && self.redSpitAI == null && self.lizard.tongue != null)
        {
            if (self.lizard.tongue.Ready)
            {
                if (self.lizard.grasps[0] != null)
                    self.lizard.ReleaseGrasp(0);

                self.lizard.EnterAnimation(Lizard.Animation.ShootTongue, false);
            }
        }
    }

    public static float LizardOnActAnimation(On.Lizard.orig_ActAnimation orig, Lizard self)
    {
        if (self.TryGetLizardData(out var data) && data.Rider != null)
        {
            if (self.animation == Lizard.Animation.ShootTongue)
            {
                self.bodyWiggleCounter = 0;
                self.jawOpen = 1f;

                if (self.AI.focusCreature != null && self.AI.focusCreature.VisualContact &&
                    (self.AI.focusCreature.representedCreature.realizedCreature is not Player targetSlug || !self.LikesPlayer(targetSlug)) &&
                    Custom.DistLess(self.mainBodyChunk.pos, self.AI.focusCreature.representedCreature.realizedCreature.mainBodyChunk.pos, self.lizardParams.tongueAttackRange))
                {
                    self.tongue.LashOut(self.AI.focusCreature.representedCreature.realizedCreature.bodyChunks[Random.Range(0, self.AI.focusCreature.representedCreature.realizedCreature.bodyChunks.Length)].pos);
                }
                else
                {
                    var aimPos = self.mainBodyChunk.pos;
                    if (data.Rider.input[0].x == 0 && data.Rider.input[0].y == 0)
                        aimPos += Custom.DegToVec(((LizardGraphics)self.graphicsModule).HeadRotation(0f)) * self.lizardParams.tongueAttackRange;
                    else
                        aimPos += data.Rider.input[0].analogueDir * self.lizardParams.tongueAttackRange;
                    self.tongue.LashOut(aimPos);
                }

                self.EnterAnimation(Lizard.Animation.Standard, true);

                return 0.2f;
            }
        }

        return orig(self);
    }

    public static void LizardOnEnterAnimation(On.Lizard.orig_EnterAnimation orig, Lizard self, Lizard.Animation anim, bool forceanimationchange)
    {
        if (self.TryGetLizardData(out var data) && data.Rider != null)
        {
            if (anim == Lizard.Animation.ShootTongue && !data.Rider.input[0].thrw)
                return;
        }

        orig(self, anim, forceanimationchange);
    }


    // IL_0069: ldloc.0      // vector2_1
    // IL_006a: call         float32 [UnityEngine.CoreModule]UnityEngine.Vector2::Dot(valuetype [UnityEngine.CoreModule]UnityEngine.Vector2, valuetype [UnityEngine.CoreModule]UnityEngine.Vector2)
    // IL_006f: ldc.r4       0.3
    // IL_0074: ble.un       IL_0368
    public static void LizardTongueILLashOut(ILContext il)
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<Vector2>(nameof(Vector2.Dot)),
                i => i.MatchLdcR4(0.3f),
                i => i.MatchBleUn(out label)
            );

            c.GotoPrev(MoveType.After, i => i.MatchLdcR4(0.3f));
            c.Remove(); //Forgive me modding God, for I have sinned...

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((float val1, float val2, LizardTongue self) =>
            {
                if (self.lizard.TryGetLizardData(out var data) && data.Rider != null)
                {
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brfalse, label);

        }
        catch (Exception ex)
        {
            RideableLizards.LogSource.LogError("RideableLizards - ILHook failed (LizardTongueILUpdate)");
            RideableLizards.LogSource.LogError(ex);
        }
    }

    public static void LizardSpitTrackerOnUpdate(On.LizardAI.LizardSpitTracker.orig_Update orig, LizardAI.LizardSpitTracker self)
    {
        orig(self);
        if (!self.lizardAI.lizard.TryGetLizardData(out var data)) return;
        if (data.Rider == null) return;

        if (data.Rider.input[0].thrw)
        {
            self.spitFromPos = self.AI.creature.pos;
            self.spitting = true;
            self.wantToSpit = true;

            if (self.lizardAI.preyTracker.MostAttractivePrey != null)
            {
                self.wantToSpitAtPos = self.lizardAI.preyTracker.MostAttractivePrey.BestGuessForPosition();
            }
            else
            {
                var aimPos = self.lizardAI.creature.pos;
                aimPos.x += (int)(data.Rider.input[0].analogueDir.x * 100);
                aimPos.y += (int)(data.Rider.input[0].analogueDir.y * 100);
                self.wantToSpitAtPos = aimPos;
            }
        }
        else
        {
            self.spitting = false;
            self.wantToSpit = false;
        }
    }

    public static Vector2? LizardSpitTrackerOnAimPos(On.LizardAI.LizardSpitTracker.orig_AimPos orig, LizardAI.LizardSpitTracker self)
    {
        var result = orig(self);

        if (self.lizardAI.lizard.TryGetLizardData(out var data) && data.Rider != null)
        {
            if (result == null)
            {
                var aimPos = self.lizardAI.lizard.mainBodyChunk.pos + (data.Rider.input[0].analogueDir * 100);
                if (data.Rider.input[0].x == 0)
                    aimPos.x += data.Rider.flipDirection * 100;

                result = aimPos;
            }
        }

        return result;
    }


    // IL_08cf: ldfld        valuetype [UnityEngine.CoreModule]UnityEngine.Vector2 BodyChunk::pos
    // IL_08d4: call         valuetype [UnityEngine.CoreModule]UnityEngine.Vector2 RWCustom.Custom::DirVec(valuetype [UnityEngine.CoreModule]UnityEngine.Vector2, valuetype [UnityEngine.CoreModule]UnityEngine.Vector2)
    // IL_08d9: call         float32 [UnityEngine.CoreModule]UnityEngine.Vector2::Dot(valuetype [UnityEngine.CoreModule]UnityEngine.Vector2, valuetype [UnityEngine.CoreModule]UnityEngine.Vector2)
    // IL_08de: ldc.r4       0.3
    // IL_08e3: bgt.s        IL_08f0
    public static void LizardILActAnimation(ILContext il) //Red lizard spit fix
    {
        try
        {
            var c = new ILCursor(il);
            ILLabel label = null;

            c.GotoNext(MoveType.After,
                i => i.MatchCallOrCallvirt<Vector2>(nameof(Vector2.Dot)),
                i => i.MatchLdcR4(0.3f),
                i => i.MatchBgt(out label)
            );

            c.Emit(OpCodes.Ldarg_0);
            c.EmitDelegate((Lizard self) =>
            {
                if (self.TryGetLizardData(out var data) && data.Rider != null)
                {
                    if (self.grasps[0] != null)
                        self.ReleaseGrasp(0);
                    return true;
                }
                return false;
            });
            c.Emit(OpCodes.Brtrue, label);
        }
        catch (Exception ex)
        {
            RideableLizards.LogSource.LogError("RideableLizards - ILHook failed (LizardILActAnimation)");
            RideableLizards.LogSource.LogError(ex);
        }
    }

}