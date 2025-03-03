using System;
using UnityEngine;
using TMPro;  // Import TextMeshPro namespace

public class NFCChoreChecker : MonoBehaviour
{
    // Assign your TextMeshPro (or TextMeshProUGUI) component in the Inspector.
    public TMP_Text nfcTextMesh;

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

                        // Set the text of the TextMeshPro object to display the NFC tag ID.
                        if (nfcTextMesh != null)
                        {
                            nfcTextMesh.text = tagID;
                        }
                        else
                        {
                            Debug.LogWarning("TextMeshPro object not assigned in the inspector.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Log("Error reading tag: " + ex.Message);
            }
        }
    }
}
