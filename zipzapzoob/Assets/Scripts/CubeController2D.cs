using UnityEngine;

public class CubeController2D : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    [SerializeField] private float moveSpeed = 10f;
    [SerializeField] private float accelTime = 0.05f;        // Tam hıza ulaşma süresi (saniye) - çok düşük = sıkı
    [SerializeField] private float decelTime = 0.05f;        // Tuşu bırakınca durma süresi
    [SerializeField] private float airControl = 1f;          // 1 = havada yerdekiyle aynı kontrol (Celeste tarzı)

    [Header("Zıplama Ayarları")]
    [SerializeField] private float jumpForce = 15f;          // Anında uygulanan zıplama gücü
    [SerializeField] private float jumpCutMultiplier = 0.5f; // Tuş erken bırakılırsa hız bununla çarpılır

    [Header("Yerçekimi His Ayarları")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float maxFallSpeed = 20f;

    [Header("His Detayları")]
    [SerializeField] private float coyoteTime = 0.1f;
    [SerializeField] private float jumpBufferTime = 0.1f;

    [Header("Zemin Kontrolü")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;

    private Rigidbody2D rb;
    private float horizontalInput;
    private bool isGrounded;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private bool isJumping;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Coyote time
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            isJumping = false;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Jump buffer
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

        // ANINDA zıplama (şarj yok)
        if (jumpBufferCounter > 0f && coyoteCounter > 0f)
        {
            Jump();
        }

        // Jump cut: Tuş erken bırakılırsa zıplamayı kısalt (variable jump height)
        if ((Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.Space)) 
            && rb.linearVelocity.y > 0 && isJumping)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * jumpCutMultiplier);
        }
    }

    void FixedUpdate()
    {
        HandleMovement();
        ApplyBetterGravity();
    }

    private void HandleMovement()
    {
        float targetSpeed = horizontalInput * moveSpeed;
        
        // Çok kısa accel/decel süreleri ile neredeyse anında hıza ulaşma
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) 
            ? moveSpeed / accelTime 
            : moveSpeed / decelTime;
        
        // Havadaysa airControl çarpanını uygula (1 = yerdekiyle aynı)
        if (!isGrounded)
            accelRate *= airControl;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = Mathf.Sign(speedDiff) * Mathf.Min(Mathf.Abs(speedDiff), accelRate * Time.fixedDeltaTime);
        
        rb.linearVelocity = new Vector2(rb.linearVelocity.x + movement, rb.linearVelocity.y);
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        coyoteCounter = 0f;
        jumpBufferCounter = 0f;
        isJumping = true;
    }

    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }

        if (rb.linearVelocity.y < -maxFallSpeed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, -maxFallSpeed);
        }
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}