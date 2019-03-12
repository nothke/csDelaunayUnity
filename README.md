<p align="center">
<img src="https://i.imgur.com/KO1h1Qs.gif" width="600">
</p>

A GC alloc free version of [csDelaunay by PouletFrit](https://github.com/PouletFrit/csDelaunay) optimised for runtime generation

This version has breaking changes between the original, so it is not necesarily a "fork". Since this is used for a personal project where only constructing voronoi diagrams and retrieving clipped edges was needed, all other features are not tested nor guaranteed to be GC alloc free (like LloydRelaxation), and some methods may be removed or commented out.

## How to use

#### 1. Analyse capacities
To make the code run alloc-free you must initialize element pools with capacities suited to your [maximum] number of sites. You can find out the required capacities by creating a test voronoi diagram and rebuild it many times, at the end call `voronoi.DebugCapacities()` like so:

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
#### 2. Init pools with capacities
These are your maximum capacities. Now, take the last 3 numbers (and add a with a few more as a margin, just in case) and call `InitPools` before you create your Voronoi:
```
Voronoi.InitPools(225, 150, 25);
voronoi = new Voronoi(points, new Rectf(0, 0, 2, 2));
```
#### 3. Profit
Now, next time you call:
```
voronoi.Redo(points, new Rectf(0,0,2,2));
```
You should have no GC allocs at all!
