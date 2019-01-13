﻿using System;
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

            App.AddKeyDownHandler();

            Scheduler.SetInterval(App.DoStuff, null, 10, 3000, 3000, null);

            while (App.LoopAGK())
            {
#if DEBUG
                App.Log("Program.cs", 1, "main", "--- Begin main loop ---");
#endif
                //Always update timing, and do it first
                App.UpdateTiming();

                Agk.Print(Agk.ScreenFPS());
                Agk.Print("Pause: " + App.Timing.PauseState.ToString());

                Agk.Print("Press A to toggle pause");

                Agk.Print("A is down: " + Data.GetBit(1, Hardware.Input[(int)System.Windows.Forms.Keys.A]));
                Agk.Print("A was down: " + Data.GetBit(2, Hardware.Input[(int)System.Windows.Forms.Keys.A]));

                Agk.Print("Left mouse is down: " + Data.GetBit(1, Hardware.Input[Hardware.MouseEnum((int)MouseButtons.Left)]));
                Agk.Print("Left mouse was down: " + Data.GetBit(2, Hardware.Input[Hardware.MouseEnum((int)MouseButtons.Left)]));

                Agk.Sync();
            }

            App.CleanUp();
        }

    }

}
