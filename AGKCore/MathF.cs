using AgkSharp;
using System;
using System.Globalization;

namespace AGKCore
{
    public static class MathF
    {
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
