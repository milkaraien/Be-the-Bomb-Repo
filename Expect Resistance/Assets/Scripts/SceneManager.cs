using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    [Header("Настройки сцены")]
    [Tooltip("Точное название сцены, на которую нужно перейти")]
    [SerializeField] private string targetSceneName = "GameScreen";

    /// <summary>
    /// Метод для вызова из компонента Button (On Click)
    /// </summary>
    public void LoadGameScene()
    {
        // Перед загрузкой новой сцены восстанавливаем скорость времени 
        // (на случай, если предыдущая игра завершилась паузой / Time.timeScale = 0)
        Time.timeScale = 1f;

        SceneManager.LoadScene(targetSceneName);
    }

    /// <summary>
    /// Перегруженная версия метода, позволяющая передать имя сцены прямо из инспектора кнопки
    /// </summary>
    public void LoadSceneByName(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }
}