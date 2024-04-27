using HarmonyLib;
using SoG;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using static SoG.EquipmentInfo;
using static SoG.Inventory.PreviewPair;
using static SoG.Inventory;
using Microsoft.Xna.Framework;
using ModBagman;

namespace Marioalexsan.GrindeaQoL
{
    [HarmonyPatch]
    internal static class BetterSpecialEffectNames
    {
        public static readonly Dictionary<SpecialEffect, string> Displays = new Dictionary<SpecialEffect, string>()
        {
            // Max 18-19 characters
            [SpecialEffect._Unique_AncientPendant_IncreasedChargeSpeed] = "Quicker Charging",
            [SpecialEffect._Unique_ArcherApple] = "More Bow Damage",
            [SpecialEffect._Unique_BarrelHat_BarrelShieldSynergy] = "Barrel Combo",
            [SpecialEffect._Unique_BladeOfEchoes_Cursed] = "Cursed!",
            [SpecialEffect._Unique_BootsOfBloodthirst_HPDrainASPDBuff] = "Drains Health",
            [SpecialEffect._Unique_BugNet_BugCatcher] = "Catch Butterflies",
            [SpecialEffect._Unique_CactusClub_SpawnThorns] = "Melee Casts Thorns",
            [SpecialEffect._Unique_CameraLens_CritIncreaseOnPG] = "Crits on PGuard",
            [SpecialEffect._Unique_CameraShield_StunOnPG] = "Stun on PGuard",
            [SpecialEffect._Unique_CaptainBonesHead_Summon] = "Summon Cpt.Bones",
            [SpecialEffect._Unique_CogShield_EPRegenOnGuard] = "EP on PGuard",
            [SpecialEffect._Unique_CrystalPumps_MoveSlowWhileChargingMagic] = "Sluggish Charges",
            [SpecialEffect._Unique_CrystalShield_PGDamageReduce01] = "PGuards Block More",
            [SpecialEffect._Unique_CrystalShield_PGDamageReduce02] = "PGuards Block All",
            [SpecialEffect._Unique_EmptyBottle_QuickerPotionRecharge] = "Faster Potions",
            [SpecialEffect._Unique_EvilEye_EyeLaser] = "?????",
            [SpecialEffect._Unique_ExtraDamagePerCard] = "Gain ATK per Card",
            [SpecialEffect._Unique_FertilizerHat_ItsPoo] = "Flies Last Longer",
            [SpecialEffect._Unique_GasMask_PoisonResistance] = "Poison Resist",
            [SpecialEffect._Unique_GiantIcicle_ChillingTouch] = "Chill on Hit",
            [SpecialEffect._Unique_GoblinShoes_LessSlippery] = "Less Ice Slipping",
            [SpecialEffect._Unique_GoldenEarrings_IncreasedGoldDrops] = "More Gold",
            [SpecialEffect._Unique_IcePendant_StrongerIceSpells] = "Strong Ice Magic",
            [SpecialEffect._Unique_KobesTag_SummonCollar] = "Cheaper Summons",
            [SpecialEffect._Unique_LightningGlove_StaticTouch] = "Zap on Hit",
            [SpecialEffect._Unique_LuckySeven_GuaranteedCrits] = "7th Hit Crits",
            [SpecialEffect._Unique_MagicBattery_EPRegOnCrit] = "EP on Hit",
            [SpecialEffect._Unique_MissileControlUnit] = "?????",
            [SpecialEffect._Unique_MushroomShield_SpawnShrooms] = "Shrooms on Block",
            [SpecialEffect._Unique_MushroomSlippers_EPRegWhileBlinded] = "EP while Blind",
            [SpecialEffect._Unique_MysteryCube_RandomEffectOnBasicAttackHit] = "Random Effects",
            [SpecialEffect._Unique_Pan_PotionRechargeIncrease] = "Fast Potions",
            [SpecialEffect._Unique_Pickaxe_InstantBreakEnvironment] = "Break Things",
            [SpecialEffect._Unique_PlantBlade_IncreasedSeedDrops] = "More Seed Drops",
            [SpecialEffect._Unique_RecipeBook_LoosePages] = "Drops Pages on Hit",
            [SpecialEffect._Unique_RedFlowerWhip_LongerPlantDurations] = "Plants Last Longer",
            [SpecialEffect._Unique_RestlessSpirit_RandomMovementWhenIdle] = "Move when AFK",
            [SpecialEffect._Unique_RollerBlades_FasterChargeMovement] = "Hasty Charges",
            [SpecialEffect._Unique_SailorHat_FasterChargeMovement] = "Swift Charges",
            [SpecialEffect._Unique_Shiidu] = "Pacifist?",
            [SpecialEffect._Unique_SlimeFriend] = "Summon Slimes",
            [SpecialEffect._Unique_SmashLight_BurningTouch] = "Burn on Hit",
            [SpecialEffect._Unique_SolemShield_LaserBlast] = "Laser on PGuard",
            [SpecialEffect._Unique_SpectralBlindfold_TradeHPForEP] = "Drain HP into EP",
            [SpecialEffect._Unique_StingerBonuses] = "Fast Dash Charge",
            [SpecialEffect._Unique_SunShield_SpinningSun] = "Sun Orbital",
            [SpecialEffect._Unique_ThornMane_ThornWormTrail] = "Thorn Walk",
            [SpecialEffect._Unique_ThornWorm_ReturnDamage] = "Retaliate Attacks",
            [SpecialEffect._Unique_TripleA_EXPIncrease] = "More EXP",
            [SpecialEffect._Unique_WinterShield_ColdGuard] = "Chill on Block",
            [SpecialEffect._Unique_WispShield_BetterProjectileReflect] = "Strong Reflects"
        };

        public static string GetEffectText(SpecialEffect effect)
        {
            return Displays.TryGetValue(effect, out string value) ? value : "Special Effect";
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PreviewPair), "SetInfo")]
        private static IEnumerable<CodeInstruction>
            BetterSpecialEffectDescription(IEnumerable<CodeInstruction> code, ILGenerator gen)
        {
            // It's much easier to just jump to the end of the method and overwrite previous changes

            List<CodeInstruction> codeList = code.ToList();

            var ret = codeList.Last();
            codeList.RemoveAt(codeList.Count - 1);

            int insertStart = codeList.Count;

            codeList.AddRange(new CodeInstruction[]
            {
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(ret.ExtractLabels()),
                new CodeInstruction(OpCodes.Ldloc_2),
                new CodeInstruction(OpCodes.Ldloc_1),
                new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(BetterSpecialEffectNames), nameof(LoadStatChanges))),
                new CodeInstruction(OpCodes.Ret)
            });

            return codeList;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(Game1), nameof(Game1._InGameMenu_RenderEquipPreview))]
        private static IEnumerable<CodeInstruction>
            FixColor(IEnumerable<CodeInstruction> code, ILGenerator gen)
        {
            List<CodeInstruction> codeList = code.ToList();

            int index = codeList.FindPosition(
                (list, i) => list[i].Calls(SymbolExtensions.GetMethodInfo(() => Globals.Game._RenderMaster_RenderTextWithOutline(default, default, default, default, default(float), default, default, default))), 
                1
                );

            var ins = codeList.AsEnumerable().Reverse().Skip(codeList.Count - index).First(x => x.LoadsField(AccessTools.Field(typeof(PreviewPair), nameof(PreviewPair.lxStatChanges))));

            ins.operand = AccessTools.Field(typeof(PreviewPair), nameof(PreviewPair.lxEquipStats));

            return codeList;
        }


        private static void LoadStatChanges(PreviewPair view, EquipmentInfo equip, EquipmentInfo discard)
        {
            string text = CAS.GetLibraryText("Menus", "InGameMenu_LeftInfo_SpecialEffect");

            view.lxStatChanges = view.lxStatChanges.Where(x => x.sString != text).ToList();

            var gained = equip.lenSpecialEffects;
            var lost = discard?.lenSpecialEffects ?? new List<SpecialEffect>();

            view.lxStatChanges.AddRange(
                gained
                .Select(x => new ColoredString(GetEffectText(x), lost.Contains(x) ? Color.LightGreen : new Color(141, 255, 110)))
                );

            view.lxStatChanges.AddRange(
                lost
                .Where(x => !gained.Contains(x))
                .Select(x => new ColoredString(GetEffectText(x), new Color(255, 119, 119)))
                );

            if (discard != null && equip.enItemType == discard.enItemType)
                view.lxEquipStats.AddRange(
                    equip.lenSpecialEffects
                    .Select(x => new ColoredString(GetEffectText(x), Color.LightGreen))
                    );

            view.bKeepEffect = false;
        }
    }
}
