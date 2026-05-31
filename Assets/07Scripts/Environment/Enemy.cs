using UnityEngine;
using System.Collections;

public class EnemyChaser : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMove playerMoveScript;
    [SerializeField] private PlayerBoost playerBoostScript; // To detect player boost

    [Header("Chase Settings")]
    [SerializeField] private float chaseSpeed = 6f;
    [SerializeField] private float startChasingDistance = 15f;
    [SerializeField] private float stopChasingDistance = 30f;

    [Header("Boost Settings")] // NEW SECTION
    [SerializeField] private bool matchPlayerBoost = true;
    [SerializeField] private float boostSpeedMultiplier = 1.8f;
    [SerializeField] private float teleportDistance = 10f;
    [SerializeField] private float maxDistanceBehind = 20f;
    [SerializeField] private float catchUpDistance = 3f; // How close to stay behind player

    [Header("Track Following")]
    [SerializeField] private float[] trackPositions = { -2f, 0f, 2f };
    [SerializeField] private float laneChangeSpeed = 8f;

    [Header("Jump Matching")]
    [SerializeField] private bool matchPlayerJumps = true;
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private float gravity = -20f;

    [Header("Collision Settings")]
    [SerializeField] private float collisionStopTime = 2f;
    [SerializeField] private bool destroyOnCollision = false;
    [SerializeField] private GameObject collisionEffect;

    [SerializeField] GameObject EnemyAnimation;

    private int currentTrackIndex = 1;
    private bool isChangingLanes = false;
    private int targetTrackIndex;

    // Jump variables
    private bool isGrounded = true;
    private float verticalVelocity = 0f;
    private float groundY = 0f;

    // Collision variables
    private bool isStopped = false;
    private bool canChase = true;

    // Boost variables - NEW
    private float originalChaseSpeed;
    private bool isBoosting = false;
    private float boostTimer = 0f;
    private bool playerWasBoosting = false;

    private void Start()
    {
        // Auto-find player if not assigned
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                playerMoveScript = playerObj.GetComponent<PlayerMove>();
                playerBoostScript = playerObj.GetComponent<PlayerBoost>();
            }
        }

        groundY = transform.position.y;
        originalChaseSpeed = chaseSpeed;
    }

    private void Update()
    {
        if (player == null || isStopped) return;

        float distanceToPlayer = Vector3.Distance(
            new Vector3(0, 0, transform.position.z),
            new Vector3(0, 0, player.position.z)
        );

        //Check if player JUST ended a boost
        CheckForBoostEnd();

        //Check player boost status
        CheckPlayerBoostStatus();

        //Check if falling too far behind
        if (matchPlayerBoost && distanceToPlayer > maxDistanceBehind && canChase)
        {
            TeleportToCatchUp();
        }

        // Only chase if player is within range and can chase
        if (distanceToPlayer <= startChasingDistance && canChase)
        {
            ChasePlayer();
        }
        else if (distanceToPlayer > stopChasingDistance && canChase)
        {
            // Move forward slowly when far from player
            transform.Translate(Vector3.forward * chaseSpeed * 0.5f * Time.deltaTime, Space.World);
        }

        //Always try to stay visible on screen
        EnsureVisibility();
    }

    //Detect when player boost ends and adjust enemy behavior
    private void CheckForBoostEnd()
    {
        if (playerBoostScript == null) return;

        // Check if player WAS boosting but now ISN'T
        if (playerWasBoosting && !playerBoostScript.IsBoosting)
        {
            OnPlayerBoostEnd();
        }

        // Update tracking
        playerWasBoosting = playerBoostScript.IsBoosting;
    }

    //Handle player boost ending
    private void OnPlayerBoostEnd()
    {
        Debug.Log("Player boost ended - adjusting enemy behavior");

        // Option 1: Slow down enemy immediately
        if (isBoosting)
        {
            EndBoost();
        }

        // Option 2: Teleport enemy further back
        float safeDistance = 15f; // How far behind to place enemy
        float newZ = player.position.z - safeDistance;

        transform.position = new Vector3(
            transform.position.x,
            groundY,
            Mathf.Min(newZ, transform.position.z) // Don't teleport forward, only backward
        );

        // Option 3: Temporarily slow enemy chase
        StartCoroutine(TemporarySlowdown(2f)); // Slow for 2 seconds
    }

    //Temporary slowdown after player boost ends
    private IEnumerator TemporarySlowdown(float duration)
    {
        float originalSpeed = chaseSpeed;
        chaseSpeed = originalSpeed * 0.5f; // Slow down to 50% speed

        yield return new WaitForSeconds(duration);

        // Gradually return to normal speed
        float returnTime = 1f;
        float elapsedTime = 0f;

        while (elapsedTime < returnTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / returnTime;
            chaseSpeed = Mathf.Lerp(chaseSpeed, originalSpeed, t);
            yield return null;
        }

        chaseSpeed = originalSpeed;
    }

    //Check if player is boosting
    private void CheckPlayerBoostStatus()
    {
        if (playerBoostScript == null || !matchPlayerBoost) return;

        // Check if player just started boosting
        if (playerBoostScript.IsBoosting && !playerWasBoosting && !isBoosting)
        {
            StartBoost(3f); // Default 3 seconds if we can't get exact duration
            playerWasBoosting = true;
        }
        // Check if player stopped boosting
        else if (!playerBoostScript.IsBoosting && playerWasBoosting)
        {
            playerWasBoosting = false;
        }
    }

    private void ChasePlayer()
    {
        // Calculate current speed (boosted or normal)
        float currentSpeed = isBoosting ? originalChaseSpeed * boostSpeedMultiplier : chaseSpeed;

        // 1. Always move forward
        transform.Translate(Vector3.forward * currentSpeed * Time.deltaTime, Space.World);

        // 2. Match player's track position
        MatchPlayerTrack();

        // 3. Match player's jump if enabled
        if (matchPlayerJumps && playerMoveScript != null)
        {
            MatchPlayerJump();
        }

        // 4. Apply gravity for jumps
        ApplyGravity();

        // 5. Update boost timer
        if (isBoosting)
        {
            boostTimer -= Time.deltaTime;
            if (boostTimer <= 0)
            {
                EndBoost();
            }
        }
    }

    private void StartBoost(float duration)
    {
        if (!matchPlayerBoost || isBoosting) return;

        isBoosting = true;
        boostTimer = duration;

        // Increase speed
        chaseSpeed = originalChaseSpeed * boostSpeedMultiplier;

        // Visual effects
        StartCoroutine(EnemyBoostEffects());

        Debug.Log($"Enemy started boost for {duration} seconds!");
    }

    private void EndBoost()
    {
        isBoosting = false;
        chaseSpeed = originalChaseSpeed;

        // Fade out boost effects
        StartCoroutine(FadeOutBoostEffects());

        Debug.Log("Enemy boost ended");
    }

    private IEnumerator EnemyBoostEffects()
    {
        Renderer enemyRenderer = GetComponentInChildren<Renderer>();
        Color originalColor = Color.white;

        if (enemyRenderer != null)
        {
            originalColor = enemyRenderer.material.color;

            // Make enemy glow during boost
            while (isBoosting)
            {
                float pulse = Mathf.Sin(Time.time * 5f) * 0.3f + 0.7f;
                enemyRenderer.material.color = Color.Lerp(originalColor, Color.red, pulse);
                yield return null;
            }

            enemyRenderer.material.color = originalColor;
        }
    }

    private IEnumerator FadeOutBoostEffects()
    {
        Renderer enemyRenderer = GetComponentInChildren<Renderer>();
        if (enemyRenderer != null)
        {
            Color boostColor = enemyRenderer.material.color;
            float fadeTime = 0.5f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeTime)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / fadeTime;
                enemyRenderer.material.color = Color.Lerp(boostColor, Color.white, t);
                yield return null;
            }
        }
    }

    // NEW: Simple method to keep enemy on screen
    private void EnsureVisibility()
    {
        if (player == null) return;

        float distanceBehind = player.position.z - transform.position.z;

        // If enemy is getting too far behind, speed up a bit
        if (distanceBehind > maxDistanceBehind * 0.7f && !isBoosting)
        {
            chaseSpeed = originalChaseSpeed * 1.3f; // 30% speed boost to catch up
        }
        else if (distanceBehind < catchUpDistance && !isBoosting)
        {
            // If too close, slow down a bit
            chaseSpeed = originalChaseSpeed * 0.8f;
        }
        else if (!isBoosting)
        {
            // Return to normal speed
            chaseSpeed = originalChaseSpeed;
        }
    }

    // Teleport enemy when too far behind
    private void TeleportToCatchUp()
    {
        if (!matchPlayerBoost || isStopped) return;

        // Check if player is boosting
        bool playerIsBoosting = playerBoostScript != null && playerBoostScript.IsBoosting;

        // If player is boosting, teleport further behind
        // If player just finished boosting, teleport even further behind
        float teleportMultiplier = 1f;

        if (playerIsBoosting)
        {
            teleportMultiplier = 2f; // Stay further behind during boost
        }
        else if (playerWasBoosting && !playerIsBoosting)
        {
            teleportMultiplier = 3f; // Stay much further behind after boost
        }

        float targetZ = player.position.z - (teleportDistance * teleportMultiplier);

        // Don't teleport forward if we're already close enough
        if (transform.position.z > targetZ)
        {
            Debug.Log("Not teleporting - already close enough");
            return;
        }

        transform.position = new Vector3(
            transform.position.x,
            groundY,
            targetZ
        );

        PlayTeleportEffect();
        Debug.Log($"Enemy teleported to safe distance. Z: {targetZ}");
    }

    private void PlayTeleportEffect()
    {
        if (collisionEffect != null)
        {
            GameObject effect = Instantiate(collisionEffect, transform.position, Quaternion.identity);

            ParticleSystem ps = effect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startColor = Color.cyan;
            }

            Destroy(effect, 1f);
        }
    }

    // Existing methods (keep these the same as before)
    private void MatchPlayerTrack()
    {
        if (player == null || isChangingLanes || !canChase) return;

        float playerX = player.position.x;
        int playerTrackIndex = GetClosestTrackIndex(playerX);

        if (playerTrackIndex != currentTrackIndex)
        {
            StartCoroutine(ChangeToTrack(playerTrackIndex));
        }
        else
        {
            float targetX = trackPositions[currentTrackIndex];
            float currentX = transform.position.x;

            if (Mathf.Abs(currentX - targetX) > 0.1f)
            {
                float newX = Mathf.Lerp(currentX, targetX, laneChangeSpeed * Time.deltaTime);
                transform.position = new Vector3(newX, transform.position.y, transform.position.z);
            }
        }
    }

    private IEnumerator ChangeToTrack(int newTrackIndex)
    {
        isChangingLanes = true;
        targetTrackIndex = newTrackIndex;

        float startX = transform.position.x;
        float targetX = trackPositions[newTrackIndex];
        float journeyLength = Mathf.Abs(targetX - startX);
        float startTime = Time.time;

        while (isChangingLanes)
        {
            float distanceCovered = (Time.time - startTime) * laneChangeSpeed;
            float fractionOfJourney = distanceCovered / journeyLength;

            float newX = Mathf.Lerp(startX, targetX, fractionOfJourney);
            transform.position = new Vector3(newX, transform.position.y, transform.position.z);

            if (fractionOfJourney >= 1f)
            {
                currentTrackIndex = targetTrackIndex;
                isChangingLanes = false;
                transform.position = new Vector3(
                    trackPositions[currentTrackIndex],
                    transform.position.y,
                    transform.position.z
                );
            }

            yield return null;
        }
    }

    private int GetClosestTrackIndex(float xPosition)
    {
        int closestIndex = 0;
        float closestDistance = Mathf.Abs(xPosition - trackPositions[0]);

        for (int i = 1; i < trackPositions.Length; i++)
        {
            float distance = Mathf.Abs(xPosition - trackPositions[i]);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
            }
        }

        return closestIndex;
    }

    private void MatchPlayerJump()
    {
        if (playerMoveScript.isJumping && isGrounded)
        {
            Jump();
        }
    }

    private void Jump()
    {
        isGrounded = false;
        verticalVelocity = jumpForce;
    }

    private void ApplyGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity += gravity * Time.deltaTime;
            transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime, Space.World);

            if (transform.position.y <= groundY)
            {
                transform.position = new Vector3(
                    transform.position.x,
                    groundY,
                    transform.position.z
                );
                isGrounded = true;
                verticalVelocity = 0f;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            OnPlayerCollision();
        }
    }

    private void OnPlayerCollision()
    {
        if (isStopped) return;

        Debug.Log("Enemy caught the player!");
        isStopped = true;
        canChase = false;

        if (EnemyAnimation != null)
            EnemyAnimation.GetComponent<Animator>().Play("Zombie Punching");

        if (collisionEffect != null)
        {
            Instantiate(collisionEffect, transform.position, Quaternion.identity);
        }

        if (playerMoveScript != null)
        {
            playerMoveScript.moveSpeed = 0f;
            playerMoveScript.sideSpeed = 0f;
            playerMoveScript.isMoving = false;
        }
    }

    public void StopEnemy(float duration)
    {
        StartCoroutine(ManualStop(duration));
    }

    private IEnumerator ManualStop(float duration)
    {
        isStopped = true;
        canChase = false;
        yield return new WaitForSeconds(duration);
        isStopped = false;
        canChase = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            // Draw chase range
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, startChasingDistance);

            // Draw max distance behind
            Gizmos.color = Color.cyan;
            Vector3 maxBehindPos = new Vector3(
                transform.position.x,
                transform.position.y,
                player.position.z - maxDistanceBehind
            );
            Gizmos.DrawLine(transform.position, maxBehindPos);

            // Draw line to player
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, player.position);

            // Draw catch-up distance
            Gizmos.color = Color.green;
            Vector3 catchUpPos = new Vector3(
                transform.position.x,
                transform.position.y,
                player.position.z - catchUpDistance
            );
            Gizmos.DrawLine(transform.position, catchUpPos);
        }

        // Draw track positions
        Gizmos.color = Color.blue;
        for (int i = 0; i < trackPositions.Length; i++)
        {
            Vector3 trackPos = new Vector3(
                trackPositions[i],
                transform.position.y,
                transform.position.z + 1
            );
            Gizmos.DrawSphere(trackPos, 0.2f);
        }

        // Draw boost status
        if (isBoosting)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.5f);
            Gizmos.DrawSphere(transform.position, 1.5f);
        }
    }
}