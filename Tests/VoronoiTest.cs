using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace csDelaunay.Tests
{
    public class VoronoiTest
    {
        [Test]
        public void PlotBoundsEqualityTest()
        {
            Random.InitState(10);
            var points = CreateRandomPoints(50);
            Voronoi voronoi = TestVoronoi(points);

            Rectf plotBounds = voronoi.PlotBounds;

            voronoi.Redo(points, TestBounds());

            Rectf plotBounds2 = voronoi.PlotBounds;

            //Debug.Log($"Plot bounds: {plotBounds.x}, { plotBounds.y}, { plotBounds.width}, {plotBounds.height}");

            Assert.AreEqual(plotBounds.x, plotBounds2.x, "Plot bounds x not equal");
            Assert.AreEqual(plotBounds.y, plotBounds2.y, "Plot bounds y not equal");
            Assert.AreEqual(plotBounds.width, plotBounds2.width, "Plot bounds width not equal");
            Assert.AreEqual(plotBounds.height, plotBounds2.height, "Plot bounds height not equal");
        }

        [Test]
        public void FirstClippedEdgeEqualityTest()
        {
            Random.InitState(10);
            var points = CreateRandomPoints(50);
            Voronoi voronoi = TestVoronoi(points);

            Rectf plotBounds = voronoi.PlotBounds;

            // first clipped edge
            Edge clippedEdge = GetFirstClippedEdge(voronoi);

            Vector2f e1v1 = clippedEdge.ClippedEnds[0];

            voronoi.Redo(points, TestBounds());

            // first clipped edge
            Edge clippedEdge2 = GetFirstClippedEdge(voronoi);

            Vector2f e2v1 = clippedEdge2.ClippedEnds[0];

            Assert.AreEqual(e1v1, e2v1);
        }

        [Test]
        public void GetRegionFromDictTest()
        {
            Random.InitState(10);
            var points = CreateRandomPoints(50);
            Voronoi voronoi = TestVoronoi(points);

            var site = voronoi.sites[0];
            Vector2f position = site.Coord;

            var siteTest = voronoi.SitesIndexedByLocation[position];

            Assert.AreEqual(site, siteTest);
        }

        [Test]
        public void SameRegionFromRedoDiagramByLocationTest()
        {
            Random.InitState(10);
            var points = CreateRandomPoints(50);
            Voronoi voronoi = TestVoronoi(points);

            var site = voronoi.sites[0];
            Vector2f site1coord = site.Coord;

            // redo multiple times
            for (int i = 0; i < 100; i++)
            {
                voronoi.Redo(points, TestBounds());
            }

            var siteTest = voronoi.SitesIndexedByLocation[site1coord];
            Vector2f site2coord = siteTest.Coord;

            Assert.AreEqual(site1coord, site2coord);
        }

        [Test]
        public void SameClippedVerticesAfterRedo()
        {
            Random.InitState(10);
            var points = CreateRandomPoints(50);
            Voronoi voronoi = TestVoronoi(points);

            var edges = new List<Vector2f>();
            voronoi.GetAllClippedLines(edges);

            // redo multiple times
            for (int i = 0; i < 100; i++)
            {
                voronoi.Redo(points, TestBounds());
            }

            var edges2 = new List<Vector2f>();
            voronoi.GetAllClippedLines(edges2);

            for (int i = 0; i < edges.Count; i++)
            {
                Assert.AreEqual(edges[i], edges2[i]);
            }
        }

        

        [UnityTest]
        public IEnumerator SKIPFRAME()
        {
            yield return null;
            yield return null;
        }



        // Utilities:

        Edge GetFirstClippedEdge(Voronoi voronoi)
        {
            foreach (var edge in voronoi.Edges)
            {
                if (edge.Clipped)
                {
                    return edge;
                }
            }

            return null;
        }

        public static Voronoi TestVoronoi(List<Vector2f> points) { return new Voronoi(points, TestBounds()); }
        public static Rectf TestBounds() { return new Rectf(0, 0, 2, 2); }
        public static Vector2f MidPoint() { return new Vector2f(1, 1); }

        public static List<Vector2f> CreateRandomPoints(int num)
        {
            List<Vector2f> points = new List<Vector2f>(num);
            for (int i = 0; i < num; i++)
            {
                Vector2 rv = Vector2.one + Random.insideUnitCircle * 0.9f;
                points.Add(new Vector2f(rv.x, rv.y));
            }

            return points;
        }
    }
}
