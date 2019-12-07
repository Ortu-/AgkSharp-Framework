using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AGKCore
{
    public class Camera3dHandler
    {
        public static List<Camera3d> CameraList = new List<Camera3d>();
        public static float DefaultFOV = 35.489f; //50mm

        public Camera3dHandler()
        {
            Agk.SetCameraFOV(1, DefaultFOV);
            Dispatcher.Add(Camera3dHandler.UpdateCameras);
            App.UpdateList.Add(new UpdateHandler("Camera3dHandler.UpdateCameras", "World3d.UpdateEntities,Controls3d.GetGameplayInput", false));
        }

        public static void UpdateCameras(object rArgs)
        {
            foreach (var c in CameraList)
            {
                if (c.IsAutoUpdate)
                {
                    c.Update();
                }
            }
        }
    }

    public class Camera3d
    {
        
        private static AGKVector3 _up = new AGKVector3(0, 1, 0);

        public string Name;
        public bool IsAutoUpdate = true;
        public bool IsActive = false;
        public CameraMode ControlMode = CameraMode.Freeflight;

        public float Precision = 8.0f;
        public float Near = 0.1f;
        public float Far = 1000.0f;
        public float FOV = Camera3dHandler.DefaultFOV;
        public float Phi;
        public float Theta;
        public AGKVector3 Position = new AGKVector3();
        public AGKVector3 Rotation = new AGKVector3();

        public AGKMatrix4 ViewMatrix = AGKMatrix4.Identity;
        public AGKMatrix4 ProjectionMatrix = AGKMatrix4.Identity;

        public dynamic Anchor;
        public AGKVector3 Offset = new AGKVector3();
        public float OrbitDistance = 100.0f;
        public float OrbitSpeed = 0.25f;

        public dynamic Target;
        public AGKVector3 TargetPosition = new AGKVector3();

        public Camera3d(string rName)
        {
            if (String.IsNullOrEmpty(rName))
            {
                rName = "camera" + Camera3dHandler.CameraList.Count().ToString();
            }

            Name = rName;

            Camera3dHandler.CameraList.Add(this);
        }

        public void UpdateFromAgk()
        {
            FOV = Agk.GetCameraFOV(1);
            Position.X = Agk.GetCameraX(1);
            Position.Y = Agk.GetCameraY(1);
            Position.Z = Agk.GetCameraZ(1);
            Rotation.X = Agk.GetCameraAngleX(1);
            Rotation.Y = Agk.GetCameraAngleY(1);
            Rotation.Z = Agk.GetCameraAngleZ(1);

            //TODO: reverse calc orbit/target values if bound

            //ProjectionMatrix = AGKMatrix4.Perspective(0.25f * (float)Math.PI, AspectRatio, 1.0f, 1000.0f);
            ProjectionMatrix = AGKMatrix4.Perspective(FOV, App.Config.Screen.AspectRatio, Near, Far);
        }

        public void ApplyToAgk()
        {
            Agk.SetCameraFOV(1, FOV);
            Agk.SetCameraPosition(1, Position.X, Position.Y, Position.Z);
            Agk.SetCameraRange(1, Near, Far);
            Agk.SetCameraRotation(1, Rotation.X, Rotation.Y, Rotation.Z);
            if (Target != null)
            {
                Agk.SetCameraLookAt(1, (Target.Properties.Position.X + Offset.X), (Target.Properties.Position.Y + Offset.Y), (Target.Properties.Position.Z + Offset.Z), Rotation.Z);
            }
        }

        public void Update()
        {
            if (IsActive)
            {
                var ox = MathUtil.ToRadians(OrbitSpeed * Hardware.Mouse.MoveX) * -1.0f; //remove * -1.0f to invert direction
                var oy = MathUtil.ToRadians(OrbitSpeed * Hardware.Mouse.MoveY) * -1.0f;
                var oz = OrbitSpeed * 0.3f * Hardware.Mouse.MoveZ;
                Theta += ox;
                Phi += oy;
                Phi = MathUtil.Clamp(Phi, 0.1f, (float)Math.PI - 0.1f);
                OrbitDistance += oz;
                OrbitDistance = MathUtil.Clamp(OrbitDistance, 10.0f, 150.0f);
                ApplyToAgk();
            }

            if (Anchor != null)
            {
                var p = MathUtil.ToDegrees(Phi);
                var t = MathUtil.ToDegrees(Theta);
                Position.X = (Anchor.Properties.Position.X + Offset.X) + (OrbitDistance * Agk.Sin(p) * Agk.Cos(t));
                Position.Z = (Anchor.Properties.Position.Z + Offset.Z) + (OrbitDistance * Agk.Sin(p) * Agk.Sin(t));
                Position.Y = (Anchor.Properties.Position.Y + Offset.Y) + (OrbitDistance * Agk.Cos(p));
                /*
                var rx = Anchor.Properties.Position.X + (OrbitDistance * Agk.Sin(Phi) * Agk.Cos(Theta));
                var rz = Anchor.Properties.Position.Z + (OrbitDistance * Agk.Sin(Phi) * Agk.Sin(Theta));
                var ry = Anchor.Properties.Position.Y + (OrbitDistance * Agk.Cos(Phi));
                ViewMatrix = AGKMatrix4.LookAt(new AGKVector3(rx, ry, rz), TargetPosition, _up);
                */
            }
            else
            {
                //LocalRotation.X += Phi;
                //LocalRotation.Y += Theta;
            }

            if (Target != null)
            {
                TargetPosition = Target.Properties.Position;
            }
            
            
        }

        public enum CameraMode
        {
            Cinematic,
            Anchored,
            Freeflight
        }

    }
}
