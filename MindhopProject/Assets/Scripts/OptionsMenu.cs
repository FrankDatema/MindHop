using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ChoreFieldMapping
{
    public string choreTag;     // Unique tag that identifies the chore
    public Slider timeSlider;   // Slider to set the maximum allowed days (configured to use whole numbers)
}


public class OptionsMenu : MonoBehaviour
{
    [Header("Chore Field Mappings")]
    // List of mappings between a chore tag and a slider.
    public List<ChoreFieldMapping> choreFieldMappings;

    // Button to save the options.
    public Button saveButton;

    void Start()
    {
        // Ensure the persistent SpawnAroundObject instance exists.
        if (SpawnAroundObject.Instance == null)
        {
            Debug.LogError("SpawnAroundObject instance not found!");
            return;
        }

        // Load saved settings for each mapping.
        foreach (var mapping in choreFieldMappings)
        {
            // Look up the chore using its unique tag.
            var chore = SpawnAroundObject.Instance.householdChores.Find(c => c.tagID == mapping.choreTag);
            if (chore != null)
            {
                // Use a key that uniquely identifies the chore (here using the chore's name).
                string key = chore.choreName + "MaxDays";
                int maxDays = PlayerPrefs.GetInt(key, 1); // default is 1 if not set
                mapping.timeSlider.value = maxDays;
            }
            else
            {
                Debug.LogWarning("No chore found for tag: " + mapping.choreTag);
            }
        }

        // Set up the save button.
        saveButton.onClick.AddListener(SaveSettings);
    }

    public void SaveSettings()
    {
        // Iterate through each mapping and save the slider's value as the max days setting.
        foreach (var mapping in choreFieldMappings)
        {
            var chore = SpawnAroundObject.Instance.householdChores.Find(c => c.tagID == mapping.choreTag);
            if (chore != null)
            {
                int maxDays = (int)mapping.timeSlider.value;
                string key = chore.choreName + "MaxDays";
                PlayerPrefs.SetInt(key, maxDays);
            }
        }
        PlayerPrefs.Save();
        Debug.Log("Chore time settings saved.");
    }
}

