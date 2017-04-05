using SharpDX;

namespace RocknSpace.Entities
{
    class Wall : PhysicsEntity
    {
        public static Wall Instance;

        static Wall()
        {
            Instance = new Wall();
        }

        public Wall()
        {
            MassInv = 0;
            InertiaInv = 0;
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Omega = 0;
        }

        public override void PreUpdate()
        { }

        public override void Update()
        { }

        public override void PostUpdate()
        { }
    }
}
