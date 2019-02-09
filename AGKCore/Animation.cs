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
        private static List<AnimationSet> _AnimationList = new List<AnimationSet>();

        public AnimationHandler()
        {
            
        }

        public static void LoadAnimation(string rFile)
        {
            string doc = System.IO.File.ReadAllText(rFile);
            _AnimationList.Add(new AnimationSet()
            {
                Name = rFile,
                Animations = JsonConvert.DeserializeObject<List<Animation>>(doc)
            });
        }

        public static void UnloadAllAnimation()
        {
            _AnimationList.Clear();
        }

    }

    public class AnimationSet
    {
        public string Name;
        public List<Animation> Animations = new List<Animation>();
    }

    public class Animation
    {
        public string Name;
        public int Framerate = 30;
        public bool IsMoveEnabled = true;
        public int VariantDelayMin = 15000;
        public int VariantDelayMax = 65000;
        public List<int[]> Keys = new List<int[]>();
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
