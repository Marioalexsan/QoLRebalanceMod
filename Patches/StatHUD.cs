using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ModBagman;
using SoG;
using System.Collections.Generic;
using System.Linq;

namespace Marioalexsan.GrindeaQoL;

[HarmonyPatch]
public static class StatHUD
{
    public static bool Enabled { get; set; } = true;

    [HarmonyPatch(typeof(HudRenderComponent), nameof(HudRenderComponent.Render))]
    [HarmonyPostfix]
    public static void RenderStats(SpriteBatch spriteBatch)
    {
        if (Globals.Game.bDontRenderGUI || !Enabled)
        {
            return;
        }

        var font = FontManager.GetFont(FontManager.FontType.Reg7);
        var stats = Globals.Game.xLocalPlayer.xEntity.xBaseStats;
        var entity = Globals.Game.xLocalPlayer.xEntity;
        var strings = new List<string>
        {
            $"ATK {stats.iATK:0.##} = {stats.iBaseATK:0.##} * {stats.fBaseATKMultiplier:0.##}x" + (stats.fMATKToATKConversionInPCT > 0 ? $" + {stats.iMATK:0.##} * {stats.fMATKToATKConversionInPCT / 100f:0.##}x": ""),
            $"MATK {stats.iMATK:0.##} = {stats.iBaseMATK:0.##} * {stats.fBaseMATKMultiplier:0.##}x",
            $"DEF {stats.iDefense:0.##} = {stats.iBaseDEF:0.##} {(stats.iBaseDEF >=0 ? "*" : "/")} {stats.fBaseDEFMultiplier:0.##}x",
            $"EP Regen {(stats.iEPRegenMultiplierInPCT + 100) / 100f:0.##}x",
            $"EP/s {(stats.iEPRegenMultiplierInPCT + 100) / 100f * stats.fEPRecoveryRate * 60:0.#}",
            $"ASPD {stats.fAttackSPDFactor:0.##}x",
            $"CSPD {stats.fCastSPDFactor:0.##}x",
            $"MoveSPD {stats.fMovementSpeed:0.##}x",
            stats.iSpellTemporaryCritChanceBonus > 0 ? $"Bonus Skill Crit {stats.iSpellTemporaryCritChanceBonus:0.##}%" : "",
            $"Crit {stats.iCritChanceBonus:0.##}%",
            $"Crit DMG {(stats.iCritDamageModifier + 100) / 100f:0.##}x",
            stats.iBarrierHP > 0 ? $"Barrier HP {stats.iBarrierHP:F0}" : "",
            $"ShldMove {entity.MovementModInShield:0.##}x",
            $"CastMove {entity.MovementModInCharge:0.##}x",
        }
        .Where(x => !string.IsNullOrEmpty(x))
        .ToList();

        var start = new Vector2(2, 90);

        foreach (var str in strings)
        {
            Globals.Game._RenderMaster_RenderTextWithOutline(font, str, start, Vector2.Zero, 1f, Color.White, Color.Black);
            start.Y += font.MeasureString(str).Y + 1;
        }
    }
}
