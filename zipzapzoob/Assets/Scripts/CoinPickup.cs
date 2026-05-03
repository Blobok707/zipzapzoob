using UnityEngine;

/// <summary>
/// Coin / toplanabilir item script'i
/// Kullanım:
/// 1. Bir GameObject'e BoxCollider2D veya CircleCollider2D ekle
/// 2. Collider'ın "Is Trigger" özelliğini aç
/// 3. Bu script'i ekle
/// 4. Oyuncunun tag'inin "Player" olduğundan emin ol
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class CoinPickup : MonoBehaviour
{
    [Header("Coin Ayarları")]
    [Tooltip("Bu coin kaç değerinde?")]
    [SerializeField] private int coinValue = 1;

    [Header("Buff (Opsiyonel)")]
    [Tooltip("Bu coin toplanınca çift zıplama açılsın mı?")]
    [SerializeField] private bool grantsDoubleJump = false;

    [Header("Görsel/Ses (Opsiyonel)")]
    [Tooltip("Toplanınca oynayacak efekt (prefab)")]
    [SerializeField] private GameObject pickupEffect;

    [Tooltip("Toplanınca çalacak ses")]
    [SerializeField] private AudioClip pickupSound;

    private void Awake()
    {
        // Collider'ın trigger olduğundan emin ol
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Sadece oyuncu ile etkileşime gir
        if (!other.CompareTag("Player")) return;

        PlayerController player = other.GetComponent<PlayerController>();
        if (player == null) return;

        // Coin'i ekle
        player.AddCoin(coinValue);

        // Buff ver (eğer ayarlandıysa)
        if (grantsDoubleJump)
        {
            player.EnableDoubleJump();
            Debug.Log("Çift zıplama buff'ı kazanıldı!");
        }

        // Görsel efekt
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        // Ses efekti
        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // Coin'i yok et
        Destroy(gameObject);
    }
}
