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

        public Controls3d()
        {
            Dispatcher.Add(Controls3d.GetGameplayInput);
            App.UpdateList.Add(new UpdateHandler("Controls3d.GetGameplayInput", null, false));
        }

        public static void GetGameplayInput(object rArgs)
        {
            /*
            if(CharacterHandler3d.MyCharacter == null)
            {
                return;
            }
            */
            /*
            //exit early if main gameplay not in progress
            if(App.Status.LoadState < 3)
            {
                return;
            }
            */

            //grab old state
            //CharacterHandler3d.MyCharacter.OldStatus = JsonConvert.DeserializeObject<CharacterEntity3d.CharacterStatus>(JsonConvert.SerializeObject(CharacterHandler3d.MyCharacter.Status));

            //reset forced
            UserInterface.Status.MouseModeForced = "";
            UserInterface.Status.KeyModeForced = "";
            /*
            //exit early if user does not have control
            if (UserInterface.Status.KeyMode != "gameplay" || CharacterHandler3d.MyCharacter.Status.MotionState == CharacterEntity3d.MotionStates.Forced)
            {
                return;
            }

            if (!CharacterHandler3d.MyCharacter.Status.IsLiving)
            {
                return;
            }

            //reset states that need to be held by user
            if(CharacterHandler3d.MyCharacter.Status.ActionState == CharacterEntity3d.ActionStates.Moving)
            {
                CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Idle;
            }

            if(CharacterHandler3d.MyCharacter.Status.MotionState != CharacterEntity3d.MotionStates.Forced)
            {
                CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Stationary;
            }

            CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Up;
            
            //move up
            if (Hardware.IsKeyDown((int)Keys.W))
            {
                CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Up;
                CharacterHandler3d.MyCharacter.Properties.Facing = 180.0f;
                CharacterHandler3d.MyCharacter.Properties.Heading = 180.0f;
            }

            //move right
            if (Hardware.IsKeyDown((int)Keys.D))
            {
                CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Right;
                CharacterHandler3d.MyCharacter.Properties.Facing = 90.0f;
                CharacterHandler3d.MyCharacter.Properties.Heading = 90.0f;
            }

            //move left
            if (Hardware.IsKeyDown((int)Keys.A))
            {
                CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Left;
                CharacterHandler3d.MyCharacter.Properties.Facing = 270.0f;
                CharacterHandler3d.MyCharacter.Properties.Heading = 270.0f;
            }

            //move down
            if (Hardware.IsKeyDown((int)Keys.S))
            {
                CharacterHandler3d.MyCharacter.Status.ActionState = CharacterEntity3d.ActionStates.Moving;
                CharacterHandler3d.MyCharacter.Status.DirectionState = CharacterEntity3d.DirectionStates.Down;
                CharacterHandler3d.MyCharacter.Properties.Facing = 0.0f;
                CharacterHandler3d.MyCharacter.Properties.Heading = 0.0f;
            }

            if(CharacterHandler3d.MyCharacter.Status.ActionState == CharacterEntity3d.ActionStates.Moving)
            {
                CharacterHandler3d.MyCharacter.Status.MotionState = CharacterEntity3d.MotionStates.Normal;
            }
            */

            if (Controls3d.ActiveCamera.Name == "main")
            {
                var ox = MathF.ToRadians(Controls3d.ActiveCamera.OrbitSpeed * Hardware.Mouse.MoveX) * -1.0f; //remove * -1.0f to invert direction
                var oy = MathF.ToRadians(Controls3d.ActiveCamera.OrbitSpeed * Hardware.Mouse.MoveY) * -1.0f;
                var oz = Controls3d.ActiveCamera.OrbitSpeed * 0.1f * Hardware.Mouse.MoveZ;
                Controls3d.ActiveCamera.Theta += ox;
                Controls3d.ActiveCamera.Phi += oy;
                Controls3d.ActiveCamera.Phi = MathF.Clamp(Controls3d.ActiveCamera.Phi, 0.1f, (float)Math.PI - 0.1f);
                Controls3d.ActiveCamera.OrbitDistance += oz;
                Controls3d.ActiveCamera.OrbitDistance = MathF.Clamp(Controls3d.ActiveCamera.OrbitDistance, 10.0f, 150.0f);
                Controls3d.ActiveCamera.ApplyToAgk();
            }

            //TEMP: toggle active
            if (Hardware.IsKeyDown((int)Keys.C) && UserInterface.Status.InputReady)
            {
                UserInterface.Status.InputMark = App.Timing.Timer;
                Controls3d.ActiveCamera = Camera3dHandler.CameraList.FirstOrDefault(c => c.Name != Controls3d.ActiveCamera.Name);
                Controls3d.ActiveCamera.ApplyToAgk();
            }
        }

    }
    
}
