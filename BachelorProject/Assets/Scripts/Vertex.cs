using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Vertex
{
    /// <summary>Position of the Vertex in worldspace</summary>
    public Vector3 Position;
    /// <summary>All vertices this Vertex is connected to</summary>
    public List<Vector3> Connectors; // in Liste von vertex umändern

    public Vertex(Vector3 position)
    {
        Position = position;
        Connectors = new List<Vector3>();
    }
    public Vertex(Vector3 position, Vector3 connector)
    {
        Position = position;
        Connectors = new List<Vector3>();
        Connectors.Add(connector);
    }
    public Vertex(Vector3 position, List<Vector3> connectors)
    {
        Position = position;
        Connectors = connectors;
    }
}
