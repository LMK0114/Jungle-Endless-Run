using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : MonoBehaviour
{
    [SerializeField] public float moveSpeed = 10;
    [SerializeField] public float sideSpeed = 9;

    // Track positions (X coordinates)
    private float[] trackPositions = { -2f, 0f, 2f };

    [SerializeField] public int currentTrackIndex = 1; // Start at middle track (index 1 = position 0)
    [SerializeField] public bool isMoving = false;
    [SerializeField] public bool isInvincible = false;
    [SerializeField] int moveDirection; // 1 = left, 2 = right
    [SerializeField] int targetTrackIndex;

    [SerializeField] GameObject playerAnimation;

    [SerializeField] private PlayerMagnet playerMagnet;
    [SerializeField] private PlayerBoost playerBoost;

    [SerializeField] AudioSource collisionFX;

    [SerializeField] GameObject GameOver;


    public bool isJumping = false;
    public bool comingdown = false;
    public GameObject playerObject;

    // Score System
    public TMP_Text scoreText;
    int score = 0;
    private Vector3 startPosition;
    private float startZ; // Get Player Start Position for Z

    // Mobile Control
    Vector2 inputStart;
    Vector2 inputEnd;
    bool isDragging;
    float swipeThreshold;

    private void OnTriggerEnter(Collider other)
    {
        // Check if we're invincible from boost
        if (playerBoost != null && playerBoost.IsInvincible)
        {
            // Check what type of obstacle we hit
            if (other.CompareTag("Obstacle") || other.CompareTag("ObstacleAnySide"))
            {
                Debug.Log("Boost invincibility! Obstacle ignored.");

                // Don't process collision - player is invincible!
                return;
            }
        }

        // If not invincible, proceed with normal collision handling
        if (other.CompareTag("Obstacle"))
        {
            HandleFrontOnlyObstacle(other);
            StartCoroutine(BackScene());
        }
        else if (other.CompareTag("ObstacleAnySide"))
        {
            HandleAnySideObstacle();
            StartCoroutine(BackScene());
        }
    }

    private void HandleFrontOnlyObstacle(Collider obstacle)
    {
        // Get the direction from player to obstacle
        Vector3 toObstacle = obstacle.transform.position - transform.position;

        // Calculate the angle between player's forward and direction to obstacle
        float angle = Vector3.Angle(transform.forward, toObstacle);

        // If the obstacle is within 45 degrees in front of the player
        if (angle <= 45f)
        {
            collisionFX.Play();
            // FRONT COLLISION - STOP THE GAME
            StopPlayer();
            playerAnimation.GetComponent<Animator>().Play("Stunned");
            Debug.Log("Game Over - Hit front-only obstacle from front!");

        }
        else
        {
            // SIDE/TOP/BACK COLLISION - Keep running, but prevent moving through it
            HandleNonFrontCollision(obstacle);
        }
    }

    private void HandleAnySideObstacle()
    {
        collisionFX.Play();
        // ANY COLLISION - STOP THE GAME IMMEDIATELY
        StopPlayer();
        playerAnimation.GetComponent<Animator>().Play("Stunned");
        Debug.Log("Game Over - Hit obstacle (any side)!");
        
    }

    IEnumerator BackScene()
    {
        yield return new WaitForSeconds(3);
        GameOver.SetActive(true);
    }

    private void HandleNonFrontCollision(Collider obstacle)
    {
        // For side collisions during lane switching
        if (isMoving)
        {
            // Calculate which side the obstacle is on
            Vector3 toObstacle = obstacle.transform.position - transform.position;

            // Check if we're moving toward the obstacle
            if (moveDirection == 1) // Moving left
            {
                if (toObstacle.x < 0) // Obstacle is on the left side
                {
                    // Snap back to current track position
                    SnapBackToCurrentTrack();
                    Debug.Log("Blocked by obstacle on left side - snapped back to track");
                }
            }
            else if (moveDirection == 2) // Moving right
            {
                if (toObstacle.x > 0) // Obstacle is on the right side
                {
                    // Snap back to current track position
                    SnapBackToCurrentTrack();
                    Debug.Log("Blocked by obstacle on right side - snapped back to track");
                }
            }
        }
    }

    private void StopPlayer()
    {
        // Stop all movement
        moveSpeed = 0f;
        sideSpeed = 0f;
        isMoving = false;
    }

    private void SnapBackToCurrentTrack()
    {
        // Stop movement
        isMoving = false;
        moveDirection = 0;

        // Snap back to current track position
        transform.position = new Vector3(
            trackPositions[currentTrackIndex],
            transform.position.y,
            transform.position.z
        );
    }

    private void Start()
    {
        swipeThreshold = Screen.width * 0.1f; // 10% screen width
    }

    private void Update()
    {
        // Always move forward (unless game is over with speed = 0)
        transform.Translate(Vector3.forward * Time.deltaTime * moveSpeed, Space.World);

        // INPUT HANDLING - only if not already moving AND still running
        if (!isMoving && moveSpeed > 0)
        {
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            {
                MoveLeft();
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            {
                MoveRight();
            }
            else if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            {
                if (isJumping == false)
                {
                    isJumping = true;
                    StartCoroutine(JumpSequence());
                }
            }

            HandleSwipeInput();
        }

        if (isJumping == true)
        {
            if (comingdown == false)
            {
                transform.Translate(Vector3.up * Time.deltaTime * 5, Space.World);
            }
            if (comingdown == true)
            {
                transform.Translate(Vector3.up * Time.deltaTime * -5, Space.World);
            }
        }

        // MOVEMENT EXECUTION
        if (isMoving && moveSpeed > 0)
        {
            float targetX = trackPositions[targetTrackIndex];

            if (moveDirection == 1) // Moving left
            {
                transform.Translate(Vector3.left * Time.deltaTime * sideSpeed, Space.World);

                // Check if we've reached or passed the target
                if (transform.position.x <= targetX)
                {
                    CompleteMovement();
                }
            }
            else if (moveDirection == 2) // Moving right
            {
                transform.Translate(Vector3.right * Time.deltaTime * sideSpeed, Space.World);

                // Check if we've reached or passed the target
                if (transform.position.x >= targetX)
                {
                    CompleteMovement();
                }
            }
        }

        // Score System
        score = Mathf.FloorToInt(transform.position.z - startZ);

        if (score < 0) score = 0; // safety clamp

        scoreText.text = "Score: " + score;
    }

    IEnumerator JumpSequence()
    {
        yield return new WaitForSeconds(0.45f);
        comingdown = true;
        yield return new WaitForSeconds(0.45f);
        isJumping = false;
        comingdown = false;
    }

    void MoveLeft()
    {
        if (currentTrackIndex > 0) // Can move left if not on leftmost track
        {
            targetTrackIndex = currentTrackIndex - 1;
            isMoving = true;
            moveDirection = 1;
        }
    }

    void MoveRight()
    {
        if (currentTrackIndex < trackPositions.Length - 1) // Can move right if not on rightmost track
        {
            targetTrackIndex = currentTrackIndex + 1;
            isMoving = true;
            moveDirection = 2;
        }
    }

    void CompleteMovement()
    {
        // Snap to exact track position
        transform.position = new Vector3(
            trackPositions[targetTrackIndex],
            transform.position.y,
            transform.position.z
        );

        // Update current track index
        currentTrackIndex = targetTrackIndex;

        // Reset movement state
        isMoving = false;
        moveDirection = 0;
    }

    void HandleSwipeInput()
    {
        // -------- MOBILE --------
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                inputStart = touch.position;
                isDragging = true;
            }

            if (touch.phase == TouchPhase.Ended && isDragging)
            {
                inputEnd = touch.position;
                DetectSwipe();
                isDragging = false;
            }
        }
        // -------- PC --------
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                inputStart = Input.mousePosition;
                isDragging = true;
            }

            if (Input.GetMouseButtonUp(0) && isDragging)
            {
                inputEnd = Input.mousePosition;
                DetectSwipe();
                isDragging = false;
            }
        }
    }

    void DetectSwipe()
    {
        Vector2 delta = inputEnd - inputStart;

        // Ignore tiny movement
        if (delta.magnitude < swipeThreshold)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            if (delta.x > 0)
                MoveRight();
            else
                MoveLeft();
        }
        else
        {
            TryJump(); // Swipe Up or Tap
        }
    }

    void TryJump()
    {
        if (!isJumping)
        {
            isJumping = true;
            StartCoroutine(JumpSequence());
        }
    }

}

