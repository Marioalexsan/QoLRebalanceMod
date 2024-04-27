using Bagmen;
using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Handle = SoG.SpellVariable.Handle;

namespace Marioalexsan.GrindeaQoL
{
    /* 08/25/2020 9:13PM Discord - Marioalexsan
        I'm assuming that the way plants work, ultimately, is:
        Non-empowered plants decay at 100% rate during combat, and at 40% rate out of combat
        Empowered plants can be considered to decay at 100% rate all the time, but their lifetime (boss boost included) is increased by 65%
        Uncharged vines: ~3.8-ish seconds in combat unempowered, ~10 seconds out of combat unempowered, ~6 seconds empowered
        I think the empower + boss boost is linear
        Pea shooters: ~7 seconds in combat, ~17 seconds out, ~11 seconds empowered
    */

    // So, TLDR for Plant QoL:
    // 1. Rewrite plant lifetime logic
    //    * 100% decay speed in combat, 40% decay speed out of combat (like in vanilla)
    //    * No weird countdown shenanigans when empowered
    //    * Boss lifetime boost changed from 10% to 60% to more closely match vanilla combat behaviour
    //    * Add effect radius indicator

    internal class AttachedPlantData
    {
        public SortedAnimated Effect { get; set; }
    }

    [HarmonyPatch]
    internal static class SummonPlantQoL
    {
        // TODO: Add prediction for silver plants

        private static readonly Dictionary<_Spells_PlantInstance, AttachedPlantData> _attachedPlantData = new Dictionary<_Spells_PlantInstance, AttachedPlantData>();

        private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

        public static void Init()
        {
            _editor.EditValueBy(Handle.Spells_SummonPlant_LeaderLifeProlongWholePct, 60f);
        }

        // Do not name Cleanup since Harmony will turn Cleanup()
        // methods into Finalizers in HarmonyPatch classes
        public static void CleanupMethod()
        {
            _editor.RestoreValues();
        }

        public static SortedAnimated CreateBossIcon(_Spells_PlantInstance owner)
        {
            var indicator = new SortedAnimated(Vector2.Zero, SortedAnimated.SortedAnimatedEffects._SkillEffects_OneHand_SpiritSlash_AOE_Lv1);
            
            indicator.xRenderComponent.fVirtualHeight += owner.xRenderComponent.fVirtualHeight;
            indicator.xRenderComponent.xTransform = owner.xTransform;
            indicator.xTransform = owner.xTransform;

            indicator.xRenderComponent.cColor = Color.LightGreen;
            indicator.xRenderComponent.fAlpha = 0.18f;
            indicator.xRenderComponent.fScale = SpellVariable.Get(Handle.Spells_SummonPlant_LeaderEffectRadius) / 55f;  // radius / effect base size

            Globals.Game._EffectMaster_AddEffect(indicator);
            return indicator;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(_Spells_PlantInstance), nameof(_Spells_PlantInstance.OnDestroy))]
        private static void DestroyBossIndicator(_Spells_PlantInstance __instance)
        {
            if (_attachedPlantData.TryGetValue(__instance, out var data))
            {
                _attachedPlantData.Remove(__instance);

                if (data.Effect != null)
                    data.Effect.bToBeDestroyed = true;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(_Spells_PlantInstance), nameof(_Spells_PlantInstance.Update))]
        private static void CreateBossIndicatorInInit(_Spells_PlantInstance __instance)
        {
            if (__instance.iPowerLevel != 4)
                return;

            if (!_attachedPlantData.ContainsKey(__instance))
            {
                _attachedPlantData[__instance] = new AttachedPlantData()
                {
                    Effect = CreateBossIcon(__instance)
                };
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(_Spells_PlantInstance), nameof(_Spells_PlantInstance.Init))]
        private static void CreateBossIndicatorInUpdate(_Spells_PlantInstance __instance)
        {
            if (__instance.iPowerLevel != 4)
                return;

            if (!_attachedPlantData.ContainsKey(__instance))
            {
                _attachedPlantData[__instance] = new AttachedPlantData()
                {
                    Effect = CreateBossIcon(__instance)
                };
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(_Spells_PlantInstance), nameof(_Spells_PlantInstance.Update))]
        private static IEnumerable<CodeInstruction> ReworkPlantLifetime(IEnumerable<CodeInstruction> code, ILGenerator gen)
        {
            FieldInfo existTimerField = AccessTools.Field(typeof(_Spells_PlantInstance), nameof(_Spells_PlantInstance.iExistTimer));

            var codeList = code.ToList();

            int index = codeList.FindIndex(x => x.LoadsField(existTimerField)) + 4;

            var labels = codeList[index].ExtractLabels();

            codeList.ReplaceAt(index, 47, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldarg_0).WithLabels(labels),
                CodeInstruction.Call(() => PlantLifetimeUpdate(default))
            });

            return codeList;
        }

        private static void PlantLifetimeUpdate(_Spells_PlantInstance plant)
        {
            bool plantInCombat = plant.iCounterAtLastAttack > plant.iExistTimer - plant.iCooldown;

            // 100% decay speed in combat
            if (plantInCombat)
                plant.iToLive--;

            // 40% decay speed (2 ticks out of 5)
            else if (plant.iExistTimer % 5 == 0 || plant.iExistTimer % 5 == 3)
                plant.iToLive--;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(PlantSpellCharge), nameof(PlantSpellCharge.ExecuteSpellCallback))]
        private static IEnumerable<CodeInstruction> FixBossPlantBodyguards(IEnumerable<CodeInstruction> code, ILGenerator gen)
        {
            // Just add the missing iBitCollider setter for the mini after SetInfo

            MethodInfo setInfoMethod = AccessTools.Method(typeof(_Spells_PlantInstance), nameof(_Spells_PlantInstance.SetInfo));

            var codeList = code.ToList();

            codeList.InsertAfterMethod(setInfoMethod, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc_S, 19),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(SpellCharge), nameof(SpellCharge.xOwnerView))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(PlayerView), nameof(PlayerView.xEntity))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(WorldActor), nameof(WorldActor.xCollisionComponent))),
                new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(CollisionComponent), nameof(CollisionComponent.ibitCurrentColliderLayer))),
                new CodeInstruction(OpCodes.Stfld, AccessTools.Field(typeof(_Spells_PlantInstance), nameof(_Spells_PlantInstance.iBitCurrentLayer))),
            }, methodIndex: 1);

            // Buff bouncers a bit (PS there should be only one ldc.r4 instruction with 0.2 as param)
            return codeList.Select(x => x.opcode == OpCodes.Ldc_R4 && x.operand.Equals(0.2f) ? new CodeInstruction(OpCodes.Ldc_R4, 0.4f) : x);
        }
    }
}
