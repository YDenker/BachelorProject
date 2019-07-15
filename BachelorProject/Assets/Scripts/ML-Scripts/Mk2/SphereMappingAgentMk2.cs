using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMappingAgentMk2 : Agent
{
    [SerializeField]
    private FakePlayerInput sphereParams;
    [SerializeField]
    private GameObject debug;
    private List<GameObject> debugs = new List<GameObject>();
    private float radius;
    private int attemptedVerticesPerFace;
    private int approximateVerticesTotal;
    private float marginOfError;

    private List<Vertex> vertices = new List<Vertex>();

    private int allEdges = 0;

    private bool shapeFinished = false;

    private int currentShapeEdges = 0;
    private float currentEdgeLength = 0;

    private Vertex previousVert;

    private Vertex shapeStarter;

    private Vector3 environmenCenter = Vector3.zero;

    private void Start()
    {
        environmenCenter = transform.parent.position;
        ResetAllToStartValues();
    }

    private void ResetAllToStartValues()
    {
        radius = sphereParams.Radius;
        attemptedVerticesPerFace = sphereParams.AttemptedVerticesPerFace;
        approximateVerticesTotal = sphereParams.ApproximateVerticdesTotal;
        marginOfError = sphereParams.MarginOfError;

        vertices = new List<Vertex>();

        previousVert = new Vertex(environmenCenter + Vector3.one.normalized * radius,0);
        shapeStarter = previousVert;

        allEdges = 0;
        shapeFinished = false;
        currentShapeEdges = 0;
        currentEdgeLength = 0;

        vertices.Add(previousVert);

        GLDrawer.ClearLines();

        foreach (var debugger in debugs)
        {
            GameObject.Destroy(debugger);
        }
        debugs = new List<GameObject>
        {
            GameObject.Instantiate(debug, previousVert.Position, Quaternion.identity)
        };
    }

    public override void AgentReset()
    {
        ResetAllToStartValues();
    }

    public override void CollectObservations()
    {
        // 14 current Observations
        AddVectorObs(radius);
        AddVectorObs(attemptedVerticesPerFace);
        AddVectorObs(approximateVerticesTotal);
        AddVectorObs(marginOfError);
        AddVectorObs(allEdges);
        AddVectorObs(shapeFinished);
        AddVectorObs(currentShapeEdges);
        AddVectorObs(currentEdgeLength);
        AddVectorObs(previousVert.Position - environmenCenter);
        AddVectorObs(shapeStarter.Position - environmenCenter);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Action-size = 4
        Vertex nextVertex = new Vertex(new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]),previousVert); // The Agent chooses the next Vertex

        // Incrementing the edge counts
        currentShapeEdges++;
        allEdges++;

        bool finishShape = vectorAction[3] <= 0 ? true : false; // The Agent should choose when to close the shape

        if (finishShape && !shapeFinished)
        {
            nextVertex = NearestVertex(nextVertex.Position.normalized * radius + environmenCenter);
            shapeFinished = true;

            //GLDrawer.AddLine(new GLDrawer.Line(nextVertex.Position, previousVert.Position));

            //Rewards
            int shapeEdges = ShortestPathToShapeStart(nextVertex, previousVert,0) + currentShapeEdges;
            int shapeOff = Mathf.Abs(attemptedVerticesPerFace - shapeEdges);

            if (shapeEdges < currentShapeEdges)
                SetReward(-1); // Negative Reward for a too large shape size
            else
            {
                if (shapeEdges <= 2)
                {
                    // Negative Reward for a too small shape size and reset
                    SetReward(-10);
                    Done();
                }
                else // Positive Reward for approximate shape size
                    SetReward(1 / (shapeOff + 1));
            }
            SetLineLenghtReward((previousVert.Position - nextVertex.Position).magnitude);

            //Next Shape Setup
            previousVert = SelectOptimalNewShapeVertex();
            shapeStarter = previousVert;
            currentShapeEdges = 0;
            currentEdgeLength = 0;
        }
        else if(shapeFinished)
        {
            shapeFinished = false;
            AddVertex(nextVertex);
        }
        else
        {
            AddVertex(nextVertex);
            SetLineLenghtReward((previousVert.Position - nextVertex.Position).magnitude);
        }

        // Calculating the new edge length
        currentEdgeLength = (previousVert.Position - nextVertex.Position).magnitude;

        if (approximateVerticesTotal < vertices.Count + (vertices.Count * marginOfError) || CheckVertexConnectionsPowerOfThree())
            IterationFinished();
    }

    private void SetLineLenghtReward(float length)
    {
        float offset = Mathf.Abs(currentEdgeLength - length);

        SetReward(1 / (offset + 1));
    }

    private void AddVertex(Vertex addition) // Also sets th previous Vert correctly and draws a Line
    {
        addition.Position = addition.Position.normalized * radius + environmenCenter;
        addition.Index = vertices.Count;
        vertices.Add(addition);
        vertices[previousVert.Index].Connectors.Add(addition);

        //GLDrawer.AddLine(new GLDrawer.Line(addition.Position, previousVert.Position));
        debugs.Add(GameObject.Instantiate(debug, addition.Position, Quaternion.identity));

        previousVert = vertices[previousVert.Index];
    } 

    private void IterationFinished()
    {
        Done();
    }

    private Vertex SelectOptimalNewShapeVertex()
    {
        foreach (var vert in vertices)
        {
            if (vert.Connectors.Count % 3 != 0 || vert.Connectors.Count == 0)
                return vert;
        }
        return new Vertex();
    } // Select the next best Vertex to start a shape on

    private Vertex NearestVertex(Vector3 vec)
    {
        Vertex tempVertex = new Vertex();
        float distance = 0f;
        foreach (var vector in vertices)
        {
            tempVertex = distance < (vector.Position - vec).magnitude ? vector : tempVertex;
            distance = Mathf.Max(distance, (vector.Position - vec).magnitude);
        }
        return tempVertex;
    }

    private int ShortestPathToShapeStart(Vertex current, Vertex previous, int iteration) // recursive function
    {
        int i = 0;
        foreach (var vert in current.Connectors)
        {
            if (vert.Equals(shapeStarter))
                return iteration;
            if (vert.Equals(previous) || iteration > attemptedVerticesPerFace + attemptedVerticesPerFace * marginOfError)
                return -1;
            int s = ShortestPathToShapeStart(vert, current, ++iteration);
            i = s >= 0 ? Mathf.Max(s,i) : -1;
        }
        return i;
    }

    private float DistanceToNearestPoint(Vector3 vec, Vertex ignoreVex)
    {
        float distance = 0f;
        foreach (var vector in vertices)
        {
            if (!vector.Equals(ignoreVex))
                distance = Mathf.Max(distance, (vector.Position - vec).magnitude);
        }
        return distance;
    }

    private bool CheckVertexConnectionsPowerOfThree() // Gives Positive Rewards if true
    {
        bool allPO3 = true;
        foreach (var vert in vertices)
        {
            if (vert.Connectors.Count % 3 != 0 || vert.Connectors.Count == 0)
                return false;
        }
        SetReward(100);
        return allPO3;
    }
}
