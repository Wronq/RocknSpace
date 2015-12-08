using RocknSpace.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace RocknSpace.Entities
{
    public abstract class Entity
    {
        protected static Random rand = new Random();

        public GameShape Shape;

        public Vector2 Position;
        public float Orientation;
        public Color4 Color;
        
        public bool isExpired;

        public abstract void PreUpdate();
        public abstract void Update();
        public abstract void PostUpdate();
    }
}
