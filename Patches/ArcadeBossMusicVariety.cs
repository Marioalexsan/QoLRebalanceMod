using HarmonyLib;
using ModBagman;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using BossType = SoG.RogueLikeMode.Room.BossEncounters;

namespace Marioalexsan.GrindeaQoL;

[HarmonyPatch]
internal static class ArcadeBossMusicVariety
{
    private static void PlayMusicForBoss()
    {
        bool musicOverride = CAS.LocalRogueLikeData.byOption_OverrideRunSongs == 1;

        float health = Globals.Game.dixPlayers.Values.Select(x => x.xEntity.xBaseStats.iHP).Sum();
        float maxHealth = Globals.Game.dixPlayers.Values.Select(x => x.xEntity.xBaseStats.iMaxHP).Sum();

        float teamHurtLevel = (maxHealth - health) / maxHealth;

        float altBattlePlayChance = Math.Max(0, Math.Min(1, (teamHurtLevel - 0.25f) / 0.5f));

        RogueLikeMode.Room xRoom = Globals.Game.xGameSessionData.xRogueLikeSession.xCurrentRoom;

        var shardsmithHeavy = QoLRebalanceMod.Instance.GetAudio().GetMusicID("BossRushHeavy").Value.ToString();

        string song = xRoom.enBossEncounter switch
        {
            // Mod
            BossType.VilyaSolo => "MiniBossBattle01",
            BossType.VilyaElite => "MiniBossBattle02",
            BossType.BlackFerrets => "MiniBossBattle02",
            BossType.Marino => "MarinoBattle",
            BossType.MarinoV2 => "MarinoBattle",
            BossType.CrystalChallenge => "Challenge",
            BossType.Phaseman => "Challenge",
            BossType.Bishop => "BishopBattle",
            BossType.PowerFlower => "Challenge",

            BossType.CptBones => shardsmithHeavy,
            BossType.EvilEye => shardsmithHeavy,

            // Vanilla
            BossType.Luke => musicOverride ? "LukeBattle" : "LukeBattle_Arcade",
            BossType.AncientMimic => null,

            // Vanilla + Modded fun
            _ => Globals.Game.randomInVisual.NextDouble() <= altBattlePlayChance ? "BossRush" : "BossBattle01"
        };

        if (song != null)
        {
            Globals.Game.xSoundSystem.QueueSong(song);
        }
    }

    [HarmonyPatch(typeof(Game1), nameof(Game1._LevelLoading_DoStuff_ArcadeModeRoom))]
    [HarmonyTranspiler]
    private static IEnumerable<CodeInstruction> MusicSelect(IEnumerable<CodeInstruction> code)
    {
        var codeMatcher = new CodeMatcher(code)
            .MatchStartForward(
                new CodeMatch(OpCodes.Ldstr, "LukeBattle_Arcade")
            )
            .ThrowIfInvalid("What?!")
            .Advance(-6);

        var extractedLabels = codeMatcher.Instruction.labels;

        codeMatcher.RemoveInstructions(17);

        codeMatcher.Insert(
            CodeInstruction.Call(() => PlayMusicForBoss()).WithLabels(extractedLabels)
            );

        return codeMatcher.InstructionEnumeration();
    }
}
