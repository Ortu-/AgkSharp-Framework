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

            Agk.SetSunActive(0);
            Agk.SetAmbientColor(0, 0, 0);
            //Agk.SetClearColor(52, 125, 217);

            //TEMP most init will happen during a scene load, stick them here until things get built out more
            Celestials.Add(new CelestialBody("sun")
            {
                LightProperties = new AGKCore.Lighting.Basic.DirectionalLight()
                {
                    Ambient = new AGKVector4(0.2f, 0.2f, 0.2f, 0),
                    Diffuse = new AGKVector4(0.5f, 0.5f, 0.5f, 0),
                    Specular = new AGKVector4(0.5f, 0.5f, 0.5f, 0),
                    Direction = new AGKVector3(1, 1, -0.3f)
                }
            });

        }
        
        public static void UpdateEntities(object rArgs)
        {
            foreach(var e in WorldEntityList)
            {
                e.Update();
            }

            //TEMP - For SurfaceDiffuseClipLit - TODO we dont want to apply this every loop, only when lighting changes, also handle multiple lights
            var dirLight = World3d.Celestials.First().LightProperties;
            foreach (var s in Media.ShaderList)
            {
                if (s.ReceiveDirectionalLight)
                {
                    Agk.SetShaderConstantByName(s.ResourceNumber, "u_lightDirection", dirLight.Direction.X, dirLight.Direction.Y, dirLight.Direction.Z, 0);
                    Agk.SetShaderConstantByName(s.ResourceNumber, "u_lightDiffuse", dirLight.Diffuse.R, dirLight.Diffuse.G, dirLight.Diffuse.B, 0);
                    Agk.SetShaderConstantByName(s.ResourceNumber, "u_lightAmbient", dirLight.Ambient.R, dirLight.Ambient.G, dirLight.Ambient.B, 0);
                }
            }
        }
    }

    public class WorldEntity3d : Entity
    {
        public WorldEntityProperties Properties = new WorldEntityProperties();

        public WorldEntity3d()
        {
            
        }


        public void Update()
        {
            Agk.SetObjectPosition(Properties.ResourceNumber, Properties.Position.X, Properties.Position.Y, Properties.Position.Z);
        }

        public void SetShader(ShaderAsset rShader)
        {
            Properties.Shader = rShader;
            Agk.SetObjectShader(Properties.ResourceNumber, Properties.Shader.ResourceNumber);
        }

        public class WorldEntityProperties : EntityProperties
        {
            public ShaderAsset Shader;
            public AGKVector3 Rotation;
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
