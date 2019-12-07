using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AGKCore
{

    public class Entity
    {
        public List<AppliedAnimation> AnimationQ = new List<AppliedAnimation>();

        public Entity()
        {
            
        }

        public class EntityProperties
        {
            public uint ResourceNumber;
            public string Filebase;
            public AGKVector3 Position = new AGKVector3();
            public float Facing;
            public float Heading;
            public float Speed;
            public float Mass;
            public bool IsObject;
        }

    }
    
}
