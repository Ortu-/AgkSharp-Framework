using System;
using AgkSharp;
using AGKCore;
using System.Reflection;
using System.Linq;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using UI = AGKCore.UI;

namespace AgkSharp_Template
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).FirstOrDefault() as AssemblyTitleAttribute;

            //init Agk
            if(!App.Init(args, attrs.Title))
            {
                return;
            }

            //init modules
            var _ah = new AnimationHandler();
            var _ui = new UI.UserInterface();
            UI.UserInterface.ControllerList.Add(new UI.CommonController());
            UI.UserInterface.ControllerList.Add(new UI.GameMenuController());
            UI.UserInterface.PreloadImages();

            //clean up
            UpdateHandler.SortUpdateList();


            App.Log("Program.cs", 3, "main", "> Init Complete");


            UI.Element tElement = new UI.Element();
            tElement.Id = "sky-panel";
            tElement.Style.SetProp("width", "320px");
            tElement.Style.SetProp("height", "480px");
            tElement.Style.SetProp("background-image", "media/background.jpg");
            tElement.Style.SetProp("position-alignH", "center");
            tElement.Style.SetProp("position-alignV", "center");
            tElement.SetParent("root");
            UI.UserInterface.ElementList.Add(tElement);

            tElement = new UI.Element();
            tElement.Id = "balloon";
            tElement.Style.SetProp("width", "60px");
            tElement.Style.SetProp("height", "88px");
            tElement.Style.SetProp("background-image", "media/item0.png|media/item1.png|media/item2.png|media/item3.png|media/item4.png");
            tElement.Style.SetProp("position-alignH", "center");
            tElement.Style.SetProp("position-alignV", "center");
            tElement.SetParent("sky-panel");
            tElement.EnableEvents = 1;
            tElement.HoldMouseFocus = true;
            tElement.OnPress = "App.DoStuff";
            UI.UserInterface.ElementList.Add(tElement);

            Dispatcher.Add(App.DoStuff);

            /*
            UI.UserInterface.ElementDrag.DragElement = tElement;
            UI.UserInterface.ElementDrag.IsActive = true;
            UI.UserInterface.ElementDrag.OffsetX = 30;
            UI.UserInterface.ElementDrag.OffsetY = 40;
            */


            var testMark = App.Timing.Timer;

            while (App.LoopAGK())
            {

                App.Log("Program.cs", 1, "main", "--- Begin main loop ---");

                //Always update timing, and do it first
                App.UpdateTiming();

                App.Log("Program.cs", 1, "main", "Processing " + App.UpdateList.Count.ToString() + " updates in queue");

                foreach (var u in App.UpdateList)
                {
                    if (!App.Status.IsRunning)
                    {
                        break;
                    }
                    if(App.Timing.PauseState != 1 || u.IgnorePause)
                    {

                        App.Log("Program.cs", 1, "main", "Process from queue " + u.FunctionName);

                        Dispatcher.Invoke(u.FunctionName, null);
                    }
                }
                if (!App.Status.IsRunning)
                {
                    break;
                }

                Agk.Print(Agk.ScreenFPS());

                Agk.Sync();
            }

            App.CleanUp();
        }

    }

}
