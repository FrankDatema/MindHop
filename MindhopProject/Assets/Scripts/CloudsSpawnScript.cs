using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAroundObject : MonoBehaviour
{
    [System.Serializable]
    public class HouseholdChore
    {
        public string choreName; // Name of the chore
        public Sprite choreSprite; // Sprite associated with the chore
        public int daysUntilReset;
    }

    public List<HouseholdChore> householdChores; // List of household chores
    public List<HouseholdChore> spawnedChores = new List<HouseholdChore>(); // List of spawned chores
    public GameObject prefabToSpawn; // The prefab to spawn (should have a SpriteRenderer component)
    public Transform centerObject;   // The center GameObject around which prefabs will spawn
    public Camera mainCamera;        // The camera to face and check visibility
    public float spawnRadius = 5f;   // Radius around the center object to spawn prefabs
    public float minDistance = 1f;   // Minimum distance between spawned objects
    public float minSpawnInterval = 0.5f; // Minimum time between spawns
    public float maxSpawnInterval = 2f;   // Maximum time between spawns
    public float growthDuration = 1f; // Time it takes for objects to grow from scale 0 to 1
    public float minHeight = 1f;     // Minimum height for spawning objects
    public float maxHeight = 3f;     // Maximum height for spawning objects

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private Plane[] cameraFrustumPlanes;

    void Start()
    {
        // Start the gradual spawning process
        StartCoroutine(SpawnAndGrowPrefabs());
    }

    void Update()
    {
        // Update camera frustum planes if the camera has moved
        cameraFrustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        // Ensure all spawned objects face the camera
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                obj.transform.LookAt(mainCamera.transform);
                obj.SetActive(IsVisible(obj));
            }
        }
    }

    IEnumerator SpawnAndGrowPrefabs()
    {
        while (true)
        {
            // Clear null objects from the list
            spawnedObjects.RemoveAll(obj => obj == null);

            // Spawn clouds until we reach the initial number of chores
            if (spawnedObjects.Count < householdChores.Count)
            {
                Vector3 spawnPosition = GetRandomSpawnPosition();
                GameObject newObj = GetPooledObject(spawnPosition);

                spawnedObjects.Add(newObj);

                // Grow the new object
                yield return StartCoroutine(GrowObject(newObj));

                // Wait for a random interval before spawning the next object
                float randomDelay = Random.Range(minSpawnInterval, maxSpawnInterval);
                yield return new WaitForSeconds(randomDelay);
            }
            else
            {
                // If we've reached the target spawn count, wait a bit before checking again
                yield return new WaitForSeconds(1f);
            }
        }
    }

    GameObject GetPooledObject(Vector3 position)
    {
        GameObject obj;
        if (objectPool.Count > 0)
        {
            obj = objectPool.Dequeue();
            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefabToSpawn, position, Quaternion.identity);
        }
        obj.transform.localScale = Vector3.zero; // Start with scale 0
        return obj;
    }

    IEnumerator GrowObject(GameObject obj)
    {
        float elapsedTime = 0f;
        Vector3 initialScale = Vector3.zero;
        Vector3 targetScale = Vector3.one * 100;

        while (elapsedTime < growthDuration)
        {
            if (obj != null)
            {
                // Interpolate the scale from 0 to 1 over the growth duration
                obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / growthDuration);
                elapsedTime += Time.deltaTime;
            }
            yield return null;
        }

        // Ensure the object reaches the target scale
        if (obj != null)
        {
            obj.transform.localScale = targetScale;
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPosition;
        bool positionFound = false;
        int attempts = 0;

        do
        {
            // Generate a random position within the spawn radius
            spawnPosition = centerObject.position + Random.insideUnitSphere * spawnRadius;

            // Set a random height within the specified range
            spawnPosition.y = centerObject.position.y + Random.Range(minHeight, maxHeight);

            // Check if the position is far enough from other spawned objects
            positionFound = true;
            foreach (var obj in spawnedObjects)
            {
                if (obj != null && Vector3.Distance(spawnPosition, obj.transform.position) < minDistance)
                {
                    positionFound = false;
                    break;
                }
            }

            attempts++;
        } while (!positionFound && attempts < 100); // Limit attempts to avoid infinite loops

        return spawnPosition;
    }

    bool IsVisible(GameObject obj)
    {
        // Check if the object is within the camera's view frustum
        return GeometryUtility.TestPlanesAABB(cameraFrustumPlanes, obj.GetComponent<Renderer>().bounds);
    }
}