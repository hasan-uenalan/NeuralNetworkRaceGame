﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class CarAgent : Agent
{
    public GameObject wall;

    
    private Vector3 resetPosition;
    private Quaternion resetAngle;
    private GameObject nextCheckpoint;
    
    //Components
    private Rigidbody rBody;
    private Checkpoint checkpoint;
    private NNCarController carController;

    private float dist;
    void Start()
    {
        //Initializing components
        carController = gameObject.GetComponent<NNCarController>();
        checkpoint = gameObject.GetComponent<Checkpoint>();
        rBody = gameObject.GetComponent<Rigidbody>();
         
        nextCheckpoint = checkpoint.nextCheckpoint();

        dist = Vector3.Distance(nextCheckpoint.transform.position, gameObject.transform.position);
    }

    public override void CollectObservations()
    {
        //Checkpoint and Car positions
        AddVectorObs(nextCheckpoint.transform.localPosition);
        AddVectorObs(gameObject.transform.localPosition);

        //Agent velocity
        AddVectorObs(rBody.velocity.x);
        AddVectorObs(rBody.velocity.z);
    }

    //For manual control
    public override float[] Heuristic() 
    {
        var action = new float[2];
        //steering
        action[0] = Input.GetAxis("Horizontal");
        //Accelaration & Brakes
        action[1] = Input.GetAxis("Vertical");
        return action;
    }

    public override void AgentReset()
    {
        gameObject.GetComponent<CollisionWall>().resetCar(); //resets the car from external script

    }

    public override void AgentAction(float[] vectorAction)
    {
        float oldist = Vector3.Distance(nextCheckpoint.transform.position, gameObject.transform.position);
        Vector3 controlSignal = Vector3.zero;
        controlSignal.z = vectorAction[0];
        controlSignal.x = vectorAction[1];
        carController.Steer(controlSignal.z);
        carController.Drive(controlSignal.x);

        //on screen monitoring
        Monitor.Log("Steering", controlSignal.z);
        Monitor.Log("Driving", controlSignal.x);

        float newdist = Vector3.Distance(nextCheckpoint.transform.position, gameObject.transform.position);
        if (newdist < oldist)
        {
            AddReward(.1f);
        }
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject == wall)
        {
            AddReward(-.8f);
            Debug.Log("reward removed");
            Done();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject == nextCheckpoint)
        {
            AddReward(1f);
            Debug.Log("reward added");
            //updating next checkpoint
            nextCheckpoint = checkpoint.nextCheckpoint();
        }
    }

}
