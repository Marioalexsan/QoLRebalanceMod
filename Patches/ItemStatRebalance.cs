using HarmonyLib;
using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.Extensions.Logging;
using ModBagman;
using SoG;
using System.Collections.Generic;
using System.Reflection.Emit;
using static SoG.EquipmentInfo;
using static SoG.SpellVariable;

namespace Marioalexsan.GrindeaQoL;

[HarmonyPatch]
internal class ItemStatRebalance
{
    private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

    private static int AddAngelsThirstMATK(BaseStats baseStats, EquipmentInfo info)
    {
        int value = GetInt(Handle.Equipment_AngelsThirst_ATKPerCard) * CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iBaseMATK += value;
            baseStats._ichkBaseMATK += value * 2;
        }

        return value;
    }

    private static int RemoveAngelsThirstMATK(BaseStats baseStats, EquipmentInfo info)
    {
        int value = GetInt(Handle.Equipment_AngelsThirst_ATKPerCard) * CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iBaseMATK += value;
            baseStats._ichkBaseMATK += value * 2;
        }

        return value;
    }

    private static int AddAngelsThirstASPD(BaseStats baseStats, EquipmentInfo info)
    {
        int value = CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count / 3;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iAttackSPD += value;
        }

        return value;
    }

    private static int RemoveAngelsThirstASPD(BaseStats baseStats, EquipmentInfo info)
    {
        int value = CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count / 3;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iAttackSPD -= value;
        }

        return value;
    }

    private static int AddAngelsThirstCSPD(BaseStats baseStats, EquipmentInfo info)
    {
        int value = CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count / 2;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iCastSPD += value;
        }

        return value;
    }

    private static int RemoveAngelsThirstCSPD(BaseStats baseStats, EquipmentInfo info)
    {
        int value = CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count / 2;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iCastSPD -= value;
        }

        return value;
    }

    private static int AddAngelsThirstEP(BaseStats baseStats, EquipmentInfo info)
    {
        int value = CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count / 2;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iMaxEP += value;
            baseStats._ichkBaseMaxEP += value * 2;
        }

        return value;
    }

    private static int RemoveAngelsThirstEP(BaseStats baseStats, EquipmentInfo info)
    {
        int value = CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count / 2;

        if (baseStats != null && info != null && info.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            baseStats.iMaxEP -= value;
            baseStats._ichkBaseMaxEP -= value * 2;
        }

        return value;
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1._Cards_AddCardBonusToPlayer))]
    [HarmonyPrefix]
    private static void AddAngelsThirstStats(PlayerView xView, EnemyCodex.EnemyTypes enType)
    {
        if (enType != EnemyCodex.EnemyTypes.GhostShip_Obsession && !xView.xJournalInfo.henCardAlbum.Contains(enType))
        {
            if (xView.xEquipment.xBufferWeapon != null && xView.xEquipment.xBufferWeapon.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
            {
                xView.xEntity.xBaseStats.iBaseMATK += GetInt(Handle.Equipment_AngelsThirst_ATKPerCard);
                xView.xEntity.xBaseStats._ichkBaseMATK += GetInt(Handle.Equipment_AngelsThirst_ATKPerCard) * 2;

                if ((CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count + 1) % 3 == 0)
                    xView.xEntity.xBaseStats.iAttackSPD += 1;

                if ((CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count + 1) % 2 == 0)
                    xView.xEntity.xBaseStats.iCastSPD += 1;

                if ((CAS.LocalPlayer.xJournalInfo.henCardAlbum.Count + 1) % 2 == 0)
                {
                    xView.xEntity.xBaseStats.iMaxEP += 1;
                    xView.xEntity.xBaseStats._ichkBaseMaxEP += 1 * 2;
                }

            }
        }
    }

    [HarmonyPatch(typeof(BaseStats), nameof(BaseStats.AddStatBonus))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> AngelsThirstBuff(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var codeMatcher = new CodeMatcher(code, gen)
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iMaxEP))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_3),
                CodeInstruction.Call(() => AddAngelsThirstEP(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iBaseMATK))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_3),
                CodeInstruction.Call(() => AddAngelsThirstMATK(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iAttackSPD))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_3),
                CodeInstruction.Call(() => AddAngelsThirstASPD(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iCastSPD))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_3),
                CodeInstruction.Call(() => AddAngelsThirstCSPD(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .ThrowIfInvalid("What?!");

        return codeMatcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(BaseStats), nameof(BaseStats.RemoveEquipmentBonus))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> AngelsThirstBuffRemove(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var codeMatcher = new CodeMatcher(code, gen)
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iMaxEP))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                CodeInstruction.Call(() => RemoveAngelsThirstEP(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iBaseMATK))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                CodeInstruction.Call(() => RemoveAngelsThirstMATK(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iAttackSPD))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                CodeInstruction.Call(() => RemoveAngelsThirstASPD(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .MatchStartForward(new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(BaseStats), nameof(BaseStats.iCastSPD))))
            .ThrowIfInvalid("What?!")
            .Advance(-2)
            .Insert(
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                CodeInstruction.Call(() => RemoveAngelsThirstCSPD(default, default)),
                new CodeInstruction(OpCodes.Pop)
            )
            .ThrowIfInvalid("What?!");

        return codeMatcher.InstructionEnumeration();
    }

    [HarmonyPatch(typeof(EquipmentInfo), nameof(EquipmentInfo.GetItemComparison))]
    [HarmonyPostfix]
    private static void EditResultForAngelsThirst(EquipmentInfo xFrom, EquipmentInfo xTo, Dictionary<EquipmentInfo.StatEnum, int> __result)
    {
        if (xTo != null && (xFrom == null || xTo.enItemType != xFrom.enItemType))
        {
            if (xTo != null && xTo.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
            {
                __result[StatEnum.MATK] += AddAngelsThirstMATK(null, null);
                __result[StatEnum.ASPD] += AddAngelsThirstASPD(null, null);
                __result[StatEnum.CSPD] += AddAngelsThirstCSPD(null, null);
                __result[StatEnum.EP] += AddAngelsThirstEP(null, null);
            }
        }
        if (xFrom != null && xFrom.lenSpecialEffects.Contains(SpecialEffect._Unique_ExtraDamagePerCard))
        {
            __result[StatEnum.MATK] -= RemoveAngelsThirstMATK(null, null);
            __result[StatEnum.ASPD] -= RemoveAngelsThirstASPD(null, null);
            __result[StatEnum.CSPD] -= RemoveAngelsThirstCSPD(null, null);
            __result[StatEnum.EP] -= RemoveAngelsThirstEP(null, null);
        }
    }

    public static void Init()
    {
        Mod vanilla = QoLRebalanceMod.Instance.GetMod("SoG");

        // Lightning Glove
        _editor.SetValue(Handle.Equipment_LightningGlove_CooldownInFrames, 4);
        _editor.SetValue(Handle.Equipment_LightningGlove_MATKRatio, 0.2f);
        _editor.EditValueBy(Handle.Equipment_AngelsThirst_ATKPerCard, -1);

        // Skeleton Hand
        var skeleHand = vanilla.GetItem(ItemCodex.ItemTypes._OneHanded_SkeletonHand.ToString());
        skeleHand[StatEnum.MATK] += 15; // Bring up to 100 MATK

        // Angel's Thirst
        var angelsWorst = vanilla.GetItem(ItemCodex.ItemTypes._TwoHanded_AngelsThirst.ToString());
        angelsWorst[StatEnum.ATK] = 30;
        angelsWorst[StatEnum.MATK] = 5;
        angelsWorst[StatEnum.ASPD] = 1;
        angelsWorst[StatEnum.CSPD] = 1;
        angelsWorst[StatEnum.EP] = 1;

        // Gas Mask
        var gasMask = vanilla.GetItem(ItemCodex.ItemTypes._Facegear_GasMask.ToString());
        gasMask[StatEnum.Crit] -= 10;
        gasMask[StatEnum.DEF] += 15;

        // Noh Mask
        var noh = vanilla.GetItem(ItemCodex.ItemTypes._Facegear_NohMask.ToString());
        noh[StatEnum.DEF] += 5;
        noh[StatEnum.EPRegen] += 10;

        // Ski Goggles
        var skiGoggles = vanilla.GetItem(ItemCodex.ItemTypes._Facegear_SkiGoggles.ToString());
        skiGoggles[StatEnum.CritDMG] = 25;

        // Santa's Beard
        var santaBeard = vanilla.GetItem(ItemCodex.ItemTypes._Facegear_SantaBeard.ToString());
        santaBeard[StatEnum.HP] = 60;
        santaBeard[StatEnum.DEF] = 5;

        // Masque
        var masque = vanilla.GetItem(ItemCodex.ItemTypes._Facegear_Masque.ToString());
        masque[StatEnum.EP] = 15;

        // Hood of Darkness
        var hood = vanilla.GetItem(ItemCodex.ItemTypes._Hat_HoodOfDarkness.ToString());
        hood[StatEnum.DEF] = -9999; // Consider this a challenge
    }

    public static void CleanupMethod()
    {
        _editor.RestoreValues();
    }
}
