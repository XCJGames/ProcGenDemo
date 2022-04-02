using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using QuikGraph;

public class GraphTesting : MonoBehaviour
{
    private void Start()
    {
        // grafo dirigido con lista de adyacencia con vértices enteros
        var graph = new AdjacencyGraph<int, Edge<int>>(); 
        // lista de aristas entre aristas
        var edges = new List<Edge<int>>() { 
            new Edge<int>(0, 1), 
            new Edge<int>(1, 2), 
            new Edge<int>(2, 3), 
            new Edge<int>(3, 0) 
        };
        graph.AddVerticesAndEdgeRange(edges);
        foreach(var vertex in graph.Vertices)
        {
            Debug.Log(vertex);
            foreach(Edge<int> edge in graph.OutEdges(vertex))
            {
                Debug.Log(" - " + edge);
            }
        }
    }
}
