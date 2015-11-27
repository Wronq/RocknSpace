using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RocknSpace
{
    abstract class Entity
    {
        public GameShape Shape;

        public Vector Position;
        public float Orientation;

        public float Radius;

        public bool isExpired;

        public abstract void PreUpdate();
        public abstract void Update();
        public abstract void PostUpdate();
    }
}
