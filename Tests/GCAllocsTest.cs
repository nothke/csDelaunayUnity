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
            Voronoi.InitPools(500, 500, 100);

            var points = VoronoiTest.CreateRandomPoints(50);

            Voronoi voronoi = VoronoiTest.TestVoronoi(points);
            // First redo GC expected
            voronoi.Redo(points, VoronoiTest.TestBounds());

            yield return null;

            for (int i = 0; i < 20; i++)
            {
                UnityEngine.Profiling.Profiler.BeginSample("NoGC Voronoi.Redo same points");
                voronoi.Redo(points, VoronoiTest.TestBounds());
                UnityEngine.Profiling.Profiler.EndSample();
                yield return null;
            }

            yield return null;
        }

        [UnityTest]
        public IEnumerator RedoRandom20TimesNoGCAllocsTest()
        {
            Voronoi.FlushPools();
            Voronoi.InitPools(500, 500, 100);

            var points = VoronoiTest.CreateRandomPoints(50);

            Voronoi voronoi = VoronoiTest.TestVoronoi(points);
            // First redo GC expected
            voronoi.Redo(points, VoronoiTest.TestBounds());

            yield return null;

            for (int i = 0; i < 20; i++)
            {
                points = VoronoiTest.CreateRandomPoints(50);

                UnityEngine.Profiling.Profiler.BeginSample("NoGC Voronoi.Redo random points");
                voronoi.Redo(points, VoronoiTest.TestBounds());
                UnityEngine.Profiling.Profiler.EndSample();
                yield return null;
            }

            yield return null;
        }
    }
}
