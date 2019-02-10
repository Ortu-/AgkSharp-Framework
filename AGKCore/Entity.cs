using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AGKCore
{
    public class EntityHandler
    {
        public static List<Entity> EntityList = new List<Entity>();

        public EntityHandler()
        {
            Dispatcher.Add(EntityHandler.UpdateEntities);
            App.UpdateList.Add(new UpdateHandler("EntityHandler.UpdateEntities", null, false));
        }

        public static void UpdateEntities(object rArgs)
        {
            foreach(var e in EntityList)
            {
                e.Update();
            }
        }
    }

    public class Entity
    {
        public uint ResourceNumber;
        public AGKVector3 Position = new AGKVector3();
        public float Facing;
        public float Heading;
        public float Speed;
        public bool IsObject;
        public List<AppliedAnimation> AnimationQ = new List<AppliedAnimation>();

        public Entity()
        {
            
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

    }
    
}
