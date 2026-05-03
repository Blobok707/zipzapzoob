using UnityEngine;

public class CameraFollow2D : MonoBehaviour
{
    [Header("Hedef")]
    [SerializeField] private Transform target;

    [Header("Takip Ayarları")]
    [SerializeField] private float smoothTime = 0.2f;        // Düşük = sıkı takip, Yüksek = gevşek takip
    [SerializeField] private Vector2 offset = Vector2.zero;  // Hedefe göre kamera ofseti
    [SerializeField] private float zPosition = -10f;         // 2D için kamera Z değeri

    [Header("Look Ahead (İsteğe Bağlı)")]
    [SerializeField] private bool useLookAhead = true;
    [SerializeField] private float lookAheadDistance = 2f;   // Hareket yönüne ne kadar bakacak
    [SerializeField] private float lookAheadSmoothTime = 0.5f;

    [Header("Sınırlar (İsteğe Bağlı)")]
    [SerializeField] private bool useBounds = false;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private Vector3 velocity = Vector3.zero;
    private Vector2 currentLookAhead = Vector2.zero;
    private Vector2 lookAheadVelocity = Vector2.zero;
    private Vector3 lastTargetPosition;

    void Start()
    {
        if (target != null)
        {
            lastTargetPosition = target.position;
            // Başlangıçta kamerayı direkt hedefe yerleştir (ilk frame'de süzülme olmasın)
            transform.position = new Vector3(target.position.x + offset.x, target.position.y + offset.y, zPosition);
        }
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Hedefin hareket yönünü hesapla (look ahead için)
        Vector2 targetVelocity = ((Vector2)target.position - (Vector2)lastTargetPosition) / Time.deltaTime;
        lastTargetPosition = target.position;

        // Look ahead: Hedef hareket ettiği yöne doğru kamerayı kaydır
        Vector2 targetLookAhead = Vector2.zero;
        if (useLookAhead && targetVelocity.sqrMagnitude > 0.01f)
        {
            targetLookAhead = new Vector2(Mathf.Sign(targetVelocity.x) * lookAheadDistance, 0f);
        }
        currentLookAhead = Vector2.SmoothDamp(currentLookAhead, targetLookAhead, ref lookAheadVelocity, lookAheadSmoothTime);

        // Hedef pozisyonu hesapla
        Vector3 desiredPosition = new Vector3(
            target.position.x + offset.x + currentLookAhead.x,
            target.position.y + offset.y + currentLookAhead.y,
            zPosition
        );

        // Sınırları uygula (varsa)
        if (useBounds)
        {
            desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
            desiredPosition.y = Mathf.Clamp(desiredPosition.y, minBounds.y, maxBounds.y);
        }

        // Yumuşak takip
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    // Editor'de sınırları görselleştir
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.yellow;
            Vector3 center = new Vector3((minBounds.x + maxBounds.x) / 2, (minBounds.y + maxBounds.y) / 2, 0);
            Vector3 size = new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 0);
            Gizmos.DrawWireCube(center, size);
        }
    }
}