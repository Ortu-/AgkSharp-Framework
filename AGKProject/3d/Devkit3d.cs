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
        private static bool _axisWidget;

        public Devkit3d()
        {
            Dispatcher.Add(Devkit3d.GetDevkitInput);
            App.UpdateList.Add(new UpdateHandler("Devkit3d.GetDevkitInput", null, false));
        }

        public static void GetDevkitInput(object rArgs)
        {
            if (Hardware.IsKeyDown((int)Keys.F1) && UserInterface.Status.InputReady)
            {
                UserInterface.Status.InputMark = App.Timing.Timer;

                if (!_sunLine)
                {
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
                        Agk.SetShaderConstantByName(fx.ResourceNumber, "u_color", clr.R, clr.G, clr.B, 0);
                    }
                    _sunLine = true;
                }

                if (!_axisWidget)
                {
                    var tObj = Agk.CreateObjectPlane(1.0f, 1.0f);
                    var tEnt = new WorldEntity3d();
                    tEnt.Properties.ResourceNumber = tObj;
                    tEnt.Properties.IsObject = true;
                    var fx = Media.GetShaderAsset("media/shaders/3dLine.vs", "media/shaders/3dLine.ps", false);
                    fx.ReceiveDirectionalLight = false;
                    tEnt.SetShader(fx);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "pos1", 10.0f, 0, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "pos2", 0, 0, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "thickness", 0.3f, 0, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "u_color", 1.0f, 0, 0, 0);

                    tObj = Agk.CreateObjectPlane(1.0f, 1.0f);
                    tEnt = new WorldEntity3d();
                    tEnt.Properties.ResourceNumber = tObj;
                    tEnt.Properties.IsObject = true;
                    fx = Media.GetShaderAsset("media/shaders/3dLine.vs", "media/shaders/3dLine.ps", false);
                    fx.ReceiveDirectionalLight = false;
                    tEnt.SetShader(fx);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "pos1", 0, 10.0f, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "pos2", 0, 0, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "thickness", 0.3f, 0, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "u_color", 0, 1.0f, 0, 0);

                    tObj = Agk.CreateObjectPlane(1.0f, 1.0f);
                    tEnt = new WorldEntity3d();
                    tEnt.Properties.ResourceNumber = tObj;
                    tEnt.Properties.IsObject = true;
                    fx = Media.GetShaderAsset("media/shaders/3dLine.vs", "media/shaders/3dLine.ps", false);
                    fx.ReceiveDirectionalLight = false;
                    tEnt.SetShader(fx);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "pos1", 0, 0, 10.0f, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "pos2", 0, 0, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "thickness", 0.3f, 0, 0, 0);
                    Agk.SetShaderConstantByName(fx.ResourceNumber, "u_color", 0, 0, 1.0f, 0);

                    _axisWidget = true;
                }
            }
        }

    }
    
}
