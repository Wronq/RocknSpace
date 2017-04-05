using RocknSpace.Utils;
using SharpDX;
using RocknSpace.Collision;
using System.Xml.Serialization;

namespace RocknSpace.Entities
{
    public class PhysicsEntity : Entity
    {
        public float Mass;
        public float MassInv;
        public float Inertia;
        public float InertiaInv;

        public Vector2 Velocity;
        [XmlIgnore]
        protected Vector2 Force;
        [XmlIgnore]
        protected float Torque;
        public float Omega;

        public PhysicsEntity()
        {
            Mass = MassInv = Inertia = InertiaInv = 0.0f;
            Velocity = Vector2.Zero;
            Force = Vector2.Zero;
        }

        public PhysicsEntity(GameShape shape)
        {
            float dens = 1.0f;
            Shape = shape;

            Mass = shape.GetMass(dens);
            MassInv = 1 / Mass;

            Inertia = shape.GetInertia(dens);
            InertiaInv = 1 / Inertia;
        }

        public override void PreUpdate()
        {
            Force = Vector2.Zero;
            Torque = 0.0f;

            Position += Velocity * Time.Dt;
        }

        public override void Update()
        { }

        public void Collision(CollisionData data)
        {
            Velocity = Velocity + data.J * MassInv * data.N;
            Omega = Omega + data.rAP.Dot(data.J * data.N) * InertiaInv;
            Position -= data.N * data.Depth / 2;
        }

        public override void PostUpdate()
        {
            Vector2 acceleration = Force * MassInv;
            Velocity += acceleration * 0.5f * Time.Dt;

            Torque += Omega;
            Omega += Torque * Time.Dt * InertiaInv;
            Omega *= 0.9999f;
            Orientation += Omega * Time.Dt;
        }
    }
}
