using UnityEngine;

public class EnemyShooter : MonoBehaviour
{
    [Header("Префаб и точка спавна")]
    [Tooltip("Префаб пули (должен содержать компонент EnemyBullet)")]
    [SerializeField] private GameObject bulletPrefab;

    [Tooltip("Точка, из которой вылетает пуля (дуло оружия)")]
    [SerializeField] private Transform firePoint;

    [Tooltip("Тег объекта игрока")]
    [SerializeField] private string playerTag = "Player";

    [Header("Параметры стрельбы")]
    [Tooltip("Скорость полёта пули")]
    [SerializeField] private float bulletSpeed = 15f;

    [Tooltip("Урон от одной пули")]
    [SerializeField] private float bulletDamage = 15f;

    [Tooltip("Перезарядка между выстрелами в секундах")]
    [SerializeField] private float cooldownDuration = 2f;

    [Tooltip("Разброс стрельбы в градусах (0 = идеально точно)")]
    [SerializeField] private float spreadAngle = 5f;

    private float nextFireTime = 0f;

    private void Awake()
    {
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    /// <summary>
    /// Вызывается из EnemyAI, когда враг видит цель
    /// </summary>
    public void ShootAtTarget(Vector3 targetPosition)
    {
        if (Time.time < nextFireTime || bulletPrefab == null) return;

        // Направление на цель
        Vector3 targetDirection = (targetPosition - firePoint.position).normalized;

        // Накладываем случайный разброс
        Quaternion baseRotation = Quaternion.LookRotation(targetDirection);
        float randomSpreadX = Random.Range(-spreadAngle, spreadAngle);
        float randomSpreadY = Random.Range(-spreadAngle, spreadAngle);
        Quaternion spreadRotation = Quaternion.Euler(randomSpreadX, randomSpreadY, 0f);

        Quaternion finalRotation = baseRotation * spreadRotation;

        // Спавним пулю
        GameObject bulletObj = Instantiate(bulletPrefab, firePoint.position, finalRotation);

        EnemyBullet bulletScript = bulletObj.GetComponent<EnemyBullet>();
        if (bulletScript != null)
        {
            bulletScript.Initialize(bulletSpeed, bulletDamage, playerTag);
        }

        nextFireTime = Time.time + cooldownDuration;
    }
}