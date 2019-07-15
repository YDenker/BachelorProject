using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vertex
{
    /// <summary>Position of the Vertex in worldspace</summary>
    public Vector3 Position;
    /// <summary>Index of the vertices list this is in
    /// </summary>
    public int Index;
    /// <summary>All vertices this Vertex is connected to</summary>
    public List<Vertex> Connectors; // in Liste von vertex umändern

    public Vertex(Vector3 position)
    {
        Position = position;
        Connectors = new List<Vertex>();
        Index = -1;
    }
    public Vertex(Vector3 position, int index)
    {
        Position = position;
        Connectors = new List<Vertex>();
        Index = index;
    }
    public Vertex(Vector3 position, Vertex connector)
    {
        Position = position;
        Connectors = new List<Vertex>();
        Connectors.Add(connector);
        Index = -1;
    }
    public Vertex(Vector3 position, List<Vertex> connectors)
    {
        Position = position;
        Connectors = connectors;
        Index = -1;
    }
}
