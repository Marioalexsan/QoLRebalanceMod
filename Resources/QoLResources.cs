using ModBagman;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Logging;

namespace Marioalexsan.GrindeaQoL
{
    public static class QoLResources
    {
        public static void ReloadResources()
        {
            NormalPlusTitle?.Dispose();
            NormalPlusTitle = null;

            using (MemoryStream stream = new MemoryStream(Resources.Resource.difficulty_normalplus))
            {
                NormalPlusTitle = Texture2D.FromStream(Globals.Game.GraphicsDevice, stream)
                    ?? throw new InvalidOperationException("Failed to load a resource.");
            }
        }

        public static Texture2D NormalPlusTitle { get; private set; }
    }
}