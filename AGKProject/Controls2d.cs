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
    public class Controls2d
    {

        public Controls2d()
        {
            Dispatcher.Add(Controls2d.GetGameplayInput);
            App.UpdateList.Add(new UpdateHandler("Controls2d.GetGameplayInput", null, false));
        }

        public static void GetGameplayInput(object rArgs)
        {
            /*
            //exit early if main gameplay not in progress
            if(App.Status.LoadState < 3)
            {
                return;
            }
            */

            //grab old state
            CharacterHandler2d.MyCharacter.OldStatus = JsonConvert.DeserializeObject<CharacterEntity2d.CharacterStatus>(JsonConvert.SerializeObject(CharacterHandler2d.MyCharacter.Status));

            //reset forced
            UserInterface.Status.MouseModeForced = "";
            UserInterface.Status.KeyModeForced = "";

            //exit early if user does not have control
            if (UserInterface.Status.KeyMode != "gameplay" || CharacterHandler2d.MyCharacter.Status.MotionState == CharacterEntity2d.MotionStates.Forced)
            {
                return;
            }

            if (!CharacterHandler2d.MyCharacter.Status.IsLiving)
            {
                return;
            }

            //reset states that need to be held by user
            if(CharacterHandler2d.MyCharacter.Status.ActionState == CharacterEntity2d.ActionStates.Moving)
            {
                CharacterHandler2d.MyCharacter.Status.ActionState = CharacterEntity2d.ActionStates.Idle;
            }

            if(CharacterHandler2d.MyCharacter.Status.MotionState != CharacterEntity2d.MotionStates.Forced)
            {
                CharacterHandler2d.MyCharacter.Status.MotionState = CharacterEntity2d.MotionStates.Stationary;
            }

            CharacterHandler2d.MyCharacter.Status.DirectionState = CharacterEntity2d.DirectionStates.Up;

            //move up
            if (Hardware.IsKeyDown((int)Keys.W))
            {
                CharacterHandler2d.MyCharacter.Status.ActionState = CharacterEntity2d.ActionStates.Moving;
                CharacterHandler2d.MyCharacter.Status.DirectionState = CharacterEntity2d.DirectionStates.Up;
                CharacterHandler2d.MyCharacter.Properties.Facing = 180.0f;
                CharacterHandler2d.MyCharacter.Properties.Heading = 180.0f;
            }

            //move right
            if (Hardware.IsKeyDown((int)Keys.D))
            {
                CharacterHandler2d.MyCharacter.Status.ActionState = CharacterEntity2d.ActionStates.Moving;
                CharacterHandler2d.MyCharacter.Status.DirectionState = CharacterEntity2d.DirectionStates.Right;
                CharacterHandler2d.MyCharacter.Properties.Facing = 90.0f;
                CharacterHandler2d.MyCharacter.Properties.Heading = 90.0f;
            }

            //move left
            if (Hardware.IsKeyDown((int)Keys.A))
            {
                CharacterHandler2d.MyCharacter.Status.ActionState = CharacterEntity2d.ActionStates.Moving;
                CharacterHandler2d.MyCharacter.Status.DirectionState = CharacterEntity2d.DirectionStates.Left;
                CharacterHandler2d.MyCharacter.Properties.Facing = 270.0f;
                CharacterHandler2d.MyCharacter.Properties.Heading = 270.0f;
            }

            //move down
            if (Hardware.IsKeyDown((int)Keys.S))
            {
                CharacterHandler2d.MyCharacter.Status.ActionState = CharacterEntity2d.ActionStates.Moving;
                CharacterHandler2d.MyCharacter.Status.DirectionState = CharacterEntity2d.DirectionStates.Down;
                CharacterHandler2d.MyCharacter.Properties.Facing = 0.0f;
                CharacterHandler2d.MyCharacter.Properties.Heading = 0.0f;
            }

            if(CharacterHandler2d.MyCharacter.Status.ActionState == CharacterEntity2d.ActionStates.Moving)
            {
                CharacterHandler2d.MyCharacter.Status.MotionState = CharacterEntity2d.MotionStates.Normal;
            }
        }

    }
    
}
