using UnityEngine;

public class ParticleEffect : MonoBehaviour
{
    public ParticleSystem dustParticles;  // Reference to dust particle system
    public ParticleSystem collisionParticles;  // Reference to collision particle system

    public void PlayCollisionEffect(Vector2 position)
    {
        if (collisionParticles != null)
        {
            collisionParticles.transform.position = position;  // Set particle effect position
            collisionParticles.Play();  // Play the collision effect
        }
    }

    public void PlayDustParticles()
    {
        if (dustParticles != null && !dustParticles.isPlaying)
        {
            dustParticles.Play();  // Play dust particles during movement
        }
    }

    public void StopDustParticles()
    {
        if (dustParticles != null && dustParticles.isPlaying)
        {
            dustParticles.Stop();  // Stop dust particles when idle
        }
    }
}
