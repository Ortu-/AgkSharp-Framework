using AgkSharp;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace AGKCore
{
    public static class PhysicsConfig
    {
        public static float Gravity = 3.0f;
        public static float ArcMassRate = 0.005f;
        public static float ArcAcceleration = 600.0f;
    }

    public class CollisionRay
    {
        public uint ResourceNumber = (uint)Agk.Create3DPhysicsRay();
        public uint FromVectorNumber = (uint)Agk.CreateVector3();
        public uint ToVectorNumber = (uint)Agk.CreateVector3();
        public uint HitVectorNumber = (uint)Agk.CreateVector3();
        public uint HitObjectNumber;
        public AGKVector3 HitPosition = new AGKVector3();
        public AGKVector3 HitNormalVector = new AGKVector3();
        public AGKVector3 SlideVector = new AGKVector3();

        public CollisionRay()
        {

        }

        public void Dispose()
        {
            Agk.Delete3DPhysicsRay(ResourceNumber);
            Agk.DeleteVector3(FromVectorNumber);
            Agk.DeleteVector3(ToVectorNumber);
            Agk.DeleteVector3(HitVectorNumber);
            this.Dispose();
        }

        public void SphereCast(AGKVector3 rFrom, AGKVector3 rTo, float rRadius, bool rSlide)
        {
            Agk.SetVector3(FromVectorNumber, rFrom.X, rFrom.Y, rFrom.Z);
            Agk.SetVector3(ToVectorNumber, rTo.X, rTo.Y, rTo.Z);
            Agk.SphereCast3DPhysics(ResourceNumber, (int)FromVectorNumber, (int)ToVectorNumber, rRadius);
            HitObjectNumber = (uint)Agk.Get3DPhysicsRayCastObjectHit(ResourceNumber, Agk.Get3DPhysicsRayCastFraction(ResourceNumber));
            if (HitObjectNumber > 0)
            {
                Agk.Get3DPhysicsRayCastContactPosition(ResourceNumber, Agk.Get3DPhysicsRayCastFraction(ResourceNumber), (int)HitVectorNumber);
                HitPosition.X = Agk.GetVector3X(HitVectorNumber);
                HitPosition.Y = Agk.GetVector3Y(HitVectorNumber);
                HitPosition.Z = Agk.GetVector3Z(HitVectorNumber);

                if (!rSlide)
                {
                    return;
                }

                Agk.Get3DPhysicsRayCastNormalVector(ResourceNumber, (int)HitVectorNumber);

                if (Math.Abs(HitNormalVector.Y) > 0.999)
                {
                    SlideVector = new AGKVector3(HitNormalVector.Y, 0.0f, 0.0f);
                }
                else
                {
                    SlideVector = new AGKVector3(-HitNormalVector.Z, 0.0f, HitNormalVector.X).Normalize();
                }
                HitPosition *= SlideVector;
            }
        }

        public void RayCast(AGKVector3 rFrom, AGKVector3 rTo)
        {
            Agk.SetVector3(FromVectorNumber, rFrom.X, rFrom.Y, rFrom.Z);
            Agk.SetVector3(ToVectorNumber, rTo.X, rTo.Y, rTo.Z);
            Agk.RayCast3DPhysics(ResourceNumber, (int)FromVectorNumber, (int)ToVectorNumber, 1);
            HitObjectNumber = (uint)Agk.Get3DPhysicsRayCastObjectHit(ResourceNumber, Agk.Get3DPhysicsRayCastFraction(ResourceNumber));
            if (HitObjectNumber > 0)
            {
                Agk.Get3DPhysicsRayCastContactPosition(ResourceNumber, Agk.Get3DPhysicsRayCastFraction(ResourceNumber), (int)HitVectorNumber);
                HitPosition.X = Agk.GetVector3X(HitVectorNumber);
                HitPosition.Y = Agk.GetVector3Y(HitVectorNumber);
                HitPosition.Z = Agk.GetVector3Z(HitVectorNumber);
            }
        }


    }

    public class ArcHandler
    {
        private static List<ArcPath> _ArcList = new List<ArcPath>();

        public static void DisposeAll()
        {
            _ArcList.Clear();
        }

        public static void Dispose(ArcPath a)
        {
            _ArcList.Remove(a);
        }

        public static ArcPath Create(dynamic rObject, float rVelocity, float rTilt, float rPan)
        {
            var a = new ArcPath(rObject, rVelocity, rTilt, rPan);
            _ArcList.Add(a);
            return a;
        }


    }

    public class ArcPath
    {
        public dynamic Owner;
        public float Velocity;
        public float Tilt;
        public float Pan;
        public AGKVector3 InitialPosition;
        public uint StartTime;

        public ArcPath(dynamic rObject, float rVelocity, float rTilt, float rPan)
        {
            Owner = rObject;
            Velocity = rVelocity;
            Tilt = rTilt;
            Pan = rPan; //Pan should be 0 if 2d
            InitialPosition = rObject.Properties.Position;
            StartTime = App.Timing.Timer;
        }

        public void Dispose()
        {
            ArcHandler.Dispose(this);
        }

        public AGKVector3 GetPosition(uint rTime, uint rPauseTime)
        {
            //get vectored velocity
            var d = Velocity * Math.Cos(Tilt);
            var vy = Velocity * Math.Sin(Tilt);
            var vx = d * Math.Sin(Pan);
            var vz = d * Math.Cos(Pan);

            //adjust time, using a passed in time allows us to project into the future or rollback to the past if we want
            StartTime += rPauseTime;
            var elapsed = rTime;

            //get position at time
            var relativeMass = Owner.Properties.Mass + (PhysicsConfig.ArcMassRate * elapsed);
            var pos = new AGKVector3();
            pos.Y = InitialPosition.Y + (vy * elapsed) - (0.5f * PhysicsConfig.ArcAcceleration * elapsed * elapsed) * PhysicsConfig.Gravity * relativeMass;
            pos.X = InitialPosition.X + vx * elapsed * relativeMass;
            pos.Z = InitialPosition.Z + vz * elapsed * relativeMass;
            return pos;
        }


    }

}
