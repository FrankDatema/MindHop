using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CloudTrackerScript : MonoBehaviour
{
    private SpawnAroundObject spawnManager; // Reference to the SpawnAroundObject script
    public SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer component
    private SpawnAroundObject.HouseholdChore assignedChore; // The chore assigned to this cloud
    public float shrinkSpeed = 5.0f;

    void Start()
    {
        // Get the SpriteRenderer component if not assigned
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        // Find the SpawnAroundObject script in the scene
        spawnManager = FindObjectOfType<SpawnAroundObject>();

        if (spawnManager != null && spawnManager.householdChores.Count > 0)
        {
            // Get a random chore that hasn't been spawned yet
            assignedChore = GetRandomUnspawnedChore();

            if (assignedChore != null)
            {
                // Set the sprite of this cloud to the assigned chore's sprite
                if (spriteRenderer != null && assignedChore.choreSprite != null)
                {
                    spriteRenderer.sprite = assignedChore.choreSprite;
                }

                // Add the assigned chore to the spawned chores list
                spawnManager.spawnedChores.Add(assignedChore);
            }
            else
            {
                Debug.LogWarning("No unspawned chores available.");
            }
        }
        else
        {
            Debug.LogWarning("No chores available or SpawnAroundObject not found.");
        }
    }

    public void StartShrinkAndDestroy()
    {
        StartCoroutine(ShrinkAndDestroyCoroutine());
    }

    private IEnumerator ShrinkAndDestroyCoroutine()
    {
        // Gradually shrink the object
        while (transform.localScale.sqrMagnitude > 0.01f)
        {
            // Shrink the object
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, shrinkSpeed * Time.deltaTime);

            // Wait for the next frame
            yield return null;
        }

        // Ensure the object is completely shrunk
        transform.localScale = Vector3.zero;

        // Debug: Log that the object is being destroyed
        Debug.Log("Destroying object: " + gameObject.name);

        // Destroy the object once it's small enough
        Destroy(gameObject);
    }


    void Update()
    {
        // Optional: Add any additional behavior for the cloud here
    }

    // Helper method to get a random unspawned chore
    private SpawnAroundObject.HouseholdChore GetRandomUnspawnedChore()
    {
        // Create a list of chores that haven't been spawned yet
        List<SpawnAroundObject.HouseholdChore> unspawnedChores = new List<SpawnAroundObject.HouseholdChore>();

        foreach (var chore in spawnManager.householdChores)
        {
            if (!spawnManager.spawnedChores.Contains(chore))
            {
                unspawnedChores.Add(chore);
            }
        }

        // If there are unspawned chores, return a random one
        if (unspawnedChores.Count > 0)
        {
            int randomIndex = Random.Range(0, unspawnedChores.Count);
            return unspawnedChores[randomIndex];
        }

        // If all chores have been spawned, return null
        return null;
    }


}