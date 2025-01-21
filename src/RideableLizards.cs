using System;
using System.Security;
using System.Security.Permissions;
using BepInEx;
using BepInEx.Logging;
using RideableLizards.LizardThings;

#pragma warning disable CS0618

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace RideableLizards;

[BepInPlugin("NoirCat.RideableLizards", "Rideable Lizards", "3.0.0")]
public class RideableLizards : BaseUnityPlugin
{
    public static ManualLogSource LogSource;

    public void OnEnable()
    {
        On.RainWorld.OnModsInit += RainWorldOnOnModsInit;
        LogSource = Logger;
    }

    private bool IsInit;
    private void RainWorldOnOnModsInit(On.RainWorld.orig_OnModsInit orig, RainWorld self)
    {
        orig(self);

        try
        {
            if (IsInit) return;
            IsInit = true;

            Hooks.Apply();
            LizardHeart.LoadAtlases();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex);
        }
    }

}
