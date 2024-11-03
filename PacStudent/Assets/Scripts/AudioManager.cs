using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;  // Singleton instance

    public AudioClip pelletSound;
    public AudioClip powerPelletSound;
    public AudioClip wallCollisionSound;  // Wall collision sound clip
    public AudioClip bananaSound;  // Banana sound clip
    public AudioClip deathSound;  // Death sound clip

    // Music clips for different ghost states
    public AudioClip normalGhostMusic;
    public AudioClip scaredGhostMusic;
    public AudioClip deadGhostMusic;

    private AudioSource audioSource;

    void Awake()
    {
        // Ensure only one instance of AudioManager exists
        if (Instance == null)
        {
            Instance = this;
            // Removed DontDestroyOnLoad
        }
        else
        {
            Destroy(gameObject);
            return; // Exit the Awake method if a duplicate instance is destroyed
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void PlayPelletSound()
    {
        audioSource.PlayOneShot(pelletSound);
    }

    public void PlayPowerPelletSound()
    {
        audioSource.PlayOneShot(powerPelletSound);
    }

    public void PlayWallCollisionSound()
    {
        audioSource.PlayOneShot(wallCollisionSound);  // Play wall collision sound
    }

    public void PlayBananaSound()
    {
        audioSource.PlayOneShot(bananaSound);  // Play banana sound
    }

    public void PlayDeathSound()
    {
        audioSource.PlayOneShot(deathSound);  // Play death sound
    }

    // Play ghost music based on state
    public void PlayGhostMusic(string state)
    {
        if (state == "normal")
        {
            audioSource.clip = normalGhostMusic;
            audioSource.loop = true; // Set to loop
        }
        else if (state == "scared")
        {
            audioSource.clip = scaredGhostMusic;
            audioSource.loop = true; // Set to loop
        }
        else if (state == "dead")
        {
            audioSource.clip = deadGhostMusic;
            audioSource.loop = false; // Do not loop for dead state
        }
        audioSource.Play(); // Start playing the music
    }

    // Stop the current playing ghost music
    public void StopGhostMusic()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Stop(); // Stop playing music
        }
    }
}
