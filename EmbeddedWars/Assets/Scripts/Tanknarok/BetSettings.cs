// Assets/Scripts/BetSettings.cs
using System;
using System.Globalization;
using UnityEngine;

public static class BetSettings
{
    public const string ModeCasual  = "Casual";
    public const string ModeBetting = "Betting";

    private static string _mode = ModeCasual; // "Casual" | "Betting"
    private static float  _betAmount = 0f;    // Solo se usa en "Betting"

    public static string Mode => _mode;

    /// En "Casual" siempre devuelve 0.5f; en "Betting" devuelve _betAmount.
    public static float GetEffectiveBetAmount() =>
        string.Equals(_mode, ModeCasual, StringComparison.InvariantCultureIgnoreCase) ? 0.5f : _betAmount;

    public static void SetMode(string mode)
    {
        var m = (mode ?? string.Empty).Trim();

        if (string.Equals(m, ModeBetting, StringComparison.InvariantCultureIgnoreCase))
        {
            _mode = ModeBetting; // preserva capitalización
        }
        else if (string.Equals(m, ModeCasual, StringComparison.InvariantCultureIgnoreCase))
        {
            _mode = ModeCasual;  // preserva capitalización
        }
        else
        {
            _mode = ModeCasual;
            Debug.LogWarning($"BetSettings.SetMode: valor desconocido '{mode}', usando '{ModeCasual}'.");
        }

        Debug.Log($"🎮 BetSettings.Mode = {_mode} (efectivo bet={GetEffectiveBetAmount()})");
    }

    public static void SetBetAmount(float amount)
    {
        _betAmount = amount < 0f ? 0f : amount;
        Debug.Log($"💰 BetSettings.BetAmount = {_betAmount} (efectivo={GetEffectiveBetAmount()})");
    }

    public static void SetBetAmount(string amountStr)
    {
        if (float.TryParse(amountStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var val))
            SetBetAmount(val);
        else
            Debug.LogWarning($"BetSettings.SetBetAmount parse fail: '{amountStr}'");
    }
}
