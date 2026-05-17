using UnityEngine;

public class Coin : MonoBehaviour
{
    [SerializeField] private int value = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece Player'a değdiğinde tepki ver
        if (other.CompareTag("Player"))
        {
            // GameManager'a coin değerini ilet
            GameManager.Instance.AddCoin(value);

            // Coin'i sahneden sil
            Destroy(gameObject);
        }
    }
}