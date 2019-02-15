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
            App.UpdateList.Add(new UpdateHandler("CharacterHandler2d.UpdateCharacters", null, false));
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
            public float NormalMaxSpeed = 10.0f;
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

            ImageAsset tSheet = Media.GetImageAsset(rFileBase + ".png", 1.0f, 1.0f);
            Agk.Swap();
            var spSheet = Agk.CreateSprite(tSheet.Number);
            Agk.SetSpritePosition(spSheet, 0.0f, 0.0f);
            Agk.Render();
            bool isFirst = true;
            for(int row = 0; row < (Agk.GetImageHeight(tSheet.Number) / rHeight); row++)
            {
                for (int col = 0; col < (Agk.GetImageWidth(tSheet.Number) / rWidth); col++)
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
            if(AnimationQ.Count > 0)
            {
                bool isDone = AnimationQ[0].Update();
                if (isDone)
                {
                    AnimationQ.RemoveAt(0);
                }
            }
        }

        public class CharacterProperties : EntityProperties
        {
            public int ZIndex = 10;
            public float Rotation = 0.0f;
        }

        public class CharacterStatus
        {
            public bool IsLiving = true;
            public StanceStates StanceState = StanceStates.Basic;
            public ActionStates ActionState = ActionStates.Idle;
            public MotionStates MotionState = MotionStates.Normal;
            public DirectionStates DirectionState = DirectionStates.Right;
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
