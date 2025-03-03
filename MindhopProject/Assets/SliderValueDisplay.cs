using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Import the TextMeshPro namespace

public class SliderValueDisplayTMP : MonoBehaviour
{
    public Slider slider;       // Reference to the slider component.
    public TMP_Text valueText;  // Reference to the TextMeshPro text component.

    void Start()
    {
        // Ensure the slider uses whole numbers.
        slider.wholeNumbers = true;

        // Add a listener to update the TMP text when the slider value changes.
        slider.onValueChanged.AddListener(UpdateValueText);

        // Initialize the text value.
        UpdateValueText(slider.value);
    }

    void UpdateValueText(float value)
    {
        if (valueText != null)
        {
            // Convert the slider value to an integer and update the TMP text.
            valueText.text = ((int)value).ToString();
        }
    }
}
