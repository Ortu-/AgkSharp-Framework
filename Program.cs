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
            //--------------------------------------------
            #region boilerplate inits

            var attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).FirstOrDefault() as AssemblyTitleAttribute;

            //init Agk
            if(!App.Init(args, attrs.Title))
            {
                return;
            }

            //init modules
            new Hardware();
            new Media();
            new Devkit3d();
            new AnimationHandler();
            new CharacterHandler3d();
            new World3d();
            new Controls3d();
            new Camera3dHandler();
            new ArcHandler();

            new UI.UserInterface();
                UI.UserInterface.ControllerList.Add(new UI.CommonController());
                UI.UserInterface.ControllerList.Add(new UI.GameMenuController());
                UI.UserInterface.PreloadImages();

            //clean up
            UpdateHandler.SortUpdateList();

            //init 3D physics
            Agk.Create3DPhysicsWorld();

            App.Log("Program.cs", 3, "main", "> Init Complete");

            #endregion
            //--------------------------------------------

            //TEMP setups

            var floorObj = Agk.CreateObjectBox(40.0f, 2.0f, 40.0f);
            var floorEnt = new WorldEntity3d();
            floorEnt.Properties.ResourceNumber = floorObj;
            floorEnt.Properties.IsObject = true;
            var floorFx = Media.GetShaderAsset("media/shaders/SurfaceColorLit.vs", "media/shaders/SurfaceColorLit.ps", true);
            floorEnt.SetShader(floorFx);
            Agk.SetShaderConstantByName(floorFx.ResourceNumber, "u_color", 0.8f, 0.8f, 0.8f, 0);
            floorEnt.Properties.Position.Y = -1.0f;
            World3d.WorldEntityList.Add(floorEnt);
            Agk.Create3DPhysicsStaticBody(floorEnt.Properties.ResourceNumber);

            /*
            var ent = new WorldEntity3d();
            var tObj = Media.LoadObjectAsset("media/characters/woman.dae", true, false, Guid.NewGuid().ToString());
            Agk.SetObjectScalePermanent(tObj.ResourceNumber, 30.0f, 30.0f, 30.0f);
            Agk.FixObjectPivot(tObj.ResourceNumber);
            var tImg = Media.GetImageAsset("media/characters/woman_d.png", 1.0f, 1.0f);
            Agk.SetObjectImage(tObj.ResourceNumber, tImg.ResourceNumber, 0);
            tImg = Media.GetImageAsset("media/characters/woman_n.png", 1.0f, 1.0f);
            Agk.SetObjectImage(tObj.ResourceNumber, tImg.ResourceNumber, 1);
            tImg = Media.GetImageAsset("media/characters/woman_s.png", 1.0f, 1.0f);
            Agk.SetObjectImage(tObj.ResourceNumber, tImg.ResourceNumber, 2);
            ent.Properties.ResourceNumber = tObj.ResourceNumber;
            ent.Properties.IsObject = true;
            ent.Properties.Filebase = "media/characters/woman";
            var wFx = Media.GetShaderAsset("media/shaders/SurfaceDiffSpec.vs", "media/shaders/SurfaceDiffSpec.ps", true);
            ent.SetShader(wFx);
            World3d.WorldEntityList.Add(ent);
            */

            //var tChar = new CharacterEntity3d("media/characters/automata");
            var tChar = new CharacterEntity3d("media/characters/adventurer-carliet");
            CharacterHandler3d.MyCharacter = tChar;

            //Agk.SetObjectVisible(tChar.Properties.ResourceNumber, false);


            var cam0 = new Camera3d("main");
            cam0.UpdateFromAgk();
            cam0.ControlMode = Camera3d.CameraMode.Anchored;
            cam0.Anchor = floorEnt;
            cam0.Target = floorEnt;

            var cam1 = new Camera3d("myOtherCam");
            cam1.UpdateFromAgk();
            cam1.Position.X = 0.0f;
            cam1.Position.Y = 10.0f;
            cam1.Position.Z = -20.0f;
            cam1.ApplyToAgk();

            Controls3d.SetActiveCamera(cam0);

            //ENDTEMP

            //App main
            while (App.LoopAGK())
            {

                App.Log("Program.cs", 1, "main", "--- Begin main loop ---");

                //Always update timing, and do it first
                App.UpdateTiming();

                //sync internal physics
                Agk.Step3DPhysicsWorld();

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

                Agk.Print(Agk.ScreenFPS().ToString() + " " + App.Timing.Delta.ToString());
                Agk.Print("Camera: " + Controls3d.ActiveCamera.Name);
                Agk.Print("Cam Pos: " + Controls3d.ActiveCamera.Position.X + ", " + Controls3d.ActiveCamera.Position.Y + ", " + Controls3d.ActiveCamera.Position.Z);
                Agk.Print("Cam P/T: " + Controls3d.ActiveCamera.Phi + ", " + Controls3d.ActiveCamera.Theta);
                Agk.Print(CharacterHandler3d.MyCharacter.Properties.Position.AsString());
                Agk.Print(CharacterHandler3d.MyCharacter.Properties.Heading.ToString() + " " + CharacterHandler3d.MyCharacter.Properties.Facing.ToString());
                Agk.Print(CharacterHandler3d.MyCharacter.Properties.Speed.ToString());
                
                Agk.Print("Press C to toggle active camera");
                Agk.Print("F1 to debug light");
                Agk.Print("Esc to quit");

                /*
                var anim = CharacterHandler3d.MyCharacter.AnimationQ.First();
                Agk.Print(anim.Animation.Name);
                
                string animName = Agk.GetObjectAnimationName((uint)anim.Owner.Properties.ResourceNumber, 1);
                float totalTime = Agk.GetObjectAnimationDuration((uint)anim.Owner.Properties.ResourceNumber, animName);
                int totalFrame = (int)Math.Floor(totalTime * anim.Animation.Framerate);
                float animTime = (anim.CurrentFrame * totalTime) / totalFrame;

                Agk.Print(Math.Floor((double)anim.CurrentFrame).ToString() + " / " + totalFrame.ToString());
                Agk.Print(animTime.ToString("0.00") + " / " + totalTime.ToString("0.00"));
                */

                App.Sync();
            }

            App.CleanUp();
        }

    }

}
