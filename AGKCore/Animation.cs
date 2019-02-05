using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AGKCore
{
    public class AnimationHandler
    {
        private List<Animation> _animationList = new List<Animation>();

        public AnimationHandler()
        {
            
        }

        public static void LoadAnimation(string rFilename)
        {

        }

        public static void UnloadAllAnimation()
        {

        }

    }

    public class Animation
    {
        public string FileName;
        public string Name;
        public int FirstFrame;
        public int LastFrame;
        public int Framerate = 30;
        public bool IsMoveEnabled;
        //public bool IsVariantEnabled; // TODO: hold list of variants?
        public int VariantDelayMin = 15000;
        public int VariantDelayMax = 65000;
    }

    public class AppliedAnimation
    {
        public Animation Animation;
        public float Speed = 1.0f;
        public bool IsLoop = false;
        public string Callback;
        public string CallbackArgs;
        public float CurrentFrame;
        public int VariantMark;
        public int VariantDelay;

        public AppliedAnimation()
        {

        }
    }

}
