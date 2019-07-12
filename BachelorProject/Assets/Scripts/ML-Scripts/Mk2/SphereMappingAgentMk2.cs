using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMappingAgentMk2 : Agent
{
    [SerializeField]
    private FakePlayerInput sphereParams;

    private float radius;
    private int attemptedVerticesPerFace;
    private int approximateVerticesTotal;
    private float marginOfError;

    private List<Vertex> vertices = new List<Vertex>();

    private int allEdges = 0;

    private bool shapeFinished = false;

    private int currentShapeEdges = 0;
    private int currentEdgeLength = 0;

    private Vector3 previousPoint = Vector3.zero;  // zu Vertex ändern!

    private Vector3 shapeStarter = Vector3.zero; // zu Vertex ändern!

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

        previousPoint = environmenCenter + Vector3.one * radius;

        allEdges = 0;
        shapeFinished = false;
        currentShapeEdges = 0;
        currentEdgeLength = 0;
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
        AddVectorObs(previousPoint-environmenCenter);
        AddVectorObs(shapeStarter - environmenCenter);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //action, size = 4
        Vertex nextVector = new Vertex(new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]),previousPoint);
        bool finishShape = vectorAction[3] >= 0 ? true : false;
        if (finishShape && !shapeFinished)
        {
            nextVector = NearestVertex(nextVector.Position.normalized * radius + environmenCenter);
            shapeFinished = true;
        }
        else if(shapeFinished)
        {
            shapeFinished = false;
            nextVector.Position = nextVector.Position.normalized * radius + environmenCenter;
        }
        else
        {
            nextVector.Position = nextVector.Position.normalized * radius + environmenCenter;
        }


        if(approximateVerticesTotal < vertices.Count + (vertices.Count * marginOfError) || CheckVertexConnectionsPowerOfThree())
            IterationFinished();
    }

    private void IterationFinished()
    {
        Done();
    }

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

    private float DistanceToNearestPoint(Vector3 vec, Vector3 ignoreVec) // ignoreVec zu Vertex ändern!
    {
        float distance = 0f;
        foreach (var vector in vertices)
        {
            if (!vector.Equals(ignoreVec))
                distance = Mathf.Max(distance, (vector.Position - vec).magnitude);
        }
        return distance;
    }

    private bool CheckVertexConnectionsPowerOfThree()
    {
        bool allPO3 = true;
        foreach (var vert in vertices)
        {
            if (vert.Connectors.Count % 3 != 0 || vert.Connectors.Count == 0)
                return false;
        }
        return allPO3;
    }
}
