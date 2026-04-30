using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [Header("Paddle Movement")]
    public float followSpeed = 10f;
    public Camera gameCamera;

    [Header("Limits")]
    public float horizontalLimit = 4f;
    public float verticalLimit = 3f;
    public float fixedZPosition = 0f;

    [Header("Forehand / Backhand Tilt")]
    [Tooltip("Kat przechylenia paletki przy forehand/backhand (stopnie)")]
    public float tiltAngle = 30f;
    [Tooltip("Jak plynnie paletka sie przechyla (wyzsze = szybciej)")]
    public float tiltSpeed = 8f;

    private float originX;
    private float originY;
    private float currentTilt = 0f;   // aktualny kat Z (interpolowany)
    private bool isForehand = true;    // true = kursor po prawej

    // Publiczna wlasciwosc - PingPongBall moze to odczytac
    public bool IsForehand => isForehand;

    void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        originX = transform.position.x;
        originY = transform.position.y;
        fixedZPosition = transform.position.z;

        if (!gameObject.CompareTag("Paddle"))
        {
            Debug.LogWarning("PaddleController: This GameObject is not tagged 'Paddle'. " +
                             "Please add the 'Paddle' tag so the ball can detect collisions.");
        }
    }

    void Update()
    {
        MoveWithMouse();
        RotateBasedOnCursor();
    }

    void MoveWithMouse()
    {
        Vector3 mouseScreen = Input.mousePosition;
        float distFromCamera = Mathf.Abs(gameCamera.transform.position.z - fixedZPosition);
        mouseScreen.z = distFromCamera;
        Vector3 worldTarget = gameCamera.ScreenToWorldPoint(mouseScreen);

        float clampedX = Mathf.Clamp(worldTarget.x, originX - horizontalLimit, originX + horizontalLimit);
        float clampedY = Mathf.Clamp(worldTarget.y, originY - verticalLimit, originY + verticalLimit);

        Vector3 targetPosition = new Vector3(clampedX, clampedY, fixedZPosition);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);
    }

    void RotateBasedOnCursor()
    {
        float screenCenter = Screen.width / 2f;
        isForehand = Input.mousePosition.x >= screenCenter;

        float targetYaw;   // obrot Y (strona paletki)
        float targetTilt;  // obrot Z (przechylenie)

        if (isForehand)
        {
            // Kursor po prawej - FOREHAND
            // Paletka przechylona w prawo -> pilka odbija sie w lewo
            targetYaw  =   0f;
            targetTilt = -tiltAngle;
        }
        else
        {
            // Kursor po lewej - BACKHAND
            // Paletka przechylona w lewo -> pilka odbija sie w prawo
            targetYaw  = 180f;
            targetTilt =  tiltAngle;
        }

        // Plynne przejscie kata przechylenia
        currentTilt = Mathf.LerpAngle(currentTilt, targetTilt, tiltSpeed * Time.deltaTime);

        transform.rotation = Quaternion.Euler(0f, targetYaw, currentTilt);
    }
}