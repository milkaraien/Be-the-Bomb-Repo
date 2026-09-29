using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Настройки зрения (FOV)")]
    [Tooltip("Максимальная дистанция зрения")]
    [SerializeField] private float viewDistance = 15f;

    [Tooltip("Угол обзора в градусах (например, 90 = 45 градусов влево и 45 вправо)")]
    [Range(0f, 360f)]
    [SerializeField] private float viewAngle = 90f;

    [Tooltip("Высота, с которой смотрит враг (глаза/центр). Если не задано, используется позиция врага.")]
    [SerializeField] private Transform eyePoint;

    [Tooltip("Слои, которые блокируют зрение врага (стены, укрытия, препятствия)")]
    [SerializeField] private LayerMask obstacleMask;

    [Header("Цель")]
    [Tooltip("Тег объекта игрока")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string shootingPoint = "ShootingPoint";

    [Tooltip("Слой, на котором находится игрок")]
    [SerializeField] private LayerMask playerMask;

    private NavMeshAgent agent;
    private EnemyShooter shooter;
    private Transform playerTransform;

    private Vector3 lastKnownPlayerPosition;
    private bool hasLastKnownPosition = false;
    private bool canSeePlayer = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        shooter = GetComponent<EnemyShooter>();

        if (eyePoint == null)
        {
            eyePoint = transform;
        }

        // Поиск игрока по тегу при старте
        GameObject playerObj = GameObject.FindGameObjectWithTag(shootingPoint);
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        // 1. Проверяем, виден ли игрок прямо сейчас
        canSeePlayer = CheckVisibility();

        if (canSeePlayer)
        {
            // Запоминаем последнюю известную позицию
            lastKnownPlayerPosition = playerTransform.position;
            hasLastKnownPosition = true;

            // Останавливаем движение, чтобы вести огонь
            agent.isStopped = true;

            // Поворачиваемся в сторону игрока
            RotateTowards(playerTransform.position);

            // Вызываем стрельбу из компонента EnemyShooter
            if (shooter != null)
            {
                Debug.Log("Shoot");
                shooter.ShootAtTarget(playerTransform.position);
            }
        }
        else
        {
            // 2. Если игрок не виден, но у нас есть его последняя известная позиция
            if (hasLastKnownPosition)
            {
                agent.isStopped = false;
                agent.SetDestination(lastKnownPlayerPosition);

                // Если пришли в последнюю точку и так и не увидели игрока
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    hasLastKnownPosition = false; // Забываем позицию, возвращаемся в режим ожидания
                }
            }
        }
    }

    /// <summary>
    /// Проверка видимости игрока с учётом угла, дистанции и препятствий (Raycast)
    /// </summary>
    private bool CheckVisibility()
    {
        Vector3 origin = eyePoint.position;
        Vector3 targetPos = playerTransform.position;
        Vector3 directionToPlayer = (targetPos - origin).normalized;

        float distanceToPlayer = Vector3.Distance(origin, targetPos);

        // Проверка 1: Дистанция
        if (distanceToPlayer <= viewDistance)
        {
            // Проверка 2: Угол обзора
            if (Vector3.Angle(transform.forward, directionToPlayer) <= viewAngle / 2f)
            {
                // Проверка 3: Проверка препятствий через Raycast
                // Пускаем луч до игрока. Если он упирается в препятствие раньше, чем долетает до игрока — враг его не видит.
                if (!Physics.Raycast(origin, directionToPlayer, distanceToPlayer, obstacleMask))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Игнорируем наклон по Y, чтобы враг не наклонялся вниз/вверх

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    // Визуализация сектора обзора в Scene View
    private void OnDrawGizmosSelected()
    {
        Transform origin = eyePoint != null ? eyePoint : transform;

        Gizmos.color = canSeePlayer ? Color.red : Color.yellow;
        Gizmos.DrawWireSphere(origin.position, viewDistance);

        // Рисуем крайние лучи угла обзора
        Vector3 leftRayDirection = Quaternion.Euler(0, -viewAngle / 2f, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, viewAngle / 2f, 0) * transform.forward;

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(origin.position, leftRayDirection * viewDistance);
        Gizmos.DrawRay(origin.position, rightRayDirection * viewDistance);

        // Рисуем точку, куда направляется враг (последняя позиция)
        if (hasLastKnownPosition)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(lastKnownPlayerPosition, 0.5f);
        }
    }
}