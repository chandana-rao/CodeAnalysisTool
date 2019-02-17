/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// StrongConnectedComponent.cs - Generate the graph for the dependent files and check for strong connected components     //
//                             - Demostrate the strongly connected components in the dependency graph                    //
// Language:    C#, 2017, .Net Framework 4.6.1                                                                          //
// Application: Project #3, CSE681 Fall 2018                                                                           //
// Author:      Chandana Rao                                                                                          //
//  Source:     Jim Fawcett                                                                                          //
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
/*
 * Package Operations:
 * -------------------
 * Accepts the graph from dependency analysis
 * Generates the nodes and edges for the graph
 * Checks for the strongly connected components in the graph
 * 
 */
/* Required Files:
 *   Executive.cs
 *   DependencyAnalysis.cs
 */
/*Public InterfaceDocumentation:
 *  public class CsEdge<V, E>	//holds child node and instance of edge type E
*	public class CsNode<V, E>
*	public CsNode(string nodeName)
*	public void addChild(CsNode<V, E> childNode, E edgeVal)	//add child vertex and its associated edge value to vertex
*	public CsEdge<V, E> getNextUnmarkedChild()	//find the next unvisited child
*	public bool hasUnmarkedChild()	//has unvisited child?
*	public void unmark()	//assign visited to false
*	public CsGraph(string graphName)	//contains nodes and edges
*	public Operation<V, E> setOperation(Operation<V, E> newOp) //register an Operation with the graph
*	public void addNode(CsNode<V, E> node) //add vertex to graph adjacency list
*	public void clearMarks() //clear visitation marks to prepare for next walk 
*	public void walk() //depth first search from startNode
*	public void walk(CsNode<V, E> node)	//depth first search from specific node
*	public void showDependencies()	//Display the file dependences
*	public class TestGraph
*	public List<CsNode<string, string>> acceptGraph(List<Tuple<string, string>> graph, List<string> files)
*	public List<string> tarjan(List<CsNode<string, string>> Nodes)	//Tarjan's algorithm to find the strongly connected component
 */
/* 
* Maintenance History:
* --------------------
* ver 1.0 : 02 Nov 2018
* - first release
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.IO;

namespace StrongComponent
{
        public class CsEdge<V, E> 
        {
            public CsNode<V, E> targetNode { get; set; } = null;
            public E edgeValue { get; set; }

            public CsEdge(CsNode<V, E> node, E value)
            {
                targetNode = node;
                edgeValue = value;
            }
        };

        public class CsNode<V, E>
        {
            public V nodeValue { get; set; }
            public int index { get; set; }
            public int lowlink { get; set; }
            public string name { get; set; }
            public List<CsEdge<V, E>> children { get; set; }
            public bool visited { get; set; }

            //----< construct a named node >---------------------------------------
            public CsNode(string nodeName)
            {
                name = nodeName;
                index = -1;
                lowlink = 0;
                children = new List<CsEdge<V, E>>();
                visited = false;
            }

            //----< add child vertex and its associated edge value to vertex >-----
            public void addChild(CsNode<V, E> childNode, E edgeVal)
            {
                children.Add(new CsEdge<V, E>(childNode, edgeVal));
            }

            //----< find the next unvisited child >--------------------------------
            public CsEdge<V, E> getNextUnmarkedChild()
            {
                foreach (CsEdge<V, E> child in children)
                {
                    if (!child.targetNode.visited)
                    {
                        child.targetNode.visited = true;
                        return child;
                    }
                }
                return null;
            }
            //----< has unvisited child? >-----------------------------------

            public bool hasUnmarkedChild()
            {
                foreach (CsEdge<V, E> child in children)
                {
                    if (!child.targetNode.visited)
                    {
                        return true;
                    }
                }
                return false;
            }

            public void unmark()
            {
                visited = false;
            }

            public override string ToString()
            {
                return name;
            }
        }
        /////////////////////////////////////////////////////////////////////////
        // Operation<V,E> class

        class Operation<V, E>
        {
            //----< graph.walk() calls this on every node >------------------------

            virtual public bool doNodeOp(CsNode<V, E> node)
            {
                Console.Write("\n  {0}", node.ToString());
                return true;
            }
            //----< graph calls this on every child visitation >-------------------

            virtual public bool doEdgeOp(E edgeVal)
            {
                Console.Write(" {0}", edgeVal.ToString());
                return true;
            }
        }
        /////////////////////////////////////////////////////////////////////////
        // CsGraph<V,E> class

        class CsGraph<V, E>
        {
            public CsNode<V, E> startNode { get; set; }
            public string name { get; set; }
            public bool showBackTrack { get; set; } = false;

            private List<CsNode<V, E>> adjList { get; set; }  // node adjacency list
            private Operation<V, E> gop = null;

            //----< construct a named graph >--------------------------------------

            public CsGraph(string graphName)
            {
                name = graphName;
                adjList = new List<CsNode<V, E>>();
                gop = new Operation<V, E>();
                startNode = null;
            }
            //----< register an Operation with the graph >-------------------------

            public Operation<V, E> setOperation(Operation<V, E> newOp)
            {
                Operation<V, E> temp = gop;
                gop = newOp;
                return temp;
            }
            //----< add vertex to graph adjacency list >---------------------------

            public void addNode(CsNode<V, E> node)
            {
                adjList.Add(node);
            }
            //----< clear visitation marks to prepare for next walk >--------------

            public void clearMarks()
            {
                foreach (CsNode<V, E> node in adjList)
                    node.unmark();
            }
            //----< depth first search from startNode >----------------------------

            public void walk()
            {
                if (adjList.Count == 0)
                {
                    Console.Write("\n  no nodes in graph");
                    return;
                }
                if (startNode == null)
                {
                    Console.Write("\n  no starting node defined");
                    return;
                }
                if (gop == null)
                {
                    Console.Write("\n  no node or edge operation defined");
                    return;
                }
                this.walk(startNode);
                foreach (CsNode<V, E> node in adjList)
                    if (!node.visited)
                        walk(node);
                foreach (CsNode<V, E> node in adjList)
                    node.unmark();
                return;
            }
            //----< depth first search from specific node >------------------------

            public void walk(CsNode<V, E> node)
            {
                gop.doNodeOp(node);
                node.visited = true;
                do
                {
                    CsEdge<V, E> childEdge = node.getNextUnmarkedChild();
                    if (childEdge == null)
                    {
                        return;
                    }
                    else
                    {
                        gop.doEdgeOp(childEdge.edgeValue);
                        walk(childEdge.targetNode);
                        if (node.hasUnmarkedChild() || showBackTrack)
                        {                        
                            gop.doNodeOp(node);    
                        }                         
                    }
                } while (true);
            }

            public void showDependencies()
            {
                List<string> edges = new List<string>();
                Console.Write("\n  Dependency Table:");
                Console.Write("\n -------------------");
                foreach (var node in adjList)
                {
                    Console.Write("\n  {0}", node.name);
                    for (int i = 0; i < node.children.Count; ++i)
                    {
                        Console.Write("\n    {0}", node.children[i].targetNode.name);
                        edges.Add(node.children[i].targetNode.name);
                    }
                }
            }
        }

        public class TestGraph
        {
            //---------------<Accept the graph from dependnecy analysis>---------------------------//
          public List<CsNode<string, string>> acceptGraph(List<Tuple<string, string>> graph, List<string> files)
          {
            List<CsNode<string, string>> nodes = new List<CsNode<string, string>>();
            List<string> scc = new List<string>();
            for(int i = 0;i<files.Count; i++)
            {
                CsNode<string, string> node = new CsNode<string, string>(files[i]);
                node.name = Path.GetFileName(files[i]);
                nodes.Add(node);
            }

            int n = graph.Count; 

            for(int i =0;i<n;i++)
            {
                int j = 0;
                for(j =0;j<nodes.Count;j++)
                {
                    if (nodes[j].name == graph[i].Item1)
                        break;
                }
                for (int k = 0; k < nodes.Count; k++)
                {
                    if (nodes[k].name == graph[i].Item2 && j<nodes.Count)
                        nodes[j].addChild(nodes[k], " ");
                }
            }
            //scc = tarjan(nodes);
            return nodes;

            
        }
        //-------------<Tarjan's algorithm to find the strongly connected component>----------
        public List<string> tarjan(List<CsNode<string, string>> Nodes)
            {
            string s1 =null;
            List<string> scc = new List<string>();
                var index = 0;
                var S = new Stack<CsNode<string, string>>();

                void stronglyConnected(CsNode<string, string> v)
                {
                    v.index = index;
                    v.lowlink = index;
                    index++;
                    S.Push(v);
                //int i = 1;
                //scc.Add(i.ToString());
                    CsNode<string, string> w = null;
                    foreach (var child in v.children)
                    {
                        w = child.targetNode;
                        if (w.index < 0)
                        {
                            stronglyConnected(w);
                            v.lowlink = Math.Min(v.lowlink, w.lowlink);
                        }
                        else if (S.Contains(w))
                            v.lowlink = Math.Min(v.lowlink, w.index);
                    }
                    if (v.lowlink == v.index)
                    {
                        do
                        {
                        w = S.Pop();
                        s1 = s1 + w.name + ", ";
                        } while (w != v);
                    scc.Add(s1);
                    s1 = null;
                    }
                }
                foreach (var v in Nodes)
                    if (v.index < 0)
                        stronglyConnected(v);

            return scc;
        }
        }
    
        class demoOperation : Operation<string, string>
        {
            override public bool doNodeOp(CsNode<string, string> node)
            {
                Console.Write("\n -- {0}", node.name);
                return true;
            }
        }

#if (Test_Stub)
    class Test
    {
       static void Main(string[] args)
        {
            List<string> scc = new List<string>();
            List<CsNode<string, string>> nodes = new List<CsNode<string, string>>();
            Console.Write("\n  Testing Graph class");
            Console.WriteLine("\n =======================");
            TestGraph t = new TestGraph();

            CsNode<string, string> node1 = new CsNode<string, string>("node1");
            CsNode<string, string> node2 = new CsNode<string, string>("node2");
            CsNode<string, string> node3 = new CsNode<string, string>("node3");
            CsNode<string, string> node4 = new CsNode<string, string>("node4");
            CsNode<string, string> node5 = new CsNode<string, string>("node5");

            node1.addChild(node2, "edge12");
            node2.addChild(node1, "edge21");
            node2.addChild(node3, "edge23");
            node2.addChild(node4, "edge24");
            node3.addChild(node1, "edge31");
            node5.addChild(node1, "edge51");
            node5.addChild(node4, "edge54");

            CsGraph<string, string> graph = new CsGraph<string, string>("Fred");
            graph.addNode(node1);
            graph.addNode(node2);
            graph.addNode(node3);
            graph.addNode(node4);
            graph.addNode(node5);
            nodes.Add(node1);
            nodes.Add(node2);
            nodes.Add(node3);
            nodes.Add(node4);
            nodes.Add(node5);

            scc= t.tarjan(nodes);
            Console.WriteLine("Graph with parent and nodes");
            foreach(var node in nodes)
            {
                Console.Write("\n  {0}", node.name);
                for (int i = 0; i < node.children.Count; ++i)
                {
                    Console.Write("\n    {0}", node.children[i].targetNode.name);
                }
            }
            Console.WriteLine("SCCs:\n");
            foreach (var s in scc)
            {
                Console.Write(s);
            }
            Console.Read();
        }
         

    }
#endif
}
