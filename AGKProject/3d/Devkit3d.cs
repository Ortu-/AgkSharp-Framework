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
    public class Devkit3d
    {
        private static bool _sunLine;

        public Devkit3d()
        {
            Dispatcher.Add(Devkit3d.GetDevkitInput);
            App.UpdateList.Add(new UpdateHandler("Devkit3d.GetDevkitInput", null, false));
        }

        public static void GetDevkitInput(object rArgs)
        {
            if (Hardware.IsKeyDown((int)Keys.F1) && UserInterface.Status.InputReady)
            {
                if (!_sunLine)
                {
                    UserInterface.Status.InputMark = App.Timing.Timer;

                    foreach (var light in World3d.Celestials) {
                        var tObj = Agk.CreateObjectPlane(1.0f, 1.0f);
                        var tEnt = new WorldEntity3d();
                        tEnt.Properties.ResourceNumber = tObj;
                        tEnt.Properties.IsObject = true;

                        var fx = Media.GetShaderAsset("media/shaders/3dLine.vs", "media/shaders/3dLine.ps", false);
                        fx.ReceiveDirectionalLight = false;
                        tEnt.SetShader(fx);
                        var dir = light.LightProperties.Direction;
                        var clr = light.LightProperties.Diffuse;
                        Agk.SetShaderConstantByName(fx.ResourceNumber, "pos1", dir.X * 100.0f, dir.Y * 100.0f, dir.Z * 100.0f, 0);
                        Agk.SetShaderConstantByName(fx.ResourceNumber, "pos2", dir.X * -100.0f, dir.Y * -100.0f, dir.Z * -100.0f, 0);
                        Agk.SetShaderConstantByName(fx.ResourceNumber, "thickness", 0.5f, 0, 0, 0);
                        Agk.SetShaderConstantByName(fx.ResourceNumber, "appliedColor", clr.R, clr.G, clr.B, 0);
                    }
                    _sunLine = true;
                }
            }
        }

    }
    
}
