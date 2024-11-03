using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GhostController : MonoBehaviour
{
    public enum GhostType { AvoidPacStudent, ChasePacStudent, Random, FollowWall };
    public GhostType ghostType;

    public float speed = 4f;
    public Transform homePosition;
    public Transform pacStudent;

    private Vector2 direction;
    private Vector2 previousDirection = Vector2.zero;
    private Rigidbody2D rb;

    public enum GhostState { Walking, Scared, Recovering, Dead };
    public GhostState currentState = GhostState.Walking;

    private Animator animator;

    public float scaredDuration = 10.0f;
    public float recoveringDuration = 3.0f;

    private Coroutine stateCoroutine;
    private bool canMove = false;
    private bool isRespawning = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        SetInitialDirection();
    }

    void Update()
    {
        if (!canMove) return;

        if (currentState == GhostState.Dead && !isRespawning)
        {
            MoveTowards(homePosition.position);

            if (Vector2.Distance(transform.position, homePosition.position) < 0.1f)
            {
                isRespawning = true;
                StartCoroutine(RespawnCountdown());
            }
        }
        else
        {
            Move();
            UpdateAnimation();
        }
    }

    private void Move()
    {
        if (currentState == GhostState.Dead) return;

        if (HitWall() || AtIntersection())
        {
            ChooseNewDirection();
        }

        Vector2 newPosition = rb.position + direction * speed * Time.deltaTime;
        rb.MovePosition(newPosition);
    }

    private void MoveTowards(Vector3 targetPosition)
    {
        direction = (targetPosition - transform.position).normalized;
        Vector2 newPosition = rb.position + direction * speed * Time.deltaTime;
        rb.MovePosition(newPosition);
    }

    private bool HitWall()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, 1f, LayerMask.GetMask("Walls"));
        return hit.collider != null;
    }

    private bool AtIntersection()
    {
        int openPaths = 0;
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        foreach (var dir in directions)
        {
            if (!HitWallInDirection(dir))
            {
                openPaths++;
            }
        }
        return openPaths > 2;
    }

    private bool HitWallInDirection(Vector2 dir)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, 1f, LayerMask.GetMask("Walls"));
        return hit.collider != null;
    }

    private void ChooseNewDirection()
    {
        switch (ghostType)
        {
            case GhostType.AvoidPacStudent:
                direction = GetFurthestDirection();
                break;
            case GhostType.ChasePacStudent:
                direction = GetClosestDirection();
                break;
            case GhostType.Random:
                direction = GetRandomDirection();
                break;
            case GhostType.FollowWall:
                direction = FollowWallClockwise();
                break;
        }
    }

    private Vector2 GetFurthestDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 bestDirection = Vector2.zero;
        float maxDistance = 0f;

        foreach (var dir in directions)
        {
            float distance = Vector2.Distance((Vector2)pacStudent.position, rb.position + dir);
            if (distance > maxDistance && dir != -previousDirection && !HitWallInDirection(dir))
            {
                maxDistance = distance;
                bestDirection = dir;
            }
        }

        previousDirection = direction;
        return bestDirection;
    }

    private Vector2 GetClosestDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        Vector2 bestDirection = Vector2.zero;
        float minDistance = float.MaxValue;

        foreach (var dir in directions)
        {
            float distance = Vector2.Distance((Vector2)pacStudent.position, rb.position + dir);
            if (distance < minDistance && dir != -previousDirection && !HitWallInDirection(dir))
            {
                minDistance = distance;
                bestDirection = dir;
            }
        }

        previousDirection = direction;
        return bestDirection;
    }

    private Vector2 GetRandomDirection()
    {
        Vector2[] directions = { Vector2.up, Vector2.down, Vector2.left, Vector2.right };
        List<Vector2> validDirections = new List<Vector2>();

        foreach (var dir in directions)
        {
            if (!HitWallInDirection(dir) && dir != -previousDirection)
            {
                validDirections.Add(dir);
            }
        }

        if (validDirections.Count > 0)
        {
            direction = validDirections[Random.Range(0, validDirections.Count)];
        }

        previousDirection = direction;
        return direction;
    }

    private Vector2 FollowWallClockwise()
    {
        Vector2[] clockwiseDirections = { Vector2.right, Vector2.down, Vector2.left, Vector2.up };

        foreach (var dir in clockwiseDirections)
        {
            if (!HitWallInDirection(dir) && dir != -previousDirection)
            {
                previousDirection = direction;
                return dir;
            }
        }

        // If all directions are blocked, keep moving in the same direction
        return direction;
    }

    private void SetInitialDirection()
    {
        direction = GetRandomDirection();
    }

    public void ResetToSpawn()
    {
        transform.position = homePosition.position;
        SetInitialDirection();
        SetGhostState(GhostState.Walking);
    }

    public void SetGhostState(GhostState newState)
    {
        currentState = newState;
        isRespawning = false;

        switch (newState)
        {
            case GhostState.Walking:
                speed = 4f;
                animator.SetBool("isNormal", true);
                animator.SetBool("isDead", false);
                AudioManager.Instance.PlayGhostMusic("normal");
                break;
            case GhostState.Scared:
                speed = 2f;
                animator.SetBool("isScared", true);
                AudioManager.Instance.PlayGhostMusic("scared");
                break;
            case GhostState.Recovering:
                speed = 3f;
                animator.SetBool("isRecovering", true);
                break;
            case GhostState.Dead:
                speed = 6f;
                animator.SetBool("isDead", true);
                AudioManager.Instance.PlayGhostMusic("dead");
                break;
        }

        UpdateAnimation();
    }

    private IEnumerator RespawnCountdown()
    {
        yield return new WaitForSeconds(5.0f);
        SetGhostState(GhostState.Walking);
        SetInitialDirection();
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isScared", currentState == GhostState.Scared);
            animator.SetBool("isDead", currentState == GhostState.Dead);
            animator.SetBool("isRecovering", currentState == GhostState.Recovering);
        }
    }

    public void SetMovementEnabled(bool enabled)
    {
        canMove = enabled;
    }
}
