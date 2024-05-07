using Behaviours;
using ModBagman;
using HarmonyLib;
using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using static SoG.Inventory;
using static SoG.Inventory.PreviewPair;
using static SoG.SpellVariable;

namespace Marioalexsan.GrindeaQoL
{
    public class QoLRebalanceMod : Mod
    {
        public override string Name => "Marioalexsan-QoLRebalance";
        public override Version Version => new Version(1, 0, 0);

        public static QoLRebalanceMod Instance { get; private set; }

        private readonly List<(Action, Action)> _initCleanupList;

        private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

        private bool ToggledLukeMusic = false;

        public QoLRebalanceMod()
        {
            Globals.Game.EXT_AddMiscText("Menus", "MainMenu_Options_NormalPlus", "Normal+");

            _initCleanupList = new List<(Action, Action)>
            {
                (BetterLootChance.Init, null),
                (BerserkerStyleQoL.Init, BerserkerStyleQoL.CleanupMethod),
                (SummonPlantQoL.Init, SummonPlantQoL.CleanupMethod),
                (BetterBuffs.Init, BetterBuffs.CleanupMethod),
                (ProvokeRebalance.Init, ProvokeRebalance.CleanupMethod),
                (ItemStatRebalance.Init, ItemStatRebalance.CleanupMethod),
            };
        }

        public BaseStats.StatusEffectSource HasteMSPD = default;

        public override void Load()
        {
            Instance = this;
            
            QoLResources.ReloadResources();

            foreach (var (init, _) in _initCleanupList)
                init?.Invoke();

            HasteMSPD = CreateStatusEffect("HasteMSPD").GameID;

            CreateAudio()
                .AddMusicFromWaveBank(Name + "_StreamedMusic", "FriendsToTheEndVersionA", "BossRushHeavy");

            CreateCommands()
                .AutoAddModCommands("qol");
        }

        public override void Unload()
        {
            foreach (var (_, cleanup) in _initCleanupList)
                cleanup?.Invoke();

            Instance = null;
        }

        [ModCommand(nameof(ToggleLukeMusic))]
        public void ToggleLukeMusic()
        {
            ToggledLukeMusic = !ToggledLukeMusic;

            if (ToggledLukeMusic)
            {
                GetAudio().RedirectMusic("LukeBattle", GetAudio().GetMusicID("FriendsToTheEndVersionA").Value.ToString());
            }
            else
            {
                GetAudio().RedirectMusic("LukeBattle", "");
            }

            CAS.AddChatMessage($"Toggled Luke alt music {(ToggledLukeMusic ? "on" : "off")}.");
        }

        [ModCommand(nameof(ToggleStatHud))]
        public void ToggleStatHud()
        {
            StatHUD.Enabled = !StatHUD.Enabled;

            CAS.AddChatMessage($"Toggled Stat HUD {(StatHUD.Enabled ? "on" : "off")}.");
        }
    }
}
