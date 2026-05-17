using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform target;

    [Header("Follow Settings")]
    [Tooltip("Kameranın oyuncuya ne kadar hızlı yetişeceği. Küçük = yumuşak/gecikmeli, büyük = sert/anında.")]
    [SerializeField] private float smoothSpeed = 5f;

    [Tooltip("Kameranın oyuncuya göre offset'i. Z mutlaka -10 olmalı (2D kamera için).")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);

    [Header("Axis Lock (isteğe bağlı)")]
    [SerializeField] private bool followX = true;
    [SerializeField] private bool followY = true;

    private void LateUpdate()
    {
        if (target == null) return;

        // Hedef pozisyon = oyuncunun pozisyonu + offset
        Vector3 desiredPosition = target.position + offset;

        // Eksen kilitleri (istemediğin eksende kamerayı sabit tut)
        if (!followX) desiredPosition.x = transform.position.x;
        if (!followY) desiredPosition.y = transform.position.y;

        // Yumuşak geçiş — Lerp ile kademeli yaklaşma
        Vector3 smoothedPosition = Vector3.Lerp(
            transform.position,
            desiredPosition,
            smoothSpeed * Time.deltaTime
        );

        transform.position = smoothedPosition;
    }
}