using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace csDelaunay.Tests
{
    public class CapacityTest
    {
        [Test]
        public void _100_PointsTest()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(100);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void _400_PointsTest()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(400);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void _1000_PointsTest()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(1000);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void _2000_PointsTest()
        {
            Voronoi.FlushPools();

            var points = VoronoiTest.CreateRandomPoints(2000);
            var voronoi = VoronoiTest.TestVoronoi(points);

            Debug.Log(voronoi.DebugCapacities());
        }

        [Test]
        public void _10000_Points_Test()
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
