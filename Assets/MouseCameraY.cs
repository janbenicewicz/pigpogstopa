using UnityEngine;

public class MouseCameraY : MonoBehaviour
{
    [Header("Ustawienia")]
    public float intensity = 200f;     // Czu³oœæ
    public float maxRotation = 30f;    // Maksymalny k¹t obrotu w stopniach

    private Quaternion startRotation;

    void Start()
    {
        startRotation = transform.localRotation;
    }

    void Update()
    {
        // Pozycja myszy 0–1
        float mouseX = Input.mousePosition.x / Screen.width;

        // Zakres -0.5 do 0.5
        float centeredX = mouseX - 0.5f;

        // Oblicz k¹t i ogranicz
        float yRotation = Mathf.Clamp(centeredX * intensity, -maxRotation, maxRotation);

        // Rotacja tylko w osi Y
        transform.localRotation = startRotation * Quaternion.Euler(0f, yRotation, 0f);
    }
}
