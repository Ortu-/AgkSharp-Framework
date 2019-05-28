using System;
using AgkSharp;
using AGKCore;
using System.Reflection;
using System.Linq;
using UI = AGKCore.UI;
using AGKProject;
using System.Windows.Forms;

namespace AgkSharp_Template
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            #region boilerplate init region

            var attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).FirstOrDefault() as AssemblyTitleAttribute;

            //init Agk
            if(!App.Init(args, attrs.Title))
            {
                return;
            }

            //init modules
            new Hardware();
            new Media();
            new AnimationHandler();

            //new CharacterHandler2d();
            //new Controls2d();

            new World3d();
            new Controls3d();
            new Camera3dHandler();

            new UI.UserInterface();
                UI.UserInterface.ControllerList.Add(new UI.CommonController());
                UI.UserInterface.ControllerList.Add(new UI.GameMenuController());
                UI.UserInterface.PreloadImages();

            //clean up
            UpdateHandler.SortUpdateList();

            App.Log("Program.cs", 3, "main", "> Init Complete");

            #endregion

            //TEMP setups

            var boxNum = Agk.CreateObjectBox(5.0f, 5.0f, 5.0f);
            var boxEnt = new WorldEntity3d();
            boxEnt.Properties.ResourceNumber = boxNum;
            boxEnt.Properties.IsObject = true;
            
            var cam0 = new Camera3d("main");
            cam0.UpdateFromAgk();
            cam0.Anchor = boxEnt;
            cam0.Target = boxEnt;

            var cam1 = new Camera3d("myOtherCam");
            cam1.UpdateFromAgk();
            cam1.Position.X = 10.0f;
            cam1.Position.Y = 10.0f;
            cam1.Position.Z = -40.0f;
            cam1.ApplyToAgk();
            Controls3d.ActiveCamera = cam1;

            //ENDTEMP

            //App main
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
                Agk.Print("Camera: " + Controls3d.ActiveCamera.Name);
                Agk.Print(Controls3d.ActiveCamera.Position.X + ", " + Controls3d.ActiveCamera.Position.Y + ", " + Controls3d.ActiveCamera.Position.Z);
                Agk.Print(Controls3d.ActiveCamera.Phi + ", " + Controls3d.ActiveCamera.Theta);
                Agk.Print(Hardware.Mouse.MoveX.ToString() + ", " + Hardware.Mouse.MoveY.ToString());
                Agk.Print("Press C to toggle active camera, Esc to quit");

                Agk.Sync();
            }

            App.CleanUp();
        }

    }

}
