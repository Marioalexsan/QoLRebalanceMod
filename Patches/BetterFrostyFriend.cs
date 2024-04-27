using HarmonyLib;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaQoL
{
    [HarmonyPatch]
    internal static class BetterFrostyFriend
    {
        [HarmonyPatch(typeof(_Spells_FrostyFriendInstance), nameof(_Spells_FrostyFriendInstance.GetMaxHP))]
        [HarmonyPrefix]
        public static bool GetMaxHP(ref int __result, int iChargeLevel, int iPlayerLevel, int iPlayerMATK, int iSpellLevel)
        {
            int spellLevelHealth = iSpellLevel * 10;
            int matkHealth = iPlayerMATK * 2;
            int chargeLevelHealth = iChargeLevel switch
            {
                1 => 50,
                2 => 75,
                3 => 100,
                4 => 200,
                _ => 0
            };

            __result = spellLevelHealth + chargeLevelHealth + matkHealth;
            return false;
        }
    }
}
