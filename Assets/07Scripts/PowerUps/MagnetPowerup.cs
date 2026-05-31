using UnityEngine;

public class MagnetPowerup : MonoBehaviour
{
    [SerializeField] private float magnetDuration = 10f;
    [SerializeField] private float magnetRadius = 15f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMagnet playerMagnet = other.GetComponent<PlayerMagnet>();
            if (playerMagnet != null)
            {
                playerMagnet.ActivateMagnet(magnetDuration, magnetRadius);

                // Visual effects
                StartCoroutine(CollectEffect());
            }
        }
    }

    private System.Collections.IEnumerator CollectEffect()
    {
        // Disable collider and renderer immediately
        GetComponent<Collider>().enabled = false;
        GetComponent<Renderer>().enabled = false;

        // Wait a bit before destroying to allow sound to play
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}