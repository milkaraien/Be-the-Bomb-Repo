using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bomb : MonoBehaviour
{
    [Header("Эффекты (Опционально)")]
    [Tooltip("Префаб эффекта взрыва (частицы/звук)")]
    [SerializeField] private GameObject explosionEffectPrefab;

    private float fuseTime;
    private float explosionRadius;
    private float damage;
    private LayerMask targetLayer;

    private Rigidbody rb;
    private bool hasLanded = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        // Включаем гравитацию и динамику для честной физики
        rb.useGravity = true;
        rb.isKinematic = false;
    }

    public void Initialize(Vector3 targetPos, float arcHeight, float fuse, float radius, float dmg, LayerMask layer)
    {
        fuseTime = fuse;
        explosionRadius = radius;
        damage = dmg;
        targetLayer = layer;

        // Рассчитываем и прикладываем физический импульс скорости
        Vector3 velocity = CalculateBallisticVelocity(transform.position, targetPos, arcHeight);
        rb.linearVelocity = velocity;
    }

    // Обработка приземления/столкновения с полом через физический Collision
    private void OnCollisionEnter(Collision collision)
    {
        if (!hasLanded)
        {
            hasLanded = true;
            // Как только бомба впервые коснулась поверхности (пола), запускаем таймер взрыва
            StartCoroutine(FuseRoutine());
        }
    }

    // Вычисление баллистической скорости для попадания точно в целевую точку с заданной высотой дуги
    private Vector3 CalculateBallisticVelocity(Vector3 start, Vector3 target, float arcHeight)
    {
        float gravity = Physics.gravity.y;

        // Корректируем начальную высоту дуги относительно максимальной из точек
        float maxHeight = Mathf.Max(start.y, target.y) + arcHeight;
        float displacementY = target.y - start.y;

        // Расчет вертикальной скорости (Vy)
        float initialVelocityY = Mathf.Sqrt(-2 * gravity * (maxHeight - start.y));

        // Расчет времени до вершины дуги и времени падения из вершины до цели
        float timeToApex = initialVelocityY / -gravity;
        float timeFromApexToTarget = Mathf.Sqrt(2 * (target.y - maxHeight) / gravity);
        float totalTime = timeToApex + timeFromApexToTarget;

        // Расчет горизонтальной скорости (Vx, Vz)
        Vector3 displacementXZ = new Vector3(target.x - start.x, 0, target.z - start.z);
        Vector3 initialVelocityXZ = displacementXZ / totalTime;

        return new Vector3(initialVelocityXZ.x, initialVelocityY, initialVelocityXZ.z);
    }

    private IEnumerator FuseRoutine()
    {
        yield return new WaitForSeconds(fuseTime);
        Explode();
    }

    private void Explode()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, targetLayer);

        foreach (Collider hit in hitColliders)
        {
            IDamageable damageable = hit.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(damage);
            }
            else
            {
                hit.gameObject.SendMessage("TakeDamage", damage, SendMessageOptions.DontRequireReceiver);
            }
        }

        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}

// Простой интерфейс урона (создайте отдельно или используйте свои методы)
public interface IDamageable
{
    void TakeDamage(float amount);
}
