using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using AGKCore;

namespace AGKProject
{
    public class CharacterHandler2d
    {
        public static CharacterConfig Config = new CharacterConfig();
        public static List<CharacterEntity2d> CharacterList = new List<CharacterEntity2d>();
        public static CharacterEntity2d MyCharacter;

        public CharacterHandler2d()
        {
            Dispatcher.Add(CharacterHandler2d.UpdateCharacters);
            App.UpdateList.Add(new UpdateHandler("CharacterHandler2d.UpdateCharacters", "Controls2d.GetGameplayInput", false));
        }

        public static void UpdateCharacters(object rArgs)
        {
            foreach(var c in CharacterList)
            {
                c.Update();
            }
        }

        public class CharacterConfig
        {
            public float NormalAcceleration = 1.0f;
            public float NormalDeceleration = 3.0f;
            public float NormalMaxSpeed = 0.08f;
        }

    }

    public class CharacterEntity2d : Entity
    {

        public CharacterProperties Properties = new CharacterProperties();
        public CharacterStatus Status = new CharacterStatus();
        public CharacterStatus OldStatus = new CharacterStatus();

        public CharacterEntity2d(string rFileBase, int rWidth, int rHeight)
        {
            Properties.IsObject = false;
            Properties.Filebase = rFileBase;

            ImageAsset tSheet = Media.GetImageAsset(rFileBase + ".png", 1.0f, 1.0f);
            Agk.Swap();
            var spSheet = Agk.CreateSprite(tSheet.ResourceNumber);
            Agk.SetSpritePosition(spSheet, 0.0f, 0.0f);
            Agk.Render();
            bool isFirst = true;
            for(int row = 0; row < (Agk.GetImageHeight(tSheet.ResourceNumber) / rHeight); row++)
            {
                for (int col = 0; col < (Agk.GetImageWidth(tSheet.ResourceNumber) / rWidth); col++)
                {
                    var tImg = Agk.GetImage(col * rWidth, row * rHeight, rWidth, rHeight);
                    if (isFirst)
                    {
                        Properties.ResourceNumber = Agk.CreateSprite(tImg);
                        isFirst = false;
                    }
                    Agk.AddSpriteAnimationFrame(Properties.ResourceNumber, tImg);
                }
            }
            Agk.DeleteSprite(spSheet);
            Agk.ClearScreen();
            Agk.Swap();

            CharacterHandler2d.CharacterList.Add(this);
        }

        public void Update()
        {
            if (!Status.IsLiving)
            {
                return;
            }

            //gather current data
            float oldPosX = Properties.Position.X;
            float oldPosY = Properties.Position.Y;
            float oldPosZ = Properties.Position.Z;
            float oldFacing = Properties.Facing;

            var animSet = AnimationHandler.GetAnimationSet(Properties.Filebase + ".anim");

            //a character should NEVER not have an animation.
            if (AnimationQ.Count == 0)
            {
                var tAnim = new AppliedAnimation(this, animSet, "0000L") //add basic idle
                {
                    IsLoop = true,
                    Speed = 1.0f
                };
                AnimationQ.Add(tAnim);
            }

            string tAnimState = AnimationQ[0].Animation.Name.Substring(0, 4); 
            string tAnimStage = AnimationQ[0].Animation.Name.Substring(4, 1); //I L O

            //get new position
            switch (Status.ActionState)
            {
                case ActionStates.Idle:
                    
                    //TODO momentum overrides

                    Properties.Speed = 0.0f;
                    Status.MotionState = MotionStates.Stationary;
                    break;
                case ActionStates.Moving:
                    Properties.Speed = CharacterHandler2d.Config.NormalMaxSpeed;
                    break;
            }

            Properties.Position.X = Properties.Position.X + Agk.Sin(Properties.Heading) * Properties.Speed * App.Timing.Delta;
            Properties.Position.Y = Properties.Position.Y + Agk.Cos(Properties.Heading) * Properties.Speed * App.Timing.Delta;
            Agk.SetSpritePosition(Properties.ResourceNumber, Properties.Position.X, Properties.Position.Y);
            Agk.SetSpriteDepth(Properties.ResourceNumber, Properties.ZIndex);

            //get new animation state
            string tState = ((int)Status.StanceState).ToString() + ((int)Status.ActionState).ToString() + ((int)Status.MotionState).ToString() + ((int)Status.DirectionState).ToString();
            string tOldState = ((int)OldStatus.StanceState).ToString() + ((int)OldStatus.ActionState).ToString() + ((int)OldStatus.MotionState).ToString() + ((int)OldStatus.DirectionState).ToString();
            string tBaseStance = ((int)Status.StanceState).ToString();
            string oldBaseStance = ((int)OldStatus.StanceState).ToString();
            string tBaseState = tBaseStance + "000";
            string keyBaseIdleStationaryUp = ((int)ActionStates.Idle).ToString() + ((int)MotionStates.Stationary).ToString() + ((int)DirectionStates.Up).ToString(); //for convenience

            if(tState != tAnimState && tAnimState + "O" != AnimationQ[0].Animation.Name && tAnimState + "I" != AnimationQ[0].Animation.Name)
            {
                //animation state has changed, clear the current animation stack and rebuild under the new state
                AnimationQ.Clear();

                if (tState == tBaseState && tBaseStance == oldBaseStance)
                {
                    //transition old state out
                    var tAnimO = new AppliedAnimation(this, animSet, tAnimState + "O")
                    {
                        IsLoop = false,
                    };
                    AnimationQ.Add(tAnimO);

                    //transition new state in
                    if (Status.StanceState != OldStatus.StanceState)
                    {
                        var tAnimI = new AppliedAnimation(this, animSet, tState + "I")
                        {
                            IsLoop = false,
                        };
                        AnimationQ.Add(tAnimI);
                    }
                }
                else
                {
                    //dont need to transition old state out, just transition new state in

                    if (false) //TODO: handle jumping and other complex states, may not always proceed to state In
                    {

                    }
                    else
                    {
                        if (Status.StanceState != OldStatus.StanceState)
                        {
                            var tAnimII = new AppliedAnimation(this, animSet, tState + "I")
                            {
                                IsLoop = false,
                            };
                            AnimationQ.Add(tAnimII);
                        }
                    }
                }

                //always proceed into state Loop -> characters should never be static!
                var tAnimL = new AppliedAnimation(this, animSet, tState + "L")
                {
                    IsLoop = true,
                };
                AnimationQ.Add(tAnimL);
            }

            //update animation frame
            bool isDone = AnimationQ[0].Update();
            if (isDone)
            {
                AnimationQ.RemoveAt(0);
            }
        }

        public class CharacterProperties : EntityProperties
        {
            public int ZIndex = 8;
            public float Rotation = 0.0f;
        }

        public class CharacterStatus
        {
            public bool IsLiving = true;
            public StanceStates StanceState = StanceStates.Basic;
            public ActionStates ActionState = ActionStates.Idle;
            public MotionStates MotionState = MotionStates.Normal;
            public DirectionStates DirectionState = DirectionStates.Up;
        }

        public enum StanceStates
        {
            Basic
        }
        public enum ActionStates
        {
            Idle,
            Moving,
            Falling
        }
        public enum MotionStates
        {
            Stationary,
            Normal,
            Forced
        }
        public enum DirectionStates
        {
            Up,
            Right,
            Left,
            Down
        }
    }
    
}
