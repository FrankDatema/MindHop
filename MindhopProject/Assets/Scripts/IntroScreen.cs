using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class IntroScreen : MonoBehaviour
{
    public Image fadePanel;                 // Assign the black UI Panel in the Inspector
    public AudioSource audioSource;         // Assign the AudioSource in the Inspector
    public string mainMenuSceneName = "MainMenu"; // Name of the main menu scene

    private float fadeDuration = 12.0f;      // Duration of the fade-in effect
    private float fadeOutDuration = 2.0f;    // Duration of the fade-out effect
    private bool fadeInComplete = false;
    private bool hasStartedFadeOut = false;

    void Start()
    {
        // Ensure the panel is fully black at the start
        fadePanel.color = Color.black;

        // Start the fade-in effect
        StartCoroutine(FadeIn());
    }

    void Update()
    {
        // Once the fade-in is complete and the audio has stopped,
        // start the fade-out effect if it hasn't started already.
        if (!audioSource.isPlaying && fadeInComplete && !hasStartedFadeOut)
        {
            StartCoroutine(FadeOut());
            hasStartedFadeOut = true;
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        Color panelColor = fadePanel.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            // Interpolate alpha from 1 (opaque) to 0 (transparent)
            float alpha = Mathf.Clamp01(1 - (elapsedTime / fadeDuration));
            fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            yield return null;
        }

        // Ensure the panel is fully transparent after fade-in
        fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0);
        fadeInComplete = true;
    }

    private IEnumerator FadeOut()
    {
        float elapsedTime = 0f;
        Color panelColor = fadePanel.color;

        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            // Interpolate alpha from 0 (transparent) to 1 (opaque)
            float alpha = Mathf.Clamp01(elapsedTime / fadeOutDuration);
            fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            yield return null;
        }

        // Ensure the panel is fully opaque before switching scenes
        fadePanel.color = new Color(panelColor.r, panelColor.g, panelColor.b, 1);
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
