using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using AGKCore;

namespace AGKProject
{
    public class CharacterHandler3d
    {
        public static CharacterConfig Config = new CharacterConfig();
        public static List<CharacterEntity3d> CharacterList = new List<CharacterEntity3d>();
        public static CharacterEntity3d MyCharacter;

        public CharacterHandler3d()
        {
            Dispatcher.Add(CharacterHandler3d.UpdateCharacters);
            App.UpdateList.Add(new UpdateHandler("CharacterHandler3d.UpdateCharacters", "Controls3d.GetGameplayInput", false));
        }

        public static void UpdateCharacters(object rArgs)
        {
            App.Log("CharacterEntity3d", LogLevel.Notice, "main", "> Begin UpdateCharacters");

            foreach (var c in CharacterList)
            {
                c.Update();
            }

            App.Log("CharacterEntity3d", LogLevel.Notice, "main", "> End UpdateCharacters");
        }

        public static void UnloadAllCharacters()
        {
            //STUB
        }

        public class CharacterConfig
        {
            public float NormalAcceleration = 0.266f;
            public float NormalDeceleration = 0.532f;
            public float NormalMaxSpeed = 85.0f;
            public float NormalTurnSpeed = 0.133f;

            public float RunMaxSpeed = 280.0f;

            public float SprintAcceleration = 0.4f;
            public float SprintMaxSpeed = 500.0f;

            public float SneakAcceleration = 0.066f;
            public float SneakMaxSpeed = 45.0f;

            public float CollisionBufferDistance = 25.0f;
        }

    }

    public partial class CharacterEntity3d : Entity
    {

        public CharacterProperties Properties = new CharacterProperties();
        public CharacterStatus Status = new CharacterStatus();
        public CharacterStatus OldStatus = new CharacterStatus();
        public CharacterBio Bio = new CharacterBio();

        public CharacterEntity3d(string rFileBase)
        {
            var tObj = Media.LoadObjectAsset(rFileBase + ".dae", false, true, Guid.NewGuid().ToString());
            Agk.SetObjectScalePermanent(tObj.ResourceNumber, 30.0f, 30.0f, 30.0f);
            Agk.RotateObjectGlobalY(tObj.ResourceNumber, 180.0f);
            Agk.FixObjectPivot(tObj.ResourceNumber);

            var fx = Media.GetShaderAsset("media/shaders/SurfaceDiffSpecBone.vs", "media/shaders/SurfaceDiffSpec.ps", true);
            Agk.SetShaderConstantByName(fx.ResourceNumber, "u_normalSize", 1.0f, 0, 0, 0);
            Agk.SetShaderConstantByName(fx.ResourceNumber, "u_specularPower", 10.0f, 0, 0, 0);
            Agk.SetObjectShader(tObj.ResourceNumber, fx.ResourceNumber);

            var tImg = Media.GetImageAsset(rFileBase + "_d.png", 1.0f, 1.0f);
            Agk.SetObjectImage(tObj.ResourceNumber, tImg.ResourceNumber, 0);
            tImg = Media.GetImageAsset(rFileBase + "_n.png", 1.0f, 1.0f);
            Agk.SetObjectImage(tObj.ResourceNumber, tImg.ResourceNumber, 1);
            tImg = Media.GetImageAsset(rFileBase + "_s.png", 1.0f, 1.0f);
            Agk.SetObjectImage(tObj.ResourceNumber, tImg.ResourceNumber, 2);

            Properties.ResourceNumber = tObj.ResourceNumber;
            Properties.IsObject = true;
            Properties.Filebase = rFileBase;
            Properties.Shader = fx;
            Properties.Height = Agk.GetObjectSizeMaxY(Properties.ResourceNumber) - Agk.GetObjectSizeMinY(Properties.ResourceNumber);

            CharacterHandler3d.CharacterList.Add(this);
        }

        public void Update()
        {
            if (!Status.IsLiving)
            {
                return;
            }

            #region begin

            //gather current data
            float oldPosX = Properties.Position.X;
            float oldPosY = Properties.Position.Y;
            float oldPosZ = Properties.Position.Z;
            float oldFacing = Agk.GetObjectWorldAngleY(Properties.ResourceNumber);

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

            string tAnimState = AnimationQ[0].Animation.Name.Substring(0, 4).ToLower(); 
            string tAnimStage = AnimationQ[0].Animation.Name.Substring(4, 1).ToLower(); //I L O
            #endregion

            #region handle vitals
            if (Status.Health.Max > 0)
            {
                if(App.Timing.Timer - Status.Health.RegenMark > Status.Health.RegenInterval)
                {
                    Status.Health.Current += (int)Math.Floor(Status.Health.RegenAmount);
                    if(Status.Health.Current > Status.Health.Max)
                    {
                        Status.Health.Current = Status.Health.Max;
                    }
                    Status.Health.RegenMark = App.Timing.Timer;
                }
            }
            if(Status.Stamina.Max > 0)
            {
                if(App.Timing.Timer - Status.Stamina.RegenMark > Status.Stamina.RegenInterval)
                {
                    //stamina if affected by locomotion and exhaustion
                    if(Status.MotionState == MotionStates.Sprinting)
                    {
                        //sprint costs stamina
                        Status.Stamina.Current -= 2;
                    }
                    else if(Status.MotionState != MotionStates.Running)
                    {
                        //run suppresses regen, otherwise recover
                        Status.Stamina.Current += (int)Math.Floor(Status.Stamina.RegenAmount);
                    }
                    if (Status.Stamina.Current > Status.Stamina.Max)
                    {
                        Status.Stamina.Current = Status.Stamina.Max;
                    }
                    if(Status.Stamina.Current > Status.Stamina.Max * 0.2f)
                    {
                        //clear exhaustion if stamina has recovered > 1/5 of max
                        Status.IsExhausted = false;
                    }
                    if (Status.Stamina.Current <= 0)
                    {
                        //if stamina runs out, apply exhaustion
                        Status.Stamina.Current = 0;
                        Status.IsExhausted = true;
                        if(Status.MotionState == MotionStates.Sprinting)
                        {
                            //drop out of sprint if exhausted
                            if (Status.IsRunLocked)
                            {
                                Status.MotionState = MotionStates.Running;
                            }
                            else
                            {
                                Status.MotionState = MotionStates.Walking;
                            }
                        }
                    }
                }
            }
            #endregion

            #region handle position

            bool mustCalcPosition = true;

            //update speed
            switch (Status.ActionState)
            {
                case ActionStates.Idle:
                    
                    if(Status.MotionState == MotionStates.Walking)
                    {
                        //immediate stop when walking cus aint no body got time for iceskating
                        Properties.Speed = 0.0f;
                    }

                    if(Properties.Speed > 0.0f)
                    {
                        //new state is idle, meaning we stopped, but we have speed meaning we were moving
                        //immediately drop from sprint to run, but if running transition to walk over time.
                        //this means if speed remains, we also have to enforce a moving state
                        if(Properties.Speed > CharacterHandler3d.Config.NormalMaxSpeed)
                        {
                            Properties.Speed = MathUtil.CurveValue(10.0f, Properties.Speed, Agk.ScreenFPS() * CharacterHandler3d.Config.NormalDeceleration * 0.5f);
                            Status.MotionState = MotionStates.Running;
                            Status.ActionState = ActionStates.Moving;
                        }
                        else if(OldStatus.MotionState == MotionStates.Running)
                        {
                            Properties.Speed = MathUtil.CurveValue(10.0f, Properties.Speed, Agk.ScreenFPS() * CharacterHandler3d.Config.NormalDeceleration);
                            Status.MotionState = MotionStates.Walking;
                            Status.ActionState = ActionStates.Moving;
                        }
                        else
                        {
                            Properties.Speed = 0.0f;
                        }
                    }
                    if(Properties.Speed < 25.0)
                    {
                        //cut the curve transitions short if they get low enough, it feels better
                        Properties.Speed = 0.0f;
                        Status.MotionState = MotionStates.Stationary;
                        Status.ActionState = ActionStates.Idle;
                    }
                    break;

                case ActionStates.Moving:

                    switch (Status.MotionState)
                    {
                        case MotionStates.Walking:
                            if(Status.StanceState == StanceStates.Sneaking)
                            {
                                Properties.Speed = MathUtil.CurveValue(CharacterHandler3d.Config.SneakMaxSpeed * Properties.SlopeModifier, Properties.Speed, Agk.ScreenFPS() * CharacterHandler3d.Config.SneakAcceleration);
                            }
                            else
                            {
                                Properties.Speed = MathUtil.CurveValue(CharacterHandler3d.Config.NormalMaxSpeed * Properties.SlopeModifier, Properties.Speed, Agk.ScreenFPS() * CharacterHandler3d.Config.NormalAcceleration);
                            }
                            break;

                        case MotionStates.Running:
                            Properties.Speed = MathUtil.CurveValue(CharacterHandler3d.Config.RunMaxSpeed * Properties.SlopeModifier, Properties.Speed, Agk.ScreenFPS() * CharacterHandler3d.Config.NormalAcceleration);
                            break;

                        case MotionStates.Sprinting:
                            Properties.Speed = MathUtil.CurveValue(CharacterHandler3d.Config.SprintMaxSpeed * Properties.SlopeModifier, Properties.Speed, Agk.ScreenFPS() * CharacterHandler3d.Config.SprintAcceleration);
                            break;
                    }

                    if (Status.MotionState != MotionStates.Forced)
                    {
                        //if a slope or other effect has reduced our speed to less than half of the max run speed, enforce a walking state. it looks weird to run or sprint in slow motion...
                        if (Properties.Speed <= (CharacterHandler3d.Config.RunMaxSpeed * 0.5f) && (Status.MotionState == MotionStates.Running || Status.MotionState == MotionStates.Sprinting))
                        {
                            Status.MotionState = MotionStates.Walking;
                        }

                        //if slope is too steep to maintain forward speed, enforce stoppage. we don't want to crawl glacially slow up a steep hill and we don't want to generate a bunch of collision data
                        if (Properties.Speed < (CharacterHandler3d.Config.SneakMaxSpeed * 1.5f) && Properties.SlopeModifier < 1.0f)
                        {
                            //TODO: apply slide downslope instead of just stopping
                            //NOTE: slide down will need to set to a forced state / ability animation which takes the character far enough down to avoid jitter
                            //			handling this through a forced move ability can probably deprecate the need for slopeLock. can just use the falling animation key

                            Properties.Speed = 0.0f;
                            Status.MotionState = MotionStates.Stationary;
                            Status.ActionState = ActionStates.Idle;
                            Status.IsSlopeLocked = true;
                        }

                        if (Properties.SlopeModifier > 1.00f)
                        {
                            Status.IsSlopeLocked = false;
                        }
                    }

                    break;

                case ActionStates.Jumping:

                    //jumps are complex and composed of multiple stages
                    if(OldStatus.ActionState == ActionStates.Jumping)
                    {
                        //if jump is in a prep stage, position is controlled by character handler. while in flight/fall position is controlled by arc handler
                        if(tAnimStage != "i")
                        {
                            mustCalcPosition = false;
                            Properties.Position = Properties.ArcPath.GetPosition(App.Timing.Timer, App.Timing.PauseElapsed);

                            //jump is in flight/fall -- are we ascending or descending?
                            if(Properties.Position.Y < oldPosY)
                            {
                                Status.ActionState = ActionStates.Falling;
                            }
                        }
                    }
                    break;

                case ActionStates.Falling:
                    mustCalcPosition = false;
                    Properties.Position = Properties.ArcPath.GetPosition(App.Timing.Timer, App.Timing.PauseElapsed);
                    break;

                
            }

            if (mustCalcPosition)
            {
                Properties.Position.X += Agk.Sin(Properties.Heading) * Properties.Speed * (App.Timing.Delta * 0.001f); //speed is in terms of units per second, delta is in ms
                Properties.Position.Z += Agk.Cos(Properties.Heading) * Properties.Speed * (App.Timing.Delta * 0.001f);
            }

            #endregion

            #region handle collision
            /*
            var colRay = new CollisionRay();
            int collisionState = 0;

            //lateral slide
            if(Status.ActionState > ActionStates.Idle)
            {
                colRay.SphereCast
                (
                    new AGKVector3(oldPosX, oldPosY + (Properties.Height * 0.5f), oldPosZ),
                    new AGKVector3(Properties.Position.X, Properties.Position.Y + (Properties.Height * 0.5f), Properties.Position.Z),
                    CharacterHandler3d.Config.CollisionBufferDistance, true
                );
                if (colRay.HitObjectNumber > 0)
                {
                    Properties.Position.X = colRay.HitPosition.X;
                    Properties.Position.Z = colRay.HitPosition.Z;
                    collisionState = 1;
                }
            }

            //vertical down

            float tHeight = 0.0f;
            //TODO: tHeight = get terrain height

            colRay.RayCast
            (
                new AGKVector3(Properties.Position.X, Properties.Position.Y + 100.0f, Properties.Position.Z),
                new AGKVector3(Properties.Position.X, Properties.Position.Y - 100.0f, Properties.Position.Z)
            );

            //TODO: compare collision height to terrain height
            //TEMP: just use col
            if (colRay.HitObjectNumber > 0)
            {
                tHeight = colRay.HitPosition.Y;
            }

            if (Properties.Position.Y < tHeight)
            {
                Properties.Position.Y = tHeight;
                collisionState = 2;
            }

            if (Properties.ArcPath != null)
            {
                if (collisionState == 1)
                {
                    //hit a vertical surface, change arc to fall straight down
                    Properties.ArcPath.Dispose();
                    Properties.ArcPath = ArcHandler.Create(this, 400.0f, 270.0f, Properties.Heading);
                    Status.ActionState = ActionStates.Falling;
                }
                else if(collisionState == 2)
                {
                    //hit a horizontal surface, end the arc
                    Properties.ArcPath.Dispose();
                    Status.ActionState = ActionStates.Idle;
                    Status.MotionState = MotionStates.Stationary;
                }
            }
            else
            {
                //no arc, just force to horizontal surface
                Properties.Position.Y = tHeight;

                //slope / cliff ?
                var tDiff = oldPosY - Properties.Position.Y;
                if(tDiff < 0.0f && Status.IsSlopeLocked)
                {
                    //negative diff means ascending

                    if(App.Timing.Timer - Status.SlopeMark > 200)
                    {
                        Properties.SlopeModifier = 0.0f;
                    }
                    Properties.Speed = 0.0f;
                    Status.ActionState = ActionStates.Idle;
                    Status.MotionState = MotionStates.Stationary;
                    Properties.Position.X = oldPosX;
                    Properties.Position.Y = oldPosY;
                    Properties.Position.Z = oldPosZ;
                }
                else if(tDiff > Properties.Height * 0.5f)
                {
                    //positive diff means descending, if descending more than waist height, change to falling

                    Properties.Position.Y = oldPosY;
                    Properties.ArcPath = ArcHandler.Create(this, 100.0f + Properties.Speed, 30.0f, Properties.Heading);
                    Status.ActionState = ActionStates.Falling;
                    Status.StanceState = StanceStates.Basic;
                    Status.MotionState = MotionStates.Forced;
                }
                else
                {
                    //NOTE: negative tDiff# indicates character moved uphill, larger the negative, the steeper the hill.
                    //NOTE: slopeMod is a speed increase 1.3 = speed * 1.3, 0.0 = stop.
                    //TODO: use a fixed length sample, currently rate of movement and framerate affect the slope result.
                    //NOTE: slopeMark / threshold smooth against jitter
                    if(App.Timing.Timer - Status.SlopeMark > 200)
                    {
                        float slopeThreshold = -0.4f;
                        if(tDiff > 0.0f || tDiff < slopeThreshold)
                        {
                            Properties.SlopeModifier = 1.0f + tDiff;
                            if(Properties.SlopeModifier < 0.0f) { Properties.SlopeModifier = 0.0f; }
                            if(Properties.SlopeModifier < 1.3f) { Properties.SlopeModifier = 1.3f; }
                        }
                        else
                        {
                            Properties.SlopeModifier = 1.0f;
                        }
                    }

                    //TEMP: disable slopeMod
                    Properties.SlopeModifier = 1.0f;
                }
            }
            */
            #endregion

            #region apply to object

            Agk.SetObjectRotation(Properties.ResourceNumber, 0.0f, MathUtil.CurveAngle(Properties.Facing, oldFacing, Agk.ScreenFPS() * CharacterHandler3d.Config.NormalTurnSpeed), 0.0f);
            Agk.SetObjectPosition(Properties.ResourceNumber, Properties.Position.X, Properties.Position.Y, Properties.Position.Z);
            
            #endregion

            #region handle animation 

            //get new animation state
            string tState = ((int)Status.StanceState).ToString() + ((int)Status.ActionState).ToString() + ((int)Status.MotionState).ToString() + ((int)Status.DirectionState).ToString();
            string tOldState = ((int)OldStatus.StanceState).ToString() + ((int)OldStatus.ActionState).ToString() + ((int)OldStatus.MotionState).ToString() + ((int)OldStatus.DirectionState).ToString();
            string tBaseStance = ((int)Status.StanceState).ToString();
            string oldBaseStance = ((int)OldStatus.StanceState).ToString();
            string tBaseState = tBaseStance + "000";
            string keyBaseIdleStationaryUp = ((int)ActionStates.Idle).ToString() + ((int)MotionStates.Stationary).ToString() + ((int)DirectionStates.Standard).ToString(); //for convenience

            if (tState != tAnimState && tAnimState + "O" != AnimationQ[0].Animation.Name && tAnimState + "I" != AnimationQ[0].Animation.Name)
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

                    if (Status.ActionState == ActionStates.Jumping) 
                    {
                        float tJumpAngle;
                        if (Properties.Speed > 0.0f)
                        {
                            tJumpAngle = 30.0f;
                        }
                        else
                        {
                            tJumpAngle = 90.0f;
                        }
                        dynamic jumpArgs = new
                        {
                            Angle = tJumpAngle,
                            Speed = 400.0f + Properties.Speed,
                            Owner = this
                        };
                        if(Properties.Speed > CharacterHandler3d.Config.NormalMaxSpeed)
                        {
                            //if running/sprinting, skip stage i and go straight into jump proper
                            AddJump(jumpArgs);
                        }
                        else
                        {
                            //walking or stationary, we need a bit of animation prep. for the feels.
                            var tAnimI = new AppliedAnimation(this, animSet, tState + "I")
                            {
                                IsLoop = false,
                                Callback = "CharacterEntity3d.AddJump",
                                CallbackArgs = JsonConvert.SerializeObject(jumpArgs)
                            };
                            AnimationQ.Add(tAnimI);
                        }
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

            #endregion

        }

        public void AddJump(object rArgs)
        {

        }


        public class CharacterProperties : EntityProperties
        {
            public ShaderAsset Shader;
            public AGKVector3 Rotation;
            public ArcPath ArcPath;
            public float SlopeModifier = 1.0f;
            public float Height;
        }

        public class CharacterStatus
        {
            public bool IsLiving = true;
            public bool IsExhausted = false;
            public CharacterVital Health = new CharacterVital(100, 500, 1.0f);
            public CharacterVital Stamina = new CharacterVital(100, 250, 1.0f);

            public bool IsStrafeLocked = false;
            public bool IsRunLocked = false;
            public bool IsSlopeLocked = true;
            public uint SlopeMark;
            public StanceStates StanceState = StanceStates.Basic;
            public ActionStates ActionState = ActionStates.Idle;
            public MotionStates MotionState = MotionStates.Stationary;
            public DirectionStates DirectionState = DirectionStates.Standard;

            public string GetAnimationState(bool isFull)
            {
                if (isFull)
                {
                    return StanceState.ToString() + ActionState.ToString() + MotionState.ToString() + DirectionState.ToString();
                }
                else
                {
                    return ((int)StanceState).ToString() + ((int)ActionState).ToString() + ((int)MotionState).ToString() + ((int)DirectionState).ToString();
                }
            }
        }

        public class CharacterVital
        {
            public int Current;
            public int Max;
            public uint RegenMark;
            public int RegenInterval;
            public float RegenAmount;

            public CharacterVital(int max, int intv, float amt)
            {
                Current = max;
                Max = max;
                RegenMark = App.Timing.Timer;
                RegenInterval = intv;
                RegenAmount = amt;
            }
        }

        public class CharacterBio
        {
            public string Name;
            public string Gender;
            public string Voice;
            public string Archetype;
            public string CharacterClass;
            public string IsUnique;
        }

        public enum StanceStates
        {
            Basic,
            Sneaking
        }
        public enum ActionStates
        {
            Idle,
            Moving,
            Jumping,
            Falling
        }
        public enum MotionStates
        {
            Stationary,
            Walking,
            Running,
            Sprinting,
            Forced
        }
        public enum DirectionStates
        {
            Standard,
            Right,
            Left,
            Backward
        }

    }
    
}
