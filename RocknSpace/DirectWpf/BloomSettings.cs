using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocknSpace.DirectWpf
{
    class BloomSettings
    {
        // Controls how bright a pixel needs to be before it will bloom.
        // Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public readonly float BloomThreshold;


        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public readonly float BlurAmount;


        // Controls the amount of the bloom and base images that
        // will be mixed into the final scene. Range 0 to 1.
        public readonly float BloomIntensity;
        public readonly float BaseIntensity;


        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public readonly float BloomSaturation;
        public readonly float BaseSaturation;


        /// <summary>
        /// Constructs a new bloom settings descriptor.
        /// </summary>
        public BloomSettings(float bloomThreshold, float blurAmount,
                             float bloomIntensity, float baseIntensity,
                             float bloomSaturation, float baseSaturation)
        {
            BloomThreshold = bloomThreshold;
            BlurAmount = blurAmount;
            BloomIntensity = bloomIntensity;
            BaseIntensity = baseIntensity;
            BloomSaturation = bloomSaturation;
            BaseSaturation = baseSaturation;
        }

        public static BloomSettings[] Presets =
        {
            //                Thresh  Blur Bloom  Base  BloomSat BaseSat
            new BloomSettings(0.25f,  4,   1.25f, 1,    1,       1), //Default
            new BloomSettings(0,      3,   1,     1,    1,       1), //Soft
            new BloomSettings(0.5f,   8,   2,     1,    0,       1), //Destaturaed
            new BloomSettings(0.25f,  4,   2,     1,    2,       0), //Saturated
            new BloomSettings(0,      2,   1,     0.1f, 1,       1), //Blurry
            new BloomSettings(0.5f,   2,   1,     1,    1,       1), //Subtle
        };
    }
}
