using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures;
using Assets.Scripts.IAJ.Unity.Pathfinding.DataStructures.GoalBounding;
using RAIN.Navigation.Graph;
using RAIN.Navigation.NavMesh;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Assets.Scripts.IAJ.Unity.Pathfinding.GoalBounding
{
    //The Dijkstra algorithm is similar to the A* but with a couple of differences
    //1) no heuristic function
    //2) it will not stop until the open list is empty
    //3) we dont need to execute the algorithm in multiple steps (because it will be executed offline)
    //4) we don't need to return any path (partial or complete)
    //5) we don't need to do anything when a node is already in closed
    public class GoalBoundsDijkstraMapFlooding
    {
        public NavMeshPathGraph NavMeshGraph { get; protected set; }
        public NavigationGraphNode StartNode { get; protected set; }
        public NodeGoalBounds NodeGoalBounds { get; protected set; }
        protected NodeRecordArray NodeRecordArray { get; set; }

        public IOpenSet Open { get; protected set; }
        public IClosedSet Closed { get; protected set; }
        
        public GoalBoundsDijkstraMapFlooding(NavMeshPathGraph graph)
        {
            this.NavMeshGraph = graph;
            //do not change this
            var nodes = this.GetNodesHack(graph);
            this.NodeRecordArray = new NodeRecordArray(nodes);
            this.Open = this.NodeRecordArray;
            this.Closed = this.NodeRecordArray;
            this.Open.Initialize();
        }

        public void Search(NavigationGraphNode startNode, NodeGoalBounds nodeGoalBounds)
        {
            NodeRecord startNodeRecord = this.NodeRecordArray.GetNodeRecord(startNode);
            startNodeRecord.node = startNode;
            startNodeRecord.gValue = 0f;
            this.Open.AddToOpen(startNodeRecord);

            while (this.Open.CountOpen() > 0)
            {
                NodeRecord Node = this.Open.GetBestAndRemove();
                this.Closed.AddToClosed(Node);
                if (Node.id != -1 && nodeGoalBounds.connectionBounds.Length > Node.id) nodeGoalBounds.connectionBounds[Node.id].UpdateBounds(Node.node.LocalPosition);

                for (int i = 0; i < Node.node.OutEdgeCount; i++)
                {
                    ProcessChildNode(Node, Node.node.EdgeOut(i), i);
                }
            }

            this.Open.Initialize();
        }

        protected void ProcessChildNode(NodeRecord BestNode, NavigationGraphEdge connectionEdge, int connectionIndex)
        {
            NavigationGraphNode childNode = connectionEdge.ToNode;
            NodeRecord childRecord = this.NodeRecordArray.GetNodeRecord(connectionEdge.ToNode);

            if (childRecord == null)
            {
                childRecord = new NodeRecord
                {
                    node = childNode,
                    parent = BestNode,
                    status = NodeStatus.Unvisited
                };
                this.NodeRecordArray.AddSpecialCaseNode(childRecord);
            }

            if (childRecord.status == NodeStatus.Closed) return;

            float g = BestNode.gValue + (childRecord.node.LocalPosition - BestNode.node.LocalPosition).magnitude;

            if (childRecord.status == NodeStatus.Unvisited)
            {
                UpdateNodeRecord(childRecord, BestNode, g, connectionIndex);
                this.Open.AddToOpen(childRecord);
            }
            else if (childRecord.gValue > g)
            {
                UpdateNodeRecord(childRecord, BestNode, g, connectionIndex);
                this.Open.Replace(childRecord, childRecord);
            }
        }

        protected void UpdateNodeRecord(NodeRecord node, NodeRecord parent, float g, int connectionIndex)
        {                
            node.gValue = g;
            node.fValue = g;
            node.parent = parent;
            node.id = connectionIndex;
        }

        private List<NavigationGraphNode> GetNodesHack(NavMeshPathGraph graph)
        {
            //this hack is needed because in order to implement NodeArrayA* you need to have full acess to all the nodes in the navigation graph in the beginning of the search
            //unfortunately in RAINNavigationGraph class the field which contains the full List of Nodes is private
            //I cannot change the field to public, however there is a trick in C#. If you know the name of the field, you can access it using reflection (even if it is private)
            //using reflection is not very efficient, but it is ok because this is only called once in the creation of the class
            //by the way, NavMeshPathGraph is a derived class from RAINNavigationGraph class and the _pathNodes field is defined in the base class,
            //that's why we're using the type of the base class in the reflection call
            return (List<NavigationGraphNode>)Utils.Reflection.GetInstanceField(typeof(RAINNavigationGraph), graph, "_pathNodes");
        }

    }
}
