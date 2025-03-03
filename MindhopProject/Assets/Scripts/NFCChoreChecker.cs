using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NFCChoreChecker : MonoBehaviour
{
    private bool tagFound = false;
    private string tagID;

    void Update()
    {
        if (Application.platform == RuntimePlatform.Android && !tagFound)
        {
            try
            {
                AndroidJavaObject mActivity = new AndroidJavaClass("com.unity3d.player.UnityPlayer")
                                                .GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject mIntent = mActivity.Call<AndroidJavaObject>("getIntent");
                string sAction = mIntent.Call<string>("getAction");

                if (sAction == "android.nfc.action.NDEF_DISCOVERED" ||
                    sAction == "android.nfc.action.TECH_DISCOVERED")
                {
                    AndroidJavaObject mNdefMessage = mIntent.Call<AndroidJavaObject>("getParcelableExtra", "android.nfc.extra.TAG");
                    if (mNdefMessage != null)
                    {
                        byte[] payLoad = mNdefMessage.Call<byte[]>("getId");
                        tagID = Convert.ToBase64String(payLoad);
                        tagFound = true;
                        CheckForMatchingChoreCloud();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error reading tag: " + ex.Message);
            }
        }
    }

    void CheckForMatchingChoreCloud()
    {
        CloudTrackerScript[] clouds = GameObject.FindObjectsOfType<CloudTrackerScript>();
        foreach (var cloud in clouds)
        {
            if (!string.IsNullOrEmpty(cloud.ChoreTagID) && cloud.ChoreTagID.Equals(tagID))
            {
                // Save the matching chore information in GameManager.
                GameManager.Instance.CurrentChore = cloud.ChoreDescription; // assuming you have a property for the description

                // Load the minigame scene.
                SceneManager.LoadScene("MinigameScene");
                return;
            }
        }
    }
}
