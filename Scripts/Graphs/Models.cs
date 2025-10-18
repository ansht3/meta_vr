using System;
using System.Collections.Generic;

namespace BlossomBuddy.Graphs
{
    /// <summary>
    /// Root object for the graph JSON structure
    /// </summary>
    [Serializable]
    public class GraphData
    {
        public AdjacencyList adjList;
    }

    /// <summary>
    /// Adjacency list containing nodes and edges
    /// </summary>
    [Serializable]
    public class AdjacencyList
    {
        public string subject;
        public List<Node> nodes;
        public List<List<string>> edges;
    }

    /// <summary>
    /// Node representing a concept in the graph
    /// </summary>
    [Serializable]
    public class Node
    {
        public string id;
        public string topic;
        public string small_content;
        public List<string> resources;
        public string overall_summary;
    }
}