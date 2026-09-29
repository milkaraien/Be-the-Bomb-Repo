using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider))]
public class PlayerHealth : MonoBehaviour, IDamageable
{
    [Header("Настройки здоровья")]
    [Tooltip("Максимальное количество здоровья игрока")]
    [SerializeField] private float maxHealth = 100f;

    [Header("Настройки получения урона")]
    [Tooltip("Урон, получаемый при попадании вражеской пули")]
    [SerializeField] private float bulletDamage = 20f;

    [Tooltip("Тег для определения пули врага (например, EnemyBullet)")]
    [SerializeField] private string bulletTag = "EnemyBullet";

    [Header("UI Компоненты")]
    [Tooltip("Компонент Image с типом Filled для отображения здоровья")]
    [SerializeField] private Image healthBarFill;

    [Tooltip("Экран GameOver (UI Panel или CanvasGroup), который активируется при смерти")]
    [SerializeField] private GameObject gameOverScreen;

    private float currentHealth;
    private bool isDead = false;
    

    private void Awake()
    {
        currentHealth = maxHealth;

        // Отключаем экран GameOver при старте игры
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(false);
        }

        UpdateHealthUI();
    }

    /// <summary>
    /// Реализация интерфейса IDamageable. Вызывается напрямую или при попадании пули.
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

    // Обработка пересечения триггеров (когда пуля является IsTrigger)
    private void OnTriggerEnter(Collider other)
    {
        CheckBulletCollision(other.gameObject);
    }

    // Обработка обычных физических столкновений (когда у пули обычный Collider)
    private void OnCollisionEnter(Collision collision)
    {
        CheckBulletCollision(collision.gameObject);
    }

    private void CheckBulletCollision(GameObject obj)
    {
        if (isDead) return;

        // Проверяем, является ли объект пулей по тегу
        if (obj.CompareTag(bulletTag))
        {
            TakeDamage(bulletDamage);

            // Уничтожаем пулю после попадания
            Destroy(obj);
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }
    }

    private void Die()
    {
        isDead = true;

        // Включаем экран GameOver
        if (gameOverScreen != null)
        {
            gameOverScreen.SetActive(true);
        }

        // Останавливаем время в игре
        Time.timeScale = 0f;
    }

    // Метод для перезапуска игры/времени (например, для кнопки "Рестарт" в GameOver)
    public static void ResumeTime()
    {
        Time.timeScale = 1f;
    }
}