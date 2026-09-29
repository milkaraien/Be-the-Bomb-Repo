using UnityEngine;

public class PauseManager : MonoBehaviour
{
    [Header("UI Панель")]
    [Tooltip("Панель PauseMenu из Canvas, которая включается при паузе")]
    [SerializeField] private GameObject pauseMenuPanel;

    [Header("Настройки инпута")]
    [Tooltip("Клавиша для вызова паузы")]
    [SerializeField] private KeyCode pauseKey = KeyCode.Escape;

    private bool isPaused = false;

    // Публичный геттер, чтобы другие скрипты могли проверять состояние паузы
    public bool IsPaused => isPaused;

    private void Start()
    {
        // Гарантируем, что панель паузы выключена при старте игры
        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    /// <summary>
    /// Поставить игру на паузу
    /// </summary>
    public void PauseGame()
    {
        isPaused = true;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(true);
        }

        // Останавливаем физику, таймеры и все анимации, зависящие от Time.deltaTime
        Time.timeScale = 0f;
    }

    /// <summary>
    /// Снять игру с паузы (можно привязать к кнопке "Продолжить")
    /// </summary>
    public void ResumeGame()
    {
        isPaused = false;

        if (pauseMenuPanel != null)
        {
            pauseMenuPanel.SetActive(false);
        }

        // Возобновляем обычную скорость времени
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        // Восстанавливаем время при смене или уничтожении сцены,
        // чтобы следующая сцена не запустилась «замороженной»
        Time.timeScale = 1f;
    }
}