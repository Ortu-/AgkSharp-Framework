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
        public static List<CelestialBody> Celestials = new List<CelestialBody>();

        public World3d()
        {
            Dispatcher.Add(World3d.UpdateEntities);
            App.UpdateList.Add(new UpdateHandler("World3d.UpdateEntities", "", false));

            //TEMP most init will happen during a scene load, stick them here until things get built out more
            Celestials.Add(new CelestialBody("sun")
            {
                LightProperties = new AGKCore.Lighting.Basic.DirectionalLight()
                {
                    Ambient = new AGKVector4(0.2f, 0.2f, 0.2f, 0),
                    Diffuse = new AGKVector4(0.5f, 0.5f, 0.5f, 0),
                    Specular = new AGKVector4(0.5f, 0.5f, 0.5f, 0),
                    Direction = new AGKVector3(0.57735f, -.057735f, 0.57735f)
                }
            });
            /*
            Celestials.Add(new CelestialBody("sun")
            {
                LightProperties = new AGKCore.Lighting.Basic.DirectionalLight()
                {
                    Ambient = new AGKVector4(0),
                    Diffuse = new AGKVector4(0.2f, 0.2f, 0.2f, 0),
                    Specular = new AGKVector4(0.25f, 0.25f, 0.25f, 0),
                    Direction = new AGKVector3(-0.57735f, -0.57735f, 0.57735f)
                }
            });
            Celestials.Add(new CelestialBody("sun")
            {
                LightProperties = new AGKCore.Lighting.Basic.DirectionalLight()
                {
                    Ambient = new AGKVector4(0),
                    Diffuse = new AGKVector4(0.2f, 0.2f, 0.2f, 0),
                    Specular = new AGKVector4(0),
                    Direction = new AGKVector3(0.0f, -0.707f, -0.707f)
                }
            });
            */

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

        public void SetShader(ShaderAsset rShader)
        {
            Properties.Shader = rShader;
            Agk.SetObjectShader(Properties.ResourceNumber, Properties.Shader.ResourceNumber);
        }

        public class WorldEntityProperties : EntityProperties
        {
            public ShaderAsset Shader;
        }
    }

    public class CelestialBody
    {
        public string Name; 
        public AGKCore.Lighting.Basic.DirectionalLight LightProperties;
        //TODO: image? orbital speed/axis

        public CelestialBody(string rName)
        {
            Name = rName;
        }
    }


}
