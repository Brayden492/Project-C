using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public AudioSource musicSource;

    private void Start()
    {
        // Load the player's saved volume
        musicSource.volume = PlayerPrefs.GetFloat("Volume", 1f);

        // Make sure music starts turned off
        musicSource.Stop();
    }

    // Start playing music when the match begins
    public void StartMusic()
    {
        if (!musicSource.isPlaying)
        {
            musicSource.Play();
        }
    }

    // Stop music when the match ends
    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void ChangeVolume(float volume)
    {
        musicSource.volume = volume;
        PlayerPrefs.SetFloat("Volume", volume);
    }
}
