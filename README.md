A GC alloc free version of [csDelaunay by PouletFrit](https://github.com/PouletFrit/csDelaunay) optimised for runtime generation

## How to use
To make the code run alloc-free you must initialize element pools with capacities suited to your [maximum] number of sites. You can find out the required capacities by creating a test voronoi diagram and rebuing it many times, at the end call `voronoi.DebugCapacities()` like so:

```
void AnalyseCapacities()
{
    voronoi = new Voronoi(points, new Rectf(0, 0, 2, 2));

    for (int i = 0; i < 1000; i++)
    {
        CreateRandomPoints(points);
        voronoi.Redo(points, new Rectf(0, 0, 2, 2));
    }

    Debug.Log(voronoi.DebugCapacities());
}
```
You might get a result like this:
```
Sites: 50, Halfedges: 218, Edges: 139, EdgesPerSite: 20
```
These are your maximum capacities. Now, plug those numbers (with a few more as a margin, just in case) into the Voronoi constructor:
```
voronoi = new Voronoi(points, new Rectf(0, 0, 2, 2), 225, 150, 25);
```