using System;
using AgkSharp;
using AGKCore;
using System.Reflection;
using System.Linq;
using UI = AGKCore.UI;
using AGKProject;

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
            new AnimationHandler();
            new CharacterHandler2d();
            new UI.UserInterface();
                UI.UserInterface.ControllerList.Add(new UI.CommonController());
                UI.UserInterface.ControllerList.Add(new UI.GameMenuController());
                UI.UserInterface.PreloadImages();

            //clean up
            UpdateHandler.SortUpdateList();

            App.Log("Program.cs", 3, "main", "> Init Complete");

            //TEMP setups
            UI.Element tElement = new UI.Element();
            tElement.Id = "sky-panel";
            tElement.Style.SetProp("width", "320px");
            tElement.Style.SetProp("height", "480px");
            tElement.Style.SetProp("background-image", "media/background.jpg");
            tElement.Style.SetProp("position-alignH", "center");
            tElement.Style.SetProp("position-alignV", "center");
            tElement.SetParent("root");
            UI.UserInterface.ElementList.Add(tElement);

            Dispatcher.Add(App.DoStuff);

            var tEntity = new CharacterEntity2d("media/characters/balloon", 60, 88);
            tEntity.Properties.Position.X = 50;
            tEntity.Properties.Position.Y = 100;
            Agk.SetSpritePosition(tEntity.Properties.ResourceNumber, tEntity.Properties.Position.X, tEntity.Properties.Position.Y);
            var animSet = AnimationHandler.GetAnimationSet("media/characters/balloon.anim");
            var tAnim = new AppliedAnimation(tEntity, animSet, "0113L")
            {
                IsLoop = true
            };
            tEntity.AnimationQ.Add(tAnim);
            CharacterHandler2d.MyCharacter = tEntity;
            
            
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

                Agk.Sync();
            }

            App.CleanUp();
        }

    }

}
