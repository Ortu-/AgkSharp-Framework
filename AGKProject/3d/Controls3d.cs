using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using AGKCore;
using AGKCore.UI;
using System.Windows.Forms;

namespace AGKProject
{
    public class Controls3d
    {
        public static Camera3d ActiveCamera;
        public static AssignedKeyMovementData MovementKey = new AssignedKeyMovementData();

        public Controls3d()
        {
            Dispatcher.Add(Controls3d.GetGameplayInput);
            App.UpdateList.Add(new UpdateHandler("Controls3d.GetGameplayInput", null, false));
        }

        public class AssignedKeyMovementData
        {
            public int Forward = (int)Keys.W;
            public int Backward = (int)Keys.S;
            public int Left = (int)Keys.A;
            public int Right = (int)Keys.D;
            public int Upward = (int)Keys.Space;
            public int Downward = (int)Keys.V;
            public int SpeedBoost = (int)Keys.ShiftKey;
            public int SpeedSlow = (int)Keys.ControlKey;
            public int RunToggle = (int)Keys.R;
            public int StrafeToggle = (int)Keys.T;
            public int OffsetToggle = (int)Keys.B;

            public AssignedKeyMovementData()
            {
                if (!System.IO.File.Exists("config.ini"))
                {
                    return;
                }
                string l = "";
                using (var f = new System.IO.StreamReader("config.ini"))
                {
                    while((l = f.ReadLine()) != null)
                    {
                        var c = l.Split('=');
                        switch (c[0].ToLower())
                        {
                            case "keymap_forward": Forward = Convert.ToInt32(c[1]); break;
                            case "keymap_backward": Backward = Convert.ToInt32(c[1]); break;
                            case "keymap_left": Left = Convert.ToInt32(c[1]); break;
                            case "keymap_right": Right = Convert.ToInt32(c[1]); break;
                            case "keymap_upward": Upward = Convert.ToInt32(c[1]); break;
                            case "keymap_downward": Downward = Convert.ToInt32(c[1]); break;
                            case "keymap_speedboost": SpeedBoost = Convert.ToInt32(c[1]); break;
                            case "keymap_speedslow": SpeedSlow = Convert.ToInt32(c[1]); break;
                            case "keymap_runtoggle": RunToggle = Convert.ToInt32(c[1]); break;
                            case "keymap_strafetoggle": StrafeToggle = Convert.ToInt32(c[1]); break;
                            case "keymap_offsettoggle": OffsetToggle = Convert.ToInt32(c[1]); break;
                        }
                    }
                    f.Dispose();
                }
            }
        }


        public static void SetActiveCamera(Camera3d cam)
        {
            ActiveCamera = cam;
            cam.IsActive = true;
            cam.ApplyToAgk();
        }


        public static void GetGameplayInput(object rArgs)
        {
            App.Log("controls3d", LogLevel.Notice, "main", "> Begin GetGameplayInput");

            //reset forced
            UserInterface.Status.MouseModeForced = "";
            UserInterface.Status.KeyModeForced = "";

            //exit early if user does not have control
            /*
            if(App.Status.LoadState < 3)
            {
                return;
            }
            */

            if (CharacterHandler3d.MyCharacter == null)
            {
                return;
            }

            if (!CharacterHandler3d.MyCharacter.Status.IsLiving)
            {
                return;
            }

            if (UserInterface.Status.KeyMode != "gameplay" || CharacterHandler3d.MyCharacter.Status.MotionState == CharacterEntity3d.MotionStates.Forced)
            {
                return;
            }

            if(ActiveCamera.ControlMode != Camera3d.CameraMode.Anchored)
            {
                return;
            }

            bool animWait = false;
            if(CharacterHandler3d.MyCharacter.AnimationQ.Count > 0)
            {
                animWait = CharacterHandler3d.MyCharacter.AnimationQ.First().Animation.Wait;
            }

            //grab old state
            CharacterHandler3d.MyCharacter.OldStatus = JsonConvert.DeserializeObject<CharacterEntity3d.CharacterStatus>(JsonConvert.SerializeObject(CharacterHandler3d.MyCharacter.Status));

            //reset states that need to be held by user
            if (CharacterHandler3d.MyCharacter.Status.StanceState == CharacterEntity3d.StanceStates.Sneaking)
            {
                CharacterHandler3d.MyCharacter.Status.StanceState = CharacterEntity3d.StanceStates.Basic;
            }

            if (CharacterHandler3d.MyCharacter.Status.ActionState == CharacterEntity3d.ActionStates.Moving)
            {
                CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Idle;
            }

            if(CharacterHandler3d.MyCharacter.Status.MotionState != CharacterEntity3d.MotionStates.Forced)
            {
                CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Stationary;
            }

            CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;

            //get mouse input
            if (UserInterface.Status.MouseMode == "gameplay")
            {

            }

            //get keyboard input
            if(UserInterface.Status.KeyMode == "gameplay" && !animWait)
            {
                if(CharacterHandler3d.MyCharacter.Status.MotionState != CharacterEntity3d.MotionStates.Forced)
                {
                    //toggle strafelock
                    if(Hardware.IsKeyDown(MovementKey.StrafeToggle) && UserInterface.Status.InputReady)
                    {
                        UserInterface.Status.InputMark = App.Timing.Timer;
                        CharacterHandler3d.MyCharacter.Status.IsStrafeLocked = !CharacterHandler3d.MyCharacter.Status.IsStrafeLocked;
                    }

                    //toggle runlock
                    if (Hardware.IsKeyDown(MovementKey.RunToggle) && UserInterface.Status.InputReady)
                    {
                        UserInterface.Status.InputMark = App.Timing.Timer;
                        CharacterHandler3d.MyCharacter.Status.IsRunLocked = !CharacterHandler3d.MyCharacter.Status.IsRunLocked;
                    }

                    //apply run if locked
                    if(CharacterHandler3d.MyCharacter.Status.IsRunLocked 
                        && CharacterHandler3d.MyCharacter.OldStatus.StanceState == CharacterEntity3d.StanceStates.Basic
                    )
                    {
                        if(CharacterHandler3d.MyCharacter.OldStatus.ActionState == CharacterEntity3d.ActionStates.Moving 
                            && CharacterHandler3d.MyCharacter.OldStatus.MotionState < CharacterEntity3d.MotionStates.Sprinting
                        )
                        {
                            CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Running;
                        }
                    }

                    //sneak
                    if (Hardware.IsKeyDown(MovementKey.SpeedSlow))
                    {
                        CharacterHandler3d.MyCharacter.Status.StanceState = CharacterEntity3d.StanceStates.Sneaking;
                    }

                    //sprint
                    if (Hardware.IsKeyDown(MovementKey.SpeedBoost))
                    {
                        if(CharacterHandler3d.MyCharacter.Status.StanceState == CharacterEntity3d.StanceStates.Basic
                            && CharacterHandler3d.MyCharacter.OldStatus.ActionState == CharacterEntity3d.ActionStates.Moving
                            && CharacterHandler3d.MyCharacter.Status.MotionState != CharacterEntity3d.MotionStates.Forced
                        )
                        {
                            CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Sprinting;
                        }
                    }

                    //jump
                    if (Hardware.IsKeyDown(MovementKey.Upward) && UserInterface.Status.InputReady)
                    {
                        //don't allow a jump if sneaking or already in a jump
                        if(CharacterHandler3d.MyCharacter.Status.StanceState == CharacterEntity3d.StanceStates.Basic 
                            && CharacterHandler3d.MyCharacter.Status.ActionState < CharacterEntity3d.ActionStates.Jumping
                        )
                        {
                            UserInterface.Status.InputMark = App.Timing.Timer;
                            CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Jumping;
                            CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Forced;
                        }
                    }

                    float camY = MathUtil.Wrap(Agk.GetCameraAngleY(1) + 180.0f, 0.0f, 360.0f);

                    //forward
                    if (Hardware.IsKeyDown(MovementKey.Forward))
                    {
                        CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                        if(CharacterHandler3d.MyCharacter.Status.MotionState == CharacterEntity3d.MotionStates.Stationary)
                        {
                            CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Walking;
                        }
                        CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                        CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                    }

                    //backward
                    if (Hardware.IsKeyDown(MovementKey.Backward))
                    {
                        CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                        if (CharacterHandler3d.MyCharacter.Status.MotionState == CharacterEntity3d.MotionStates.Stationary)
                        {
                            CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Walking;
                        }
                        if (CharacterHandler3d.MyCharacter.Status.IsStrafeLocked)
                        {
                            CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                            CharacterHandler3d.MyCharacter.Properties.Heading = camY;
                            CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Backward;
                        }
                        else
                        {
                            CharacterHandler3d.MyCharacter.Properties.Facing = camY;
                            CharacterHandler3d.MyCharacter.Properties.Heading = camY;
                        }
                    }

                    //left
                    if (Hardware.IsKeyDown(MovementKey.Left))
                    {
                        CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                        if (CharacterHandler3d.MyCharacter.Status.MotionState == CharacterEntity3d.MotionStates.Stationary)
                        {
                            CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Walking;
                        }
                        if (CharacterHandler3d.MyCharacter.Status.IsStrafeLocked)
                        {
                            if (Hardware.IsKeyDown(MovementKey.Forward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 180.0f - 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                            else if (Hardware.IsKeyDown(MovementKey.Backward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Backward;
                            }
                            else
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 90.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Left;
                            }
                        }
                        else
                        {
                            if (Hardware.IsKeyDown(MovementKey.Forward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f - 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 180.0f - 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                            else if (Hardware.IsKeyDown(MovementKey.Backward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                            else
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 90.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 90.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                        }
                    }

                    //right
                    if (Hardware.IsKeyDown(MovementKey.Right))
                    {
                        CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                        if (CharacterHandler3d.MyCharacter.Status.MotionState == CharacterEntity3d.MotionStates.Stationary)
                        {
                            CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Walking;
                        }
                        if (CharacterHandler3d.MyCharacter.Status.IsStrafeLocked)
                        {
                            if (Hardware.IsKeyDown(MovementKey.Forward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 180.0f + 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                            else if (Hardware.IsKeyDown(MovementKey.Backward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY - 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Backward;
                            }
                            else
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY - 90.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Right;
                            }
                        }
                        else
                        {
                            if (Hardware.IsKeyDown(MovementKey.Forward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY + 180.0f + 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY + 180.0f + 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                            else if (Hardware.IsKeyDown(MovementKey.Backward))
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY - 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY - 45.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                            else
                            {
                                CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.Wrap(camY - 90.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Properties.Heading = MathUtil.Wrap(camY - 90.0f, 0.0f, 360.0f);
                                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Standard;
                            }
                        }
                    }

                }
            }

            //mouse steers character if moving and not strafe locked
            if (UserInterface.Status.MouseMode == "gameplay")
            {
                if(CharacterHandler3d.MyCharacter.Status.ActionState == CharacterEntity3d.ActionStates.Moving
                    && !CharacterHandler3d.MyCharacter.Status.IsStrafeLocked
                )
                {
                    CharacterHandler3d.MyCharacter.Properties.Facing = MathUtil.CurveAngle(CharacterHandler3d.MyCharacter.Properties.Facing + Hardware.Mouse.MoveX, CharacterHandler3d.MyCharacter.Properties.Facing, ActiveCamera.Precision);
                }
            }

            //sprint camera effect
            if(CharacterHandler3d.MyCharacter.Status.MotionState == CharacterEntity3d.MotionStates.Sprinting)
            {
                ActiveCamera.FOV = MathUtil.CurveValue(65.0f, ActiveCamera.FOV, Agk.ScreenFPS() * CharacterHandler3d.Config.SprintAcceleration);
            }
            else if(ActiveCamera.FOV != Camera3dHandler.DefaultFOV)
            {
                ActiveCamera.FOV = MathUtil.CurveValue(Camera3dHandler.DefaultFOV, ActiveCamera.FOV, Agk.ScreenFPS() * CharacterHandler3d.Config.SprintAcceleration);
            }





            //TEMP:
            if (Hardware.IsKeyDown((int)Keys.Y))
            {
                Controls3d.ActiveCamera.Offset.Y += 0.1f;
            }
            if (Hardware.IsKeyDown((int)Keys.H))
            {
                Controls3d.ActiveCamera.Offset.Y -= 0.1f;
            }

            //TEMP: toggle active
            if (Hardware.IsKeyDown((int)Keys.C) && UserInterface.Status.InputReady)
            {
                UserInterface.Status.InputMark = App.Timing.Timer;
                Controls3d.SetActiveCamera(Camera3dHandler.CameraList.FirstOrDefault(c => c.Name != Controls3d.ActiveCamera.Name));
            }
            //ENDTEMP




            App.Log("Controls3d", LogLevel.Notice, "main", "> End GetGameplayInput");
        }



    }
    
}
