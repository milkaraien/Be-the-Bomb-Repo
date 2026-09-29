using UnityEngine;

public class WorldSpaceCanvasBillboard : MonoBehaviour
{
    public enum BillboardMode
    {
        LookAtCamera,   // Всегда полностью повернут к камере
        CameraRotation  // Копирует поворот камеры (устраняет искажения при перспектве/по краям экрана)
    }

    [Header("Настройки Биллбординга")]
    [Tooltip("CameraRotation рекомендуются для ортографической/Top-Down камеры, LookAtCamera — для классической 3D")]
    [SerializeField] private BillboardMode mode = BillboardMode.CameraRotation;

    [Tooltip("Игнорировать наклон камеры по вертикали (чтобы полоска HP не заваливалась назад/вперед)")]
    [SerializeField] private bool lockVerticalRotation = false;

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    // Используем LateUpdate, чтобы поворот Canvas срабатывал ПОСЛЕ движения врага и камеры
    private void LateUpdate()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        switch (mode)
        {
            case BillboardMode.CameraRotation:
                // Копируем вращение камеры (идеально для Top-Down, шкалы не искажаются по краям)
                if (lockVerticalRotation)
                {
                    Vector3 cameraEulerAngles = mainCamera.transform.rotation.eulerAngles;
                    transform.rotation = Quaternion.Euler(0f, cameraEulerAngles.y, 0f);
                }
                else
                {
                    transform.rotation = mainCamera.transform.rotation;
                }
                break;

            case BillboardMode.LookAtCamera:
                // Направляем Canvas прямо в точку положения камеры
                Vector3 targetPosition = mainCamera.transform.position;

                if (lockVerticalRotation)
                {
                    targetPosition.y = transform.position.y;
                }

                Vector3 direction = targetPosition - transform.position;
                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(-direction); // "-direction", чтобы Canvas не отображался зеркально
                }
                break;
        }
    }
}