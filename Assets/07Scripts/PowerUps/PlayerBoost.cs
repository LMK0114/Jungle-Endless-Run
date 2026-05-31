using System.Collections;
using UnityEngine;

public class PlayerBoost : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private Collider playerCollider;

    [Header("Boost Settings")]
    [SerializeField] private float defaultMoveSpeed = 10f;
    [SerializeField] private float boostSpeedMultiplier = 10f;
    [SerializeField] private float boostDuration = 3f;
    [SerializeField] private float boostDistance = 50f;
    [SerializeField] private float slowdownDuration = 1f;

    [Header("Visual Effects")]
    private Material originalMaterial;
    private Renderer playerRenderer;

    // State variables
    private bool isBoosting = false;
    private bool isInvincible = false;
    private float originalMoveSpeed;
    private Coroutine boostCoroutine;

    private void Start()
    {
        // Get references if not set
        if (playerMove == null)
            playerMove = GetComponent<PlayerMove>();

        if (playerCollider == null)
            playerCollider = GetComponent<Collider>();

        // Store original speed
        originalMoveSpeed = playerMove.moveSpeed;
        defaultMoveSpeed = originalMoveSpeed;

        // Store original material
        playerRenderer = GetComponentInChildren<Renderer>();
    }

    public void ActivateBoost(float duration, float speedMultiplier, float distance)
    {
        if (isBoosting) return;

        boostDuration = duration;
        boostSpeedMultiplier = speedMultiplier;
        boostDistance = distance;

        boostCoroutine = StartCoroutine(BoostSequence());
    }

    private IEnumerator BoostSequence()
    {
        isBoosting = true;
        isInvincible = true;

        // Store original state
        float startSpeed = playerMove.moveSpeed;
        float targetSpeed = defaultMoveSpeed * boostSpeedMultiplier;

        // PHASE 1: Speed up
        Debug.Log("Boost: Speeding up!");
        StartBoostEffects();

        // Gradually increase speed
        float accelerateTime = 0.5f;
        float elapsedTime = 0f;

        while (elapsedTime < accelerateTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / accelerateTime;
            playerMove.moveSpeed = Mathf.Lerp(startSpeed, targetSpeed, t);
            yield return null;
        }

        playerMove.moveSpeed = targetSpeed;

        // PHASE 2: Maintain boost
        float distanceTraveled = 0f;
        Vector3 startPosition = transform.position;

        while (distanceTraveled < boostDistance && boostDuration > 0)
        {
            // Calculate distance traveled
            distanceTraveled = Vector3.Distance(startPosition, transform.position);

            // Reduce duration
            boostDuration -= Time.deltaTime;

            // Optional: Make player auto-dodge obstacles during boost
            AutoDodgeObstacles();

            yield return null;
        }

        // PHASE 3: Slow down gradually
        Debug.Log("Boost: Slowing down!");

        float slowDownStartSpeed = playerMove.moveSpeed;
        elapsedTime = 0f;

        while (elapsedTime < slowdownDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / slowdownDuration;

            // Ease-out slowdown
            float easedT = 1 - Mathf.Pow(1 - t, 3); // Cubic ease-out

            playerMove.moveSpeed = Mathf.Lerp(slowDownStartSpeed, defaultMoveSpeed, easedT);
            yield return null;
        }

        // PHASE 4: Return to normal
        playerMove.moveSpeed = defaultMoveSpeed;
        isInvincible = false;
        isBoosting = false;

        EndBoostEffects();

        Debug.Log("Boost: Complete!");
    }

    private void AutoDodgeObstacles()
    {
        // Raycast forward to detect obstacles
        float rayDistance = 10f;
        Vector3 rayOrigin = transform.position + Vector3.up * 0.5f;

        // Cast rays for each lane
        float[] lanePositions = { -2f, 0f, 2f };

        foreach (float lanePos in lanePositions)
        {
            Vector3 laneRayOrigin = new Vector3(lanePos, rayOrigin.y, rayOrigin.z);

            if (Physics.Raycast(laneRayOrigin, transform.forward, out RaycastHit hit, rayDistance))
            {
                if (hit.collider.CompareTag("Obstacle") || hit.collider.CompareTag("ObstacleAnySide"))
                {
                    // Find the safest lane (one without obstacles)
                    int safestLane = FindSafestLane();
                    if (safestLane != playerMove.currentTrackIndex)
                    {
                        // Auto-move to safe lane
                        StartCoroutine(AutoChangeLane(safestLane));
                    }
                    break;
                }
            }
        }
    }

    private int FindSafestLane()
    {
        // Simple implementation: check which lanes are clear
        float[] lanePositions = { -2f, 0f, 2f };
        float checkDistance = 15f;
        Vector3 checkOrigin = transform.position + Vector3.up * 0.5f;

        for (int i = 0; i < lanePositions.Length; i++)
        {
            Vector3 laneCheckOrigin = new Vector3(lanePositions[i], checkOrigin.y, checkOrigin.z);

            if (!Physics.Raycast(laneCheckOrigin, transform.forward, checkDistance))
            {
                return i; // This lane is clear
            }
        }

        // If all lanes have obstacles, return current lane
        return playerMove.currentTrackIndex;
    }

    private IEnumerator AutoChangeLane(int targetLane)
    {
        if (playerMove.isMoving) yield break;

        float targetX = targetLane == 0 ? -2f : targetLane == 1 ? 0f : 2f;
        float moveSpeed = 15f; // Faster auto-dodge

        while (Mathf.Abs(transform.position.x - targetX) > 0.1f)
        {
            float direction = Mathf.Sign(targetX - transform.position.x);
            transform.Translate(Vector3.right * direction * moveSpeed * Time.deltaTime, Space.World);
            yield return null;
        }

        // Snap to lane
        transform.position = new Vector3(targetX, transform.position.y, transform.position.z);
        playerMove.currentTrackIndex = targetLane;
    }

    private void StartBoostEffects()
    {

        // Screen effects (optional)
        StartCoroutine(ScreenShake(0.2f, 0.1f));
    }

    private void EndBoostEffects()
    {

        if (playerRenderer != null && originalMaterial != null)
            playerRenderer.material = originalMaterial;
    }



    private IEnumerator ScreenShake(float duration, float magnitude)
    {
        // Simple screen shake effect
        // You might want to use Cinemachine or your camera system instead
        Vector3 originalPos = Camera.main.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            Camera.main.transform.localPosition = originalPos + new Vector3(x, y, 0);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.localPosition = originalPos;
    }

    // Call this from your PlayerMove OnTriggerEnter to prevent damage during boost
    public bool CheckBoostInvincibility(Collider obstacle)
    {
        if (isInvincible)
        {
            // Optional: Play a "deflect" effect
            PlayDeflectEffect(obstacle);
            return true; // Player is invincible
        }
        return false; // Player can take damage
    }

    public void PlayDeflectEffect(Collider obstacle)
    {
        if (!isInvincible) return; // Only play if actually invincible

        Debug.Log("Deflecting obstacle during boost!");

        // Optional: Make obstacle bounce away
        Rigidbody obstacleRb = obstacle.GetComponent<Rigidbody>();
        if (obstacleRb != null)
        {
            Vector3 deflectDirection = (obstacle.transform.position - transform.position).normalized;
            obstacleRb.AddForce(deflectDirection * 10f, ForceMode.Impulse);
        }
        else
        {
            // If no rigidbody, just disable the obstacle temporarily
            StartCoroutine(DisableObstacleTemporarily(obstacle.gameObject));
        }
    }
    private IEnumerator DisableObstacleTemporarily(GameObject obstacle)
    {
        obstacle.SetActive(false);
        yield return new WaitForSeconds(1f);
        obstacle.SetActive(true);
    }

    // Public properties
    public bool IsBoosting => isBoosting;
    public bool IsInvincible => isInvincible;

    // Manual boost activation (for testing or other triggers)
    public void ActivateBoost()
    {
        ActivateBoost(boostDuration, boostSpeedMultiplier, boostDistance);
    }

    // Cancel boost early
    public void CancelBoost()
    {
        if (boostCoroutine != null)
        {
            StopCoroutine(boostCoroutine);
        }

        playerMove.moveSpeed = defaultMoveSpeed;
        isBoosting = false;
        isInvincible = false;

        EndBoostEffects();
    }

    // Debug visualization
    private void OnDrawGizmos()
    {
        if (isBoosting)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, 1f);

            // Draw boost direction
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f); // Orange
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 5f);
        }
    }
}