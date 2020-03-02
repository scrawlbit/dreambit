using System;
using Microsoft.Xna.Framework;

namespace DreamBit.Game.Components
{
    public static class DeltaTime
    {
        internal static GameTime GameTime { get; set; }

        public static TimeSpan ElapsedTime => GameTime.ElapsedGameTime;
        public static float ElapsedSeconds => (float)ElapsedTime.TotalSeconds;
        public static float ElapsedMilliseconds => (float)ElapsedTime.TotalMilliseconds;
    }
}