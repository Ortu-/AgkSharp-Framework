using AgkSharp;
using System;
using System.Globalization;

namespace AGKCore
{
    public static class MathF
    {

        public static float ToRadians(float degrees)
        {
            return (float)Math.PI * degrees / 180.0f;
        }

        public static float ToDegrees(float radians)
        {
            return radians * (180.0f / (float)Math.PI);
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(value, max));
        }
        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(value, max));
        }
        public static uint Clamp(uint value, uint min, uint max)
        {
            return Math.Max(min, Math.Min(value, max));
        }

        public static bool IsPointInBox(int px, int py, int x1, int y1, int x2, int y2)
        {
            if (px >= x1 && px <= x2)
            {
                if (py >= y1 && py <= y2)
                {
                    return true;
                }
            }
            return false;
        }


    }

}
