using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class RollerAgent : Agent
{
    Rigidbody body;
    private void Start()
    {
        body = GetComponent<Rigidbody>();
    }

    public Transform Target;
    public override void AgentReset()
    {
        if(this.transform.position.y < 0f)
        {
            body.angularVelocity = Vector3.zero;
            body.velocity = Vector3.zero;
            transform.position = new Vector3(0f, 0.5f, 0f);
        }

        Target.position = new Vector3(Random.value * 8 - 4, 0.5f, Random.value * 8 - 4);
    }

    public override void CollectObservations()
    {
        AddVectorObs(Target.position);
        AddVectorObs(transform.position);

        AddVectorObs(body.velocity.x);
        AddVectorObs(body.velocity.z);
    }

    public float speed = 10;
    public override void AgentAction(float[] vectorAction, string textAction)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = vectorAction[0];
        controlSignal.z = vectorAction[1];
        body.AddForce(controlSignal * speed);

        // Rewards
        float distanceToTarget = Vector3.Distance(this.transform.position,
                                                  Target.position);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
            Done();
        }

        // Fell off platform
        if (this.transform.position.y < 0)
        {
            Done();
        }

    }
}
