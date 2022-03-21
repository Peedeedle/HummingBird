////////////////////////////////////////////////////////////
// File: FlowerArea.cs
// Author: Jack Peedle
// Date Created: 18/03/22
// Last Edited By: Jack Peedle
// Date Last Edited: 21/03/22
// Brief: 
//////////////////////////////////////////////////////////// 

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages a connection of flower plants and attached flowers
/// </summary>
public class FlowerArea : MonoBehaviour {

    // the diameter of the area where the agent and flowers can be
    // used for observing relative distance from flower
    public const float AreaDiameter = 20f;

    // the list of all flower plants in this flower area (flower plants have multiple flowers)
    private List<GameObject> flowerPlants;

    // A lookup dictionary for looking up a flower from a nectar collider
    private Dictionary<Collider, Flower> nectarFlowerDictionary;

    /// <summary>
    /// List of all flowers in the flower area
    /// </summary>
    // private get and set (i can modify), bird can't access this class
    public List<Flower> Flowers { get; private set; }

    /// <summary>
    /// Reset the flowers and flower plants
    /// </summary>
    public void ResetFlowers() {

        //rotate each plant around the Y Axis, subtly around the X and Z
        foreach(GameObject flowerPlant in flowerPlants) {

            //
            float xRotation = UnityEngine.Random.Range(-5f, 5f);
            float yRotation = UnityEngine.Random.Range(-180f, 180f);
            float zRotation = UnityEngine.Random.Range(-5f, 5f);

            //
            flowerPlant.transform.localRotation = Quaternion.Euler(xRotation, yRotation, zRotation);

            

        }

        // Reset each flower
        foreach (Flower flower in Flowers) {

            //
            flower.ResetFlower();

        }


    }

    /// <summary>
    /// Gets the <see cref="Flower"/> that a nectar collider belongs to
    /// </summary>
    /// <param name="collider"> the nectar collider</param>
    /// <returns> the matching flower </returns>
    public Flower GetFlowerFromNectar(Collider collider) {

        //
        return nectarFlowerDictionary[collider];

    }

    /// <summary>
    /// called when the area wakes up
    /// </summary>
    private void Awake() {

        //
        flowerPlants = new List<GameObject>();

        //
        nectarFlowerDictionary = new Dictionary<Collider, Flower>();

        //
        Flowers = new List<Flower>();

    }

    /// <summary>
    /// called when the game starts
    /// </summary>
    private void Start() {

        // find all flowers that are children of this gameobject/transform
        FindChildFlowers(transform);

    }

    /// <summary>
    /// recursively finds all flowers and flower plants that are children of a parent transform 
    /// </summary>
    /// <param name="parent"> the parent of the children to check </param>
    private void FindChildFlowers(Transform parent) {

        //
        for (int i = 0; i < parent.childCount; i++) {

            // get specific child on parent
            Transform child = parent.GetChild(i);

            //
            if (child.CompareTag("flower_plant")) {

                // found a flower plant, add to flower plant list
                flowerPlants.Add(child.gameObject);

                // look for flowers within the flower plant
                FindChildFlowers(child);


            } else {

                // not a flower plant, look for a flower component
                Flower flower = child.GetComponent<Flower>();

                //
                if (flower != null) {

                    // found a flower, add to flower list
                    Flowers.Add(flower);

                    // add the nectar collider to the lookup dictionary
                    nectarFlowerDictionary.Add(flower.nectarCollider, flower);

                    // NOTE: there are no flowers that are children of other flowers

                } else {

                    // flower component not found, so check children
                    FindChildFlowers(child);

                }

            }
        }

    }

}
