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

            new Devkit3d();
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

            var boxNum = Agk.CreateObjectBox(3.0f, 3.0f, 3.0f);
            var boxEnt = new WorldEntity3d();
            boxEnt.Properties.ResourceNumber = boxNum;
            boxEnt.Properties.IsObject = true;

            //var fx = Media.GetShaderAsset("media/shaders/HelloWorld.vs", "media/shaders/HelloWorld.ps", true); //solid color, no lighting
            //var fx = Media.GetShaderAsset("media/shaders/SurfaceColor.vs", "media/shaders/SurfaceColor.ps", true); //solid color, default lighting
            //var fx = Media.GetShaderAsset("media/shaders/SurfaceDiffuse.vs", "media/shaders/SurfaceDiffuse.ps", true); //diffuse texture, default lighting
            //var fx = Media.GetShaderAsset("media/shaders/SurfaceDiffuseClip.vs", "media/shaders/SurfaceDiffuseClip.ps"); //diffuse texture, default lighting, 1 bit alpha

            Agk.SetSunActive(0);
            Agk.SetAmbientColor(0, 0, 0);
            var fx = Media.GetShaderAsset("media/shaders/SurfaceDiffuseClipLit.vs", "media/shaders/SurfaceDiffuseClipLit.ps", true); //diffuse texture, custom VS lighting, 1 bit alpha
            boxEnt.SetShader(fx);
            

            var tImg = Media.GetImageAsset("media/props/barrel01_d.png", 1.0f, 1.0f);
            Agk.SetObjectImage(boxEnt.Properties.ResourceNumber, tImg.ResourceNumber, 0);


            Agk.SetClearColor(52, 125, 217);

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

            var colorMark = App.Timing.Timer;

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

                /* TEMP - For SurfaceColor shader 
                if (App.Timing.Timer - colorMark > 200)
                {
                    colorMark = App.Timing.Timer;
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "appliedColor", (Agk.Random(0, 255) / 255.0f), (Agk.Random(0, 255) / 255.0f), (Agk.Random(0, 255) / 255.0f), 0.0f);
                }
                */

                
                //TEMP - For SurfaceDiffuseClipLit - TODO we dont want to apply this every loop, only when lighting changes, also handle multiple lights
                var dirLight = World3d.Celestials.First().LightProperties;
                foreach (var s in Media.ShaderList)
                {
                    if (s.ReceiveDirectionalLight)
                    {
                        Agk.SetShaderConstantByName(s.ResourceNumber, "dirLightDirection", dirLight.Direction.X, dirLight.Direction.Y, dirLight.Direction.Z, 0);
                        Agk.SetShaderConstantByName(s.ResourceNumber, "dirLightDiffuse", dirLight.Diffuse.R, dirLight.Diffuse.G, dirLight.Diffuse.B, 0);
                        Agk.SetShaderConstantByName(s.ResourceNumber, "dirLightAmbient", dirLight.Ambient.R, dirLight.Ambient.G, dirLight.Ambient.B, 0);
                    }
                }
                

                Agk.Print(Agk.ScreenFPS());
                Agk.Print("Camera: " + Controls3d.ActiveCamera.Name);
                Agk.Print("Cam Pos: " + Controls3d.ActiveCamera.Position.X + ", " + Controls3d.ActiveCamera.Position.Y + ", " + Controls3d.ActiveCamera.Position.Z);
                Agk.Print("Cam P/T: " + Controls3d.ActiveCamera.Phi + ", " + Controls3d.ActiveCamera.Theta);
                Agk.Print("Press C to toggle active camera");
                Agk.Print("F1 to debug light");
                Agk.Print("Esc to quit");

                App.Sync();
            }

            App.CleanUp();
        }

    }

}
