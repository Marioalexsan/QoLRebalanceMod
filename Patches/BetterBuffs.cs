using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Watchers;
using static SoG.SpellVariable;

namespace Marioalexsan.GrindeaQoL
{
    [HarmonyPatch]
    internal static class BetterBuffs
    {
        private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

        public static void Init()
        {
            /* Changes:
             * * Triple duration of each buff spell
             * * Triple EP costs of each spell to compensate for #1
             * * Double duration of Got You Covered talent
            */

            /*
            const float AddedDurationMultiplier = 2;
            const float AddedCostMultiplier = 1.5f;
            const float AddedTalentMultiplier = 1;

            _editor.EditValueBy(Handle.Spells_Utility_BuffATK_DurationInSeconds, Get(Handle.Spells_Utility_BuffATK_DurationInSeconds) * AddedDurationMultiplier);
            _editor.EditValueBy(Handle.Spells_Utility_BuffDEF_DurationInSeconds, Get(Handle.Spells_Utility_BuffDEF_DurationInSeconds) * AddedDurationMultiplier);
            _editor.EditValueBy(Handle.Spells_Utility_BuffSPD_DurationInSeconds, Get(Handle.Spells_Utility_BuffSPD_DurationInSeconds) * AddedDurationMultiplier);

            _editor.EditValueBy(Handle.Spells_Utility_BuffATK_EPCost, Get(Handle.Spells_Utility_BuffATK_EPCost) * AddedCostMultiplier);
            _editor.EditValueBy(Handle.Spells_Utility_BuffDEF_EPCost, Get(Handle.Spells_Utility_BuffDEF_EPCost) * AddedCostMultiplier);
            _editor.EditValueBy(Handle.Spells_Utility_BuffSPD_EPCost, Get(Handle.Spells_Utility_BuffSPD_EPCost) * AddedCostMultiplier);

            _editor.EditValueBy(Handle.Talent_GotYouCovered_ExtraTimeInSeconds, Get(Handle.Talent_GotYouCovered_ExtraTimeInSeconds) * AddedTalentMultiplier);
            */
        }

        public static void CleanupMethod()
        {
            _editor.RestoreValues();
        }

        private static void GetStackingData(ISpellInstance spell, out int existingDuration, out int maxDuration)
        {
            existingDuration = spell switch
            {
                _Spells_BuffATKInstance x => x.iDuration - x.iExistTimer,
                _Spells_BuffDEFInstance x => x.iDuration - x.iExistTimer,
                _Spells_BuffSPDInstance x => x.iDuration - x.iExistTimer,
                _ => 0
            };

            const float MaxStacks = 3;

            int baseDuration = (int)(60 * (spell switch
            {
                _Spells_BuffATKInstance _ => Get(Handle.Spells_Utility_BuffATK_DurationInSeconds),
                _Spells_BuffDEFInstance _ => Get(Handle.Spells_Utility_BuffDEF_DurationInSeconds),
                _Spells_BuffSPDInstance _ => Get(Handle.Spells_Utility_BuffSPD_DurationInSeconds),
                _ => 0
            } + Get(Handle.Talent_GotYouCovered_ExtraTimeInSeconds)));

            maxDuration = (int)(baseDuration * MaxStacks);
        }

        private static int GetNewDuration(ISpellInstance spell, int durationToAdd)
        {
            GetStackingData(spell, out int existingDuration, out int maxDuration);

            return Math.Max(durationToAdd, Math.Min(maxDuration, existingDuration + durationToAdd));
        }

        private static void RenderStackStatus(ISpellInstance spell)
        {
            GetStackingData(spell, out int existingDuration, out int maxDuration);

            int percentage = existingDuration * 100 / maxDuration;

            string noticeText = percentage == 100 ? "Stacks maxed!" : $"{percentage:F0}% stacked!";

            Texture2D gradient = spell switch
            {
                _Spells_BuffATKInstance _ => Globals.Game.Content.Load<Texture2D>("Fonts/gradients/crimson_bold8"),
                _Spells_BuffDEFInstance _ => Globals.Game.Content.Load<Texture2D>("Fonts/gradients/greenie_bold8"),
                _Spells_BuffSPDInstance _ => Globals.Game.Content.Load<Texture2D>("Fonts/gradients/arcadiagoldie_bold8"),
                _ => null
            };

            var staticText = new StaticTextAsTexture(
                FontManager.FontType.Bold8Spacing1,
                gradient,
                noticeText,
                StaticTextAsTexture.TextType.Outlined,
                new Color(84, 43, 1),
                1f,
                1f);

            Globals.Game._EntityMaster_AddWatcher(new NoticeTextWatcher(
                staticText,
                spell.xSpellOwner.xTransform,
                new Vector2(0f, -36f) + Utility.RandomizeVector2Direction(Globals.Game.randomInVisual) * 5f)
                );
        }

        [HarmonyPatch(typeof(BuffATKSpellCharge), nameof(BuffATKSpellCharge.CastBuff))]
        [HarmonyPostfix]
        private static void RenderNoticeATK(BuffATKSpellCharge __instance)
        {
            RenderStackStatus(_Spells_BuffATKInstance.GetActiveInstanceOfPlayer(__instance.xOwnerView));
        }

        [HarmonyPatch(typeof(BuffATKSpellCharge), nameof(BuffATKSpellCharge.CastBuff))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AllowDurationStackingATK(IEnumerable<CodeInstruction> code)
        {
            return new CodeMatcher(code)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(_Spells_BuffATKInstance), nameof(_Spells_BuffATKInstance.Refresh)))
                )
                .ThrowIfInvalid("Transpiler failed")
                .Advance(-7)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc, 2),
                    new CodeInstruction(OpCodes.Ldloc, 0),
                    CodeInstruction.Call(() => GetNewDuration(null, 0)),
                    new CodeInstruction(OpCodes.Stloc, 0)
                )
                .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(BuffDEFSpellCharge), nameof(BuffDEFSpellCharge.CastBuff))]
        [HarmonyPostfix]
        private static void RenderNoticeDEF(BuffDEFSpellCharge __instance)
        {
            RenderStackStatus(_Spells_BuffDEFInstance.GetActiveInstanceOfPlayer(__instance.xOwnerView));
        }

        [HarmonyPatch(typeof(BuffDEFSpellCharge), nameof(BuffDEFSpellCharge.CastBuff))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AllowDurationStackingDEF(IEnumerable<CodeInstruction> code)
        {
            return new CodeMatcher(code)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(_Spells_BuffDEFInstance), nameof(_Spells_BuffDEFInstance.Refresh)))
                )
                .ThrowIfInvalid("Transpiler failed")
                .Advance(-8)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc, 1),
                    new CodeInstruction(OpCodes.Ldloc, 0),
                    CodeInstruction.Call(() => GetNewDuration(null, 0)),
                    new CodeInstruction(OpCodes.Stloc, 0)
                )
                .InstructionEnumeration();
        }


        [HarmonyPatch(typeof(BuffSPDSpellCharge), nameof(BuffSPDSpellCharge.CastBuff))]
        [HarmonyPostfix]
        private static void RenderNoticeSPD(BuffSPDSpellCharge __instance)
        {
            RenderStackStatus(_Spells_BuffSPDInstance.GetActiveInstanceOfPlayer(__instance.xOwnerView));
        }

        [HarmonyPatch(typeof(BuffSPDSpellCharge), nameof(BuffSPDSpellCharge.CastBuff))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> AllowDurationStackingSPD(IEnumerable<CodeInstruction> code)
        {
            return new CodeMatcher(code)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(_Spells_BuffSPDInstance), nameof(_Spells_BuffSPDInstance.Refresh)))
                )
                .ThrowIfInvalid("Transpiler failed")
                .Advance(-7)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc, 2),
                    new CodeInstruction(OpCodes.Ldloc, 0),
                    CodeInstruction.Call(() => GetNewDuration(null, 0)),
                    new CodeInstruction(OpCodes.Stloc, 0)
                )
                .InstructionEnumeration();
        }
    }
}
