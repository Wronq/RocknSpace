using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Windows.Media;
using RocknSpace.Utils;
using SharpDX;
using RocknSpace.Collision;

namespace RocknSpace.Entities
{
    public class PhysicsEntity : Entity
    {
        protected float Mass;
        protected float MassInv;
        protected float Inertia;
        protected float InertiaInv;

        public Vector2 Velocity;
        protected Vector2 Force;

        protected float Torque;
        public float Omega;

        public PhysicsEntity()
        {
            Mass = MassInv = Inertia = InertiaInv = 0.0f;
            Velocity = Vector2.Zero;
            Force = Vector2.Zero;
        }

        public PhysicsEntity(GameShape Shape)
        {
            float dens = 1.0f;
            this.Shape = Shape;

            Mass = Shape.GetMass(dens);
            MassInv = 1 / Mass;

            Inertia = Shape.GetInertia(dens);
            InertiaInv = 1 / Inertia;
        }

        public override void PreUpdate()
        {
            Force = Vector2.Zero;
            Torque = 0.0f;

            Position += Velocity * Time.Dt;
        }

        public override void Update()
        {
            
        }

        public void Collision(CollisionData Data)
        {
            if (Data.N == Vector2.Zero)
                return;

            PhysicsEntity p2 = (PhysicsEntity)Data.Object2;

            float e = 0.8f; //Restitution

            Vector2 rAP = (Data.Point1 - Position).Perpendicular();
            Vector2 rBP = (Data.Point1 - p2.Position).Perpendicular();

            Vector2 vAP = Velocity + Omega * rAP;
            Vector2 vBP = p2.Velocity + p2.Omega * rBP;

            Vector2 vAB = vAP - vBP;

            float j = -(1 + e) * vAB.Dot(Data.N);
            float j1 = Data.N.Dot(Data.N * (MassInv + p2.MassInv));
            float j2 = (float)Math.Pow(rAP.Dot(Data.N), 2) * InertiaInv;
            float j3 = (float)(Math.Pow(rBP.Dot(Data.N), 2) * p2.InertiaInv);
            //j /= Data.N.Dot(Data.N) * (MassInv + p2.MassInv) + (float)Math.Pow(rAP.Dot(Data.N), 2) * InertiaInv + (float)(Math.Pow(rBP.Dot(Data.N), 2) * p2.InertiaInv);
            j /= j1 + j2 + j3;

            this.Velocity = this.Velocity + j * MassInv * Data.N;
            this.Omega = this.Omega + rAP.Dot(j * Data.N) * InertiaInv;
        }

        public override void PostUpdate()
        {
            Vector2 acceleration = Force * MassInv;
            Velocity += acceleration * 0.5f * Time.Dt;

            Torque += Omega;
            Omega += Torque * InertiaInv * Time.Dt;
            Omega *= 0.9999f;
            Orientation += Omega * Time.Dt;

            /*((RotateTransform)translateMatrix.Children[0]).Angle = Orientation;
            ((TranslateTransform)translateMatrix.Children[1]).X = Position.X;
            ((TranslateTransform)translateMatrix.Children[1]).Y = Position.Y;

            Shape.RenderTransform = translateMatrix;*/
        }
    }
}
