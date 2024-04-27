using ModBagman;
using SoG;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoG.SpellVariable;

namespace Marioalexsan.GrindeaQoL
{
    internal static class BerserkerStyleQoL
    {
        private static readonly SpellVariableEditor _editor = new SpellVariableEditor();

        public static void Init()
        {
            _editor.SetValue(Handle.Spells_BerserkMode_EPDepletionPerSecond, 0);
            _editor.EditValueBy(Handle.Spells_BerserkMode_EPRecoveryPerHitL1, -2);
            _editor.EditValueBy(Handle.Spells_BerserkMode_EPRecoveryPerHitL2, -2);
            _editor.EditValueBy(Handle.Spells_BerserkMode_EPRecoveryPerHitL4, -2);
        }

        public static void CleanupMethod()
        {
            _editor.RestoreValues();
        }
    }
}
