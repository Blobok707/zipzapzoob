using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton: Sahnede tek bir GameManager olur, her yerden GameManager.Instance ile erişilir
    public static GameManager Instance { get; private set; }

    [Header("Coin")]
    [SerializeField] private int coinCount = 0;

    private void Awake()
    {
        // Eğer başka bir GameManager varsa bunu sil (duplikasyon önleme)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void AddCoin(int amount)
    {
        coinCount += amount;
        Debug.Log("Coin toplandı! Toplam: " + coinCount);
    }

    public int GetCoinCount()
    {
        return coinCount;
    }
    public void OnPlayerDeath()
    {
    Debug.Log("Game Over!");
    // Şimdilik sahneyi yeniden yükle (basit respawn)
    UnityEngine.SceneManagement.SceneManager.LoadScene(
        UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
    );
    }
}