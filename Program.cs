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

#if DEBUG
            App.Log("Program.cs", 3, "main", "> Init Complete");
#endif

            // display a background and center it
            uint bgImg = Agk.CreateSprite(Agk.LoadImage("media/background.jpg"));
            Agk.SetSpritePosition(bgImg, App.Config.Screen.CenterX - 160, App.Config.Screen.CenterY - 240);

            // create a sprite and center it
            Agk.CreateSprite(1, 0);
            Agk.SetSpritePosition(1, App.Config.Screen.CenterX - 30, App.Config.Screen.CenterY - 44);

            // add individual images into an animation list
            Agk.AddSpriteAnimationFrame(1, Agk.LoadImage("media/item0.png"));
            Agk.AddSpriteAnimationFrame(1, Agk.LoadImage("media/item1.png"));
            Agk.AddSpriteAnimationFrame(1, Agk.LoadImage("media/item2.png"));
            Agk.AddSpriteAnimationFrame(1, Agk.LoadImage("media/item3.png"));
            Agk.AddSpriteAnimationFrame(1, Agk.LoadImage("media/item4.png"));

            // play the sprite at 10 fps, looping, going from frame 1 to 5
            Agk.PlaySprite(1, 10.0f, 1, 1, 5);


            UI.Element tElement = new UI.Element();
            tElement.Style.SetProp("color", "#fff");
            tElement.Style.SetProp("color", "#ffffff");
            tElement.Style.SetProp("color", "#ffffffff");


            while (App.LoopAGK())
            {
#if DEBUG
                App.Log("Program.cs", 1, "main", "--- Begin main loop ---");
#endif
                //Always update timing, and do it first
                App.UpdateTiming();

                foreach(var u in App.UpdateList)
                {
                    if (!App.Status.IsRunning)
                    {
                        break;
                    }
                    if(App.Timing.PauseState != 1 || u.IgnorePause)
                    {
                        u.Run();
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
