using System;
using System.Collections;
using System.Drawing;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace pathfinder
{
    class Pathfinder
    {        
        public static void Main(string[] args)
        {
            // Clear Console
            try
            {
                Console.Clear();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            // init map
            // we need to create grids that have 1-2 false depth as walls to ensure algorithm don't go out of bounds.
            bool[,] mapGrid = {
                {true, true, true, false, true, true, true}, 
                {true, true, true, false, true, true, true},
                {true, true, true, false, true, true, true},
                {true, true, true, false, false, true, true},
                {true, true, true, true, true, true, true}
            };
            
            // SearchParameters searchParameters = new SearchParameters;
            // searchParameters.Map = mapGrid;

            // convert grid to nodes with parameters
            // we want to keep refreshing the calculations every iteration

            // get nodes coordinates
            // calculate fgh
            // sort f values, get smallest f values next nodes
            // make sure node is walkable, check boundary, and not closed (previous path)
            // x = column, y = row

            

            // get user input of which starting & end coordinate?
            // example first start at 0,0
            Coords startCoord = new Coords(1, 1);
            Coords endCoord = new Coords(5, 3);
            List<Node> nodes = new List<Node>();
            List<Node> savedPathNodes = new List<Node>();
            List<Node> pivotNodes = new List<Node>();
            Node currentPos = new Node();
            Node pivotNode = new Node();

            
            nodes = NodeCoordinateInit(mapGrid);
            pivotNodes.AddRange(nodes.FindAll(x => x.isPivot));
            Console.WriteLine("Pivot Count: " + pivotNodes.Count);

            //check coord out of bound
            try
            {
                bool _boundtest = mapGrid[startCoord.Y, startCoord.X];
                if (_boundtest == false)
                {
                    Console.WriteLine("COORDINATE SELECTED IS WALL!");
                    System.Environment.Exit(1);
                }
                _boundtest = mapGrid[endCoord.Y, endCoord.X];
                if (_boundtest == false)
                {
                    Console.WriteLine("COORDINATE SELECTED IS WALL!");
                    System.Environment.Exit(1);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("COORDINATE OUT OF BOUND / NULL!");
                Console.WriteLine(e);
            }

            Console.WriteLine("Map grid OK, nodes initialised!");
            if (pivotNodes.Count > 0)
            {
                Console.WriteLine("Using passageway pathfinding");
                Node endNode = new Node();
                endNode = nodes.Find(x => x.Location == endCoord);

                //use smaller F
                foreach (Node n in pivotNodes)
                {
                    //pythagoras
                    n.G = 10;
                    n.H = Math.Sqrt(Math.Pow(endNode.Location.X - n.Location.X, 2) + Math.Pow(endNode.Location.Y - n.Location.Y, 2));
                    //F set in node class
                }

                pivotNodes.Sort((p, q) => p.F.CompareTo(q.F));
                Console.WriteLine("Pivot Coordinate: " + pivotNodes[0].Location.X + "X, " + pivotNodes[0].Location.Y + "Y.");
                savedPathNodes.AddRange(PathSearch(nodes, startCoord, pivotNodes[0].Location));
                savedPathNodes.AddRange(PathSearch(nodes, pivotNodes[0].Location, endCoord));
                
            }
            else
            {
                savedPathNodes.AddRange(PathSearch(nodes, startCoord, endCoord));
            }
            

            System.Environment.Exit(1);

        } 

        private static List<Node> PathSearch(List<Node> nodes, Coords startCoord, Coords endCoord)
        {
            //SEARCH START
            // get start and end node information?
            List<Node> savedNodes = new List<Node>();
            List<Node> startNodeL = nodes.FindAll(x => x.Location == startCoord);
            //test
            //Console.WriteLine(startNodeL.Count);

            Node startNode = startNodeL[0];

            List<Node> endNodeL = nodes.FindAll(x => x.Location == endCoord);
            //Console.WriteLine(endNodeL.Count);
            Node endNode = endNodeL[0];

            // Open List of nodes to be considered
            List<Node> openNodes = new List<Node>();
            List<Node> closedNodes = new List<Node>();
            List<Node> walkableNodes = new List<Node>();

            Node currentNode = new Node();

            bool isArrived = false;
            int stepCount = 0;


            Console.WriteLine("Current Position Init!");
            currentNode = startNode;
            // Add starting node to open list.
            openNodes.Add(currentNode);

            // Loop until we get to the target location
            while(!isArrived)
            {
                Console.WriteLine("Current Position: " + currentNode.Location.X + 
                                    "X, " + currentNode.Location.Y + "Y.");
                
                // Counting steps
                
                Console.WriteLine("Step Count: " + stepCount);
                stepCount += 1;


                // Add walkable nodes adjacent to starting point to open list.
                // get walkable nodes & open nodes
                walkableNodes = GetAdjacentWalkableNodes(currentNode, nodes, closedNodes);
                openNodes.AddRange(walkableNodes);
                // Console.WriteLine("openNodes add walkable nodes count: "+openNodes.Count);
                
                // add initial start node as parent node to openNodes.
                foreach (Node n in openNodes.ToList())
                {
                    

                    if (n.Location != currentNode.Location)
                    {
                        n.ParentNode = currentNode;
                    }

                    if (n.Location == currentNode.Location)
                    {
                        // remove start node from open Nodes, add the node into closedNodes.
                        //closedNodes.Add(n);
                        openNodes.Remove(n);
                    }

                    // else if ((currentNode.ParentNode != null) && (n.Location == currentNode.ParentNode.Location))
                    // {
                    //     // remove start node from open Nodes, add the node into closedNodes.
                    //     closedNodes.Add(n);
                    //     openNodes.Remove(n);
                    // }

                    
                    // set open state
                    n.State = NodeState.Open;
                }
                closedNodes.Add(currentNode);

                // close state in closeNodes
                foreach (Node n in closedNodes)
                {
                    n.State = NodeState.Closed;
                }
                

                // calculate F G H now we know starting & end coordinate.
                // Console.WriteLine("openNodes count: "+openNodes.Count);
                // Console.WriteLine("closedNodes count: "+closedNodes.Count);
                openNodes = FGHCalculate(openNodes, currentNode, endNode);
                // Console.WriteLine("openNodes count after calc: "+openNodes.Count);
                // if no more open nodes, then no path
                if (openNodes.Count == 0)
                {
                    Console.WriteLine("No more open path!");
                    break;
                }

                // find lowest F then assign that as current Node.
                openNodes.Sort((p, q) => p.F.CompareTo(q.F));
                // Console.WriteLine("F G H first 3 nodes: ");
                Console.WriteLine("F: " + openNodes[0].F + " G: " + openNodes[0].G + " H: " + openNodes[0].H);
                // Console.WriteLine("F: " + openNodes[1].F + " G: " + openNodes[1].G + " H: " + openNodes[1].H);
                // Console.WriteLine("F: " + openNodes[2].F + " G: " + openNodes[2].G + " H: " + openNodes[2].H);

                currentNode = openNodes[0];
                savedNodes.Add(currentNode);

                //Initialize openNodes after each step
                openNodes.Clear();
                walkableNodes.Clear();

                // check if node has arrived to location
                if (currentNode.Location == endNode.Location)
                {
                    isArrived = true;
                    Console.WriteLine("Character has arrived at end location @" + currentNode.Location.X + "X, " + currentNode.Location.Y + "Y!");
                    break;
                }
                else
                {
                    Console.WriteLine("Character moved!");
                }

                Thread.Sleep(250);
            }
            return savedNodes;
        }

        //static method for getting nodes coordinate & walkable status of each nodes.
        //from top to bottom, left to right
        private static List<Node> NodeCoordinateInit(bool[,] mapArray)
        {
            Console.WriteLine("Node Grid Conversion started...");
            Console.WriteLine("Map Array:");
            for (int row = 0; row<mapArray.GetLength(0); row++)
            {
                for (int col = 0; col<mapArray.GetLength(1); col++)
                {
                    Console.Write(mapArray[row,col] + "\t");
                }
                Console.WriteLine("");
                
            }
            Console.WriteLine("Map Height: " + mapArray.GetLength(0));
            Console.WriteLine("Map Length: " + mapArray.GetLength(1));

            int mapHeight = mapArray.GetLength(0);
            int mapLength = mapArray.GetLength(1);
            //List<Node> nodes = new List<Node>(new Node[mapHeight*mapLength]);
            List<Node> nodes = new List<Node>(mapHeight*mapLength);
            //populate nodes
            for (int i = 0; i<(mapHeight*mapLength); i++) {nodes.Add(new Node());}

            Console.WriteLine("Total Grid Elements: " + nodes.Count);
            // reverse y axis vs array pos
            // use coords struct, init
            Coords _mapLoc = new Coords(0, 0);
            int r = mapHeight-1;
            int trueRow = 0;
            int c = 0;
            int rpivotTrueCount = 0;
            int cpivotTrueCount = 0;

            // get all node data, fill loop
            // fill y coordinate
            // fill walkable status (true / false)
            int nodeIndex = 0;
            foreach (Node n in nodes)
            {
                // if row is lesser than map height, increment r. else, incremet c, reset r
                // so we list nodes from top to bottom, left to right
                if (r >= 0 && c < mapLength)
                {
                    // logs coordinate. for y axis, reverse array from mapHeight to 0, coordinate from 0 to mapHeight.
                    _mapLoc.X = c;
                    _mapLoc.Y = r;
                    // fill walkable status
                    n.isWalkable = mapArray[trueRow,c];
                    r -= 1;
                    trueRow += 1;

                    if (n.isWalkable)
                    {
                        rpivotTrueCount += 1;
                    }
                    //if whole row only has walkable path less than 2
                    if (r == -1 && rpivotTrueCount <= 2)
                    {
                        n.isPivot = true;
                    }

                    
                    
                }
                else if(c < mapLength)
                {
                    //init
                    r = mapHeight-1;
                    
                    rpivotTrueCount = 0;
                    trueRow = 0;
                    c += 1;

                    _mapLoc.X = c;
                    _mapLoc.Y = r;
                    
                    n.isWalkable = mapArray[trueRow,c];
                    if (n.isWalkable)
                    {
                        cpivotTrueCount += 1;
                    }
                    //if whole row only has walkable path less than 2
                    if (c == mapLength && cpivotTrueCount <= 2)
                    {
                        n.isPivot = true;
                    }

                    r -= 1;
                    trueRow += 1;
                    
                }
                else
                {
                    //init, after all nodes are cycled.
                    r = mapHeight-1;
                    trueRow = 0;
                    c = 0;
                    Console.WriteLine("Counter init");

                }
                //assign coord of the node, then go to next node.
                n.Location = _mapLoc;
                
                nodeIndex += 1;
                
                // Console.WriteLine("Node number: " + nodeIndex);
                // Console.WriteLine("X: " + n.Location.X + " & Y: " + n.Location.Y);
                // Console.WriteLine("Walkable? :" + n.isWalkable);
                
            }
            Console.WriteLine("Nodes finished");
            
            return nodes;
        }

        private static List<Node> FGHCalculate(List<Node> adjacentNodes, Node fromNode, Node toNode)
        {
            //set adjacent terrain cost
            double gOrtho = 1.00;
            double gDiag = 1.40;
            

            List<Node> _orthoNodes = new List<Node>();
            List<Node> _diagonalNodes = new List<Node>();
            List<Node> outputNodes = new List<Node>();

            //path cost to next square
            _orthoNodes = adjacentNodes.FindAll(
            x => ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y)) 
            || ((x.Location.X == fromNode.Location.X-1) && (x.Location.Y == fromNode.Location.Y))
            || ((x.Location.X == fromNode.Location.X) && (x.Location.Y == fromNode.Location.Y+1))
            || ((x.Location.X == fromNode.Location.X) && (x.Location.Y == fromNode.Location.Y-1))
            );

            _diagonalNodes = adjacentNodes.FindAll(
            x => ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y+1)) 
            || ((x.Location.X == fromNode.Location.X-1) && (x.Location.Y == fromNode.Location.Y-1))
            || ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y-1))
            || ((x.Location.X == fromNode.Location.X-1) && (x.Location.Y == fromNode.Location.Y+1))
            );

            //set G in the lists - basic adjacent terrain cost

            foreach (Node n in _orthoNodes)
            {
                n.G = gOrtho;
            }

            foreach (Node n in _diagonalNodes)
            {
                n.G = gDiag;
            }

            //Combine List
            outputNodes.AddRange(_diagonalNodes);
            outputNodes.AddRange(_orthoNodes);

            //set H in the lists - terrain cost to final destination
            //Manhattan method = straight direct path, ignoring walls
            foreach (Node n in outputNodes)
            {
                //pythagoras
                n.H = Math.Sqrt(Math.Pow(toNode.Location.X - n.Location.X, 2) + Math.Pow(toNode.Location.Y - n.Location.Y, 2));
                //F set in node class
            }

            return outputNodes;            
        }

        private static List<Node> GetAdjacentWalkableNodes(Node fromNode, List<Node> nodeL, List<Node> nodeClosedL)
        {
            //get adjacent nodes with true in isWalkables
            
            List<Node> adjacentNodes = new List<Node>();
            List<Node> walkableNodes = new List<Node>();

            adjacentNodes = nodeL.FindAll(
                x => ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y)) 
                || ((x.Location.X == fromNode.Location.X-1) && (x.Location.Y == fromNode.Location.Y))
                || ((x.Location.X == fromNode.Location.X) && (x.Location.Y == fromNode.Location.Y+1))
                || ((x.Location.X == fromNode.Location.X) && (x.Location.Y == fromNode.Location.Y-1))
                || ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y+1)) 
                || ((x.Location.X == fromNode.Location.X-1) && (x.Location.Y == fromNode.Location.Y-1))
                || ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y-1))
                || ((x.Location.X == fromNode.Location.X-1) && (x.Location.Y == fromNode.Location.Y+1))
                );

            // if node is closed, then it is not walkable
            foreach (Node n in adjacentNodes.ToList())
            {
                foreach (Node r in nodeClosedL)
                {
                    if (n.Location == r.Location)
                    {
                        adjacentNodes.Remove(n);
                    }
                }
            }

            walkableNodes.AddRange(adjacentNodes.FindAll(x => x.isWalkable == true));
            
            return walkableNodes;
        }
    }

    

    public struct Coords
    {
        public int X;
        public int Y;
        public Coords(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        //OPERATOR OVERLOAD FOR COORDS STRUCT
        public static bool operator ==(Coords c1, Coords c2)
        {
            if ((c1.X == c2.X) && (c1.Y == c2.Y))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool operator !=(Coords c1, Coords c2)
        {
            if ((c1.X != c2.X) || (c1.Y != c2.Y))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public override bool Equals(object obj)
        {
            if((obj==null)||obj.GetType()!=typeof(Coords))
            {
                return false;
            }
            return this==(Coords)obj;
        }

        public override int GetHashCode()
        {
            int hashcode = 0;
            return hashcode;
        }
    }

    public class Node
    {
        public Coords Location { get; set;}
        public bool isWalkable {get; set;}
        //G = lenght of path from start node to current node
        public double G {get; set;}
        //H = straight line from current node to end
        public double H {get; set;}
        //F = estimated total distance / terrain cost
        public double F {get {return this.G + this.H;}}
        public NodeState State {get; set;}
        public Node? ParentNode {get; set;}
        public bool isPivot {get; set;}
        
    }

    

    public enum NodeState {Untested, Open, Closed}

}



