using Bagmen;
using ModBagman;
using HarmonyLib;
using Microsoft.Xna.Framework;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;

namespace Marioalexsan.GrindeaQoL
{
    [HarmonyPatch]
    internal static class BetterLootChance
    {
        private static double PRDThreshold = 0.025;

        private static readonly Dictionary<double, double> _cValues = new Dictionary<double, double>();

        private static readonly Dictionary<EnemyCodex.EnemyTypes, Dictionary<ItemCodex.ItemTypes, int>> _dropTracker = new Dictionary<EnemyCodex.EnemyTypes, Dictionary<ItemCodex.ItemTypes, int>>();

        public static void Init()
        {
            // TODO Persist kill counts
            _dropTracker.Clear();
        }

        [HarmonyPatch(typeof(Game1), nameof(Game1._Enemy_DropLoot))]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction>
            UsePRDForEnemyDrops(IEnumerable<CodeInstruction> code, ILGenerator gen)
        {
            MethodInfo target = SymbolExtensions.GetMethodInfo(() => Globals.Game._EntityMaster_AddItem(default, default, default, default, default));

            var codeList = code.ToList();

            int index = codeList.FindIndex(x => x.Calls(target));
            var labels = codeList[index - 43].ExtractLabels();

            codeList.ReplaceAt(index - 43, 45, new List<CodeInstruction>
            {
                new CodeInstruction(OpCodes.Ldloc, 36).WithLabels(labels),  // dropIndex
                new CodeInstruction(OpCodes.Ldloc, 38),  // iChance
                new CodeInstruction(OpCodes.Ldloc, 31),  // bGuaranteeDrop
                new CodeInstruction(OpCodes.Ldarg_1),  // xEnemy
                CodeInstruction.Call(() => HandleEnemyDrop(default, default, default, default)),
                new CodeInstruction(OpCodes.Stloc, 30) // bDropped
            });

            return codeList;
        }

        private static bool HandleEnemyDrop(int dropIndex, int iBoostedChance, bool guaranteeDrop, Enemy enemy)
        {
            // For some reason 100000 chance = 100%
            var item = enemy.lxLootTable[dropIndex].enItemToDrop;
            double baseChance = enemy.lxLootTable[dropIndex].iChance / 100000f;
            double boostedChance = iBoostedChance / 100000f;

            if (baseChance > PRDThreshold)
            {
                // Use regular random

                if (Globals.Game.randomInLogic.NextDouble() < boostedChance || guaranteeDrop)
                {
                    Globals.Game._EntityMaster_AddItem(item, enemy.xTransform.v2Pos, enemy.xRenderComponent.fVirtualHeight, enemy.xCollisionComponent.xMovementCollider.ibitLayers, new Vector2((float)Globals.Game.randomInLogic.NextDouble() * 2f - 1f, (float)Globals.Game.randomInLogic.NextDouble() * 2f - 1f));
                    return true;
                }

                return false;
            }

            if (!_dropTracker.TryGetValue(enemy.enType, out var itemTable))
                _dropTracker[enemy.enType] = itemTable = new Dictionary<ItemCodex.ItemTypes, int>();

            if (!itemTable.TryGetValue(item, out var dropAttempts))
                dropAttempts = 0;

            double trueChance = boostedChance / baseChance * CfromP(baseChance) * (dropAttempts + 1);

            if (Globals.Game.randomInLogic.NextDouble() < trueChance || guaranteeDrop)
            {
                Globals.Game._EntityMaster_AddItem(item, enemy.xTransform.v2Pos, enemy.xRenderComponent.fVirtualHeight, enemy.xCollisionComponent.xMovementCollider.ibitLayers, new Vector2((float)Globals.Game.randomInLogic.NextDouble() * 2f - 1f, (float)Globals.Game.randomInLogic.NextDouble() * 2f - 1f));
                _dropTracker[enemy.enType][item] = 0;
                return true;
            }
            else
            {
                // A fail occurred, so increase counter
                _dropTracker[enemy.enType][item] = dropAttempts + 1;
                return false;
            }
        }

        // https://gaming.stackexchange.com/questions/161430/calculating-the-constant-c-in-dota-2-pseudo-random-distribution
        private static double CfromP(double p)
        {
            if (_cValues.ContainsKey(p))
                return _cValues[p];

            double Cupper = p;
            double Clower = 0;
            double Cmid;
            double p1;
            double p2 = 1;

            while (true)
            {
                Cmid = (Cupper + Clower) / 2;
                p1 = PfromC(Cmid);

                if (Math.Abs(p1 - p2) <= 0) 
                    break;

                if (p1 > p)
                {
                    Cupper = Cmid;
                }
                else
                {
                    Clower = Cmid;
                }

                p2 = p1;
            }

            return Cmid;
        }

        private static double PfromC(double C)
        {
            double pProcOnN;
            double pProcByN = 0;
            double sumNpProcOnN = 0;

            int maxFails = (int)Math.Ceiling(1 / C);
            for (int N = 1; N <= maxFails; ++N)
            {
                pProcOnN = Math.Min(1, N * C) * (1 - pProcByN);
                pProcByN += pProcOnN;
                sumNpProcOnN += N * pProcOnN;
            }

            return 1 / sumNpProcOnN;
        }
    }
}
