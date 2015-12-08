using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using RocknSpace.Entities;

namespace RocknSpace.Collision
{
    class BVHTree<T> where T : IShape, new()
    {
        private class Node
        {
            public Node Left, Right;
            public T Shape;
            public Entity entity;

            public Node(T Shape)
            {
                this.Shape = Shape;
                this.Left = this.Right = null;
                this.entity = null;
            }

            public Node(Entity Entity)
            {
                this.Left = this.Right = null;
                entity = Entity;
                Shape = new T();
                Shape.CreateFromEntity(Entity);
            }

            public Node(Tuple<Node, Node> Val)
            {
                Left = Val.Item1;
                Right = Val.Item2;

                Shape = (T)Left.Shape.Cover(Right.Shape);
            }

            public Node(Node c1, Node c2)
            {
                Left = c1;
                Right = c2;
                Shape = (T)c1.Shape.Cover(c2.Shape);
            }

            public override string ToString()
            {
                CircleShape c = Shape as CircleShape;
                return "circle(" + c + ")";
            }
        }

        private Node Root;
        private Node[] Nodes;

        public BVHTree()
        { }

        private static int clz(uint number)
        {
            if (number == 0) return 32;
            int lz = 0;
            while ((number & 0x80000000) == 0)
            {
                lz++;
                number <<= 1;
            }
            return lz;
        }

        private int findSplit(int first, int last)
        {
            uint firstCode = Nodes[first].Shape.MortonCode;
            uint lastCode = Nodes[last].Shape.MortonCode;

            if (firstCode == lastCode)
                return (first + last) >> 1;

            int commonPrefix = clz(firstCode ^ lastCode);

            int split = first; // initial guess
            int step = last - first;

            do
            {
                step = (step + 1) >> 1; // exponential decrease
                int newSplit = split + step; // proposed new position

                if (newSplit < last)
                {
                    uint splitCode = Nodes[newSplit].Shape.MortonCode;
                    int splitPrefix = clz(firstCode ^ splitCode);
                    if (splitPrefix > commonPrefix)
                        split = newSplit; // accept proposal
                }
            }
            while (step > 1);

            return split;
        }

        private Node generateHierarchy(int first, int last)
        {
            if (first == last)
                return Nodes[first];

            int split = findSplit(first, last);

            Node childA = generateHierarchy(first, split);
            Node childB = generateHierarchy(split + 1, last);
            return new Node(childA, childB);
        }

        public void Recalculate()
        {
            Nodes = (from n in (from e in EntityManager.Entities
                                select new Node(e))
                     orderby n.Shape.MortonCode
                     select n).ToArray();

            Root = generateHierarchy(0, Nodes.Length - 1);
        }

        public IEnumerable<CollisionData> GetProbableCollisions()
        {
            foreach (var data in GetProbableCollisions(Root.Left, Root.Right))
                yield return data;
        }

        private IEnumerable<CollisionData> GetProbableCollisions(Node n1, Node n2)
        {
            if (n1.Shape.Overlap(n2.Shape))
            {
                if (n1.entity != null && n2.entity != null)
                    yield return new CollisionData(n1.entity, n2.entity);
                else
                {
                    if (n1.entity != null)
                        foreach (var data in GetProbableCollisions(n1, n2.Right))
                            yield return data;
                    else if (n2.entity != null)
                        foreach (var data in GetProbableCollisions(n1.Left, n2))
                            yield return data;
                    else
                        foreach (var data in GetProbableCollisions(n1.Left, n2.Right))
                            yield return data;
                }
            }
        }

        public IEnumerable<Entity> CheckCollision(Vector2 Point)
        {
            foreach (Entity e in CheckCollision(Point, Root))
                yield return e;
        }

        private IEnumerable<Entity> CheckCollision(Vector2 Point, Node Node)
        {
            if (Node.entity != null)
                yield return Node.entity;
            else if (Node.Shape.Overlap(Point))
            {
                foreach (Entity e in CheckCollision(Point, Node.Left))
                    yield return e;
                foreach (Entity e in CheckCollision(Point, Node.Right))
                    yield return e;
            }
        }

        /*private IEnumerable<CollisionData> GetWallCollisions()
        {
            foreach (var data in GetProbableCollisions(Root.Left, Root.Right))
                yield return data;
        }

        private IEnumerable<CollisionData> GetWallCollisions(Node Node)
        {
            if (Node.entity != null)
                yield return Node.entity;
            else if (Node.Shape.Overlap(Point))
            {
                foreach (Entity e in CheckCollision(Point, Node.Left))
                    yield return e;
                foreach (Entity e in CheckCollision(Point, Node.Right))
                    yield return e;
            }
        }*/
    }
}
