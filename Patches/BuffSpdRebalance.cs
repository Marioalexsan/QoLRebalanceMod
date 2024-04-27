using HarmonyLib;
using SoG;

namespace Marioalexsan.GrindeaQoL;

[HarmonyPatch]
internal static class BuffSpdRebalance
{
    [HarmonyPatch(typeof(_Spells_BuffSPDInstance), nameof(_Spells_BuffSPDInstance.Spawn))]
    [HarmonyPostfix]
    private static void AddMSPDBuff(_Spells_BuffSPDInstance __instance)
    {
        float baseSpeed = 2f;

        if (CAS.NetworkRole != NetworkHelperInterface.NetworkRole.Client)
        {
            __instance.xBuffedPlayer.xEntity.xBaseStats.AddStatusEffect(QoLRebalanceMod.Instance.HasteMSPD, new BaseStats.EBuffFloat(__instance.iDuration, __instance.fBuffPower / 100 * 3 * baseSpeed, EquipmentInfo.StatEnum.FlatMoveSpeed, bDeBuff: false));
        }
    }
}
