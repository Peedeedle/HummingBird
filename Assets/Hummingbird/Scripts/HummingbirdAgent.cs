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
using Unity.MLAgents.Sensors;

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

    /// <summary>
    /// Initialize the agent 
    /// </summary>
    public override void Initialize() {

        //
        rigidbody = GetComponent<Rigidbody>();

        flowerArea = GetComponentInParent<FlowerArea>();

        // if not training mode, no max step, play forever
        if (!trainingMode) {

            MaxStep = 0;

        }

    }

    /// <summary>
    /// reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin() {

        // 
        if (trainingMode) {

            // only reset flowers in training when there is 1 agent per area
            flowerArea.ResetFlowers();

        }

        // reset nectar obtained
        NectarObtained = 0f;

        // zero out velocities so that movement stops before a new episode begins
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;

        // default to spawning in front of a flower
        bool inFrontOfFlower = true;
        if (trainingMode) {

            // spawn in front of flower 50% of the time during training
            inFrontOfFlower = UnityEngine.Random.value > 0.5f;

        }

        // move agent to new random position
        MoveToSafeRandomPosition(inFrontOfFlower);

        // recalculate nearest flower now the agent has moved
        UpdateNearestFlower();


    }

    /// <summary>
    /// called when an action is recieved from either the player input or the neural network (NN)
    /// 
    /// vectorAction[i] represents:
    /// index 0: move vector x (+1 = move right, -1 = move left, 0 = dont move right or left)
    /// index 1: move vector y (+1 = move up, -1 = move down, 0 = dont move up or down)
    /// index 2: move vector z (+1 = move forward, -1 = move backwards, 0 = dont move forward or backwards)
    /// index 3: pitch angle (+1 = pitch up, -1 = pitch down, 0 = dont pitch up or down)
    /// index 4: yaw angle (+1 = turn right, -1 = turn left, 0 = dont turn right or left)
    /// 
    /// </summary>
    /// <param name="vectorAction"> the actions to take </param>
    public override void OnActionReceived(float[] vectorAction) {

        // don't take actions if frozen
        if (frozen) {
            return;
        }

        // calculate movement vector
        Vector3 move = new Vector3(vectorAction[0], vectorAction[1], vectorAction[2]);

        // add force in the direction of the move vector
        rigidbody.AddForce(move * moveForce);

        // get the current rotation (x, y, z)
        Vector3 rotationVector = transform.rotation.eulerAngles;

        // calculate pitch and yaw rotations
        float pitchChange = vectorAction[3];
        float yawChange = vectorAction[4];

        // calculate smooth rotation changes
        smoothPitchChange = Mathf.MoveTowards(smoothPitchChange, pitchChange, 2f * Time.fixedDeltaTime);
        smoothYawChange = Mathf.MoveTowards(smoothYawChange, yawChange, 2f * Time.fixedDeltaTime);

        // calculate new pitch and yaw based on smooth values
        // clamp pitch to avoid flipping upside down
        float pitch = rotationVector.x + smoothPitchChange * Time.fixedDeltaTime * pitchSpeed;
        if (pitch > 180f) {
            pitch -= 360f;
        }
        pitch = Mathf.Clamp(pitch, -MaxPitchAngle, MaxPitchAngle);

        float yaw = rotationVector.y + smoothYawChange * Time.fixedDeltaTime * yawSpeed;

        // apply new rotation
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

    }

    /// <summary>
    /// collect vector observations from the environment
    /// </summary>
    /// <param name="sensor"> the vector sensor </param>
    public override void CollectObservations(VectorSensor sensor) {

        // if nearest flower is null, observe and empty array and return early
        if (nearestflower == null) {

            sensor.AddObservation(new float[10]);
            return;

        }

        // observe the agents local rotation (4 observations)
        sensor.AddObservation(transform.localRotation.normalized);

        // get vector from the beak tip to the nearest flower
        Vector3 toFlower = nearestflower.flowerCenterPosition - beakTip.position;

        // observe a normalized vector pointing to the nearest flower (3 observations)
        sensor.AddObservation(toFlower.normalized);

        // observe DOT product that indicates whether the beak tip is in front of the flower (1 observation (float))
        // (+1 = beak tip is directly infront of the flower, -1 = directly behind)
        sensor.AddObservation(Vector3.Dot(toFlower.normalized, -nearestflower.flowerUpVector.normalized));

        // observe a DOT product that indicates whether the beak is pointing towards the flower (1 observation (float))
        // +1 = beak is pointing at the flower, -1 = pointing away
        sensor.AddObservation(Vector3.Dot(beakTip.forward.normalized, -nearestflower.flowerUpVector.normalized));

        // observe the relative distance from the beak tip to the flower (1 observation (float))
        sensor.AddObservation(toFlower.magnitude / FlowerArea.AreaDiameter);

        // 10 total observations!!!!!!!

    }

    /// <summary>
    /// move the agent to a safe random position (i.e. does not collide with anything)
    /// if in front of the flower, also point the beak at the flower
    /// </summary>
    /// <param name="inFrontOfFlower"> whether to choose a spot in front of a flower </param>
    private void MoveToSafeRandomPosition(bool inFrontOfFlower) {

        //
        bool safePositionFound = false;
        int attemptsRemaining = 100; // prevent infinite loop
        Vector3 potentialPosition = Vector3.zero;
        Quaternion potentialRotation = new Quaternion();

        // loop until a safe position is found or wqe run out of attempts
        while(!safePositionFound && attemptsRemaining > 0) {

            //
            attemptsRemaining--;
            if (inFrontOfFlower) {

                // pick a random flower
                Flower randomFlower = flowerArea.Flowers[UnityEngine.Random.Range(0, flowerArea.Flowers.Count)];

                // position 10-20 cm in front of the flower
                float distanceFromFlower = UnityEngine.Random.Range(0.1f, 0.2f);
                potentialPosition = randomFlower.transform.position + randomFlower.flowerUpVector * distanceFromFlower;

                // point beak at flower (bird's head is center of transform)
                Vector3 toFlower = randomFlower.flowerCenterPosition - potentialPosition;
                potentialRotation = Quaternion.LookRotation(toFlower, Vector3.up);

            } else {

                // pick a random height from the ground
                float height = UnityEngine.Random.Range(1.2f, 2.5f);

                // pick random radius from the center of the area
                float radius = UnityEngine.Random.Range(2f, 7f);

                // pick random direction rotated around the y axis
                Quaternion direction = Quaternion.Euler(0f, UnityEngine.Random.Range(-180f, 180f), 0f);

                // combine height, radius, and direction to pick a potential position
                potentialPosition = flowerArea.transform.position + Vector3.up * height + direction * Vector3.forward * radius;

                // choose and set random starting pitch and yaw
                float pitch = UnityEngine.Random.Range(-60f, 60f);
                float yaw = UnityEngine.Random.Range(-180f, 180f);
                potentialRotation = Quaternion.Euler(pitch, yaw, 0f);

                // check to see if the agent will collide with anything
                Collider[] colliders = Physics.OverlapSphere(potentialPosition, 0.05f);

                // safe position has been found if no colliders are overlapped
                safePositionFound = colliders.Length == 0;

            }

            // 
            Debug.Assert(safePositionFound, "Could not find a safe position to spawn");

            // set the position and rotation 
            transform.position = potentialPosition;
            transform.rotation = potentialRotation;

        }

    }

    /// <summary>
    /// Update the nearest flower to the agent
    /// </summary>
    private void UpdateNearestFlower() {
        
        //
        foreach(Flower flower in flowerArea.Flowers) {

            //
            if (nearestflower == null && flower.hasNectar) {

                // no current nearest flower, this flower has nectar so set to this flower
                nearestflower = flower;

            } else if (flower.hasNectar) {

                // calculate distance to this flower and distance to the current nearest flower
                float distanceToFlower = Vector3.Distance(flower.transform.position, beakTip.position);

                //
                float distancetoCurrentNearestFlower = Vector3.Distance(nearestflower.transform.position, beakTip.position);

                // if current nearest flower is empty OR this flower is closer, update nearest flower
                if (!nearestflower.hasNectar || distanceToFlower < distancetoCurrentNearestFlower) {

                    //
                    nearestflower = flower;

                }

            }

        }

    }

}
