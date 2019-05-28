using AgkSharp;
using System;
using System.Linq;
using System.Windows.Forms;

namespace AGKCore
{
    public class Hardware
    {
        public class MouseInput
        {
            public float MoveX;
            public float MoveY;
            public float MoveZ;
            public int PosX;
            public int PosY;
            public bool IsVisible = true;
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

        public Hardware()
        {
            Dispatcher.Add(Hardware.ResetMouseMove);
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
            Mouse.MoveX = e.X - Mouse.PosX;
            Mouse.MoveY = e.Y - Mouse.PosY;
            Mouse.PosX = e.X;
            Mouse.PosY = e.Y;

            if (!Mouse.IsVisible)
            {
                Mouse.PosX = App.Config.Screen.CenterX;
                Mouse.PosY = App.Config.Screen.CenterY;
                Agk.SetRawMousePosition(App.Config.Screen.CenterX, App.Config.Screen.CenterY);
            }

            var st = Scheduler.Scheduled.FirstOrDefault(s => s.Name == "Hardware.ResetMouseMove");
            if(st != null)
            {
                st.Timer.Dispose();
                Scheduler.Scheduled.Remove(st);
            }
            
            Scheduler.SetInterval(Hardware.ResetMouseMove, "", 1, 0, 200, "");
        }

        public static void OnMouseWheel(object sender, MouseEventArgs e)
        {
            Mouse.MoveZ = e.Delta;
        }

        public static void ResetMouseMove(object rArgs)
        {
            Mouse.MoveX = 0.0f;
            Mouse.MoveY = 0.0f;
            Mouse.MoveZ = 0.0f;
        }

        public static void SetMouseVisible(bool rVisible)
        {
            if ((rVisible && !Mouse.IsVisible) || (!rVisible && Mouse.IsVisible))
            {
                Agk.SetRawMouseVisible(rVisible);
                Mouse.IsVisible = rVisible;
            }                
        }
        
        public static void OnKeyDown(object sender, KeyEventArgs e)
        {
            //Console.WriteLine(e.KeyValue.ToString());
            Input[e.KeyValue] = Data.SetBit(2, Input[e.KeyValue], Data.GetBit(1, Input[e.KeyValue]));
            Input[e.KeyValue] = Data.SetBit(1, Input[e.KeyValue], 1);
        }

        public static void OnKeyUp(object sender, KeyEventArgs e)
        {
            Input[e.KeyValue] = Data.SetBit(2, Input[e.KeyValue], 0);
            Input[e.KeyValue] = Data.SetBit(1, Input[e.KeyValue], 0);
        }

        public static bool IsKeyDown(int keyValue)
        {
            return Data.GetBit(1, Input[keyValue]) == 1;
        }

        public static bool WasKeyDown(int keyValue)
        {
            return Data.GetBit(2, Input[keyValue]) == 1;
        }
    }
}
