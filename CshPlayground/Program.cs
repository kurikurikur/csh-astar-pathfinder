using System;
using System.Collections;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

namespace pathfinder
{
    class Pathfinder
    {        
        public static void Main(string[] args)
        {
            //init map
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
            Coords startCoord = new Coords(0, 0);
            Coords endCoord = new Coords(3, 6);
            List<Node> nodes = new List<Node>();
            nodes = NodeCoordinateInit(mapGrid);

            //check coord out of bound
            try
            {
                bool _boundtest = mapGrid[startCoord.X, startCoord.Y];
                _boundtest = mapGrid[endCoord.X, endCoord.Y];
            }
            catch (Exception e)
            {
                Console.WriteLine("COORDINATE OUT OF BOUND / NULL!");
                Console.WriteLine(e);
            }

            PathSearch(nodes, startCoord, endCoord);

        } 

        private static void PathSearch(List<Node> nodes, Coords startCoord, Coords endCoord)
        {
            //SEARCH START
            // get start and end node information?
            List<Node> startNodeL = nodes.FindAll(x => x.Location == startCoord);
            Node startNode = startNodeL[0];

            List<Node> endNodeL = nodes.FindAll(x => x.Location == endCoord);
            Node endNode = endNodeL[0];

            Node currentNode = new Node();

            //Initial
            currentNode = startNode;

            // Open List of nodes to be considered
            List<Node> openNodes = new List<Node>();
            List<Node> closedNodes = new List<Node>();
            List<Node> walkableNodes = new List<Node>();

            // Add starting node to open list.
            openNodes.Add(currentNode);

            // Add walkable nodes adjacent to starting point to open list.
            // get walkable nodes
            walkableNodes = GetAdjacentWalkableNodes(currentNode, nodes);
            openNodes.AddRange(walkableNodes);
            

            // add initial start node as parent node to openNodes.
            foreach (Node n in openNodes)
            {
                // set open state
                n.State = NodeState.Open;

                if (n.Location != currentNode.Location)
                {
                    n.ParentNode = currentNode;
                }
            }

            // remove start node from open Nodes, add the node into closedNodes.
            closedNodes.Add(openNodes[0]);
            openNodes.RemoveAt(0);

            // calculate F G H now we know starting & end coordinate.
            openNodes = FGHCalculate(openNodes, currentNode, endNode);

            // find lowest F then assign that as current Node.
            // 
        }

        //static method for getting nodes coordinate & walkable status of each nodes.
        //from top to bottom, left to right
        private static List<Node> NodeCoordinateInit(bool[,] mapArray)
        {
            int mapHeight = mapArray.GetLength(0);
            int mapLength = mapArray.GetLength(1);
            List<Node> nodes = new List<Node>(mapHeight*mapLength);

            // reverse y axis vs array pos
            // use coords struct, init
            Coords _mapLoc = new Coords(0, 0);
            int r = mapHeight-1;
            int c = 0;

            // get all node data, fill loop
            // fill y coordinate
            // fill walkable status (true / false)
            foreach (Node n in nodes)
            {
                // if row is lesser than map height, increment r. else, incremet c, reset r
                // so we list nodes from top to bottom, left to right
                if (r >= 0 && c < mapLength)
                {
                    // logs coordinate. for y axis, reverse array from mapHeight to 0, coordinate from 0 to mapHeight.
                    _mapLoc.Y = r;
                    // fill walkable status
                    n.isWalkable = mapArray[r,c];
                    r -= 1;
                    
                }
                else if(c < mapLength)
                {
                    _mapLoc.X = c;
                    n.isWalkable = mapArray[r,c];
                    c += 1;
                    

                    //init
                    r = mapHeight-1;
                }
                else
                {
                    //init, after all nodes are cycled.
                    r = mapHeight-1;
                    c = 0;
                }
                //assign coord of the node, then go to next node.
                n.Location = _mapLoc;
            }
            return nodes;
        }

        private static List<Node> FGHCalculate(List<Node> adjacentNodes, Node fromNode, Node toNode)
        {
            //set adjacent terrain cost
            double gOrtho = 10.0;
            double gDiag = 14.0;
            

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
            || ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y+1))
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

        private static List<Node> GetAdjacentWalkableNodes(Node fromNode, List<Node> nodeL)
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
                || ((x.Location.X == fromNode.Location.X+1) && (x.Location.Y == fromNode.Location.Y+1))
                );

            walkableNodes = nodeL.FindAll(x => x.isWalkable == true);
            
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

    

    public class SearchParameters
    {
        public Coords StartLocation { get; set;}
        public Coords EndLoc { get; set;}
        public bool[,] Map { get; set;}

        

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
        
    }

    

    public enum NodeState {Untested, Open, Closed}

}



