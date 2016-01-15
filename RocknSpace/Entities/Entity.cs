using RocknSpace.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using System.Xml.Serialization;

namespace RocknSpace.Entities
{
    public abstract class Entity : IDisposable, IOnDeserialized
    {
        protected static Random rand = new Random();

        public GameShape Shape;

        public Vector2 Position;
        public float Orientation;
        public Color4 Color;
        
        [XmlIgnore]
        public bool isExpired;

        public abstract void PreUpdate();
        public abstract void Update();
        public abstract void PostUpdate();

        public void Dispose()
        {
            Disposer.RemoveAndDispose(ref Shape);
        }

        public virtual void OnDeserialized()
        {
            if (Shape != null)
                Shape.OnDeserialized();
        }
    }
}
