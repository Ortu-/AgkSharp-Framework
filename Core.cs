using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AgkSharp;

namespace AGKCore
{
    public class App
    {
        static Form m_Window;
        static bool m_bMaximized = false;
        static bool m_bBreakLoop = false;
        public static bool DisableEscape { get; set; } = false;

        public static AppConfig Config;

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetKeyboardState(byte[] lpKeyState);
        [DllImport("user32.dll", SetLastError = true)]
        static extern uint MapVirtualKey(uint uCode, uint uMapType);
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr GetKeyboardLayout(uint idThread);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int ToUnicodeEx(uint wVirtKey, uint wScanCode, byte[] lpKeyState, [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pwszBuff, int cchBuff, uint wFlags, IntPtr dwhkl);
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool PostMessage(HandleRef hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static Form CreateWindow(string title, int width, int height, bool fullscreen) => CreateWin32Window(title, width, height, fullscreen);

        public static Form CreateWin32Window(string title, int width, int height, bool fullscreen)
        {
            m_Window = new Form
            {
                Text = title,
                ClientSize = new Size(width, height),
                StartPosition = FormStartPosition.CenterScreen,
            };

            if (fullscreen)
            {
                m_Window.WindowState = FormWindowState.Normal;
                m_Window.FormBorderStyle = FormBorderStyle.None;
                m_Window.Bounds = Screen.PrimaryScreen.Bounds;
            }

            if (System.IO.File.Exists("icon.ico"))
                m_Window.Icon = new Icon("icon.ico");

            m_Window.MouseDown += Core_OnMouseDown;
            m_Window.MouseUp += Core_OnMouseUp;
            m_Window.MouseWheel += Core_OnMouseWheel;
            m_Window.MouseMove += Core_OnMouseMove;
            m_Window.SizeChanged += Core_OnSizeChanged;
            m_Window.Move += Core_OnMove;
            m_Window.Activated += Core_OnActivated;
            m_Window.Deactivate += Core_OnDeactivate;
            m_Window.GotFocus += Core_OnGotFocus;
            m_Window.LostFocus += Core_OnLostFocus;
            m_Window.KeyDown += Core_OnKeyDown;
            m_Window.KeyUp += Core_OnKeyUp;
            m_Window.FormClosing += Core_OnClose;

            m_Window.Show();

            return m_Window;
        }

        static void PassKeyDown(uint key)
        {
            switch (key)
            {
                // Top Row 0-9
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                    Agk.KeyDown(key + 215);
                    break;

                // Num pad 0-9
                case 96:
                case 97:
                case 98:
                case 99:
                case 100:
                case 101:
                case 102:
                case 103:
                case 104:
                case 105:
                    Agk.KeyDown(key - 48);
                    break;

                case 16:
                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift))
                        Agk.KeyDown(257);
                    else if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
                        Agk.KeyDown(258);
                    break;

                case 17:
                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
                        Agk.KeyDown(259);
                    else if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl))
                        Agk.KeyDown(260);
                    break;

                case 18:
                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt))
                        Agk.KeyDown(261);
                    else if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt))
                        Agk.KeyDown(262);
                    break;
            }

            if (key > 0 && key < 512)
                Agk.KeyDown(key);
        }

        static void PassKeyUp(uint key)
        {
            switch (key)
            {
                // Top Row 0-9
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                    Agk.KeyUp(key + 215);
                    break;

                // Num pad 0-9
                case 96:
                case 97:
                case 98:
                case 99:
                case 100:
                case 101:
                case 102:
                case 103:
                case 104:
                case 105:
                    Agk.KeyUp(key - 48);
                    break;

                case 16:
                    if (System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.LeftShift))
                        Agk.KeyUp(257);
                    else if (System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.RightShift))
                        Agk.KeyUp(258);
                    break;

                case 17:
                    if (System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.LeftCtrl))
                        Agk.KeyUp(259);
                    else if (System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.RightCtrl))
                        Agk.KeyUp(260);
                    break;

                case 18:
                    if (System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.LeftAlt))
                        Agk.KeyUp(261);
                    else if (System.Windows.Input.Keyboard.IsKeyUp(System.Windows.Input.Key.RightAlt))
                        Agk.KeyUp(262);
                    break;
            }

            if (key > 0 && key < 512)
                Agk.KeyUp(key);
        }

        static int TranslateKey(int key)
        {
            switch (key)
            {
                // Top Row 0-9
                case 48:
                case 49:
                case 50:
                case 51:
                case 52:
                case 53:
                case 54:
                case 55:
                case 56:
                case 57:
                    key += 215;
                    break;

                // Num pad 0-9
                case 96:
                case 97:
                case 98:
                case 99:
                case 100:
                case 101:
                case 102:
                case 103:
                case 104:
                case 105:
                    key -= 48;
                    break;

                case 16:
                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftShift))
                        key = 257;
                    else if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightShift))
                        key = 258;
                    break;

                case 17:
                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftCtrl))
                        key = 259;
                    else if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightCtrl))
                        key = 260;
                    break;

                case 18:
                    if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.LeftAlt))
                        key = 261;
                    else if (System.Windows.Input.Keyboard.IsKeyDown(System.Windows.Input.Key.RightAlt))
                        key = 262;
                    break;
            }

            return key;
        }

        static Char TranslateKeyToUnicode(Keys key)
        {
            Char result = new Char();

            byte[] keyboardState = new byte[255];
            bool keyboardStateStatus = GetKeyboardState(keyboardState);

            if (!keyboardStateStatus)
            {
                return result;
            }

            uint virtualKeyCode = (uint)key;
            uint scanCode = MapVirtualKey(virtualKeyCode, 0);
            IntPtr inputLocaleIdentifier = GetKeyboardLayout(0);

            StringBuilder txt = new StringBuilder();
            ToUnicodeEx(virtualKeyCode, scanCode, keyboardState, txt, (int)5, (uint)0, inputLocaleIdentifier);
            if (txt.Length > 0)
                result = txt[0];

            return result;

        }

        static void Core_OnClose(object sender, EventArgs e)
        {
            m_bBreakLoop = true;
        }

        static void Core_OnMouseMove(object sender, MouseEventArgs e)
        {
            Agk.MouseMove(0, e.X, e.Y);
        }

        static void Core_OnMouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) Agk.MouseLeftButton(0, 1);
            if (e.Button == MouseButtons.Right) Agk.MouseRightButton(0, 1);
            if (e.Button == MouseButtons.Middle) Agk.MouseMiddleButton(0, 1);
        }

        static void Core_OnMouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) Agk.MouseLeftButton(0, 0);
            if (e.Button == MouseButtons.Right) Agk.MouseRightButton(0, 0);
            if (e.Button == MouseButtons.Middle) Agk.MouseMiddleButton(0, 0);
        }

        static void Core_OnMouseWheel(object sender, MouseEventArgs e)
        {
            Agk.MouseWheel(0, (float)e.Delta / 40);
        }

        static void Core_OnSizeChanged(object sender, EventArgs e)
        {
            m_bMaximized = m_Window.WindowState == FormWindowState.Maximized;
            m_Window.TopMost = m_bMaximized;
            m_Window.Update();

            Agk.UpdateDeviceSize();
            Agk.WindowMoved();
        }

        static void Core_OnMove(object sender, EventArgs e)
        {
            Agk.WindowMoved();
        }

        static void Core_OnDeactivate(object sender, EventArgs e)
        {
            Agk.MouseLeftButton(0, 0);
            Agk.MouseRightButton(0, 0);
            Agk.MouseMiddleButton(0, 0);

            m_Window.TopMost = m_bMaximized;
            m_Window.Update();
            Agk.Paused();

            Agk.WindowMoved();
        }

        static void Core_OnActivated(object sender, EventArgs e)
        {
            m_Window.TopMost = m_bMaximized;
            m_Window.Update();
            Agk.Resumed();

            Agk.WindowMoved();
        }

        static void Core_OnGotFocus(object sender, EventArgs e)
        {
            Agk.WindowMoved();
        }

        static void Core_OnLostFocus(object sender, EventArgs e)
        {
            Agk.WindowMoved();
        }

        static void Core_OnKeyDown(object sender, KeyEventArgs e)
        {
            PassKeyDown((uint)e.KeyValue);

            if (!DisableEscape && e.KeyValue == 27)
                m_bBreakLoop = true;

            Char chr = TranslateKeyToUnicode(e.KeyCode);
            Agk.CharDown(chr);

            e.Handled = true;
        }

        static void Core_OnKeyUp(object sender, KeyEventArgs e)
        {
            PassKeyUp((uint)e.KeyValue);
        }

        public static bool InitAGK(Form window)
        {
            if (window.Handle != null)
                Agk.InitGL(window.Handle);

            return !m_bBreakLoop;
        }

        public static bool InitAGK()
        {
            if (m_Window.Handle != null)
                Agk.InitGL(m_Window.Handle);

            return !m_bBreakLoop;
        }

        public static bool LoopAGK()
        {
            Application.DoEvents();

            if (Agk.IsCapturingImage() > 0)
                System.Threading.Thread.Sleep(10);

            return !m_bBreakLoop;
        }

        public static void CleanUp()
        {
            Agk.CleanUp();
        }

        public static void Init(string[] args, string title)
        {
            //config defaults
            App.Config.LogLevel = 3;
            App.Config.Screen.Fullscreen = true;
            App.Config.Screen.Width = 0;
            App.Config.Screen.Height = 0;
            App.Config.Screen.Vsync = true;

            //config overrides
            foreach (string arg in args)
            {
                switch (arg.Split('=').First().ToLower())
                {
                    case "sw": App.Config.Screen.Width = Convert.ToInt32(arg.Split('=').Last()); break;
                    case "sh": App.Config.Screen.Height = Convert.ToInt32(arg.Split('=').Last()); break;
                    case "fs": App.Config.Screen.Fullscreen = Convert.ToBoolean(arg.Split('=').Last()); break;
                    case "vsync": App.Config.Screen.Vsync = Convert.ToBoolean(arg.Split('=').Last()); break;
                    case "log": App.Config.LogLevel = Convert.ToInt32(arg.Split('=').Last()); break;
                    default: break;
                }
            }

            //init the screen

            if (App.Config.Screen.Width == 0)
            {
                App.Config.Screen.Width = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Width;
                App.Config.Screen.Height = System.Windows.Forms.SystemInformation.PrimaryMonitorSize.Height;
            }

            App.Config.Screen.CenterX = (int)Math.Floor(App.Config.Screen.Width * 0.5);
            App.Config.Screen.CenterY = (int)Math.Floor(App.Config.Screen.Height * 0.5);

            App.CreateWindow(title, App.Config.Screen.Width, App.Config.Screen.Height, App.Config.Screen.Fullscreen);

            if (!App.InitAGK())
            {
                return;
            }

            Agk.SetResolutionMode(1);
            Agk.SetOrientationAllowed(0, 0, 1, 0);
            if (!App.Config.Screen.Fullscreen)
            {
                Agk.SetWindowAllowResize(0);
            }
            Agk.SetVirtualResolution(App.Config.Screen.Width, App.Config.Screen.Height);
            Agk.SetScissor(0, 0, 0, 0);

            Agk.SetVSync(App.Config.Screen.Vsync);
            if (!App.Config.Screen.Vsync)
            {
                Agk.SetSyncRate(0, 0);
            }

            //init AGK Misc
            Agk.SetPrintSize(16.0f);
            Agk.SetPrintColor(255, 255, 255);
            Agk.SetClearColor(0, 0, 0);
            Agk.UseNewDefaultFonts(1);
        }

    }

    public struct AppConfig
    {
        public int LogLevel;
        public ScreenConfig Screen;
    }

    public struct ScreenConfig
    {
        public int Width;
        public int Height;
        public int X1;
        public int Y1;
        public int X2;
        public int Y2;
        public int CenterX;
        public int CenterY;
        public int Layout;
        public bool Fullscreen;
        public bool Vsync;
    }
}
