using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BananaController : MonoBehaviour
{
    public GameObject bananaPrefab;  // Reference to the banana prefab
    public float spawnInterval = 10.0f;  // Time between spawns
    private bool isSpawningEnabled = false; // Flag to control spawning

    private void Start()
    {
        // Start spawning bananas if enabled
        if (isSpawningEnabled)
        {
            InvokeRepeating("SpawnBanana", 0f, spawnInterval);
        }
    }

    public void StartSpawning()
    {
        if (!isSpawningEnabled)
        {
            isSpawningEnabled = true;
            InvokeRepeating("SpawnBanana", 0f, spawnInterval);
        }
    }

    private void SpawnBanana()
    {
        if (!isSpawningEnabled) return; // Check if spawning is enabled

        // Instantiate the banana at a random position just outside the camera view
        Vector2 spawnPosition = GetRandomSpawnPosition();
        GameObject banana = Instantiate(bananaPrefab, spawnPosition, Quaternion.identity);

        // Ensure the banana's collider is set to trigger
        Collider2D bananaCollider = banana.GetComponent<Collider2D>();
        if (bananaCollider != null)
        {
            bananaCollider.isTrigger = true;
        }

        // Start moving the banana across the screen
        StartCoroutine(MoveBanana(banana));
    }

    private Vector2 GetRandomSpawnPosition()
    {
        Camera cam = Camera.main;
        float xOffset = cam.orthographicSize * cam.aspect + 1f;
        float yOffset = cam.orthographicSize + 1f;

        // Randomly choose a side of the screen to spawn from
        int side = Random.Range(0, 4);
        switch (side)
        {
            case 0: // Left
                return new Vector2(-xOffset, Random.Range(-yOffset, yOffset));
            case 1: // Right
                return new Vector2(xOffset, Random.Range(-yOffset, yOffset));
            case 2: // Top
                return new Vector2(Random.Range(-xOffset, xOffset), yOffset);
            case 3: // Bottom
                return new Vector2(Random.Range(-xOffset, xOffset), -yOffset);
            default:
                return Vector2.zero;
        }
    }

    private IEnumerator MoveBanana(GameObject banana)
    {
        if (banana == null) yield break;  // Stop if the banana is destroyed

        Vector2 startPosition = banana.transform.position;
        Vector2 centerPosition = Vector2.zero;  // Center of the screen
        Vector2 endPosition = -startPosition;  // Opposite side of the screen

        float durationToCenter = 4.25f;  // Time to reach the center
        float durationFromCenter = 4.25f;  // Time to move out of the screen
        float elapsedTime = 0f;

        // Move towards the center
        while (elapsedTime < durationToCenter)
        {
            if (banana == null) yield break;  // Stop if the banana is destroyed

            banana.transform.position = Vector2.Lerp(startPosition, centerPosition, elapsedTime / durationToCenter);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset elapsed time for the next lerp
        elapsedTime = 0f;

        // Move from the center to the opposite side
        while (elapsedTime < durationFromCenter)
        {
            if (banana == null) yield break;  // Stop if the banana is destroyed

            banana.transform.position = Vector2.Lerp(centerPosition, endPosition, elapsedTime / durationFromCenter);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Destroy the banana when it leaves the screen
        if (banana != null)
        {
            Destroy(banana);
        }
    }
}
