////////////////////////////////////////////////////////////
// File: Flower.cs
// Author: Jack Peedle
// Date Created: 18/03/22
// Last Edited By: Jack Peedle
// Date Last Edited: 18/03/22
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
}
