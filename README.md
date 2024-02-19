# KURIKURIKUR's Simple A* Pathfinding C# with Passageway Detection
It is a makeshift A* pathfinding algorithm with a passageway detection (I named it pivot point I don't know why).
The algorithm truncates possibility of the point going around and around to go through an obstacle.

The path nodes are stored in a list ready to be used.

## Optimization
My A* Pathfinding detects small passageways in between starting & destination nodes so that these points are to be taken first depending on the smallest terrain cost between the passageway nodes to destination nodes.

### The problem was when the destination is behind a closed wall like this:
*********************

True____True____True____False____True____True____True

True____True____True____False____True____FIN______True

True____True____True____False____True____True____True

True____START__True____False____False___True____True

True____True____True____True____True____True____True

*************************

### The pathfinding will go like this:
****************************

True______4_______3______False____True____True____True

True______5________2______False____True____FIN_____True

True______6________1______False____True_____13_____True

__7______START__True____False____False_____12______True

True______8________9______10_______11______True____True

****************************
So this has 13 steps taken due to the algorihm using Manhattan Method (Terrain cost calculated by using direct line path from start point to end point).
This means the path will start by going up, regardless if there is a wall end and ended up going around to find a passage way to the end point.

This optimization ensures the pathfinding to go directly to passageway while it doesn't hit the wall and go around to find the open passageway.
### The passageway identified:
***********************************
True____True____True____False____True____True____True

True____True____True____False____True____FIN______True

True____True____True____False____True____True____True

True____START__True____False____False___True____True

True____True____True__PASSAGE__True____True____True
*************************************
### And then it will go through like this:
******************************
True____True____True____False____True____True____True

True____True____True____False____True____FIN______True

True____True____True____False____True______5______True

True____START__True____False____False_____4______True

True____True______1________2________3______True____True
******************************
Now the path finding is only using 5 steps.
This can be replicated with different obstacle arrangements and *(should)* be working.

## Next Update?
Next update is gonna be a UI for inputs and to demonstrate and inputs for map size and obstacle shapes.
This personal project is basically for me to learn but feel free to muck around the program to better understand the algorithm.

Lots of comments are for debugging and I hope they mostly make sense.
