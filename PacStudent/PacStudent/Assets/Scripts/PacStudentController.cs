using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacStudentController : MonoBehaviour
{
    public float speed = 5f;  // Movement speed for lerping
    private Vector2 lastInput;  // Last input given by the player
    private Vector2 currentInput;  // Current valid input for movement
    private Vector2 nextPosition;  // Target position to lerp towards
    private bool isLerping = false;  // Check if PacStudent is currently lerping

    private Animator animator;  // Reference to Animator component
    private Vector2 previousPosition;  // Store the previous valid position
    private ParticleEffect particleEffect;  // Reference to ParticleEffect script

    public ParticleSystem deathParticleEffect;  // Reference to the death particle effect

    private bool canTeleport = true;  // Teleport cooldown flag
    public float teleportCooldown = 0.2f;  // Cooldown duration

    public float ghostScaredDuration = 10f;  // Duration of ghost scared state
    private float ghostScaredTimer;  // Internal timer for scared state

    private GhostController[] ghosts;

    public int lives = 3;  // Initial lives
    public Transform respawnPosition;  // Position to respawn PacStudent

    private HUDManager hudManager;

    private bool isRespawning = false;  // Flag to handle respawn logic
    private bool canMove = false;  // Flag to enable or disable movement

    // Add fields for the teleporter GameObjects
    public GameObject teleportLeft;  // Assign in the inspector
    public GameObject teleportRight; // Assign in the inspector

    void Start()
    {
        animator = GetComponent<Animator>();
        particleEffect = GetComponent<ParticleEffect>();  // Get ParticleEffect reference
        nextPosition = transform.position;  // Initialize next position
        previousPosition = nextPosition;  // Initialize previous position

        hudManager = Object.FindFirstObjectByType<HUDManager>();
        if (hudManager == null)
        {
            Debug.LogError("HUDManager not found in the scene. Please add it to display HUD elements.");
        }

        ghosts = Object.FindObjectsByType<GhostController>(FindObjectsSortMode.None);

        if (AudioManager.Instance == null)
        {
            Debug.LogWarning("AudioManager.Instance is null. Ensure AudioManager is in the scene.");
        }

        hudManager?.UpdateLivesUI(lives);  // Initialize lives display
        hudManager?.UpdateScoreUI(GameManager.Instance.GetScore());  // Initialize score display from GameManager
    }

    void Update()
    {
        if (isRespawning || !canMove) return;  // Skip update if PacStudent is respawning or movement is disabled

        GetInput();  // Get player input

        if (!isLerping)
        {
            TryMove();  // Attempt movement if not lerping
        }
        
        UpdateAnimation();  // Update animations based on input
        UpdateGhostScaredTimer();  // Update scared timer if active

        // Update game timer
        if (hudManager != null)
        {
            hudManager.UpdateGameTimerUI(Time.timeSinceLevelLoad);  // Update the game timer in HUD
        }
    }

    void GetInput()
    {
        if (Input.GetKeyDown(KeyCode.W)) lastInput = Vector2.up;
        if (Input.GetKeyDown(KeyCode.S)) lastInput = Vector2.down;
        if (Input.GetKeyDown(KeyCode.A)) lastInput = Vector2.left;
        if (Input.GetKeyDown(KeyCode.D)) lastInput = Vector2.right;
    }

    void TryMove()
    {
        previousPosition = transform.position;  // Store the current position before movement

        if (CanMove((Vector2)transform.position + lastInput))
        {
            currentInput = lastInput;  // Store valid input
            StartLerp((Vector2)transform.position + lastInput);
        }
        else if (CanMove((Vector2)transform.position + currentInput))
        {
            StartLerp((Vector2)transform.position + currentInput);
        }
        else
        {
            HandleWallCollision();  // Play particle effect and sound if hitting a wall
        }
    }

    public bool IsLerping()
    {
        return isLerping;
    }

    bool CanMove(Vector2 targetPos)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, targetPos - (Vector2)transform.position, 1f, LayerMask.GetMask("Walls"));
        return hit.collider == null;
    }

    void StartLerp(Vector2 targetPos)
    {
        nextPosition = targetPos;
        StartCoroutine(LerpToPosition(nextPosition));
    }

    IEnumerator LerpToPosition(Vector2 target)
    {
        isLerping = true;
        float timeElapsed = 0f;
        Vector2 startPos = transform.position;

        while (timeElapsed < 1f / speed)
        {
            transform.position = Vector2.Lerp(startPos, target, timeElapsed * speed);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = target;
        isLerping = false;
    }

    void UpdateAnimation()
    {
        animator.SetBool("isMovingUp", currentInput == Vector2.up);
        animator.SetBool("isMovingDown", currentInput == Vector2.down);
        animator.SetBool("isMovingLeft", currentInput == Vector2.left);
        animator.SetBool("isMovingRight", currentInput == Vector2.right);
    }

    void HandleWallCollision()
    {
        transform.position = previousPosition;  // Revert to the previous valid position
        if (particleEffect != null) particleEffect.PlayCollisionEffect(transform.position);  // Play particle effect if assigned
        if (AudioManager.Instance != null) AudioManager.Instance.PlayWallCollisionSound();
        Debug.Log("PacStudent collided with a wall!");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Pellet"))
        {
            EatPellet(collision.gameObject);
        }
        else if (collision.CompareTag("PowerPellet"))
        {
            EatPowerPellet(collision.gameObject);
        }
        else if (collision.CompareTag("Banana"))
        {
            EatBanana(collision.gameObject);
        }
        else if (collision.CompareTag("Ghost"))
        {
            GhostController ghost = collision.GetComponent<GhostController>();
            if (ghost != null)
            {
                if (ghost.currentState == GhostController.GhostState.Dead)
                {
                    // Ignore collision with dead state ghost
                    return;
                }
                else if (ghost.currentState == GhostController.GhostState.Scared || ghost.currentState == GhostController.GhostState.Recovering)
                {
                    // Handle ghost being eaten by PacStudent
                    ghost.SetGhostState(GhostController.GhostState.Dead);
                    GameManager.Instance.AddScore(300);  // Use GameManager to add score
                }
                else
                {
                    // PacStudent dies if colliding with a non-Scared, non-Recovering ghost
                    HandlePacStudentDeath();
                }
            }
        }
        else if (collision.CompareTag("Teleporter"))
        {
            // Call the teleport method if PacStudent collides with the teleporter
            Teleport(collision.gameObject);
        }
    }

    private void Teleport(GameObject teleporter)
    {
        if (!canTeleport) return; // Prevent teleport if in cooldown

        canTeleport = false;  // Disable teleportation until cooldown is done

        // Determine the target teleporter based on which teleporter was triggered
        if (teleporter != null) // Ensure teleporter is not null
        {
            if (teleporter.name == "TeleportLeft")
            {
                if (teleportRight != null) // Check if the target teleporter exists
                {
                    transform.position = teleportRight.transform.position;  // Teleport to the right
                }
                else
                {
                    Debug.LogError("Target teleporter 'TeleportRight' not found.");
                }
            }
            else if (teleporter.name == "TeleportRight")
            {
                if (teleportLeft != null) // Check if the target teleporter exists
                {
                    transform.position = teleportLeft.transform.position;  // Teleport to the left
                }
                else
                {
                    Debug.LogError("Target teleporter 'TeleportLeft' not found.");
                }
            }
        }
        else
        {
            Debug.LogError("Teleporter reference is null in the teleport method.");
        }

        StartCoroutine(TeleportCooldown());  // Start the cooldown
    }

    IEnumerator TeleportCooldown()
    {
        yield return new WaitForSeconds(teleportCooldown);  // Wait for cooldown
        canTeleport = true;  // Enable teleportation again
    }

    void EatPellet(GameObject pellet)
    {
        Destroy(pellet);
        GameManager.Instance.AddScore(10);  // Use GameManager to add score
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPelletSound();
        GameManager.Instance.PelletCollected();  // Inform GameManager of pellet collection
        Debug.Log("Pellet eaten!");
    }

    void EatPowerPellet(GameObject powerPellet)
    {
        Destroy(powerPellet);
        GameManager.Instance.AddScore(50);  // Use GameManager to add score
        if (AudioManager.Instance != null) AudioManager.Instance.PlayPowerPelletSound();
        TriggerGhostScaredState();
        GameManager.Instance.PelletCollected();  // Inform GameManager of pellet collection
        Debug.Log("Power pellet eaten! Ghosts are now frightened.");
    }

    void EatBanana(GameObject banana)
    {
        Destroy(banana);
        GameManager.Instance.AddScore(100);  // Use GameManager to add score
        if (AudioManager.Instance != null) AudioManager.Instance.PlayBananaSound();
        Debug.Log("Banana eaten!");
    }

    void TriggerGhostScaredState()
    {
        ghostScaredTimer = ghostScaredDuration;
        foreach (GhostController ghost in ghosts)
        {
            ghost?.SetGhostState(GhostController.GhostState.Scared);
        }
    }

    void SetGhostsToRecovering()
    {
        foreach (GhostController ghost in ghosts)
        {
            if (ghost.currentState == GhostController.GhostState.Scared)
            {
                ghost.SetGhostState(GhostController.GhostState.Recovering);
                ghost.GetComponent<Animator>().SetBool("isRecovering", true); // Update IsRecovering parameter
            }
        }
    }

    void ResetGhosts()
    {
        foreach (GhostController ghost in ghosts)
        {
            ghost.SetGhostState(GhostController.GhostState.Walking);
            ghost.GetComponent<Animator>().SetBool("isRecovering", false); // Reset IsRecovering parameter
        }
    }

    void UpdateGhostScaredTimer()
    {
        if (ghostScaredTimer > 0)
        {
            ghostScaredTimer -= Time.deltaTime;
            hudManager?.UpdateGhostTimerUI(ghostScaredTimer);

            if (ghostScaredTimer <= 3 && ghostScaredTimer > 0)
            {
                SetGhostsToRecovering();
            }
            else if (ghostScaredTimer <= 0)
            {
                ResetGhosts();
                hudManager?.UpdateGhostTimerUI(0);
            }
        }
    }

    void HandlePacStudentDeath()
    {
        if (isRespawning) return;  // Prevent multiple respawn actions
        isRespawning = true;

        lives--;
        hudManager?.UpdateLivesUI(lives);

        // Set the isDead parameter to true to trigger the death animation
        animator.SetBool("isDead", true);

        // Trigger the particle effect on death
        if (deathParticleEffect != null)
        {
            Instantiate(deathParticleEffect, transform.position, Quaternion.identity);
        }

        if (lives > 0)
        {
            StartCoroutine(RespawnPacStudent());
        }
        else
        {
            Debug.Log("Game Over!");
            GameManager.Instance.GameOver();  // Trigger Game Over in GameManager
        }
    }

    IEnumerator RespawnPacStudent()
    {
        yield return new WaitForSeconds(1.0f);  // Optional delay before respawn
        transform.position = respawnPosition.position;

        lastInput = Vector2.zero;
        currentInput = Vector2.zero;
        isLerping = false;  // Stop any lerp movement
        isRespawning = false;  // Allow updates again

        // Reset isDead parameter after respawn
        animator.SetBool("isDead", false);

        UpdateAnimation();  // Reset to idle animation
        if (AudioManager.Instance != null) AudioManager.Instance.PlayDeathSound();
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
}
