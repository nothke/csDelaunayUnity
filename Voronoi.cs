//#define TRIANGLES

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace csDelaunay
{

    public class Voronoi
    {

        private List<Site> sites;
#if TRIANGLES
        private List<Triangle> triangles;
#endif

        public List<Edge> Edges { get; private set; }
        public Rectf PlotBounds { get; private set; }
        public Dictionary<Vector2f, Site> SitesIndexedByLocation { get; private set; }

        private Random weigthDistributor;

        public void Clear()
        {
            sites.Clear();
#if TRIANGLES
            triangles.Clear();
#endif
            Edges.Clear();
            PlotBounds = Rectf.zero;
            SitesIndexedByLocation.Clear();
        }

        [Obsolete]
        public void Dispose()
        {
            sites.Clear();
            sites = null;

#if TRIANGLES
            foreach (Triangle t in triangles)
            {
                t.Dispose();
            }
            triangles.Clear();
#endif

            foreach (Edge e in Edges)
            {
                e.Dispose();
            }
            Edges.Clear();

            PlotBounds = Rectf.zero;
            SitesIndexedByLocation.Clear();
            SitesIndexedByLocation = null;
        }

        public Voronoi(List<Vector2f> points, Rectf plotBounds)
        {
            if (weigthDistributor == null)
                weigthDistributor = new Random();

            Init(points, plotBounds);
        }

        public void Redo(List<Vector2f> points, Rectf plotBounds)
        {
            Clear();
            Init(points, plotBounds);
        }

        public Voronoi(List<Vector2f> points, Rectf plotBounds, int lloydIterations)
        {
            weigthDistributor = new Random();
            Init(points, plotBounds);
            LloydRelaxation(lloydIterations);
        }

        private void Init(List<Vector2f> points, Rectf plotBounds)
        {
            Profiler.BeginSample("Create sites and dict");

            if (sites == null)
                sites = new List<Site>(points.Count);

            if (SitesIndexedByLocation == null)
                SitesIndexedByLocation = new Dictionary<Vector2f, Site>(points.Count);

            Profiler.EndSample();

            Profiler.BeginSample("Add points to sites");
            for (int i = 0; i < points.Count; i++)
            {
                Vector2f p = points[i];

                float weigth = (float)weigthDistributor.NextDouble() * 100;
                Site site = Site.Create(p, i, weigth);
                sites.Add(site);
                SitesIndexedByLocation[p] = site;
            }
            Profiler.EndSample();

            PlotBounds = plotBounds;

            Profiler.BeginSample("Create edges and triangles");
#if TRIANGLES
            if (triangles == null) triangles = new List<Triangle>();
#endif
            if (Edges == null)
                Edges = new List<Edge>();
            Profiler.EndSample();

            FortunesAlgorithm();
        }

        #region Unused Publics

        /*
        public List<Vector2f> Region(Vector2f p)
        {
            Site site;
            if (SitesIndexedByLocation.TryGetValue(p, out site))
            {
                return site.Region(PlotBounds);
            }
            else
            {
                return new List<Vector2f>();
            }
        }*/

        /*
        public List<Vector2f> NeighborSitesForSite(Vector2f coord)
        {
            List<Vector2f> points = new List<Vector2f>();
            Site site;
            if (SitesIndexedByLocation.TryGetValue(coord, out site))
            {
                List<Site> sites = site.NeighborSites();
                foreach (Site neighbor in sites)
                {
                    points.Add(neighbor.Coord);
                }
            }

            return points;
        }*/

        /*
        public List<Circle> Circles()
        {
            return sites.Circles();
        }*/

        /*
        public List<LineSegment> VoronoiBoundarayForSite(Vector2f coord)
        {
            return LineSegment.VisibleLineSegments(Edge.SelectEdgesForSitePoint(coord, Edges));
        }*/

        /*
		public List<LineSegment> DelaunayLinesForSite(Vector2f coord) {
			return DelaunayLinesForEdges(Edge.SelectEdgesForSitePoint(coord, edges));
		}*/

        /*
        public List<LineSegment> VoronoiDiagram()
        {
            return LineSegment.VisibleLineSegments(Edges);
        }*/

        /*
		public List<LineSegment> Hull() {
			return DelaunayLinesForEdges(HullEdges());
		}*/

        /*
        public List<Edge> HullEdges()
        {
            return Edges.FindAll(edge => edge.IsPartOfConvexHull());
        }*/

        /*
        public List<Vector2f> HullPointsInOrder()
        {
            List<Edge> hullEdges = HullEdges();

            List<Vector2f> points = new List<Vector2f>();
            if (hullEdges.Count == 0)
            {
                return points;
            }

            EdgeReorderer reorderer = new EdgeReorderer(hullEdges, typeof(Site));
            hullEdges = reorderer.Edges;
            List<bool> orientations = reorderer.EdgeOrientations;
            reorderer.Dispose();

            bool orientation;
            for (int i = 0; i < hullEdges.Count; i++)
            {
                Edge edge = hullEdges[i];
                orientation = orientations[i];
                points.Add(edge.Site(orientation).Coord);
            }
            return points;
        }*/

        /*
        public List<List<Vector2f>> Regions()
        {
            return sites.Regions(PlotBounds);
        }*/

        /*
        public List<Vector2f> SiteCoords()
        {
            return sites.SiteCoords();
        }*/

        #endregion


        int currentSiteIndex;
        int nVertices;
        HalfedgePriorityQueue heap;

        EdgeList edgeList;
        List<Halfedge> halfEdges;
        List<Vertex> vertices;

        private void FortunesAlgorithm()
        {
            currentSiteIndex = 0;
            nVertices = 0;

            // vars

            Profiler.BeginSample("Fortunes: initing");
            // holds coord and list of edges
            Site newSite, bottomSite, topSite, tempSite;
            // holds coord and index?
            Vertex v, vertex;
            // ??
            Vector2f newIntStar = Vector2f.zero;
            // ??
            bool leftRight;
            // half edge is the directed edge
            Halfedge lbnd, rbnd, llbnd, rrbnd, bisector;
            // Edge is a point to point line
            Edge edge;
            Profiler.EndSample();

            // Data bounds
            Profiler.BeginSample("Fortunes: Getting data bounds");
            Rectf dataBounds = SiteUtils.GetSitesBounds(sites);
            Profiler.EndSample();


            int sqrtSitesNb = (int)Math.Sqrt(sites.Count + 4); // WTF

            Profiler.BeginSample("Fortunes: Init heap");
            heap = new HalfedgePriorityQueue(dataBounds.y, dataBounds.height, sqrtSitesNb);

            /*
            if (heap == null)
            {
                //UnityEngine.Debug.Log("Creating NEW heap");
                heap = new HalfedgePriorityQueue(dataBounds.y, dataBounds.height, sqrtSitesNb);
            }
            else
                heap.ReinitNoSizeChange(dataBounds.y, dataBounds.height);*/
            Profiler.EndSample();

            Profiler.BeginSample("Fortunes: Init EdgeList");
            /*
            if (edgeList == null)
            {
                edgeList = new EdgeList(dataBounds.x, dataBounds.width, sqrtSitesNb);
            }
            else
                edgeList.ClearNoResize(dataBounds.x, dataBounds.width);*/

            edgeList = new EdgeList(dataBounds.x, dataBounds.width, sqrtSitesNb);
            Profiler.EndSample();

            Profiler.BeginSample("Fortunes: Init HEs and vertices");
            if (halfEdges == null) // TODO: Move to init
            {
                halfEdges = new List<Halfedge>();
                vertices = new List<Vertex>();
            }
            else
            {
                halfEdges.Clear();
                vertices.Clear();
            }
            Profiler.EndSample();

            Site bottomMostSite = GetNextSite();
            newSite = GetNextSite();

            Profiler.BeginSample("Fortunes: Main Loop");
            while (true)
            {
                if (!heap.Empty())
                {
                    newIntStar = heap.Min();
                }

                if (newSite != null &&
                    (heap.Empty() || CompareByYThenX(newSite, newIntStar) < 0))
                {
                    // New site is smallest
                    //Debug.Log("smallest: new site " + newSite);

                    // Step 8:
                    // The halfedge just to the left of newSite
                    //UnityEngine.Debug.Log("lbnd: " + lbnd);
                    lbnd = edgeList.EdgeListLeftNeighbor(newSite.Coord);
                    // The halfedge just to the right
                    rbnd = lbnd.edgeListRightNeighbor;
                    //UnityEngine.Debug.Log("rbnd: " + rbnd);

                    // This is the same as leftRegion(rbnd)
                    // This Site determines the region containing the new site
                    bottomSite = RightRegion(lbnd, bottomMostSite);
                    //UnityEngine.Debug.Log("new Site is in region of existing site: " + bottomSite);

                    // Step 9
                    edge = Edge.CreateBisectingEdge(bottomSite, newSite);
                    //UnityEngine.Debug.Log("new edge: " + edge);
                    Edges.Add(edge);

                    bisector = Halfedge.Create(edge, false);
                    halfEdges.Add(bisector);
                    // Inserting two halfedges into edgelist constitutes Step 10:
                    // Insert bisector to the right of lbnd:
                    edgeList.Insert(lbnd, bisector);

                    // First half of Step 11:
                    if ((vertex = Vertex.Intersect(lbnd, bisector)) != null)
                    {
                        vertices.Add(vertex);
                        heap.Remove(lbnd);
                        lbnd.vertex = vertex;
                        lbnd.ystar = vertex.y + newSite.Dist(vertex);
                        heap.Insert(lbnd);
                    }

                    lbnd = bisector;
                    bisector = Halfedge.Create(edge, true);
                    halfEdges.Add(bisector);
                    // Second halfedge for Step 10::
                    // Insert bisector to the right of lbnd:
                    edgeList.Insert(lbnd, bisector);

                    // Second half of Step 11:
                    if ((vertex = Vertex.Intersect(bisector, rbnd)) != null)
                    {
                        vertices.Add(vertex);
                        bisector.vertex = vertex;
                        bisector.ystar = vertex.y + newSite.Dist(vertex);
                        heap.Insert(bisector);
                    }

                    newSite = GetNextSite();
                }
                else if (!heap.Empty())
                {
                    // Intersection is smallest
                    lbnd = heap.ExtractMin();
                    llbnd = lbnd.edgeListLeftNeighbor;
                    rbnd = lbnd.edgeListRightNeighbor;
                    rrbnd = rbnd.edgeListRightNeighbor;
                    bottomSite = LeftRegion(lbnd, bottomMostSite);
                    topSite = RightRegion(rbnd, bottomMostSite);
                    // These three sites define a Delaunay triangle
                    // (not actually using these for anything...)
#if TRIANGLES
                    triangles.Add(new Triangle(bottomSite, topSite, RightRegion(lbnd, bottomMostSite)));
#endif

                    v = lbnd.vertex;
                    v.VertexIndex = nVertices++;
                    lbnd.edge.SetVertex(lbnd.leftRight, v);
                    rbnd.edge.SetVertex(rbnd.leftRight, v);
                    edgeList.Remove(lbnd);
                    heap.Remove(rbnd);
                    edgeList.Remove(rbnd);
                    leftRight = false;
                    if (bottomSite.y > topSite.y)
                    {
                        tempSite = bottomSite;
                        bottomSite = topSite;
                        topSite = tempSite;
                        leftRight = true;
                    }
                    edge = Edge.CreateBisectingEdge(bottomSite, topSite);
                    Edges.Add(edge);
                    bisector = Halfedge.Create(edge, leftRight);
                    halfEdges.Add(bisector);
                    edgeList.Insert(llbnd, bisector);
                    edge.SetVertex(!leftRight, v);
                    if ((vertex = Vertex.Intersect(llbnd, bisector)) != null)
                    {
                        vertices.Add(vertex);
                        heap.Remove(llbnd);
                        llbnd.vertex = vertex;
                        llbnd.ystar = vertex.y + bottomSite.Dist(vertex);
                        heap.Insert(llbnd);
                    }
                    if ((vertex = Vertex.Intersect(bisector, rrbnd)) != null)
                    {
                        vertices.Add(vertex);
                        bisector.vertex = vertex;
                        bisector.ystar = vertex.y + bottomSite.Dist(vertex);
                        heap.Insert(bisector);
                    }
                }
                else
                {
                    break;
                }
            }
            Profiler.EndSample();

            // DISPOSE

            // Heap should be empty now
            heap.Dispose();
            edgeList.Dispose();

            foreach (Halfedge halfedge in halfEdges)
            {
                halfedge.ReallyDispose();
            }
            halfEdges.Clear();

            // we need the vertices to clip the edges
            foreach (Edge e in Edges)
            {
                e.ClipVertices(PlotBounds);
            }
            // But we don't actually ever use them again!
            foreach (Vertex ve in vertices)
            {
                ve.Dispose();
            }
            vertices.Clear();
        }

        public void LloydRelaxation(int nbIterations)
        {
            // Reapeat the whole process for the number of iterations asked
            for (int i = 0; i < nbIterations; i++)
            {
                List<Vector2f> newPoints = new List<Vector2f>();
                // Go thourgh all sites
                currentSiteIndex = 0; // sites.ResetListIndex();
                Site site = GetNextSite();

                while (site != null)
                {
                    // Loop all corners of the site to calculate the centroid
                    List<Vector2f> region = site.Region(PlotBounds);
                    if (region.Count < 1)
                    {
                        site = GetNextSite();
                        continue;
                    }

                    Vector2f centroid = Vector2f.zero;
                    float signedArea = 0;
                    float x0 = 0;
                    float y0 = 0;
                    float x1 = 0;
                    float y1 = 0;
                    float a = 0;
                    // For all vertices except last
                    for (int j = 0; j < region.Count - 1; j++)
                    {
                        x0 = region[j].x;
                        y0 = region[j].y;
                        x1 = region[j + 1].x;
                        y1 = region[j + 1].y;
                        a = x0 * y1 - x1 * y0;
                        signedArea += a;
                        centroid.x += (x0 + x1) * a;
                        centroid.y += (y0 + y1) * a;
                    }
                    // Do last vertex
                    x0 = region[region.Count - 1].x;
                    y0 = region[region.Count - 1].y;
                    x1 = region[0].x;
                    y1 = region[0].y;
                    a = x0 * y1 - x1 * y0;
                    signedArea += a;
                    centroid.x += (x0 + x1) * a;
                    centroid.y += (y0 + y1) * a;

                    signedArea *= 0.5f;
                    centroid.x /= (6 * signedArea);
                    centroid.y /= (6 * signedArea);
                    // Move site to the centroid of its Voronoi cell
                    newPoints.Add(centroid);
                    site = GetNextSite();
                }

                // Between each replacement of the cendroid of the cell,
                // we need to recompute Voronoi diagram:
                Rectf origPlotBounds = this.PlotBounds;
                //Dispose();
                Clear();
                Init(newPoints, origPlotBounds);
            }
        }

        private Site GetNextSite()
        {
            if (currentSiteIndex < sites.Count)
                return sites[currentSiteIndex++];
            else
                return null;
        }

        private Site LeftRegion(Halfedge he, Site bottomMostSite)
        {
            Edge edge = he.edge;
            if (edge == null)
            {
                return bottomMostSite;
            }
            return edge.Site(he.leftRight);
        }

        private Site RightRegion(Halfedge he, Site bottomMostSite)
        {
            Edge edge = he.edge;
            if (edge == null)
            {
                return bottomMostSite;
            }
            return edge.Site(!he.leftRight);
        }

        public static int CompareByYThenX(Site s1, Site s2)
        {
            if (s1.y < s2.y) return -1;
            if (s1.y > s2.y) return 1;
            if (s1.x < s2.x) return -1;
            if (s1.x > s2.x) return 1;
            return 0;
        }

        public static int CompareByYThenX(Site s1, Vector2f s2)
        {
            if (s1.y < s2.y) return -1;
            if (s1.y > s2.y) return 1;
            if (s1.x < s2.x) return -1;
            if (s1.x > s2.x) return 1;
            return 0;
        }
    }
}
