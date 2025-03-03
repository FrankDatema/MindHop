using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // This property will store the chore description or identifier from the scanned NFC tag.
    public string CurrentChore { get; set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist this object between scenes
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

