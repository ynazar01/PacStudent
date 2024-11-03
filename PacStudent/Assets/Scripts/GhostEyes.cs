using UnityEngine;

public class GhostEyesFollow : MonoBehaviour
{
    public Transform pacStudent; 
    private Animator eyesAnimator;

    void Start()
    {
        // Fetch Animator component at runtime
        eyesAnimator = GetComponent<Animator>();
        if (eyesAnimator == null)
        {
            Debug.LogError("Animator component is missing on the Eyes GameObject.");
        }
    }

    void Update()
    {
        if (pacStudent != null && eyesAnimator != null)
        {
            // Calculate the direction towards PacStudent
            Vector2 direction = (pacStudent.position - transform.position).normalized;

            // Set Animator parameters to trigger the appropriate animation
            SetDirectionAnimation(direction);
        }
    }

    private void SetDirectionAnimation(Vector2 direction)
    {
        if (eyesAnimator != null)
        {
            // Determine which direction the eyes should look
            if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            {
                // Horizontal direction
                if (direction.x > 0)
                {
                    // Look right
                    eyesAnimator.Play("LookRight");
                }
                else
                {
                    // Look left
                    eyesAnimator.Play("LookLeft");
                }
            }
            else
            {
                // Vertical direction
                if (direction.y > 0)
                {
                    // Look up
                    eyesAnimator.Play("LookUp");
                }
                else
                {
                    // Look down
                    eyesAnimator.Play("LookDown");
                }
            }
        }
    }
}
