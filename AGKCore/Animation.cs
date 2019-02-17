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

        public static AnimationSet LoadAnimation(string rFile)
        {
            string doc = System.IO.File.ReadAllText(rFile);
            _AnimationList.Add(new AnimationSet()
            {
                Name = rFile,
                Animations = JsonConvert.DeserializeObject<List<Animation>>(doc)
            });
            return _AnimationList.Last();
        }

        public static void UnloadAllAnimation()
        {
            _AnimationList.Clear();
        }

        public static AnimationSet GetAnimationSet(string rFile)
        {
            var s = _AnimationList.FirstOrDefault(a => a.Name == rFile);
            if(s == null)
            {
                s = LoadAnimation(rFile);
            }
            return s;
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
        public int CurrentKey = 0;
        public int VariantDelayMin = 15000;
        public int VariantDelayMax = 65000;
        public List<int[]> Keys = new List<int[]>();
    }

    public class AppliedAnimation
    {
        dynamic Owner;

        public Animation Animation;
        public float Speed = 1.0f;
        public bool IsLoop = false;
        public string Callback;
        public string CallbackArgs;

        private float _CurrentFrame;
        private int _VariantMark;
        private int _VariantDelay;

        public AppliedAnimation(object rObject, AnimationSet rAnimSet, string rName)
        {
            Owner = rObject;
            Animation = rAnimSet.Animations.FirstOrDefault(a => a.Name == rName);
            if(Animation == null)
            {
                App.Log("animation.cs", 4, "error", "adding " + rName + " from " + JsonConvert.SerializeObject(rAnimSet));
                App.StopRunning(true);
            }
        }

        public bool Update()
        {
            int firstFrame = Animation.Keys[Animation.CurrentKey][0];
            int lastFrame = Animation.Keys[Animation.CurrentKey][1];

            _CurrentFrame = _CurrentFrame + (Animation.Framerate * Speed * (App.Timing.Delta * 0.001f));

            if (_CurrentFrame < firstFrame)
            {
                _CurrentFrame = firstFrame;
            }

            if (_CurrentFrame >= lastFrame)
            {
                if (IsLoop)
                {
                    //handle frame, looping
                    _CurrentFrame = firstFrame;

                    if (Owner.Properties.IsObject)
                    {
                        float animTime = (_CurrentFrame + Agk.GetObjectAnimationDuration((uint)Owner.Properties.ResourceNumber, "")) / Animation.Keys[Animation.CurrentKey][1];
                        Agk.SetObjectAnimationFrame((uint)Owner.Properties.ResourceNumber, "", animTime, 0.0f);
                    }
                    else
                    {
                        Agk.SetSpriteFrame((uint)Owner.Properties.ResourceNumber, (int)_CurrentFrame);
                    }

                    //handle variant
                    if(Animation.Keys.Count > 1)
                    {
                        if(App.Timing.Timer - _VariantMark >= _VariantDelay)
                        {
                            Animation.CurrentKey = (int)Agk.Random(0, (uint)(Animation.Keys.Count - 1));
                            _VariantDelay = Animation.VariantDelayMin + (int)Agk.Random(0, (uint)Animation.VariantDelayMax);
                            _VariantMark = (int)App.Timing.Timer;
                        }
                    }

                    //handle callback
                    bool isLoopDone = false;
                    if (!String.IsNullOrEmpty(Callback))
                    {
                        isLoopDone = (bool)Dispatcher.Invoke(Callback, CallbackArgs);
                        if (isLoopDone)
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    //handle frame, played once, end it
                    _CurrentFrame = lastFrame;

                    if (Owner.Properties.IsObject)
                    {
                        float animTime = (_CurrentFrame + Agk.GetObjectAnimationDuration((uint)Owner.Properties.ResourceNumber, "")) / Animation.Keys[Animation.CurrentKey][1];
                        Agk.SetObjectAnimationFrame((uint)Owner.Properties.ResourceNumber, "", animTime, 0.0f);
                    }
                    else
                    {
                        Agk.SetSpriteFrame((uint)Owner.Properties.ResourceNumber, (int)_CurrentFrame);
                    }

                    //handle callback
                    if (!String.IsNullOrEmpty(Callback))
                    {
                        Dispatcher.Invoke(Callback, CallbackArgs);
                    }
                    return true;
                }
            }
            else
            {
                //handle frame, sequence hasnt completed, continue
                if (Owner.Properties.IsObject)
                {
                    float animTime = (_CurrentFrame + Agk.GetObjectAnimationDuration((uint)Owner.Properties.ResourceNumber, "")) / Animation.Keys[Animation.CurrentKey][1];
                    Agk.SetObjectAnimationFrame((uint)Owner.Properties.ResourceNumber, "", animTime, 0.0f);
                }
                else
                {
                    Agk.SetSpriteFrame((uint)Owner.Properties.ResourceNumber, (int)_CurrentFrame);
                }

                //handle callback
                bool isLoopDone = false;
                if (!String.IsNullOrEmpty(Callback))
                {
                    isLoopDone = (bool)Dispatcher.Invoke(Callback, CallbackArgs);
                    if (isLoopDone)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

    }

}
