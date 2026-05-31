using UnityEngine;

public class BoostPowerup : MonoBehaviour
{
    [Header("Boost Settings")]
    [SerializeField] private float boostDuration = 3f;
    [SerializeField] private float boostSpeedMultiplier = 10f;
    [SerializeField] private float boostDistance = 50f;

    [Header("Effects")]
    [SerializeField] private GameObject boostModel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerBoost playerBoost = other.GetComponent<PlayerBoost>();
            if (playerBoost != null && !playerBoost.IsBoosting)
            {
                // Activate boost on player
                playerBoost.ActivateBoost(boostDuration, boostSpeedMultiplier, boostDistance);

                Destroy(gameObject);
            }
        }
    }
}