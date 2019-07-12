using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SphereMappingAgent : Agent
{
    public Transform Environment;

    public bool UseMeshGenerationVisuals = false;
    public bool UseUserInput = false;

    public float Radius = 1f;

    public int MaxPoints = 100;

    [Range(0.01f, 1f)]
    public float MarginOfError = 0.05f;

    private Mesh workingMesh;
    private MeshFilter meshFilter;

    private int pointsSet = 0;

    private Vector3 previousPoint;

    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();

    private void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        workingMesh = new Mesh();
        meshFilter.mesh = workingMesh;

        SetPreviousPoint(Environment.position + (Vector3.one * Radius));
    }

    private void SetPreviousPoint(Vector3 prevPoint)
    {
        previousPoint = prevPoint;
        pointsSet++;
        if (vertices.Contains(previousPoint))
        {
            triangles.Add(vertices.IndexOf(previousPoint)); // if it's not working add -1.
        }
        else
        {
            vertices.Add(previousPoint);
            triangles.Add(vertices.Count - 1);
        }
    } 

    public override void AgentReset()
    {
        workingMesh.Clear();
        triangles = new List<int>();
        vertices = new List<Vector3>();
        pointsSet = 0;
        if (!UseUserInput)
            Radius = Random.value * 5f + 2f;
        SetPreviousPoint(Environment.position + (Vector3.one * Radius));
    }

    public override void CollectObservations()
    {
        AddVectorObs(Radius);
        AddVectorObs(MaxPoints);
        AddVectorObs(pointsSet);
        AddVectorObs(previousPoint);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //Action, size = 3
        Debug.Log((int)(vectorAction[0]*100));
        Vector3 actionVector = new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]);
        Vector3 nextPoint =  actionVector + previousPoint;

        int actionMove = pointsSet % 3;

        float distanceToNearestPoint = DistanceToNearestPoint(nextPoint);
        float distanceToNearestOtherPoint = DistanceToNearestPoint(nextPoint,previousPoint);

        switch (actionMove) //0: First of Tri | 1: Middle of Tri | 2: Last of Tri
        {
            case 0:
                if (vertices.Contains(nextPoint))
                {
                    SetReward(100 / MaxPoints);
                    SetReward(-(100 / (MaxPoints*10)) * VectorUsed(nextPoint));
                }
                else
                    SetReward(- distanceToNearestPoint);
                break;
            case 1:
                SetReward(distanceToNearestPoint == actionVector.magnitude ? (100 / MaxPoints) : distanceToNearestPoint < actionVector.magnitude ? (distanceToNearestPoint - actionVector.magnitude) : -(distanceToNearestPoint - actionVector.magnitude));
                break;
            case 2:
                SetReward(distanceToNearestOtherPoint == actionVector.magnitude ? (100 / MaxPoints) : distanceToNearestOtherPoint < actionVector.magnitude ? (distanceToNearestOtherPoint - actionVector.magnitude) : -(distanceToNearestOtherPoint - actionVector.magnitude));
                break;
            default:
                break;
        }

        SetPreviousPoint(nextPoint);
        if (actionMove >= 2 && UseMeshGenerationVisuals)
            SetWorkingMesh();

        if (MaxPoints <= pointsSet && triangles.Count % 3 == 0)
        {
            SetWorkingMesh();
            Done();
        }
        else if ((nextPoint - Environment.position).magnitude > Radius + MarginOfError || (nextPoint - Environment.position).magnitude < Radius - MarginOfError)
        {
            SetReward(-1f);
        }
    }

    private void SetWorkingMesh()
    {
        workingMesh.Clear();
        workingMesh.vertices = vertices.ToArray();
        workingMesh.triangles = triangles.ToArray();
        workingMesh.RecalculateNormals();
    }

    private float DistanceToNearestPoint(Vector3 vec)
    {
        float distance = 0f;
        foreach (var vector in vertices)
        {
            distance = Mathf.Max(distance, (vector - vec).magnitude);
        }
        return distance;
    }
    private float DistanceToNearestPoint(Vector3 vec, Vector3 ignoreVec)
    {
        float distance = 0f;
        foreach (var vector in vertices)
        {
            if(!vector.Equals(ignoreVec))
                distance = Mathf.Max(distance, (vector - vec).magnitude);
        }
        return distance;
    }

    private int VectorUsed(Vector3 vec)
    {
        if (!vertices.Contains(vec))
            return 0;
        int amount = 0;
        int vertIndex = vertices.IndexOf(vec);
        
        foreach (var triPart in triangles)
        {
            amount = triPart == vertIndex ? amount + 1 : amount;
        }
        return amount;
    }
}
