using UnityEngine;

/// <summary>
/// Modern 2D Platformer Karakter Kontrolcüsü
/// - WASD ve Ok tuşları ile hareket
/// - Variable jump height (basma süresine göre zıplama yüksekliği)
/// - Coyote time (kenardan düşerken hala zıplayabilme)
/// - Jump buffer (yere değmeden zıplama tuşuna basınca hatırlama)
/// - Double jump (sonradan açılabilir, buff için hazır)
/// - Coin sayacı (CoinPickup script'i ile uyumlu)
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
public class PlayerController : MonoBehaviour
{
    // ========== HAREKET AYARLARI ==========
    [Header("Hareket")]
    [Tooltip("Yatay hareket hızı (birim/saniye)")]
    [SerializeField] private float moveSpeed = 8f;

    [Tooltip("Hareketin hızlanma süresi - 0'a yakın = anında tepki")]
    [SerializeField] private float accelerationTime = 0.05f;

    [Tooltip("Durmadaki yavaşlama süresi")]
    [SerializeField] private float decelerationTime = 0.05f;

    // ========== ZIPLAMA AYARLARI ==========
    [Header("Zıplama")]
    [Tooltip("Maksimum zıplama gücü (uzun basınca)")]
    [SerializeField] private float jumpForce = 16f;

    [Tooltip("Zıplama tuşu erken bırakılırsa hızı bu kadara düşür (variable jump)")]
    [Range(0f, 1f)]
    [SerializeField] private float jumpCutMultiplier = 0.5f;

    [Tooltip("Düşerken yerçekimi ne kadar artsın (daha güzel hisset)")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;

    [Tooltip("Zıplama tuşu bırakıldığında yerçekimi çarpanı")]
    [SerializeField] private float lowJumpGravityMultiplier = 2f;

    // ========== ZEMİN KONTROLÜ ==========
    [Header("Zemin Kontrolü")]
    [Tooltip("Zemin kontrolü için kullanılacak boş GameObject (karakterin ayağına yerleştir)")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Zemin kontrol dairesinin yarıçapı")]
    [SerializeField] private float groundCheckRadius = 0.15f;

    [Tooltip("Hangi layer'lar zemin sayılacak")]
    [SerializeField] private LayerMask groundLayer;

    // ========== HASSASİYET AYARLARI ==========
    [Header("Hassasiyet (Game Feel)")]
    [Tooltip("Zeminden ayrıldıktan sonra kaç saniye hala zıplayabilsin")]
    [SerializeField] private float coyoteTime = 0.1f;

    [Tooltip("Yere değmeden zıplama tuşuna basınca kaç saniye hatırlansın")]
    [SerializeField] private float jumpBufferTime = 0.15f;

    // ========== BUFF / GÜÇLER ==========
    [Header("Buff'lar (sonradan açılabilir)")]
    [Tooltip("Çift zıplama aktif mi? (Coin/buff ile açılabilir)")]
    [SerializeField] private bool doubleJumpEnabled = false;

    // ========== ÖZEL DEĞİŞKENLER ==========
    private Rigidbody2D rb;
    private float horizontalInput;
    private float currentVelocityX; // SmoothDamp için

    private bool isGrounded;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private bool jumpHeld;
    private bool canDoubleJump;

    private float defaultGravityScale;

    // Coin sayacı
    private int coinCount = 0;
    public int CoinCount => coinCount;

    // ========== UNITY METHODLARI ==========
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        defaultGravityScale = rb.gravityScale;

        // Rigidbody ayarlarını platformer için optimize et
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        ReadInput();
        UpdateTimers();
        HandleJump();
        ApplyBetterGravity();
    }

    private void FixedUpdate()
    {
        CheckGround();
        HandleHorizontalMovement();
    }

    // ========== INPUT ==========
    private void ReadInput()
    {
        // GetAxisRaw -> anında tepki (Anında / sert mod için ideal)
        // Hem WASD hem ok tuşlarını otomatik yakalar (Unity'nin "Horizontal" axis'i)
        horizontalInput = Input.GetAxisRaw("Horizontal");

        // Zıplama tuşu: Space veya W veya Yukarı ok
        if (Input.GetKeyDown(KeyCode.Space) ||
            Input.GetKeyDown(KeyCode.W) ||
            Input.GetKeyDown(KeyCode.UpArrow))
        {
            jumpBufferCounter = jumpBufferTime;
        }

        // Zıplama tuşu basılı tutuluyor mu? (variable jump için)
        jumpHeld = Input.GetKey(KeyCode.Space) ||
                   Input.GetKey(KeyCode.W) ||
                   Input.GetKey(KeyCode.UpArrow);

        // Zıplama tuşu bu frame'de bırakıldı mı? (jump cut için)
        if (Input.GetKeyUp(KeyCode.Space) ||
            Input.GetKeyUp(KeyCode.W) ||
            Input.GetKeyUp(KeyCode.UpArrow))
        {
            // Eğer hala yukarı çıkıyorsa zıplamayı kes (kısa zıplama)
            if (rb.linearVelocity.y > 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
            }
        }
    }

    // ========== TİMER'LAR ==========
    private void UpdateTimers()
    {
        // Coyote time: yerden ayrıldıktan sonra geri sayım
        if (isGrounded)
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Jump buffer: zıplama tuşuna bastıktan sonra geri sayım
        if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;
    }

    // ========== ZEMİN KONTROLÜ ==========
private void CheckGround()
{
    bool wasGrounded = isGrounded;

    // Tüm collider'ları yakala (sadece Ground layer değil)
    Collider2D hit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    isGrounded = hit != null;

    // DEBUG: ne oluyor görelim
    Collider2D anyHit = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius);
    if (anyHit != null && !isGrounded)
    {
        Debug.LogWarning($"Çember bir şeye değiyor ama Ground layer'ında değil! Değdiği obje: '{anyHit.gameObject.name}', Layer: '{LayerMask.LayerToName(anyHit.gameObject.layer)}'");
    }
    else if (anyHit == null)
    {
        Debug.Log("Çember hiçbir şeye değmiyor (pozisyon veya collider sorunu)");
    }

    // Yere yeni indi - çift zıplamayı resetle
    if (!wasGrounded && isGrounded)
    {
        canDoubleJump = doubleJumpEnabled;
    }
}

    // ========== YATAY HAREKET ==========
    private void HandleHorizontalMovement()
    {
        float targetSpeed = horizontalInput * moveSpeed;

        // Hızlanıyor mu yoksa yavaşlıyor mu - ona göre süre seç
        float smoothTime = Mathf.Abs(horizontalInput) > 0.01f ? accelerationTime : decelerationTime;

        // SmoothDamp ile pürüzsüz ama hızlı tepki
        float newVelocityX = Mathf.SmoothDamp(
            rb.linearVelocity.x,
            targetSpeed,
            ref currentVelocityX,
            smoothTime
        );

        rb.linearVelocity = new Vector2(newVelocityX, rb.linearVelocity.y);
    }

    // ========== ZIPLAMA ==========
    private void HandleJump()
    {
        // Buffer'da zıplama isteği var ve (zeminde veya coyote time aktif) ise zıpla
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0)
        {
            PerformJump();
            jumpBufferCounter = 0;
            coyoteTimeCounter = 0; // çift tetiklemeyi engelle
        }
        // Havadayız ama çift zıplama hakkımız var
        else if (jumpBufferCounter > 0 && !isGrounded && canDoubleJump && coyoteTimeCounter <= 0)
        {
            PerformJump();
            canDoubleJump = false;
            jumpBufferCounter = 0;
        }
    }

    private void PerformJump()
    {
        // Y hızını sıfırla, sonra zıpla - tutarlı zıplama yüksekliği
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    // ========== DAHA İYİ HİSSEDEN YERÇEKİMİ ==========
    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            // Düşüyoruz - yerçekimini artır (daha snappy hissiyat)
            rb.gravityScale = defaultGravityScale * fallGravityMultiplier;
        }
        else if (rb.linearVelocity.y > 0 && !jumpHeld)
        {
            // Çıkıyoruz ama tuş bırakıldı - yerçekimini biraz artır
            rb.gravityScale = defaultGravityScale * lowJumpGravityMultiplier;
        }
        else
        {
            rb.gravityScale = defaultGravityScale;
        }
    }

    // ========== PUBLIC API (BUFF'LAR İÇİN) ==========
    /// <summary>
    /// Çift zıplama buff'ını aktifleştirir (örn. coin toplandığında)
    /// </summary>
    public void EnableDoubleJump()
    {
        doubleJumpEnabled = true;
        canDoubleJump = true;
    }

    /// <summary>
    /// Çift zıplama buff'ını kapatır
    /// </summary>
    public void DisableDoubleJump()
    {
        doubleJumpEnabled = false;
        canDoubleJump = false;
    }

    /// <summary>
    /// Coin ekler (CoinPickup tarafından çağrılır)
    /// </summary>
    public void AddCoin(int amount = 1)
    {
        coinCount += amount;
        Debug.Log($"Coin toplandı! Toplam: {coinCount}");
        // İleride: UI güncelleme, ses efekti vs. buraya
    }

    // ========== DEBUG GİZMOS ==========
    private void OnDrawGizmosSelected()
    {
        // Editor'de zemin kontrol dairesini göster
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
