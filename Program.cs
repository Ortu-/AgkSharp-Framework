using System;
using AgkSharp;
using AGKCore;
using System.Reflection;
using System.Linq;

namespace AgkSharp_Template
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            var attrs = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).FirstOrDefault() as AssemblyTitleAttribute;

            App.Init(args, attrs.Title);

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

            while (App.LoopAGK())
            {
                Agk.Sync();
            }

            App.CleanUp();
        }

    }
}
