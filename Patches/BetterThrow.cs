using HarmonyLib;
using ModBagman;
using ModBagman.HarmonyPatches;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaQoL;

[HarmonyPatch]
internal static class BetterThrow
{
    [HarmonyPatch(typeof(_Spells_ThrowInstance), nameof(_Spells_ThrowInstance.Update))]
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> SpinOnGoldReturn(IEnumerable<CodeInstruction> code, ILGenerator gen)
    {
        var codeList = code.ToList();

        var target = AccessTools.Method(typeof(_Spells_ThrowInstance), nameof(_Spells_ThrowInstance.ReturnEffect));

        var insertedBefore = new List<CodeInstruction>
        {
            new CodeInstruction(OpCodes.Ldarg_0),
            new CodeInstruction(OpCodes.Call, SymbolExtensions.GetMethodInfo(() => PlayerPickUpWithoutEP(default)))
        };

        return codeList.InsertBeforeMethod(target, insertedBefore);
    }

    [HarmonyPatch(typeof(_Spells_ThrowInstance), nameof(_Spells_ThrowInstance.PlayerPickUp))]
    [HarmonyReversePatch]
    static void PlayerPickUpWithoutEP(_Spells_ThrowInstance __instance)
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(BaseStats), nameof(BaseStats.AddEP)))
                )
                .RemoveInstructionsWithOffsets(-13, 0)
                .InstructionEnumeration();
        }

        _ = Transpiler(null);
        throw new NotImplementedException("Stub method.");
    }
}
