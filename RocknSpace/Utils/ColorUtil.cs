using System;
using SharpDX;

namespace RocknSpace.Utils
{
    static class ColorUtil
    {
        public static Color4 HSVToColor(float h, float s, float v)
        {
            if (h == 0 && s == 0)
                return new Color4(v, v, v, 1);

            float c = s * v;
            float x = c * (1 - Math.Abs(h % 2 - 1));
            float m = v - c;

            if (h < 1) return new Color4(c + m, x + m, m, 1);
            else if (h < 2) return new Color4(x + m, c + m, m, 1);
            else if (h < 3) return new Color4(m, c + m, x + m, 1);
            else if (h < 4) return new Color4(m, x + m, c + m, 1);
            else if (h < 5) return new Color4(x + m, m, c + m, 1);
            else return new Color4(c + m, m, x + m, 1);
        }
    }
}
