using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace RocknSpace
{
    static class Time
    {
        public static float TotalSeconds;
        public static float Dt;

        public const float TargetFramerate = 60.0f;

        private static DateTime BeginTime = DateTime.Now;
        private static DateTime LastTime = DateTime.Now;


        public static void FrameBegin()
        {
            Dt = (float)(DateTime.Now - LastTime).TotalSeconds;
            TotalSeconds = (float)(DateTime.Now - BeginTime).TotalSeconds;

            LastTime = DateTime.Now;
        }

        public static void FrameEnd()
        {
            float t = (float)(DateTime.Now - LastTime).TotalMilliseconds;
            float timeToSleep = 1000 / TargetFramerate - (float)(DateTime.Now - LastTime).TotalMilliseconds;
            if (timeToSleep > 0)
                Thread.Sleep((int)timeToSleep);
        }
    }
}
