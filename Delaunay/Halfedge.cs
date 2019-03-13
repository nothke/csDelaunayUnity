using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace csDelaunay
{

    public class Halfedge
    {
        public static List<Halfedge> all = new List<Halfedge>();

        #region Pool
        private static Queue<Halfedge> unusedPool = new Queue<Halfedge>();
        public static int PoolCapacity { get => all.Count; }
        bool disposed;

        public static void PoolDummies(int num)
        {
            var dummies = new Halfedge[num];
            for (int i = 0; i < num; i++)
            {
                dummies[i] = CreateDummy();
            }

            for (int i = 0; i < num; i++)
            {
                dummies[i].Dispose();
            }
        }

        public static void DisposeAll()
        {
            for (int i = 0; i < all.Count; i++)
            {
                if (!all[i].disposed)
                {
                    all[i].ReallyDispose();
                    //UnityEngine.Debug.Log("Found undisposed HE");
                }
            }
        }

        /// <summary>
        /// Use only for testing
        /// </summary>
        public static void FlushUnused()
        {
            all = new List<Halfedge>();
            unusedPool = new Queue<Halfedge>();
        }

        public static Halfedge Create(Edge edge, bool lr)
        {
            if (unusedPool.Count > 0)
            {
                return unusedPool.Dequeue().Init(edge, lr);
            }
            else
            {
                Profiler.BeginSample("Making new halfege");
                Halfedge halfedge = new Halfedge(edge, lr);
                all.Add(halfedge);
                Profiler.EndSample();
                return halfedge;
            }
        }
        public static Halfedge CreateDummy()
        {
            return Create(null, false);
        }
        #endregion

        #region Object
        public Halfedge edgeListLeftNeighbor;
        public Halfedge edgeListRightNeighbor;
        public Halfedge nextInPriorityQueue;

        public Edge edge;
        public bool leftRight;
        public Vertex vertex;

        // The vertex's y-coordinate in the transformed Voronoi space V
        public float ystar;

        public Halfedge(Edge edge, bool lr)
        {
            Init(edge, lr);
        }

        private Halfedge Init(Edge edge, bool lr)
        {
            this.edge = edge;
            leftRight = lr;
            nextInPriorityQueue = null;
            vertex = null;

            disposed = false;

            return this;
        }

        public override string ToString()
        {
            return "Halfedge (LeftRight: " + leftRight + "; vertex: " + vertex + ")";
        }

        public void Dispose()
        {
            if (edgeListLeftNeighbor != null || edgeListRightNeighbor != null)
            {
                // still in EdgeList
                return;
            }
            if (nextInPriorityQueue != null)
            {
                // still in PriorityQueue
                return;
            }
            edge = null;
            leftRight = false;
            vertex = null;
            unusedPool.Enqueue(this);
            disposed = true;
        }

        public void ReallyDispose()
        {
            edgeListLeftNeighbor = null;
            edgeListRightNeighbor = null;
            nextInPriorityQueue = null;
            edge = null;
            leftRight = false;
            vertex = null;
            unusedPool.Enqueue(this);
            disposed = true;
        }

        public bool IsLeftOf(Vector2f p)
        {
            Site topSite;
            bool rightOfSite, above, fast;
            float dxp, dyp, dxs, t1, t2, t3, y1;

            topSite = edge.RightSite;
            rightOfSite = p.x > topSite.x;
            if (rightOfSite && this.leftRight == false)
            {
                return true;
            }
            if (!rightOfSite && this.leftRight == true)
            {
                return false;
            }

            if (edge.a == 1)
            {
                dyp = p.y - topSite.y;
                dxp = p.x - topSite.x;
                fast = false;
                if ((!rightOfSite && edge.b < 0) || (rightOfSite && edge.b >= 0))
                {
                    above = dyp >= edge.b * dxp;
                    fast = above;
                }
                else
                {
                    above = p.x + p.y * edge.b > edge.c;
                    if (edge.b < 0)
                    {
                        above = !above;
                    }
                    if (!above)
                    {
                        fast = true;
                    }
                }
                if (!fast)
                {
                    dxs = topSite.x - edge.LeftSite.x;
                    above = edge.b * (dxp * dxp - dyp * dyp) < dxs * dyp * (1 + 2 * dxp / dxs + edge.b * edge.b);
                    if (edge.b < 0)
                    {
                        above = !above;
                    }
                }
            }
            else
            {
                y1 = edge.c - edge.a * p.x;
                t1 = p.y - y1;
                t2 = p.x - topSite.x;
                t3 = y1 - topSite.y;
                above = t1 * t1 > t2 * t2 + t3 * t3;
            }
            return this.leftRight == false ? above : !above;
        }
        #endregion
    }
}
