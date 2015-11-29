using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Windows.Media;
using RocknSpace.Utils;
using SharpDX;

namespace RocknSpace
{
    class PhysicsEntity : Entity
    {
        protected float Mass;
        protected float MassInv;
        protected float Inertia;
        protected float InertiaInv;

        protected Vector2 Velocity;
        protected Vector2 Force;

        protected float Torque;
        protected float Omega;

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

        public override void PostUpdate()
        {
            Vector2 acceleration = Force * MassInv;
            Velocity += acceleration * 0.5f * Time.Dt;

            Torque += Omega;
            Omega += Torque * InertiaInv * Time.Dt;
            Omega *= 0.99f;
            Orientation += Omega * Time.Dt;

            /*((RotateTransform)translateMatrix.Children[0]).Angle = Orientation;
            ((TranslateTransform)translateMatrix.Children[1]).X = Position.X;
            ((TranslateTransform)translateMatrix.Children[1]).Y = Position.Y;

            Shape.RenderTransform = translateMatrix;*/
        }
    }
}
