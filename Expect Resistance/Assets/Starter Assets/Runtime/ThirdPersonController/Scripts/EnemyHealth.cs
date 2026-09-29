using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody))]
public class EnemyHealth : MonoBehaviour, IDamageable
{
    [Header("Здоровье")]
    [Tooltip("Максимальное количество здоровья")]
    [SerializeField] private float maxHealth = 100f;

    [Header("UI Прогресс Бар")]
    [Tooltip("Компонент Image с типом Filled для отображения шкалы здоровья")]
    [SerializeField] private Image healthBarFill;

    [Tooltip("Canvas с шкалой здоровья (необязательно, чтобы скрыть после смерти)")]
    [SerializeField] private Canvas healthCanvas;

    [Header("Настройки физики смерти")]
    [Tooltip("Сила толчка при падении (направление зависит от последнего попадания)")]
    [SerializeField] private float deathImpulse = 5f;

    private float currentHealth;
    private Rigidbody rb;
    private bool isDead = false;
    private NavMeshAgent Agent;
    private EnemyAI EnemyAI;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentHealth = maxHealth;
        UpdateHealthUI();

        Agent = GetComponent<NavMeshAgent>();
        EnemyAI = GetComponent<EnemyAI>();
    }

    /// <summary>
    /// Реализация интерфейса IDamageable. Вызывается бомбой при взрыве.
    /// </summary>
    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        UpdateHealthUI();

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            // Обновляем шкалу здоровья (значение от 0.0 до 1.0)
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;

        // Скрываем Canvas с HP, если он задан
        if (healthCanvas != null)
        {
            healthCanvas.gameObject.SetActive(false);
        }
        EnemyAI.enabled = false;
        Agent.enabled = false;

        // Разблокируем вращения по осям X и Z, чтобы капсула могла упасть
        rb.constraints = RigidbodyConstraints.None;

        // Включаем физическую гравитацию и динамику (если они были отключены)
        rb.useGravity = true;
        rb.isKinematic = false;

        // Прикладываем случайный случайный случайный толчок в сторону, чтобы капсула красиво упала
        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            0f,
            Random.Range(-1f, 1f)
        ).normalized;

        rb.AddForce(Vector3.down * 2f, ForceMode.Impulse); // Толчок вниз
        rb.AddTorque(randomTorque * deathImpulse, ForceMode.Impulse); // Крутящий момент для падения

        // Отключаем скрипт врага (чтобы он больше не двигался/не атаковал, если есть AI)
        // this.enabled = false;
    }
}