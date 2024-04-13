using System.Collections.Generic;

namespace RideableLizards;

public static class Log
{
    public static void Info(object data) => RideableLizards.LogSource.LogInfo(data);
    public static void Error(object data) => RideableLizards.LogSource.LogError(data);
    public static void Warning(object data) => RideableLizards.LogSource.LogWarning(data);
}