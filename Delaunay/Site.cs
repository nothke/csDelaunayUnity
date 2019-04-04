//#define CAPACITY_WARNING

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace csDelaunay
{

    public class Site : ICoord
    {
        public static int edgesCapacity = 10;

        private static Queue<Site> unusedPool = new Queue<Site>();
        public static int PoolCapacity { get => unusedPool.Count; }

        /// <summary>
        /// Use only for testing
        /// </summary>
        public static void FlushUnused()
        {
            unusedPool = new Queue<Site>();
        }

        public static int GetMaxEdgeCapacity(List<Site> sites)
        {
            int max = 0;
            foreach (var site in sites)
            {
                int c = site.edges.Capacity;
                if (c > max) max = c;
            }

            return max;
        }

        public static Rectf GetSitesBounds(List<Site> sites)
        {
            /*if (!sorted)
            {
                Site.SortSites(sites);
                //SortList();
                //ResetListIndex();
                //currentIndex = 0;
            }*/

            SortSites(sites); // always sort

            float xmin, xmax, ymin, ymax;

            if (sites.Count == 0)
            {
                return Rectf.zero;
            }

            xmin = float.MaxValue;
            xmax = float.MinValue;

            foreach (Site site in sites)
            {
                if (site.x < xmin) xmin = site.x;
                if (site.x > xmax) xmax = site.x;
            }

            // here's where we assume that the sites have been sorted on y:
            ymin = sites[0].y;
            ymax = sites[sites.Count - 1].y;

            return new Rectf(xmin, ymin, xmax - xmin, ymax - ymin);
        }

        public static Site Create(Vector2f p, int index, float weigth)
        {
            //UnityEngine.Debug.Log("Queuesize: " + unusedPool.Count);

            if (unusedPool.Count > 0)
            {
                Site site = unusedPool.Dequeue();
                site.Init(p, index, weigth);
                return site;
            }
            else
            {
                Site site = new Site(p, index, weigth);
                return site;
            }
        }

        public static void SortSites(List<Site> sites)
        {
            sites.Sort(delegate (Site s0, Site s1)
            {
                int returnValue = Voronoi.CompareByYThenX(s0, s1);

                int tempIndex;

                if (returnValue == -1)
                {
                    if (s0.siteIndex > s1.SiteIndex)
                    {
                        tempIndex = s0.SiteIndex;
                        s0.SiteIndex = s1.SiteIndex;
                        s1.SiteIndex = tempIndex;
                    }
                }
                else if (returnValue == 1)
                {
                    if (s1.SiteIndex > s0.SiteIndex)
                    {
                        tempIndex = s1.SiteIndex;
                        s1.SiteIndex = s0.SiteIndex;
                        s0.SiteIndex = tempIndex;
                    }
                }

                return returnValue;
            });
        }

        public int Compare(Site s1, Site s2)
        {
            return s1.CompareTo(s2);
        }

        public int CompareTo(Site s1)
        {
            int returnValue = Voronoi.CompareByYThenX(this, s1);

            int tempIndex;

            if (returnValue == -1)
            {
                if (this.siteIndex > s1.SiteIndex)
                {
                    tempIndex = this.SiteIndex;
                    this.SiteIndex = s1.SiteIndex;
                    s1.SiteIndex = tempIndex;
                }
            }
            else if (returnValue == 1)
            {
                if (s1.SiteIndex > this.SiteIndex)
                {
                    tempIndex = s1.SiteIndex;
                    s1.SiteIndex = this.SiteIndex;
                    this.SiteIndex = tempIndex;
                }
            }

            return returnValue;
        }

        private const float EPSILON = 0.005f;
        private static bool CloseEnough(Vector2f p0, Vector2f p1)
        {
            return (p0 - p1).magnitude < EPSILON;
        }

        private int siteIndex;
        public int SiteIndex { get { return siteIndex; } set { siteIndex = value; } }

        private Vector2f coord;
        public Vector2f Coord { get { return coord; } set { coord = value; } }

        public float x { get { return coord.x; } }
        public float y { get { return coord.y; } }

        private float weigth;
        public float Weigth { get { return weigth; } }

        // The edges that define this Site's Voronoi region:
        private List<Edge> edges;
        public List<Edge> Edges { get { return edges; } }
        // which end of each edge hooks up with the previous edge in edges:
        private List<bool> edgeOrientations;
        // ordered list of points that define the region clipped to bounds:
        private List<Vector2f> region;

        public Site(Vector2f p, int index, float weigth)
        {
            Init(p, index, weigth);
        }

        private Site Init(Vector2f p, int index, float weigth)
        {
            coord = p;
            siteIndex = index;
            this.weigth = weigth;

            if (edges == null)
                edges = new List<Edge>(edgesCapacity);
            else
                edges.Clear();

            // used for regions:
            if (edgeOrientations == null)
                edgeOrientations = new List<bool>(edgesCapacity);
            else
                edges.Clear();

            if (region != null)
                region.Clear();
            //region = null;

            return this;
        }

        public override string ToString()
        {
            return "Site " + siteIndex + ": " + coord;
        }

        private void Move(Vector2f p)
        {
            Clear();
            coord = p;
        }

        public void Dispose()
        {
            Clear();
            unusedPool.Enqueue(this);
        }

        private void Clear()
        {
            if (edges != null)
            {
                int disposedct = 0;
                for (int i = 0; i < edges.Count; i++)
                {
                    if (!edges[i].disposed)
                    {
                        disposedct++;
                        edges[i].Dispose();
                    }
                }

                if (disposedct > 0)
                    UnityEngine.Debug.Log("There were " + disposedct + " undisposed edges");

                edges.Clear();
                //edges = null;
            }
            if (edgeOrientations != null)
            {
                edgeOrientations.Clear();
                //edgeOrientations = null;
            }
            if (region != null)
            {
                region.Clear();
                //region = null;
            }
        }

#if CAPACITY_WARNING
        int prevCapacity;
#endif

        public void AddEdge(Edge edge)
        {
            edges.Add(edge);

#if CAPACITY_WARNING
            if (edges.Capacity != prevCapacity)
                UnityEngine.Debug.Log($"Capacity got extended from {prevCapacity} to {edges.Capacity}");

            prevCapacity = edges.Capacity;
#endif
        }

        public Edge NearestEdge()
        {
            edges.Sort(Edge.CompareSitesDistances);
            return edges[0];
        }

        // Not correct!
        public List<Site> NeighborSites()
        {
            if (edges == null || edges.Count == 0)
            {
                return new List<Site>();
            }
            if (edgeOrientations == null)
            {
                ReorderEdges(); // alloc
            }
            List<Site> list = new List<Site>();
            foreach (Edge edge in edges)
            {
                list.Add(NeighborSite(edge));
            }
            return list;
        }

        private Site NeighborSite(Edge edge)
        {
            if (this == edge.LeftSite)
            {
                return edge.RightSite;
            }
            if (this == edge.RightSite)
            {
                return edge.LeftSite;
            }
            return null;
        }

        // alloc
        public List<Vector2f> Region(Rectf clippingBounds)
        {
            if (edges == null || edges.Count == 0)
                return null;

            ReorderEdges();

            region = ClipToBounds(clippingBounds); // alloc

            if ((new Polygon(region)).PolyWinding() == Winding.CLOCKWISE) // alloc
                region.Reverse();

            return region;
        }

        private void ReorderEdges()
        {
            EdgeReorderer.Reorder(ref edges, ref edgeOrientations, typeof(Vertex));

            /*
            EdgeReorderer reorderer = EdgeReorderer.Get(); //new EdgeReorderer(edges, typeof(Vertex));
            edges = reorderer.Edges;
            edgeOrientations = reorderer.EdgeOrientations;
            reorderer.Clear(); //reorderer.Dispose();*/
        }

        // alloc
        private List<Vector2f> ClipToBounds(Rectf bounds)
        {
            List<Vector2f> points = new List<Vector2f>();
            int n = edges.Count;
            int i = 0;
            Edge edge;

            while (i < n && !edges[i].Clipped)
            {
                i++;
            }

            if (i == n)
            {
                // No edges visible
                return new List<Vector2f>();
            }
            edge = edges[i];

            UnityEngine.Debug.Assert(edgeOrientations != null, "Edge orientations is null");

            bool orientation = edgeOrientations[i];

            if (!edge.Clipped)
                UnityEngine.Debug.LogError("Edge is not clipped!");

            points.Add(edge.ClippedEnds[orientation ? 1 : 0]);
            points.Add(edge.ClippedEnds[!orientation ? 1 : 0]);

            for (int j = i + 1; j < n; j++)
            {
                edge = edges[j];
                if (!edge.Clipped)
                {
                    continue;
                }
                Connect(ref points, j, bounds);
            }
            // Close up the polygon by adding another corner point of the bounds if needed:
            Connect(ref points, i, bounds, true);

            return points;
        }

        private void Connect(ref List<Vector2f> points, int j, Rectf bounds, bool closingUp = false)
        {
            Vector2f rightPoint = points[points.Count - 1];
            Edge newEdge = edges[j];
            bool newOrientation = edgeOrientations[j];

            // The point that must be conected to rightPoint:
            Vector2f newPoint = newEdge.ClippedEnds[newOrientation ? 1 : 0];

            if (!CloseEnough(rightPoint, newPoint))
            {
                // The points do not coincide, so they must have been clipped at the bounds;
                // see if they are on the same border of the bounds:
                if (rightPoint.x != newPoint.x && rightPoint.y != newPoint.y)
                {
                    // They are on different borders of the bounds;
                    // insert one or two corners of bounds as needed to hook them up:
                    // (NOTE this will not be correct if the region should take up more than
                    // half of the bounds rect, for then we will have gone the wrong way
                    // around the bounds and included the smaller part rather than the larger)
                    int rightCheck = BoundsCheck.Check(rightPoint, bounds);
                    int newCheck = BoundsCheck.Check(newPoint, bounds);
                    float px, py;
                    if ((rightCheck & BoundsCheck.RIGHT) != 0)
                    {
                        px = bounds.right;

                        if ((newCheck & BoundsCheck.BOTTOM) != 0)
                        {
                            py = bounds.bottom;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.TOP) != 0)
                        {
                            py = bounds.top;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.LEFT) != 0)
                        {
                            if (rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height)
                            {
                                py = bounds.top;
                            }
                            else
                            {
                                py = bounds.bottom;
                            }
                            points.Add(new Vector2f(px, py));
                            points.Add(new Vector2f(bounds.left, py));
                        }
                    }
                    else if ((rightCheck & BoundsCheck.LEFT) != 0)
                    {
                        px = bounds.left;

                        if ((newCheck & BoundsCheck.BOTTOM) != 0)
                        {
                            py = bounds.bottom;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.TOP) != 0)
                        {
                            py = bounds.top;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.RIGHT) != 0)
                        {
                            if (rightPoint.y - bounds.y + newPoint.y - bounds.y < bounds.height)
                            {
                                py = bounds.top;
                            }
                            else
                            {
                                py = bounds.bottom;
                            }
                            points.Add(new Vector2f(px, py));
                            points.Add(new Vector2f(bounds.right, py));
                        }
                    }
                    else if ((rightCheck & BoundsCheck.TOP) != 0)
                    {
                        py = bounds.top;

                        if ((newCheck & BoundsCheck.RIGHT) != 0)
                        {
                            px = bounds.right;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.LEFT) != 0)
                        {
                            px = bounds.left;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.BOTTOM) != 0)
                        {
                            if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width)
                            {
                                px = bounds.left;
                            }
                            else
                            {
                                px = bounds.right;
                            }
                            points.Add(new Vector2f(px, py));
                            points.Add(new Vector2f(px, bounds.bottom));
                        }
                    }
                    else if ((rightCheck & BoundsCheck.BOTTOM) != 0)
                    {
                        py = bounds.bottom;

                        if ((newCheck & BoundsCheck.RIGHT) != 0)
                        {
                            px = bounds.right;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.LEFT) != 0)
                        {
                            px = bounds.left;
                            points.Add(new Vector2f(px, py));

                        }
                        else if ((newCheck & BoundsCheck.TOP) != 0)
                        {
                            if (rightPoint.x - bounds.x + newPoint.x - bounds.x < bounds.width)
                            {
                                px = bounds.left;
                            }
                            else
                            {
                                px = bounds.right;
                            }
                            points.Add(new Vector2f(px, py));
                            points.Add(new Vector2f(px, bounds.top));
                        }
                    }
                }
                if (closingUp)
                {
                    // newEdge's ends have already been added
                    return;
                }
                points.Add(newPoint);
            }
            Vector2f newRightPoint = newEdge.ClippedEnds[!newOrientation ? 0 : 1];
            if (!CloseEnough(points[0], newRightPoint))
            {
                points.Add(newRightPoint);
            }
        }

        public float Dist(ICoord p)
        {
            return (this.Coord - p.Coord).magnitude;
        }
    }

    public class BoundsCheck
    {
        public const int TOP = 1;
        public const int BOTTOM = 2;
        public const int LEFT = 4;
        public const int RIGHT = 8;

        /*
		 * 
		 * @param point
		 * @param bounds
		 * @return an int with the appropriate bits set if the Point lies on the corresponding bounds lines
		 */
        public static int Check(Vector2f point, Rectf bounds)
        {
            int value = 0;
            if (point.x == bounds.left)
            {
                value |= LEFT;
            }
            if (point.x == bounds.right)
            {
                value |= RIGHT;
            }
            if (point.y == bounds.top)
            {
                value |= TOP;
            }
            if (point.y == bounds.bottom)
            {
                value |= BOTTOM;
            }

            return value;
        }
    }
}
