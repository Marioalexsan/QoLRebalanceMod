using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaQoL
{
    [HarmonyPatch]
    static class HealthNumbers
    {
        [HarmonyPatch(typeof(HudRenderComponent), nameof(HudRenderComponent.RenderBotGUI))]
        [HarmonyPostfix]
        private static void HudEnemyHP(HudRenderComponent __instance, SpriteBatch spriteBatch)
        {
            Vector2 pos = new Vector2(640f, 340f);

            var player = AccessTools.Field(typeof(HudRenderComponent), "xView").GetValue(__instance) as PlayerView;
            var enemyDisplay = player.xGUIStuff.xLastMonsterHit;

            if (enemyDisplay.iDisplayCycle <= 0)
                return;

            int iTextLength3 = 20 + (int)HudRenderComponent.fontItemPickup.MeasureString(player.xGUIStuff.xLastMonsterHit.sName).X;

            float fAlpha3 = 1f;

            if (enemyDisplay.iDisplayCycle < 10)
            {
                fAlpha3 = enemyDisplay.iDisplayCycle / 10f;
            }
            else if (enemyDisplay.iDisplayCycle > 130)
            {
                fAlpha3 = (150 - enemyDisplay.iDisplayCycle) / 20f;
            }

            RenderHP(enemyDisplay.xEnemy.xBaseStats, spriteBatch, pos + new Vector2(-iTextLength3, -20), __instance.fAlpha, fAlpha3);
        }

        // TODO Improve rendering
        //[HarmonyPatch(typeof(RegularEnemyHPRenderComponent), nameof(RegularEnemyHPRenderComponent.Render))]
        //[HarmonyPostfix]
        private static void RegularHP(RegularEnemyHPRenderComponent __instance, SpriteBatch spriteBatch)
        {
            var xOwner = AccessTools.Field(typeof(RegularEnemyHPRenderComponent), "xOwner").GetValue(__instance) as Enemy;
            Vector2 pos = xOwner.v2DamageNumberPos - Globals.Game.xCamera.v2TopLeft;

            RenderHP(xOwner.xBaseStats, spriteBatch, pos + new Vector2(4, 5), __instance.fAlpha, 1f, false);
        }

        [HarmonyPatch(typeof(MegaMiniBossHPRenderComponent), nameof(MegaMiniBossHPRenderComponent.Render))]
        [HarmonyPostfix]
        private static void PhasemanHP(MegaMiniBossHPRenderComponent __instance, SpriteBatch spriteBatch)
        {
            //Vector2 pos = new Vector2(320f, 320f);
            //pos += __instance.v2OffsetRenderPos;

            RenderHP(__instance.xEnemy.xBaseStats, spriteBatch, new Vector2(303f, 325f), __instance.fAlpha);
        }

        [HarmonyPatch(typeof(LukeBossHPRenderComponent), nameof(LukeBossHPRenderComponent.Render))]
        [HarmonyPostfix]
        private static void LukeHP(LukeBossHPRenderComponent __instance, SpriteBatch spriteBatch)
        {
            //Vector2 v2RenderPos = new Vector2(320 - __instance.iBarLength / 2, 320f);
            //pos += __instance.v2OffsetRenderPos;

            RenderHP(__instance.xEnemy.xBaseStats, spriteBatch, new Vector2(330f, 325f), __instance.fAlpha);
        }

        [HarmonyPatch(typeof(GrindeaHPRenderComponent), nameof(GrindeaHPRenderComponent.Render), typeof(SpriteBatch))]
        [HarmonyPostfix]
        private static void GrindeaHP(GrindeaHPRenderComponent __instance, SpriteBatch spriteBatch)
        {
            RenderHP(__instance.xEnemy.xBaseStats, spriteBatch, new Vector2(320f, 332f) + __instance.v2OffsetRenderPos, __instance.fAlpha);
        }

        [HarmonyPatch(typeof(MiniBossHPRenderComponent), nameof(MiniBossHPRenderComponent.Render))]
        [HarmonyPostfix]
        private static void MiniBossHP(MiniBossHPRenderComponent __instance, SpriteBatch spriteBatch)
        {
            RenderHP(__instance.xEnemy.xBaseStats, spriteBatch, new Vector2(335f, 325f), __instance.fAlpha);
        }

        private static void RenderHP(BaseStats stats, SpriteBatch batch, Vector2 pos, float alpha, float scale = 1f, bool maxHP = true)
        {
            var font = FontManager.GetFontByCategory("SmallTitle", FontManager.FontType.Reg7);
            var hpString = $"{stats.iHP}" + (maxHP ? $" / {stats.iMaxHP}" : "");

            Globals.Game._RenderMaster_RenderTextWithOutline(
                font,
                hpString,
                pos - new Vector2(font.MeasureString(hpString).X / 2, 0),
                Vector2.Zero,
                scale,
                Color.White * alpha,
                Color.Black * alpha);
        }
    }
}
