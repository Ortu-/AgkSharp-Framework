using AgkSharp;
using System;
using System.Globalization;

namespace AGKCore
{
    public static class MathUtil
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

        public static float Wrap(float value, float min, float max)
        {
            if (value < min)
            {
                return max - (min - value) % (max - min);
            }
            else
            {
                return min + (value - min) % (max - min);
            }
        }

        public static int Wrap(int value, int min, int max)
        {
            if (value < min)
            {
                return max - (min - value) % (max - min);
            }
            else
            {
                return min + (value - min) % (max - min);
            }
        }

        public static uint Wrap(uint value, uint min, uint max)
        {
            if (value < min)
            {
                return max - (min - value) % (max - min);
            }
            else
            {
                return min + (value - min) % (max - min);
            }
        }

        public static float CurveValue(float targetValue, float currentValue, float speed)
        {
            //based on Stab in the Dark Software https://forum.thegamecreators.com/thread/216506#msg2579241
            if (speed < 1.0f) { speed = 1.0f; }
            return currentValue + ((targetValue - currentValue) / speed);
        }

        public static float CurveAngle(float targetValue, float currentValue, float speed)
        {
            //based on Stab in the Dark Software https://forum.thegamecreators.com/thread/216506#msg2579241            
            if (speed < 1.0f) { speed = 1.0f; }
            targetValue = Wrap(targetValue, 0.0f, 360.0f);
            currentValue = Wrap(currentValue, 0.0f, 360.0f);
            float diff = targetValue - currentValue;
            if(diff < -180.0f)
            {
                diff = (targetValue + 360.0f) - currentValue;
            }
            if(diff > 180.0f)
            {
                diff = targetValue - (currentValue + 360.0f);
            }
            return Wrap(currentValue + (diff / speed), 0.0f, 360.0f);
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
