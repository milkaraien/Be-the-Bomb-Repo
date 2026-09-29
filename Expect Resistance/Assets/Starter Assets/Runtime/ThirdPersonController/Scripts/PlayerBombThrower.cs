using UnityEngine;

public class PlayerBombThrower : MonoBehaviour
{
    [Header("Префаб и точки")]
    [Tooltip("Префаб бомбы (должен содержать компонент Bomb)")]
    [SerializeField] private GameObject bombPrefab;

    [Tooltip("Точка, из которой вылетает бомба (если не задано, используется позиция игрока)")]
    [SerializeField] private Transform throwPoint;

    [Header("Ограничения и параметры броска")]
    [Tooltip("Максимальная дальность броска в юнитах")]
    [SerializeField] private float maxThrowDistance = 10f;

    [Tooltip("Если true — при клике за пределы радиуса бомба полетит на максимально возможную дистанцию. Если false — бросок вне радиуса запрещен.")]
    [SerializeField] private bool clampToMaxDistance = true;

    [Tooltip("Высота дуги полёта бомбы")]
    [SerializeField] private float arcHeight = 2f;

    [Tooltip("Слои, которые считаются поверхностью/полом для приземления")]
    [SerializeField] private LayerMask groundLayer = ~0; // По умолчанию Все слои

    [Header("Перезарядка (Кулдаун)")]
    [Tooltip("Задержка между бросками в секундах")]
    [SerializeField] private float cooldownDuration = 2f;

    [Header("Настройки взрыва (передаются бомбе)")]
    [Tooltip("Задержка перед взрывом ПОСЛЕ приземления (в секундах)")]
    [SerializeField] private float fuseTime = 1.5f;

    [Tooltip("Радиус поражения взрыва")]
    [SerializeField] private float explosionRadius = 3.5f;

    [Tooltip("Урон от взрыва")]
    [SerializeField] private float explosionDamage = 50f;

    [Tooltip("Физический слой объектов, которым наносится урон")]
    [SerializeField] private LayerMask targetLayer;

    private Camera mainCamera;
    private float nextThrowTime = 0f;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (throwPoint == null)
        {
            throwPoint = transform;
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextThrowTime)
        {
            TryThrowBomb();
        }
    }

    private void TryThrowBomb()
    {
        if (bombPrefab == null)
        {
            Debug.LogWarning("Префаб бомбы не назначен в инспекторе!");
            return;
        }

        if (GetMouseWorldPosition(out Vector3 targetPosition))
        {
            Vector3 originPos = throwPoint.position;

            // Вычисляем расстояние по горизонтали (XZ)
            Vector3 offsetXZ = new Vector3(targetPosition.x - originPos.x, 0, targetPosition.z - originPos.z);
            float distance = offsetXZ.magnitude;

            if (distance > maxThrowDistance)
            {
                if (clampToMaxDistance)
                {
                    // Корректируем точку по XZ и заново запрашиваем высоту пола под ней
                    Vector3 clampedXZ = originPos + offsetXZ.normalized * maxThrowDistance;
                    if (Physics.Raycast(new Vector3(clampedXZ.x, originPos.y + 10f, clampedXZ.z), Vector3.down, out RaycastHit hit, 50f, groundLayer))
                    {
                        targetPosition = hit.point;
                    }
                    else
                    {
                        targetPosition.x = clampedXZ.x;
                        targetPosition.z = clampedXZ.z;
                    }
                }
                else
                {
                    return;
                }
            }

            // Создаём бомбу
            GameObject bombObj = Instantiate(bombPrefab, originPos, Quaternion.identity);
            Bomb bombScript = bombObj.GetComponent<Bomb>();

            if (bombScript != null)
            {
                bombScript.Initialize(
                    targetPosition,
                    arcHeight,
                    fuseTime,
                    explosionRadius,
                    explosionDamage,
                    targetLayer
                );

                nextThrowTime = Time.time + cooldownDuration;
            }
            else
            {
                Debug.LogError("На префабе бомбы отсутствует скрипт Bomb!");
            }
        }
    }

    private bool GetMouseWorldPosition(out Vector3 position)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        // Пускаем луч из камеры в физический пол
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            position = hit.point;
            return true;
        }

        // Запасной вариант через математическую плоскость, если луч не попал в коллайдер
        Plane plane = new Plane(Vector3.up, throwPoint.position);
        if (plane.Raycast(ray, out float enter))
        {
            position = ray.GetPoint(enter);
            return true;
        }

        position = Vector3.zero;
        return false;
    }

    private void OnDrawGizmosSelected()
    {
        Transform origin = throwPoint != null ? throwPoint : transform;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin.position, maxThrowDistance);
    }
}