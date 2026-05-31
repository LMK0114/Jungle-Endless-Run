using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMagnet : MonoBehaviour
{
    [Header("Magnet Settings")]
    [SerializeField] private float magnetRadius = 10f;
    [SerializeField] private float magnetForce = 20f;

    [Header("Coin Collection")]
    [SerializeField] private AudioSource coinCollectSound;

    private bool isMagnetActive = false;
    private float magnetTimer = 0f;
    private float currentMagnetRadius;
    private List<GameObject> coinsInRange = new List<GameObject>();

    private void Update()
    {
        if (isMagnetActive)
        {
            magnetTimer -= Time.deltaTime;

            if (magnetTimer <= 0)
            {
                DeactivateMagnet();
            }
            else
            {
                AttractCoins();
            }
        }
    }

    public void ActivateMagnet(float duration, float radius)
    {
        isMagnetActive = true;
        magnetTimer = duration;
        currentMagnetRadius = radius;

        // Clear previous coin list
        coinsInRange.Clear();

        // Optional: Play activation sound
        // Optional: Show UI notification
        Debug.Log($"Magnet activated for {duration} seconds!");
    }

    private void DeactivateMagnet()
    {
        isMagnetActive = false;
        magnetTimer = 0f;

        coinsInRange.Clear();
        Debug.Log("Magnet deactivated!");
    }

    private void AttractCoins()
    {
        // Find all coins within range
        GameObject[] allCoins = GameObject.FindGameObjectsWithTag("Coin");

        foreach (GameObject coin in allCoins)
        {
            if (coin == null) continue;

            float distance = Vector3.Distance(transform.position, coin.transform.position);

            if (distance <= currentMagnetRadius)
            {
                // Add to list if not already
                if (!coinsInRange.Contains(coin))
                {
                    coinsInRange.Add(coin);
                }

                // Move coin towards player
                Vector3 direction = (transform.position - coin.transform.position).normalized;
                coin.transform.position += direction * magnetForce * Time.deltaTime;

                // Check if coin is very close to player
                if (distance < 0.5f)
                {
                    CollectCoin(coin);
                }
            }
            else
            {
                // Remove from list if out of range
                if (coinsInRange.Contains(coin))
                {
                    coinsInRange.Remove(coin);
                }
            }
        }
    }

    private void CollectCoin(GameObject coin)
    {
        // Increase coin count (using your existing MasterInfo)
        MasterInfo.coinCount += 1;

        // Play sound
        if (coinCollectSound != null)
            coinCollectSound.Play();

        // Remove from list
        if (coinsInRange.Contains(coin))
            coinsInRange.Remove(coin);

        // Deactivate or destroy the coin
        coin.SetActive(false);

        // Optional: Add to object pool instead of destroying
        // Destroy(coin);
    }

    // Visualize magnet radius in editor
    private void OnDrawGizmosSelected()
    {
        if (isMagnetActive)
        {
            Gizmos.color = new Color(0, 1, 0, 0.3f);
            Gizmos.DrawSphere(transform.position, currentMagnetRadius);
        }
    }

    // Public property to check if magnet is active
    public bool IsMagnetActive => isMagnetActive;
    public float RemainingTime => magnetTimer;
}