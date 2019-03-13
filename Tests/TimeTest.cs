using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace csDelaunay.Tests
{
    public class TimeTest
    {
        [Test]
        public void Nodes_100_Test()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(100);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void Nodes_400_Test()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(400);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void Nodes_1000_Test()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(1000);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void Nodes_2000_Test()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(2000);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void Nodes_10000_Test()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(10000);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [UnityTest]
        public IEnumerator SKIPFRAME()
        {
            yield return null;
            yield return null;
        }
    }
}
