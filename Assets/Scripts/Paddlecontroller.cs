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

    private float originX;
    private float originY;

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
        // Polowa szerokosci ekranu
        float screenCenter = Screen.width / 2f;

        if (Input.mousePosition.x >= screenCenter)
        {
            // Kursor po prawej - normalna rotacja
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        }
        else
        {
            // Kursor po lewej - obrot o 180 stopni
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        }
    }
}