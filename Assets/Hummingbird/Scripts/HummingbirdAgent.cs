////////////////////////////////////////////////////////////
// File: HummingbirdAgent.cs
// Author: Jack Peedle
// Date Created: 18/03/22
// Last Edited By: Jack Peedle
// Date Last Edited: 21/03/22
// Brief: 
//////////////////////////////////////////////////////////// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;

/// <summary>
/// A hummingbird Machine Learning Agent
/// </summary>
public class HummingbirdAgent : Agent {

    //
    [Tooltip("Force to apply when moving")]
    public float moveForce = 2f;

    //
    [Tooltip("Speed to pitch, up or down")]
    public float pitchSpeed = 100f;

    //
    [Tooltip("Speed to rotate around the up axis")]
    public float yawSpeed = 100f;

    //
    [Tooltip("Transform at the tip of the beak")]
    public Transform beakTip;

    //
    [Tooltip("The agents camera")]
    public Camera agentCamera;

    //
    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    // rigidbody of the agent
    new private Rigidbody rigidbody;

    //flower area that the agent is in
    private FlowerArea flowerArea;

    // nearest flower to the agent
    private Flower nearestflower;

    // allows for smoother pitch changes
    private float smoothPitchChange = 0f;

    // allows for smoother yaw changes
    private float smoothYawChange = 0f;

    // maximum angle that the bird can pitch up or down
    private const float MaxPitchAngle = 80f;

    // maximum distance from the beak tip to accept nectar collision
    private const float BeakTipRadius = 0.008f;

    // whether the agent is frozen (intentionally not flying)
    private bool frozen = false;

    /// <summary>
    /// the amount of nectar the agent has obtained this episode
    /// </summary>
    public float NectarObtained { get; private set; }

}
