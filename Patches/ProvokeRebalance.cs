using HarmonyLib;
using ModBagman;
using SoG;
using static SoG.SpellVariable;

namespace Marioalexsan.GrindeaQoL;

[HarmonyPatch]
internal static class ProvokeRebalance
{
    [HarmonyPatch(typeof(EnemyCodex), nameof(EnemyCodex.GetEnemyInstance))]
    [HarmonyPostfix]
    public static void RemoveTauntImmunity(Enemy __result)
    {
        if (__result.xBaseStats.bTauntImmune)
        {
            __result.xBaseStats.bTauntImmune = false;
            __result.xBaseStats.bAnnoyInsteadOfTaunt = true;
        }
    }

    private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

    public static void Init()
    {
        _editor.SetValue(Handle.Spells_Utility_Taunt_EPCost, 20);
        _editor.SetValue(Handle.Spells_Utility_Taunt_BaseDurationInSeconds, 6);
        _editor.SetValue(Handle.Spells_Utility_Taunt_PerLevelDurationInSeconds, 2);
        _editor.SetValue(Handle.Spells_Utility_Taunt_EffectRange, 150);
        _editor.SetValue(Handle.Spells_Utility_Taunt_LeashRange, 999);
        _editor.SetValue(Handle.Spells_Utility_Taunt_BaseMaxTargets, 999);
    }

    public static void CleanupMethod()
    {
        _editor.RestoreValues();
    }
}
