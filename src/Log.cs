using System.Collections.Generic;

namespace RideableLizards;

public static class Log
{
    public static void Info(object data) => RideableLizards.LogSource.LogInfo(data);
    public static void Error(object data) => RideableLizards.LogSource.LogError(data);

    public static readonly List<object> Messages = [];
    public static void Once(object data)
    {
        if (!Messages.Contains(data))
        {
            Messages.Add(data);
            Info(data);
        }
    }

    //--
    public static void Apply()
    {
        On.GameSession.ctor += GameSessionOnctor;
    }

    private static void GameSessionOnctor(On.GameSession.orig_ctor orig, GameSession self, RainWorldGame game)
    {
        orig(self, game);
        Messages.Clear();
    }
}