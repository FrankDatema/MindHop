using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnAroundObject : MonoBehaviour
{
    // Singleton instance for global access
    public static SpawnAroundObject Instance;

    [Serializable]
    public class HouseholdChore
    {
        public string choreName;      // Name of the chore
        public Sprite choreSprite;    // Sprite associated with the chore
        public int daysUntilReset;    // Default reset days if not set via options
        public string tagID;
    }

    [Header("Chore Settings")]
    public List<HouseholdChore> householdChores; // List of all chore definitions

    // This list tracks which chores have been spawned (for example, for CloudTrackerScript)
    public List<HouseholdChore> spawnedChores = new List<HouseholdChore>();

    [Header("Spawn Settings")]
    public GameObject prefabToSpawn;   // Prefab that should have a SpriteRenderer component
    public Transform centerObject;     // The center around which prefabs will spawn
    public Camera mainCamera;          // Camera used to face spawned objects
    public float spawnRadius = 5f;     // Radius around the centerObject to spawn prefabs
    public float minDistance = 1f;     // Minimum distance between spawned objects
    public float growthDuration = 1f;  // Duration for the object to grow from 0 to full scale
    public float minHeight = 1f;       // Minimum height offset for spawn
    public float maxHeight = 3f;       // Maximum height offset for spawn

    // Internal lists to track spawned objects and saved spawn data
    private List<GameObject> spawnedObjects = new List<GameObject>();
    private List<ChoreSpawnData> savedChoreData = new List<ChoreSpawnData>();

    // Data persistence wrapper class for JSON
    [Serializable]
    private class ChoreSpawnDataWrapper
    {
        public List<ChoreSpawnData> dataList;
    }

    // Data class for each spawn event (one per chore)
    [Serializable]
    public class ChoreSpawnData
    {
        public string choreName;
        public float spawnTimestamp;   // Unix timestamp when the chore cloud was spawned
        public Vector3 spawnPosition;  // Position where the object was spawned
        public Vector3 spawnScale;     // The intended scale of the object
    }

    void Awake()
    {
        // Singleton pattern: only one instance exists.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ResetSpawnData()
    {
        // Delete the saved spawn data from PlayerPrefs.
        PlayerPrefs.DeleteKey("ChoreSpawnData");
        PlayerPrefs.Save();

        // Optionally, clear the in-memory list.
        savedChoreData.Clear();
        Debug.Log("Spawn data has been reset.");
    }

    void Start()
    {
        LoadSpawnData();

        // If no saved data exists, this is the first launch.
        if (savedChoreData.Count == 0)
        {
            // Spawn every chore once.
            foreach (var chore in householdChores)
            {
                SpawnChore(chore);
            }
        }
        else
        {
            // Reinstantiate previously saved chore clouds.
            foreach (var data in savedChoreData)
            {
                // Check if the chore is already in the scene (this might happen if the object wasn't destroyed)
                if (!IsChoreInScene(data.choreName))
                {
                    // Instantiate at the saved position (or pick a new one if the saved position is zero)
                    Vector3 spawnPosition = data.spawnPosition != Vector3.zero ? data.spawnPosition : GetRandomSpawnPosition();
                    // Use saved spawn scale or fallback to the prefab's original scale.
                    Vector3 spawnScale = data.spawnScale != Vector3.zero ? data.spawnScale : prefabToSpawn.transform.localScale;
                    GameObject newObj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
                    newObj.name = data.choreName;  // Tag the spawned object with its chore name
                    SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
                    HouseholdChore chore = householdChores.Find(c => c.choreName == data.choreName);
                    if (chore != null && sr != null)
                    {
                        sr.sprite = chore.choreSprite;
                    }
                    spawnedObjects.Add(newObj);

                    // Set scale to zero and grow to the saved scale.
                    newObj.transform.localScale = Vector3.zero;
                    StartCoroutine(GrowObject(newObj, spawnScale));

                    // Check if enough days have passed to allow a respawn.
                    DateTime lastSpawnTime = UnixTimeStampToDateTime(data.spawnTimestamp);
                    double daysElapsed = (DateTime.UtcNow - lastSpawnTime).TotalDays;
                    if (daysElapsed >= chore.daysUntilReset)
                    {
                        // Update the timestamp so that the check in the coroutine waits until the next reset period.
                        data.spawnTimestamp = DateTimeToUnixTimeStamp(DateTime.UtcNow);
                    }
                }
            }
        }

        // Start a coroutine to periodically check for missing chores and spawn new ones as needed.
        StartCoroutine(ChoreSpawnCheckLoop());
    }

    void Update()
    {
        // Optional: Make spawned objects face the main camera.
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                obj.transform.LookAt(mainCamera.transform);
            }
        }
    }

    // Helper method to spawn a chore and record its spawn data.
    void SpawnChore(HouseholdChore chore)
    {
        Vector3 spawnPosition = GetRandomSpawnPosition();
        GameObject newObj = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
        newObj.name = chore.choreName;  // Set name for identification
        SpriteRenderer sr = newObj.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.sprite = chore.choreSprite;
        }
        spawnedObjects.Add(newObj);

        // Capture the prefab's original scale.
        Vector3 originalScale = newObj.transform.localScale;
        // Set scale to zero so it can grow.
        newObj.transform.localScale = Vector3.zero;
        // Grow the object to its original scale.
        StartCoroutine(GrowObject(newObj, originalScale));

        // Create or update spawn data for this chore.
        ChoreSpawnData data = savedChoreData.Find(d => d.choreName == chore.choreName);
        if (data == null)
        {
            data = new ChoreSpawnData();
            savedChoreData.Add(data);
        }
        data.choreName = chore.choreName;
        data.spawnTimestamp = DateTimeToUnixTimeStamp(DateTime.UtcNow);
        data.spawnPosition = spawnPosition;
        data.spawnScale = originalScale;

        SaveSpawnData();
    }

    // Coroutine that periodically checks if any chore is missing and ready to be re-spawned.
    IEnumerator ChoreSpawnCheckLoop()
    {
        while (true)
        {
            // Loop through all defined chores.
            foreach (var chore in householdChores)
            {
                if (!IsChoreInScene(chore.choreName))
                {
                    // Find saved data for this chore.
                    ChoreSpawnData data = savedChoreData.Find(d => d.choreName == chore.choreName);
                    // If data exists, check the reset timer.
                    if (data != null)
                    {
                        DateTime lastSpawnTime = UnixTimeStampToDateTime(data.spawnTimestamp);
                        double daysElapsed = (DateTime.UtcNow - lastSpawnTime).TotalDays;
                        if (daysElapsed >= chore.daysUntilReset)
                        {
                            SpawnChore(chore);
                        }
                    }
                    else
                    {
                        // If no saved data exists (should not happen normally), spawn it.
                        SpawnChore(chore);
                    }
                }
            }
            // Check every minute (adjust as needed).
            yield return new WaitForSeconds(60f);
        }
    }

    // Helper method to check if a chore is already present in the scene.
    bool IsChoreInScene(string choreName)
    {
        foreach (var obj in spawnedObjects)
        {
            if (obj != null && obj.name == choreName)
            {
                return true;
            }
        }
        return false;
    }

    // Get a random spawn position around the center object.
    Vector3 GetRandomSpawnPosition()
    {
        Vector3 spawnPosition = Vector3.zero;
        bool positionFound = false;
        int attempts = 0;

        while (!positionFound && attempts < 100)
        {
            spawnPosition = centerObject.position + UnityEngine.Random.insideUnitSphere * spawnRadius;
            spawnPosition.y = centerObject.position.y + UnityEngine.Random.Range(minHeight, maxHeight);

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
        }
        return spawnPosition;
    }

    // Coroutine to grow a spawned object from zero to its target scale.
    IEnumerator GrowObject(GameObject obj, Vector3 targetScale)
    {
        float elapsedTime = 0f;
        Vector3 initialScale = Vector3.zero;

        while (elapsedTime < growthDuration)
        {
            if (obj != null)
            {
                obj.transform.localScale = Vector3.Lerp(initialScale, targetScale, elapsedTime / growthDuration);
                elapsedTime += Time.deltaTime;
            }
            yield return null;
        }
        if (obj != null)
        {
            obj.transform.localScale = targetScale;
        }
    }

    #region Data Persistence

    // Save spawn data using PlayerPrefs.
    void SaveSpawnData()
    {
        ChoreSpawnDataWrapper wrapper = new ChoreSpawnDataWrapper { dataList = savedChoreData };
        string json = JsonUtility.ToJson(wrapper);
        PlayerPrefs.SetString("ChoreSpawnData", json);
        PlayerPrefs.Save();
        Debug.Log("Spawn data saved.");
    }

    // Load spawn data from PlayerPrefs.
    void LoadSpawnData()
    {
        if (PlayerPrefs.HasKey("ChoreSpawnData"))
        {
            string json = PlayerPrefs.GetString("ChoreSpawnData");
            ChoreSpawnDataWrapper wrapper = JsonUtility.FromJson<ChoreSpawnDataWrapper>(json);
            if (wrapper != null && wrapper.dataList != null)
            {
                savedChoreData = wrapper.dataList;
            }
        }
    }

    // Convert DateTime to Unix timestamp (in seconds).
    float DateTimeToUnixTimeStamp(DateTime date)
    {
        return (float)(date - new DateTime(1970, 1, 1)).TotalSeconds;
    }

    // Convert Unix timestamp (in seconds) to DateTime.
    DateTime UnixTimeStampToDateTime(float unixTimeStamp)
    {
        return new DateTime(1970, 1, 1).AddSeconds(unixTimeStamp);
    }

    #endregion
}
