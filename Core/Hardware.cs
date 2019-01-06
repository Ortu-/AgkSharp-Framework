﻿using AgkSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//using System.Windows.Input;
//using MouseEventArgs = System.Windows.Forms.MouseEventArgs;

namespace AGKCore
{
    public static class Hardware
    {
        public struct MouseInput
        {
            public float MoveX;
            public float MoveY;
            public float MoveZ;
            public int PosX;
            public int PosY;
        }

        public static MouseInput Mouse = new MouseInput();
        public static int[] Input = new int[255]; //bit 1 = isDown, bit 2 = wasDown
        
        public static uint MouseEnum(int rBtn)
        {
            switch (rBtn)
            {
                case 1048576:
                    return 1; //Left
                case 2097152:
                    return 2; //Right
                case 4194304:
                    return 4; //Middle
                default:
                    return 0;
            }
        }

        public static void OnMouseDown(object sender, MouseEventArgs e)
        {
            Input[MouseEnum((int)e.Button)] = Data.SetBit(2, Input[MouseEnum((int)e.Button)], Data.GetBit(1, Input[MouseEnum((int)e.Button)]));
            Input[MouseEnum((int)e.Button)] = Data.SetBit(1, Input[MouseEnum((int)e.Button)], 1);
        }

        public static void OnMouseUp(object sender, MouseEventArgs e)
        {
            Input[MouseEnum((int)e.Button)] = Data.SetBit(2, Input[MouseEnum((int)e.Button)], 0);
            Input[MouseEnum((int)e.Button)] = Data.SetBit(1, Input[MouseEnum((int)e.Button)], 0);
        }

        public static void OnMouseMove(object sender, MouseEventArgs e)
        {
            Mouse.MoveX = e.X;
            Mouse.MoveY = e.Y;
        }

        public static void OnMouseWheel(object sender, MouseEventArgs e)
        {
            Mouse.MoveZ = e.Delta;
        }

        
        public static void OnKeyDown(object sender, KeyEventArgs e)
        {
            Input[e.KeyValue] = Data.SetBit(2, Input[e.KeyValue], Data.GetBit(1, Input[e.KeyValue]));
            Input[e.KeyValue] = Data.SetBit(1, Input[e.KeyValue], 1);
        }

        public static void OnKeyUp(object sender, KeyEventArgs e)
        {
            Input[e.KeyValue] = Data.SetBit(2, Input[e.KeyValue], 0);
            Input[e.KeyValue] = Data.SetBit(1, Input[e.KeyValue], 0);
        }

    }
}
