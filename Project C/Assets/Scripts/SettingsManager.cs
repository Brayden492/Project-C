using UnityEngine;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;
    public Slider sensitivitySlider;

    private void Start()
    {
        // Load saved settings
        volumeSlider.value = PlayerPrefs.GetFloat("Volume", 1f);
        sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity", 200f);

        // Apply the saved volume
        AudioListener.volume = volumeSlider.value;
    }

    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;

        PlayerPrefs.SetFloat("Volume", volumeSlider.value);
    }

    public void ChangeSensitivity()
    {
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
        //Debug.Log("New sensitivity: " + sensitivitySlider.value);
    }
}
