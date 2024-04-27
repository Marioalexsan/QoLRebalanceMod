using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaQoL
{
    [HarmonyPatch]
    static class BetterDifficulties
    {
        [HarmonyPatch(typeof(Game1), nameof(Game1._Menu_SelectDifficulty))]
        [HarmonyPrefix]
        private static bool SelectDifficultyMainMenu()
        {
            var xGlobalData = Globals.Game.xGlobalData;
            var xInput_Menu = Globals.Game.xInput_Menu;
            var xSoundSystem = Globals.Game.xSoundSystem;

            if (xGlobalData.xMainMenuData.enTargetMenuLevel != GlobalData.MainMenu.MenuLevel.Null || xGlobalData.xMainMenuData.pWaitForPlayerLoad != null || xGlobalData.xMainMenuData.bInPreGameThread || xGlobalData.xMainMenuData.bLockInput)
            {
                return false;
            }
            if (xInput_Menu.Right.bPressed)
            {
                xSoundSystem.PlayInterfaceCue("Menu_Move");
                xGlobalData.xMainMenuData.iDifficultySelection++;
                if (xGlobalData.xMainMenuData.iDifficultySelection > 2)
                {
                    xGlobalData.xMainMenuData.iDifficultySelection = 0;
                }
            }
            else if (xInput_Menu.Left.bPressed)
            {
                xSoundSystem.PlayInterfaceCue("Menu_Move");
                xGlobalData.xMainMenuData.iDifficultySelection--;
                if (xGlobalData.xMainMenuData.iDifficultySelection < 0)
                {
                    xGlobalData.xMainMenuData.iDifficultySelection = 2;
                }
            }
            if (xInput_Menu.Action.bPressed)
            {
                GameSessionData.iBaseDifficulty = xGlobalData.xMainMenuData.iDifficultySelection switch
                {
                    0 => 0,
                    1 => 1,
                    2 => 3,
                    _ => 0
                };

                xSoundSystem.PlayInterfaceCue("Menu_PressAnyKey");
                Globals.Game._Menu_CreateCurrentCharacter(Globals.Game.xTextInputMaster.lxActiveInputs["EnterName"].sInput);
                Globals.Game._Menu_CharacterSelect_Selected();
            }
            else if (xInput_Menu.MenuBack.bPressed || xInput_Menu.KeyPressed(Keys.Escape))
            {
                xSoundSystem.PlayInterfaceCue("Menu_Cancel");
                xGlobalData.xMainMenuData.enMenuLevel = GlobalData.MainMenu.MenuLevel.EnterName;
                Globals.Game.xTextInputMaster.lxActiveInputs["EnterName"].Activate();
            }

            return false;
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._Menu_SelectDifficulty_Render))]
        [HarmonyPrefix]
        static bool SelectDifficultyMainMenuRender(SpriteBatch spriteBatch)
        {
            var xGlobalData = Globals.Game.xGlobalData;

            float fAlpha = xGlobalData.xMainMenuData.fCurrentMenuAlpha;
            spriteBatch.Draw(xGlobalData.xMainMenuData.txDifficulty_window, new Vector2(234f, 106f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(xGlobalData.xMainMenuData.txArrowLeft, new Vector2(276f, 151f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(xGlobalData.xMainMenuData.txArrowRight, new Vector2(358f, 151f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            spriteBatch.Draw(xGlobalData.xMainMenuData.txDifficulty_note, new Vector2(407f, 175f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            if (xGlobalData.xMainMenuData.iDifficultySelection == 0)
            {
                spriteBatch.Draw(xGlobalData.xMainMenuData.txDifficulty_normal, new Vector2(298f, 149f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(xGlobalData.xMainMenuData.txDifficulty_normalinfo, new Vector2(250f, 165f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            else if (xGlobalData.xMainMenuData.iDifficultySelection == 1)
            {
                spriteBatch.Draw(QoLResources.NormalPlusTitle, new Vector2(294f, 149f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(RenderMaster.txNullTex, new Vector2(250f, 165f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }
            else if (xGlobalData.xMainMenuData.iDifficultySelection == 2)
            {
                spriteBatch.Draw(xGlobalData.xMainMenuData.txDifficulty_hard, new Vector2(306f, 149f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                spriteBatch.Draw(xGlobalData.xMainMenuData.txDifficulty_hardinfo, new Vector2(250f, 165f), null, Color.White * fAlpha, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
            }

            return false;
        }

        static int SelectOptionFromDifficulty(int diff)
        {
            return diff switch
            {
                0 => 0,
                1 => 1,
                3 => 2,
                _ => 0
            };
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._InGameMenu_RenderSystem_RenderOption))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RenderSystem(IEnumerable<CodeInstruction> code, ILGenerator gen)
        {
            return new CodeMatcher(code)
                .MatchStartForward(
                    new CodeMatch(OpCodes.Ldstr, "MainMenu_Options_Hard")
                )
                .ThrowIfInvalid("What?!")
                .Advance(-1)
                .Insert(
                    new CodeInstruction(OpCodes.Ldloc_S, 15),
                    new CodeInstruction(OpCodes.Ldstr, "MainMenu_Options_NormalPlus"),
                    new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(List<string>), nameof(List<string>.Add)))
                )
                .MatchStartForward(
                    new CodeMatch(OpCodes.Brfalse_S)
                )
                .RemoveInstructions(5)
                .Insert(
                    CodeInstruction.Call(() => SelectOptionFromDifficulty(default)),
                    new CodeInstruction(OpCodes.Stloc_S, 4)
                )
                .InstructionEnumeration();
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._InGameMenu_Input))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> InputSystem(IEnumerable<CodeInstruction> code)
        {
            var codeList = code.ToList();

            var firstPos = codeList.FindPosition((list, i) => list[i].Calls(SymbolExtensions.GetMethodInfo(() => Globals.Game._Enemy_AdjustForDifficultyForAll())));
            var secondPos = codeList.FindPosition((list, i) => list[i].Calls(SymbolExtensions.GetMethodInfo(() => Globals.Game._Enemy_AdjustForDifficultyForAll())), 1);

            codeList.ReplaceAt(secondPos - 11, 10, new List<CodeInstruction>()
            {
                CodeInstruction.Call(() => DifficultyLeftButton())
            });

            codeList.ReplaceAt(firstPos - 11, 10, new List<CodeInstruction>()
            {
                CodeInstruction.Call(() => DifficultyRightButton())
            });

            return codeList;
        }

        private static void DifficultyRightButton()
        {
            GameSessionData.iBaseDifficulty = GameSessionData.iBaseDifficulty switch
            {
                0 => 1,
                1 => 3,
                3 => 0,
                _ => 0
            };
        }

        private static void DifficultyLeftButton()
        {
            GameSessionData.iBaseDifficulty = GameSessionData.iBaseDifficulty switch
            {
                0 => 3,
                1 => 0,
                3 => 1,
                _ => 0
            };
        }


        [HarmonyPatch(typeof(Game1), nameof(Game1._Loading_LoadWorldFromFile))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> LoadDifficultyProperly(IEnumerable<CodeInstruction> code)
        {
            // This is kinda hacky
            var codeList = code.ToList();

            var firstPos = codeList.FindPosition((list, i) => list[i].LoadsConstant(31));

            codeList.ReplaceAt(firstPos + 5, 7, new List<CodeInstruction>()
            {
                CodeInstruction.Call(() => SetCorrectWorldDifficulty())
            });

            return codeList;
        }

        private static void SetCorrectWorldDifficulty()
        {
            if (GameSessionData.iBaseDifficulty != 0 && GameSessionData.iBaseDifficulty != 1 && GameSessionData.iBaseDifficulty != 3)
            {
                GameSessionData.iBaseDifficulty = 0;
            }
        }
    }
}
