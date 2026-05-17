using UnityEngine;

public class Spike : MonoBehaviour
{
    [SerializeField] private int damage = 1;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        TryDamage(collision.collider);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // Dikenlerin üstünde durursa da hasar almaya devam etsin
        // (invincibility frames sayesinde spam olmuyor)
        TryDamage(collision.collider);
    }

    private void TryDamage(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        PlayerHealth health = other.GetComponent<PlayerHealth>();
        if (health != null)
        {
            health.TakeDamage(damage, transform.position);
        }
    }
}