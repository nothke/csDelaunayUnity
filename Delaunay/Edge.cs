using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace csDelaunay
{

    /*
	 * The line segment connecting the two Sites is part of the Delaunay triangulation
	 * The line segment connecting the two Vertices is part of the Voronoi diagram
	 */
    public class Edge
    {
        static List<Edge> all = new List<Edge>();

        #region Pool
        private static Queue<Edge> unusedPool = new Queue<Edge>();
        public static int PoolCapacity { get => all.Count; }
        public bool disposed { get; private set; }

        public static void PoolDummies(int num)
        {
            all.Capacity = num;

            var dummies = new Edge[num];
            for (int i = 0; i < num; i++)
            {
                dummies[i] = Create();
            }

            for (int i = 0; i < num; i++)
            {
                dummies[i].Dispose();
            }
        }

        public bool Clipped { get; private set; }

        private static int nEdges = 0;

        public static void DisposeAll()
        {
            for (int i = 0; i < all.Count; i++)
            {
                if (!all[i].disposed)
                {
                    all[i].Dispose();
                    UnityEngine.Debug.Log("Found undisposed Edge");
                }
            }
        }

        /*
		 * This is the only way to create a new Edge
		 * @param site0
		 * @param site1
		 * @return
		 */
        public static Edge CreateBisectingEdge(Site s0, Site s1)
        {
            float dx, dy;
            float absdx, absdy;
            float a, b, c;

            dx = s1.x - s0.x;
            dy = s1.y - s0.y;
            absdx = dx > 0 ? dx : -dx;
            absdy = dy > 0 ? dy : -dy;
            c = s0.x * dx + s0.y * dy + (dx * dx + dy * dy) * 0.5f;

            if (absdx > absdy)
            {
                a = 1;
                b = dy / dx;
                c /= dx;
            }
            else
            {
                b = 1;
                a = dx / dy;
                c /= dy;
            }

            Profiler.BeginSample("Edge create");
            Edge edge = Create(); // alloc
            Profiler.EndSample();

            edge.LeftSite = s0;
            edge.RightSite = s1;
            Profiler.BeginSample("AddEdge");
            s0.AddEdge(edge); // alloc
            s1.AddEdge(edge); // alloc
            Profiler.EndSample();

            edge.a = a;
            edge.b = b;
            edge.c = c;

            return edge;
        }

        private static Edge Create()
        {
            //UnityEngine.Debug.Log("Pool size: " + pool.Count);

            Edge edge;
            if (unusedPool.Count > 0)
            {
                edge = unusedPool.Dequeue();
                edge.disposed = false;
                edge.Init();
            }
            else
            {
                edge = new Edge();
                all.Add(edge);
            }

            return edge;
        }
        #endregion

        public static List<Edge> SelectEdgesForSitePoint(Vector2f coord, List<Edge> edgesToTest)
        {
            return edgesToTest.FindAll(
            delegate (Edge e)
            {
                if (e.LeftSite != null)
                {
                    if (e.LeftSite.Coord == coord) return true;
                }
                if (e.RightSite != null)
                {
                    if (e.RightSite.Coord == coord) return true;
                }
                return false;
            });
        }

        public static readonly Edge DELETED = new Edge();

        #region Object
        // The equation of the edge: ax + by = c
        public float a, b, c;

        // The two Voronoi vertices that the edge connects (if one of them is null, the edge extends to infinity)
        public Vertex LeftVertex { get; private set; }
        public Vertex RightVertex { get; private set; }

        public Vertex Vertex(bool leftRight)
        {
            return leftRight == false ? LeftVertex : RightVertex;
        }

        public void SetVertex(bool leftRight, Vertex v)
        {
            if (leftRight == false)
            {
                LeftVertex = v;
            }
            else
            {
                RightVertex = v;
            }
        }

        public bool IsPartOfConvexHull()
        {
            return LeftVertex == null || RightVertex == null;
        }

        public float SitesDistance()
        {
            return (LeftSite.Coord - RightSite.Coord).magnitude;
        }

        public static int CompareSitesDistances_MAX(Edge edge0, Edge edge1)
        {
            float length0 = edge0.SitesDistance();
            float length1 = edge1.SitesDistance();
            if (length0 < length1)
            {
                return 1;
            }
            if (length0 > length1)
            {
                return -1;
            }
            return 0;
        }

        public static int CompareSitesDistances(Edge edge0, Edge edge1)
        {
            return -CompareSitesDistances_MAX(edge0, edge1);
        }

        // Once clipVertices() is called, this array will hold two Points
        // representing the clipped coordinates of the left and the right ends...
        public Vector2f[] ClippedEnds { get; private set; }

        // The two input Sites for which this Edge is a bisector:
        private Site[] sites;
        public Site LeftSite { get { return sites[0]; } set { sites[0] = value; } }
        public Site RightSite { get { return sites[1]; } set { sites[1] = value; } }

        public Site Site(bool leftRight)
        {
            return sites[leftRight ? 1 : 0];
        }

        private int edgeIndex;
        public int EdgeIndex { get { return edgeIndex; } }

        public void Dispose()
        {
            LeftVertex = null;
            RightVertex = null;

            unusedPool.Enqueue(this);
            disposed = true;
        }

        public Edge()
        {
            edgeIndex = nEdges++;

            sites = new Site[2];
            ClippedEnds = new Vector2f[2];

            Init();
        }

        void Init()
        {
            sites[0] = null;
            sites[1] = null;

            Clipped = false;
        }

        public override string ToString()
        {
            return "Edge " + edgeIndex + "; sites " + sites[0] + ", " + sites[1] +
                "; endVertices " + (LeftVertex != null ? LeftVertex.VertexIndex.ToString() : "null") + ", " +
                    (RightVertex != null ? RightVertex.VertexIndex.ToString() : "null") + "::";
        }

        /*
		 * Set clippedVertices to contain the two ends of the portion of the Voronoi edge that is visible
		 * within the bounds. If no part of the Edge falls within the bounds, leave clippedVertices null
		 * @param bounds
		 */
        public void ClipVertices(Rectf bounds)
        {
            float xmin = bounds.x;
            float ymin = bounds.y;
            float xmax = bounds.right;
            float ymax = bounds.bottom;

            Vertex vertex0, vertex1;
            float x0, x1, y0, y1;

            if (a == 1 && b >= 0)
            {
                vertex0 = RightVertex;
                vertex1 = LeftVertex;
            }
            else
            {
                vertex0 = LeftVertex;
                vertex1 = RightVertex;
            }

            if (a == 1)
            {
                y0 = ymin;
                if (vertex0 != null && vertex0.y > ymin)
                {
                    y0 = vertex0.y;
                }
                if (y0 > ymax)
                {
                    return;
                }
                x0 = c - b * y0;

                y1 = ymax;
                if (vertex1 != null && vertex1.y < ymax)
                {
                    y1 = vertex1.y;
                }
                if (y1 < ymin)
                {
                    return;
                }
                x1 = c - b * y1;

                if ((x0 > xmax && x1 > xmax) || (x0 < xmin && x1 < xmin))
                {
                    return;
                }

                if (x0 > xmax)
                {
                    x0 = xmax;
                    y0 = (c - x0) / b;
                }
                else if (x0 < xmin)
                {
                    x0 = xmin;
                    y0 = (c - x0) / b;
                }

                if (x1 > xmax)
                {
                    x1 = xmax;
                    y1 = (c - x1) / b;
                }
                else if (x1 < xmin)
                {
                    x1 = xmin;
                    y1 = (c - x1) / b;
                }
            }
            else
            {
                x0 = xmin;
                if (vertex0 != null && vertex0.x > xmin)
                {
                    x0 = vertex0.x;
                }
                if (x0 > xmax)
                {
                    return;
                }
                y0 = c - a * x0;

                x1 = xmax;
                if (vertex1 != null && vertex1.x < xmax)
                {
                    x1 = vertex1.x;
                }
                if (x1 < xmin)
                {
                    return;
                }
                y1 = c - a * x1;

                if ((y0 > ymax && y1 > ymax) || (y0 < ymin && y1 < ymin))
                {
                    return;
                }

                if (y0 > ymax)
                {
                    y0 = ymax;
                    x0 = (c - y0) / a;
                }
                else if (y0 < ymin)
                {
                    y0 = ymin;
                    x0 = (c - y0) / a;
                }

                if (y1 > ymax)
                {
                    y1 = ymax;
                    x1 = (c - y1) / a;
                }
                else if (y1 < ymin)
                {
                    y1 = ymin;
                    x1 = (c - y1) / a;
                }
            }

            Clipped = true;

            if (vertex0 == LeftVertex)
            {
                ClippedEnds[0] = new Vector2f(x0, y0);
                ClippedEnds[1] = new Vector2f(x1, y1);
            }
            else
            {
                ClippedEnds[1] = new Vector2f(x0, y0);
                ClippedEnds[0] = new Vector2f(x1, y1);
            }
        }
        #endregion
    }
}
