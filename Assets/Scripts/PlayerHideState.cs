using System;
using UnityEngine;

public static class PlayerHideState
{
    public static event Action<bool> OnHiddenStateChanged;

    public static bool IsPlayerHidden { get; private set; }
    public static Vector3 LastHiddenPosition { get; private set; }

    public static void SetHidden(bool isHidden, Vector3 hiddenPosition)
    {
        if (IsPlayerHidden == isHidden)
        {
            LastHiddenPosition = hiddenPosition;
            return;
        }

        IsPlayerHidden = isHidden;
        LastHiddenPosition = hiddenPosition;
        OnHiddenStateChanged?.Invoke(IsPlayerHidden);
    }
}