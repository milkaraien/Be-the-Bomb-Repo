using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class EnemyBullet : MonoBehaviour
{
    [Header("Настройки пули")]
    [Tooltip("Время жизни пули в секундах (автоуничтожение, если ни во что не попала)")]
    [SerializeField] private float lifeTime = 5f;

    [Tooltip("Префаб эффекта попадания/взрыва пули (опционально)")]
    [SerializeField] private GameObject hitEffectPrefab;

    private float speed;
    private float damage;
    private string targetPlayerTag;
    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // Настройки физики пули для предотвращения пролетания сквозь стены
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous; // Не даёт пуле пролететь сквозь коллайдеры
    }

    public void Initialize(float bulletSpeed, float bulletDamage, string playerTag)
    {
        speed = bulletSpeed;
        damage = bulletDamage;
        targetPlayerTag = playerTag;

        // Задаем физическую скорость полёта вперед
        rb.linearVelocity = transform.forward * speed;

        // Автоматическое уничтожение через N секунд, если пуля улетела в пустоту
        Destroy(gameObject, lifeTime);
    }

    // Обработка касания триггера (если у пули IsTrigger = true)
    private void OnTriggerEnter(Collider other)
    {
        HandleImpact(other.gameObject);
    }

    // Обработка физического столкновения (если IsTrigger = false)
    private void OnCollisionEnter(Collision collision)
    {
        HandleImpact(collision.gameObject);
    }

    private void HandleImpact(GameObject hitObject)
    {
        // Проверяем, попали ли в игрока
        if (hitObject.CompareTag(targetPlayerTag))
        {
            // Пытаемся нанести урон через IDamageable
            IDamageable damageable = hitObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
        }

        // Спавним эффект попадания, если задан
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
        }

        // Пуля уничтожается при ЛЮБОМ столкновении (стена, пол, препятствие, игрок)
        Destroy(gameObject);
    }
}