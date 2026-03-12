using UnityEngine;

public class PaddleFollowMouse : MonoBehaviour
{
    [Header("Ruch")]
    public float moveSpeed = 15f;
    public float xLimit = 8f;
    public float yLimit = 4f;

    [Header("Przechy³")]
    public float maxTilt = 74f;      // Maksymalny przechy³ w lewo/prawo
    public float tiltSpeed = 10f;    // Jak szybko siê przechyla

    private float fixedZ;
    private float currentTilt;

    void Start()
    {
        fixedZ = transform.position.z;
    }

    void Update()
    {
        // Konwersja pozycji myszy na œwiat
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = Mathf.Abs(Camera.main.transform.position.z - fixedZ);

        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);

        // Ograniczenie ruchu
        float targetX = Mathf.Clamp(worldPos.x, -xLimit, xLimit);
        float targetY = Mathf.Clamp(worldPos.y, -yLimit, yLimit);

        // Ruch (Z zamro¿one)
        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(targetX, targetY, fixedZ),
            moveSpeed * Time.deltaTime
        );

        // ----- PRZECHY£ ZALE¯NY OD POZYCJI -----

        // Obliczamy procentowe wychylenie wzglêdem limitu X (-1 do 1)
        float normalizedX = transform.position.x / xLimit;

        // Docelowy k¹t przechy³u
        float targetTilt = -normalizedX * maxTilt;

        // P³ynne przejœcie
        currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, currentTilt);
    }
}
