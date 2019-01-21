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

            if(!App.Init(args, attrs.Title))
            {
                return;
            }

            var ui = new UI.UserInterface();

            UpdateHandler.SortUpdateList();

#if DEBUG
            App.Log("Program.cs", 3, "main", "> Init Complete");
#endif

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
            UI.UserInterface.ElementList.Add(tElement);

            /*
            UI.Element tElement = new UI.Element();
            tElement.Style.SetProp("width", "100px");
            tElement.Style.SetProp("height", "40px");
            tElement.Style.SetProp("background-color", "#f00");
            tElement.Style.SetProp("text-decoration", "bold");
            tElement.Style.SetProp("position-alignH", "center");
            tElement.Style.SetProp("position-alignV", "bottom");
            tElement.Style.SetProp("margin-bottom", "20%");
            tElement.Style.SetProp("text-alignH", "center");
            tElement.Style.SetProp("text-alignV", "center");
            tElement.Value = "Hi there!";
            tElement.SetParent("root");
            UI.UserInterface.ElementList.Add(tElement);
            */

            while (App.LoopAGK())
            {
#if DEBUG
                App.Log("Program.cs", 1, "main", "--- Begin main loop ---");
#endif
                //Always update timing, and do it first
                App.UpdateTiming();
#if DEBUG
                App.Log("Program.cs", 1, "main", "Processing " + App.UpdateList.Count.ToString() + " updates in queue");
#endif
                foreach (var u in App.UpdateList)
                {
                    if (!App.Status.IsRunning)
                    {
                        break;
                    }
                    if(App.Timing.PauseState != 1 || u.IgnorePause)
                    {
#if DEBUG
                        App.Log("Program.cs", 1, "main", "Process from queue " + u.FunctionName);
#endif
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
