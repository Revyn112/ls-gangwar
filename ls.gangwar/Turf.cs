using System;
using AltV.Net.Data;

namespace ls.gangwar
{
    public class Turf
    {
        public readonly float X1;

        public readonly float Y1;

        public readonly float X2;

        public readonly float Y2;

        public Turf(float x1, float y1, float x2, float y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        private static bool Between(float min, float p, float max)
        {
            var result = false;

            if (min < max)
            {
                if (p > min && p < max)
                    result = true;
            }

            if (min > max)
            {
                if (p > max && p < min)
                    result = true;
            }

            if (Math.Abs(p - min) < Position.TOLERANCE || Math.Abs(p - max) < Position.TOLERANCE)
                result = true;
            return result;
        }

        public bool Contains(float x, float y)
        {
            return Between(X1, x, X2) && Between(Y1, y, Y2);
        }
    }
}