using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using AgkSharp;
using Newtonsoft.Json;

namespace AGKCore
{
    public static class App
    {
        static Form m_Window;
        static bool m_bMaximized = false;
        public static bool DisableEscape { get; set; } = false;

        public static AppConfig Config;
        public static AppStatus Status;
        public static TimingStatus Timing;
        public static List<UpdateHandler> UpdateList = new List<UpdateHandler>();

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

            m_Window.MouseDown += Hardware.OnMouseDown;
            m_Window.MouseUp += Hardware.OnMouseUp;
            m_Window.MouseWheel += Hardware.OnMouseWheel;
            m_Window.MouseMove += Hardware.OnMouseMove;
            m_Window.KeyDown += Hardware.OnKeyDown;
            m_Window.KeyUp += Hardware.OnKeyUp;

            m_Window.SizeChanged += Core_OnSizeChanged;
            m_Window.Move += Core_OnMove;
            m_Window.Activated += Core_OnActivated;
            m_Window.Deactivate += Core_OnDeactivate;
            m_Window.GotFocus += Core_OnGotFocus;
            m_Window.LostFocus += Core_OnLostFocus;
            m_Window.FormClosing += Core_OnClose;

            m_Window.Show();

            return m_Window;
        }

        static void Core_OnClose(object sender, EventArgs e)
        {
            App.Status.IsRunning = false;
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

        public static bool InitAGK(Form window)
        {
            if (window.Handle != null)
                Agk.InitGL(window.Handle);

            return true;
        }

        public static bool InitAGK()
        {
            if (m_Window.Handle != null)
                Agk.InitGL(m_Window.Handle);

            return true;
        }

        public static bool LoopAGK()
        {
            Application.DoEvents();

            if (Agk.IsCapturingImage() > 0)
                System.Threading.Thread.Sleep(10);

            return App.Status.IsRunning;
        }

        public static void CleanUp()
        {
            Agk.CleanUp();
        }

        public static bool Init(string[] args, string title)
        {
            Dispatcher.Add(App.Log);
            
            App.Status.LoadState = 1;
            App.Status.LoadStage = 1;

            //config defaults
            App.Config.Log.Level = 3;
            App.Config.Log.File = System.AppDomain.CurrentDomain.BaseDirectory + "app.log";
            //App.Config.Log.Channels = "*";
            App.Config.Log.Channels = "|!|error|main|ui|";

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
                    case "log": App.Config.Log.Level = Convert.ToInt32(arg.Split('=').Last()); break;
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

            if (App.Config.Screen.Width > 1280)
            {
                App.Config.Screen.Layout = 1;
            }
            else {
                App.Config.Screen.Layout = 2;
            }

            App.CreateWindow(title, App.Config.Screen.Width, App.Config.Screen.Height, App.Config.Screen.Fullscreen);

            if (!App.InitAGK())
            {
                return false;
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

            //init log
            System.IO.File.WriteAllText(App.Config.Log.File, "Timestamp    | File                     | Level | Channel    | Log" + Environment.NewLine);
            System.IO.File.AppendAllText(App.Config.Log.File, "==================================================================" + Environment.NewLine);

            App.Status.IsRunning = true;
            return true;
        }

        public static void UpdateTiming()
        {
            App.Timing.Timer = Agk.GetMilliseconds();
            App.Timing.Delta = (uint)Agk.Abs(App.Timing.Timer - App.Timing.Delta);

            int oldPause = App.Timing.PauseState;
            if(App.Timing.PauseHold == 0)
            {
                App.Timing.PauseState = 0;
            }
            else
            {
                App.Timing.PauseState = 1;
                if(oldPause == 0)
                {
                    App.Timing.PauseMark = App.Timing.Timer;
                }
                else
                {
                    App.Timing.PauseElapsed = (uint)Agk.Abs(App.Timing.Timer - App.Timing.PauseMark);
                }
            }

            if(App.Timing.PauseState == 2)
            {
                //pause ended and elapsed time has been applied, clear down
                App.Timing.PauseState = 0;
                App.Timing.PauseMark = 0;
                App.Timing.PauseElapsed = 0;
            }

            if(App.Timing.PauseState == 0 && oldPause == 1)
            {
                //pause ended, resume all updates but keep the elapsed time available for paused updates to adjust against elapsed times.
                App.Timing.PauseState = 2;
                Hardware.Mouse.MoveX = 0.0f;
                Hardware.Mouse.MoveY = 0.0f;
                Hardware.Mouse.MoveZ = 0.0f;
            }
        }

        public static void Log(string rSource, int rLevel, string rChannel, string rContent)
        {
            if(rLevel >= App.Config.Log.Level)
            {
                if(App.Config.Log.Channels == "*" || App.Config.Log.Channels.Contains("|" + rChannel + "|"))
                {
                    System.IO.File.AppendAllText(App.Config.Log.File, DateTime.Now.ToString("HH:mm:ss.fff") + " | " + rSource.PadRight(24) + " | " + rLevel.ToString().PadRight(5) + " | " + rChannel.PadRight(10) + " | " + rContent + Environment.NewLine);
                }
            }
        }
        
        public static void Log(object rArgs)
        {
            dynamic a = JsonConvert.DeserializeObject<dynamic>(rArgs.ToString());
            Log(a.Source.ToString(), (int)a.Level, a.Channel.ToString(), a.Content.ToString());
        }

        public static void StopRunning(bool rError)
        {
            App.Log("Core.cs", 255, "!", "Program end. Error: " + rError.ToString());
            if (rError)
            {
                MessageBox.Show("The application has encountered an error.\r\nCheck the log for details.");
            }
            App.Status.IsRunning = false;
        }
        
        public static void DoStuff(object rArgs)
        {
            MessageBox.Show("doing stuff");
        }

        public static void DoStuff2(object sender, KeyEventArgs e)
        {
            if(Data.GetBit(1, Hardware.Input[(int)Keys.A]) == 1)
            {
                App.Timing.PauseHold = Data.SetBit((int)TimingStatus.PauseType.Player, App.Timing.PauseHold, 1 - Data.GetBit((int)TimingStatus.PauseType.Player, App.Timing.PauseHold));
            }
        }

        public static void AddKeyDownHandler()
        {
            m_Window.KeyDown += DoStuff2;
        }

    }

    public struct AppConfig
    {
        public LogConfig Log;
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

    public struct LogConfig
    {
        public int Level;
        public string Channels;
        public string File;
    }

    public struct AppStatus
    {
        public bool IsRunning;
        public int LoadState; //0 not loaded | 1 loading | 2 title loop (UI only) | 3 level load finished | 4 game in progress | 5 level reload/transition
        public int LoadStage; //0 init resources | 1 transition in | 2 body | 3 transition out
        public int LoadType; //0 not loading | 1 title load | 2 level load	
    }

    public struct TimingStatus
    {
        public uint Timer;
        public uint Delta;
        public uint UpdateMark;
        public uint PauseMark;
        public uint PauseElapsed;
        public int PauseState;
        public int PauseHold;
        public enum PauseType
        {
            None,
            System,
            UI,
            Player
        }
    }

    public class UpdateHandler
    {
        public bool IgnorePause;
        public string FunctionName;
        public string Required;

        public UpdateHandler(string rName, string rRequired, bool rIgnore)
        {
            FunctionName = rName;
            Required = rRequired;
            IgnorePause = rIgnore;
        }

        public static void AddRequiredToUpdate(string rName, string rRequired)
        {
            var tUpdate = App.UpdateList.FirstOrDefault(u => u.FunctionName == rName);
            if (String.IsNullOrEmpty(tUpdate.Required))
            {
                tUpdate.Required = rRequired;
            }
            else
            {
                tUpdate.Required += "," + rRequired;
            }
        }

        public static void SortUpdateList()
        {
#if DEBUG
            App.Log("Core.cs", 2, "main", "Begin sorting update queue");
#endif            
            var updateQueue = new List<UpdateHandler>();
            for (int i = App.UpdateList.Count - 1; i >= 0; i--)
            {
                if (String.IsNullOrEmpty(App.UpdateList[i].Required))
                {
#if DEBUG
                    App.Log("Core.cs", 2, "main", "> moved " + App.UpdateList[i].FunctionName + " to queue");
#endif
                    updateQueue.Add(App.UpdateList[i]);
                    App.UpdateList.RemoveAt(i);
                }
            }
#if DEBUG
            App.Log("Core.cs", 2, "main", "  finished updates without requirements. " + App.UpdateList.Count.ToString() + " remain");
#endif
            while (App.UpdateList.Count > 0)
            {
                for(int i = 0; i < App.UpdateList.Count; i++)
                {
                    var req = App.UpdateList[i].Required.Split(',');
                    int okCount = 0;
                    int reqCount = req.Length;
                    foreach(var r in req)
                    {
                        foreach(var u in updateQueue)
                        {
                            if(u.FunctionName == r)
                            {
                                ++okCount;
                            }
                        }
                    }

                    if(okCount == reqCount)
                    {
#if DEBUG
                        App.Log("Core.cs", 2, "main", "> moved " + App.UpdateList[i].FunctionName + " to queue");
#endif
                        updateQueue.Add(App.UpdateList[i]);
                        App.UpdateList.RemoveAt(i);
                        break;
                    }
                }
            }

            App.UpdateList = new List<UpdateHandler>(updateQueue);
#if DEBUG
            App.Log("Core.cs", 2, "main", "End sorting update queue");
#endif     
        }

    }

}
