using UnityEngine;

public static class SessionPlaytime
{
    private static float restoredSeconds;
    private static float startRealtime = -1f;

    public static float GetSeconds()
    {
        if (startRealtime < 0f)
        {
            startRealtime = Time.realtimeSinceStartup;
        }

        return restoredSeconds + Mathf.Max(0f, Time.realtimeSinceStartup - startRealtime);
    }

    public static void RestoreFromSave(float seconds)
    {
        restoredSeconds = Mathf.Max(0f, seconds);
        startRealtime = Time.realtimeSinceStartup;
    }

    public static void Reset()
    {
        restoredSeconds = 0f;
        startRealtime = Time.realtimeSinceStartup;
    }
}
