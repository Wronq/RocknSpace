using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocknSpace
{
    static class Extensions
    {
        public static float NextFloat(this Random rand, float maxValue = 1.0f, float minValue=0.0f)
        {
            return (float)rand.NextDouble() * (maxValue - minValue) + minValue;
        }

        public static Vector NextVector(this Random rand, float maxLength = 1.0f, float minLength = 1.0f)
        {
            double theta = rand.NextDouble() * 2 * Math.PI;
            float length = rand.NextFloat(maxLength, minLength);

            return new Vector(length * Math.Cos(theta), length * Math.Sin(theta));
        }

    }
}
