using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using AGKCore;

namespace AGKProject
{
    public class World3d
    {
        public static List<WorldEntity3d> WorldEntityList = new List<WorldEntity3d>();


        public World3d()
        {
            Dispatcher.Add(World3d.UpdateEntities);
            App.UpdateList.Add(new UpdateHandler("World3d.UpdateEntities", "", false));
        }
        
        public static void UpdateEntities(object rArgs)
        {
            foreach(var e in WorldEntityList)
            {
                e.Update();
            }
        }
    }

    public class WorldEntity3d : Entity
    {
        public WorldEntityProperties Properties = new WorldEntityProperties();

        public WorldEntity3d()
        {
            World3d.WorldEntityList.Add(this);
        }


        public void Update()
        {

        }

        public class WorldEntityProperties : EntityProperties
        {

        }
    }
    
}
