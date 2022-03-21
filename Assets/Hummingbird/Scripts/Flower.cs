////////////////////////////////////////////////////////////
// File: Flower.cs
// Author: Jack Peedle
// Date Created: 18/03/22
// Last Edited By: Jack Peedle
// Date Last Edited: 21/03/22
// Brief: Manages a single flower with nectar
//////////////////////////////////////////////////////////// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a single flower with nectar
/// </summary>

public class Flower : MonoBehaviour {

    // Tooltip, full flower colour = 3 RGB values
    [Tooltip("The colour when the flower is full")]
    public Color fullFlowerColour = new Color(1f, 0f, 0.3f);

    // Tooltip, empty flower colour = 3 RGB values
    [Tooltip("The colour when the flower is empty")]
    public Color emptyFlowerColour = new Color(0.5f, 0f, 1f);

    /// <summary>
    /// The trigger collider representing the nectar
    /// </summary>
    [HideInInspector]
    public Collider nectarCollider;

    // The solid collider representing the flower petals
    private Collider flowerCollider;

    // the flowers material
    private Material flowerMaterial;

    /// <summary>
    /// A vector pointing straight out the flower
    /// </summary>
    public Vector3 flowerUpVector {

        //
        get {

            //
            return nectarCollider.transform.up;

        }

    }

    /// <summary>
    /// The center position of the nectar collider
    /// </summary>
    public Vector3 flowerCenterPosition {

        get {

            //
            return nectarCollider.transform.position;

        }
        

    }

    // public value but set internally (privately)
    /// <summary>
    /// The amount of nectar remaining in the flower
    /// </summary>
    public float NectarAmount { get; private set; }

    /// <summary>
    /// Whether the flower has nectar greater than 0
    /// </summary>
    public bool hasNectar {

        //
        get {

            //
            return NectarAmount > 0f;

        }

    }

    /// <summary>
    /// Attempts to remove nectar from the flower
    /// </summary>
    /// <param name="amount"> The amount of nectar to remove </param>
    /// <returns> The actual amount successfully removed </returns>
    public float Feed(float amount) {

        // Track how much nectar was taken (can't take more than available)
        float nectarTaken = Mathf.Clamp(amount, 0f, NectarAmount);

        //subtract nectar
        NectarAmount -= amount;

        //
        if (NectarAmount <= 0) {

            // no nectar remaining
            NectarAmount = 0;

            // Disable the flower and nectar colliders
            flowerCollider.gameObject.SetActive(false);

            nectarCollider.gameObject.SetActive(false);

            // change flower colour (empty)
            flowerMaterial.SetColor("_BaseColor", emptyFlowerColour);

        }

        // Return the amount of nectar that was taken
        return nectarTaken;

    }

    /// <summary>
    /// Resets the flower
    /// </summary>
    public void ResetFlower() {

        // refil the nectar
        NectarAmount = 1f;

        // enable the flower and nectar colliders
        flowerCollider.gameObject.SetActive(true);
        nectarCollider.gameObject.SetActive(true);

        // change flower colour to indicate full flower
        flowerMaterial.SetColor("_BaseColor", fullFlowerColour);


    }

    /// <summary>
    /// called when flower wakes up
    /// </summary>
    private void Awake() {

        // find the flowers mesh renderer, get main material
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();

        //
        flowerMaterial = meshRenderer.material;

        // find flower and nectar colliders
        flowerCollider = transform.Find("FlowerCollider").GetComponent<Collider>();

        //
        nectarCollider = transform.Find("FlowerNectarCollider").GetComponent<Collider>();

    }


}
