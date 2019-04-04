using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Profiling;

namespace csDelaunay.Tests
{

    public class EdgeReordererTest
    {
        [UnityTest]
        public IEnumerator AllocTest()
        {
            Profiler.BeginSample("Initing");
            Random.InitState(10);
            var points = VoronoiTest.CreateRandomPoints(50);
            Voronoi voronoi = VoronoiTest.TestVoronoi(points);
            List<Edge> edges = voronoi.sites[0].Edges;
            List<bool> edgeOrientations = new List<bool>(edges.Count);
            Profiler.EndSample();

            //var reorderedEdges = er.Edges;
            List<Edge> originalEdges = new List<Edge>();
            originalEdges.AddRange(edges);

            yield return null;

            Profiler.BeginSample("EdgeReorderer create");
            EdgeReorderer.CreateInstance();
            Profiler.EndSample();

            yield return null;

            Profiler.BeginSample("First EdgeReorderer reorder");
            EdgeReorderer.Reorder(ref edges, ref edgeOrientations, typeof(Vertex));
            Profiler.EndSample();

            yield return null;

            Profiler.BeginSample("NoGC EdgeReorderer reorder");
            EdgeReorderer.Reorder(ref edges, ref edgeOrientations, typeof(Vertex));
            Profiler.EndSample();

            yield return null;

            edges = new List<Edge>();
            edges.AddRange(originalEdges);

            Profiler.BeginSample("NoGC EdgeReorder another reorder");
            EdgeReorderer.Reorder(ref edges, ref edgeOrientations, typeof(Vertex));
            Profiler.EndSample();

            yield return null;

            Assert.Greater(originalEdges.Count, 0);
            Assert.IsNotNull(edges);
            Assert.IsNotNull(originalEdges);
            Assert.AreEqual(edges.Count, originalEdges.Count);

            yield return null;
        }

        [UnityTest]
        public IEnumerator CapacitiesInspect()
        {
            Debug.LogWarning(EdgeReorderer.GetCapacities());

            yield return null;
        }
    }
}