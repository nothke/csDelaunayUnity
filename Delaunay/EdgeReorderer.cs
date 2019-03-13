using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Profiling;

namespace csDelaunay
{

    public class EdgeReorderer
    {
        public List<Edge> Edges { get; private set; }
        public List<bool> EdgeOrientations { get; private set; }

        static EdgeReorderer instance;
        const int BUFFER_CAPACITY = 64;

        List<Edge> newEdgesBuffer;
        List<bool> doneBuffer;

        public static EdgeReorderer Get()
        {
            if (instance == null)
                instance = new EdgeReorderer();
            else
            {
                instance.Clear();
            }

            return instance;
        }

        EdgeReorderer()
        {
            Edges = new List<Edge>();//
            EdgeOrientations = new List<bool>();

            newEdgesBuffer = new List<Edge>(BUFFER_CAPACITY);
            doneBuffer = new List<bool>(BUFFER_CAPACITY);
        }

        [Obsolete("Use Get() instead")]
        public EdgeReorderer(List<Edge> origEdges, Type criterion)
        {
            Edges = new List<Edge>();
            EdgeOrientations = new List<bool>();
        }

        public static void StaticReorder(List<Edge> origEdges, Type criterion)
        {
            Get();

            if (origEdges == null || origEdges.Count == 0)
                return;

            instance.ReorderEdges(origEdges, criterion);

            origEdges.Clear();
            for (int i = 0; i < instance.newEdgesBuffer.Count; i++)
            {
                origEdges.Add(instance.newEdgesBuffer[i]);
            }
        }

        [Obsolete]
        public void Reorder(List<Edge> origEdges, Type criterion)
        {
            if (origEdges.Count > 0)
            {
                Edges = ReorderEdges(origEdges, criterion);

                origEdges.Clear();
            }
        }

        public void Clear()
        {
            Edges.Clear();
            EdgeOrientations.Clear();

            newEdgesBuffer.Clear();
            doneBuffer.Clear();
        }

        /*
        [Obsolete("Use Clear instead")]
        public void Dispose()
        {
            Edges = null;
            EdgeOrientations = null;
        }*/

        private List<Edge> ReorderEdges(List<Edge> origEdges, Type criterion)
        {
            int i;
            int n = origEdges.Count;
            Edge edge;
            // We're going to reorder the edges in order of traversal
            //List<bool> done = new List<bool>(); // alloc
            int nDone = 0;
            for (int b = 0; b < n; b++) doneBuffer.Add(false);
            //List<Edge> newEdges = new List<Edge>(); // alloc

            i = 0;
            edge = origEdges[i];
            newEdgesBuffer.Add(edge); // extend alloc
            EdgeOrientations.Add(false); // extend alloc
            ICoord firstPoint;
            ICoord lastPoint;
            if (criterion == typeof(Vertex))
            {
                firstPoint = edge.LeftVertex;
                lastPoint = edge.RightVertex;
            }
            else
            {
                firstPoint = edge.LeftSite;
                lastPoint = edge.RightSite;
            }

            if (firstPoint == Vertex.VERTEX_AT_INFINITY || lastPoint == Vertex.VERTEX_AT_INFINITY)
            {
                UnityEngine.Debug.LogError("Puk");
                return new List<Edge>(); // alloc
            }

            doneBuffer[i] = true;
            nDone++;

            while (nDone < n)
            {
                for (i = 1; i < n; i++)
                {
                    if (doneBuffer[i])
                    {
                        continue;
                    }
                    edge = origEdges[i];
                    ICoord leftPoint;
                    ICoord rightPoint;
                    if (criterion == typeof(Vertex))
                    {
                        leftPoint = edge.LeftVertex;
                        rightPoint = edge.RightVertex;
                    }
                    else
                    {
                        leftPoint = edge.LeftSite;
                        rightPoint = edge.RightSite;
                    }
                    if (leftPoint == Vertex.VERTEX_AT_INFINITY || rightPoint == Vertex.VERTEX_AT_INFINITY)
                    {
                        UnityEngine.Debug.LogError("Puk2");
                        return new List<Edge>(); // alloc
                    }
                    if (leftPoint == lastPoint)
                    {
                        lastPoint = rightPoint;
                        EdgeOrientations.Add(false); // extend alloc
                        newEdgesBuffer.Add(edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    else if (rightPoint == firstPoint)
                    {
                        firstPoint = leftPoint;
                        EdgeOrientations.Insert(0, false); // extend alloc
                        newEdgesBuffer.Insert(0, edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    else if (leftPoint == firstPoint)
                    {
                        firstPoint = rightPoint;
                        EdgeOrientations.Insert(0, true); // extend alloc
                        newEdgesBuffer.Insert(0, edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    else if (rightPoint == lastPoint)
                    {
                        lastPoint = leftPoint;
                        EdgeOrientations.Add(true); // extend alloc
                        newEdgesBuffer.Add(edge); // extend alloc
                        doneBuffer[i] = true;
                    }
                    if (doneBuffer[i])
                    {
                        nDone++;
                    }
                }
            }
            return newEdgesBuffer;
        }
    }
}
