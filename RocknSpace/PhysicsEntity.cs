using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Windows.Media;

namespace RocknSpace
{
    class PhysicsEntity : Entity
    {
        protected float Mass;
        protected float MassInv;
        protected float Inertia;
        protected float InertiaInv;

        protected Vector Velocity;
        protected Vector Force;

        protected float Torque;
        protected float Omega;

        public PhysicsEntity()
        {
            Mass = MassInv = Inertia = InertiaInv = 0.0f;
            Velocity = Vector.Zero;
            Force = Vector.Zero;

            /*translateMatrix = new TransformGroup();
            translateMatrix.Children.Add(new RotateTransform(Orientation));
            translateMatrix.Children.Add(new TranslateTransform(Position.X, Position.Y));*/
        }

        public override void PreUpdate()
        {
            Force = Vector.Zero;
            Torque = 0.0f;

            Position += Velocity * Time.Dt;
        }

        public override void Update()
        {
            
        }

        public override void PostUpdate()
        {
            Vector acceleration = Force * MassInv;
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
