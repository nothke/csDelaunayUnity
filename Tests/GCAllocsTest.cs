/// Note, these tests must be performed manually by looking into the profiler,
/// because there is no way to assert GC allocations automatically
/// 1. Run the tests
/// 2. Go to the profiler window and type in "NoGC" in the search
/// 3. Scroll through the Profiler frames and see if any methods starting with "NoGC" has more than 0 GCAllocs

using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.Profiling;

namespace csDelaunay.Tests
{

    public class GCAllocsTest
    {
        [UnityTest]
        public IEnumerator RedoSame20TimesNoGCAllocsTest()
        {
            Voronoi.FlushPools();
            Voronoi.InitPools(500, 500, 40, 200);

            var points = VoronoiTest.CreateRandomPoints(50);

            yield return null;

            Voronoi voronoi = VoronoiTest.TestVoronoi(points);

            yield return null;

            for (int i = 0; i < 20; i++)
            {
                Profiler.BeginSample("NoGC Voronoi.Redo same points");
                voronoi.Redo(points, VoronoiTest.TestBounds());
                Profiler.EndSample();
                yield return null;
            }

            Debug.Log(voronoi.DebugCapacities());

            yield return null;
        }

        [UnityTest]
        public IEnumerator RedoRandom20TimesNoGCAllocsTest()
        {
            Voronoi.FlushPools();
            Voronoi.InitPools(500, 500, 40, 200);

            var points = VoronoiTest.CreateRandomPoints(50);

            Voronoi voronoi = VoronoiTest.TestVoronoi(points);

            yield return null;

            for (int i = 0; i < 20; i++)
            {
                points = VoronoiTest.CreateRandomPoints(50);

                Profiler.BeginSample("NoGC Voronoi.Redo 20x50 random points");
                voronoi.Redo(points, VoronoiTest.TestBounds());
                Profiler.EndSample();
                yield return null;
            }

            Debug.Log(voronoi.DebugCapacities());

            yield return null;
        }

        [UnityTest]
        public IEnumerator RedoRandom200TimesWith200PointsTest()
        {
            Voronoi.FlushPools();
            Voronoi.InitPools(900, 600, 40, 800);

            var points = VoronoiTest.CreateRandomPoints(200);

            Voronoi voronoi = VoronoiTest.TestVoronoi(points);

            yield return null;

            for (int i = 0; i < 200; i++)
            {
                points = VoronoiTest.CreateRandomPoints(200);

                Profiler.BeginSample("NoGC Voronoi.Redo 200x200 random points");
                voronoi.Redo(points, VoronoiTest.TestBounds());
                Profiler.EndSample();
                yield return null;
            }

            Debug.Log(voronoi.DebugCapacities());

            yield return null;
        }

        [UnityTest]
        public IEnumerator RedoRandom20TimesWith3000PointsTest()
        {
            Voronoi.FlushPools();
            Voronoi.InitPools(12500, 9000, 40, 11000);

            var points = VoronoiTest.CreateRandomPoints(3000);

            Voronoi voronoi = VoronoiTest.TestVoronoi(points);

            yield return null;

            for (int i = 0; i < 20; i++)
            {
                points = VoronoiTest.CreateRandomPoints(3000);

                Profiler.BeginSample("NoGC Voronoi.Redo 20x3000 random points");
                voronoi.Redo(points, VoronoiTest.TestBounds());
                Profiler.EndSample();
                yield return null;
            }

            Debug.Log(voronoi.DebugCapacities());

            yield return null;
        }
    }
}
