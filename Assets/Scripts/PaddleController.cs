using UnityEngine;

public class PaddleController : MonoBehaviour
{
    [Header("Paddle Movement")]
    public float followSpeed = 10f;
    public Camera gameCamera;

    [Header("Limits")]
    public float horizontalLimit = 4f;
    public float verticalLimit   = 3f;
    public float fixedZPosition  = 0f;

    [Header("Serwis")]
    public PingPongBall ball;
    // Offset pilki wzgledem paletki podczas serwisu (przed graczem)
    public Vector3 serveOffset      = new Vector3(0f, 0.3f, 1.2f);
    // Predkosc pilki przy serwisie
    public float   serveSpeed       = 6f;
    // Kat w gore przy serwisie
    [Range(0f, 45f)]
    public float   serveLaunchAngle = 15f;

    [Header("Celowanie (LPM podczas gry)")]
    [Range(0f, 1f)]
    public float aimStrength = 0.6f;
    [Range(5f, 60f)]
    public float maxAimAngle = 35f;
    public float hitSpeed    = 7f;

    // ---- stan wewnetrzny ----
    private float      originX;
    private float      originY;
    private bool       isServing = false;
    private Rigidbody  ballRb;
    private MonoBehaviour chargeShot; // referencja przez base class - brak zaleznosci od typu

    // -------------------------------------------------------

    void Start()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;

        originX        = transform.position.x;
        originY        = transform.position.y;
        fixedZPosition = transform.position.z;

        if (!gameObject.CompareTag("Paddle"))
            Debug.LogWarning("PaddleController: brak tagu 'Paddle'.");

        // Szukamy ChargeShotSystem po nazwie - nie wymaga bezposredniej referencji do typu
        chargeShot = GetComponent("ChargeShotSystem") as MonoBehaviour;

        if (ball != null)
        {
            ballRb = ball.GetComponent<Rigidbody>();
            EnterServeMode();
        }
    }

    void Update()
    {
        MoveWithMouse();
        RotateBasedOnCursor();

        if (isServing)
        {
            HoldBallAtServePosition();

            if (Input.GetKeyDown(KeyCode.S) || Input.GetMouseButtonDown(0))
                ExecuteServe();
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
                TryHitBall();
        }
    }

    // -------------------------------------------------------
    //  Ruch i rotacja paletki
    // -------------------------------------------------------

    void MoveWithMouse()
    {
        Vector3 mouseScreen  = Input.mousePosition;
        float distFromCamera = Mathf.Abs(gameCamera.transform.position.z - fixedZPosition);
        mouseScreen.z        = distFromCamera;
        Vector3 worldTarget  = gameCamera.ScreenToWorldPoint(mouseScreen);

        float clampedX = Mathf.Clamp(worldTarget.x, originX - horizontalLimit, originX + horizontalLimit);
        float clampedY = Mathf.Clamp(worldTarget.y, originY - verticalLimit,   originY + verticalLimit);

        transform.position = Vector3.Lerp(
            transform.position,
            new Vector3(clampedX, clampedY, fixedZPosition),
            followSpeed * Time.deltaTime);
    }

    void RotateBasedOnCursor()
    {
        float screenCenter = Screen.width / 2f;
        transform.rotation = Input.mousePosition.x >= screenCenter
            ? Quaternion.Euler(0f, 0f,   0f)
            : Quaternion.Euler(0f, 180f, 0f);
    }

    // -------------------------------------------------------
    //  Serwis
    // -------------------------------------------------------

    public void EnterServeMode()
    {
        if (ball == null || ballRb == null) return;

        isServing = true;

        // Wylacz ChargeShotSystem zeby nie ruszal pilki podczas serwisu
        if (chargeShot != null)
            chargeShot.enabled = false;

        ballRb.velocity        = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic     = true;

        HoldBallAtServePosition();
    }

    void HoldBallAtServePosition()
    {
        ball.transform.position = transform.position
                                  + transform.right   * serveOffset.x
                                  + transform.up      * serveOffset.y
                                  + transform.forward * serveOffset.z;
    }

    void ExecuteServe()
    {
        isServing = false;

        ballRb.isKinematic = false;

        Vector3 serveDir = Quaternion.AngleAxis(-serveLaunchAngle, transform.right) * transform.forward;
        serveDir         = ApplyAimDirection(serveDir).normalized;
        ballRb.velocity  = serveDir * serveSpeed;

        // Wlacz ChargeShotSystem z powrotem
        if (chargeShot != null)
            chargeShot.enabled = true;

        Debug.Log("Serwis!");
    }

    // -------------------------------------------------------
    //  Odbicie LPM
    // -------------------------------------------------------

    void TryHitBall()
    {
        if (ball == null) return;

        float dist = Vector3.Distance(transform.position, ball.transform.position);
        if (dist > 1.5f) return;

        ballRb.isKinematic = false;
        ballRb.velocity    = ApplyAimDirection(transform.forward).normalized * hitSpeed;
    }

    // -------------------------------------------------------
    //  Celowanie poziome wg pozycji X paletki
    // -------------------------------------------------------

    Vector3 ApplyAimDirection(Vector3 baseDir)
    {
        float normalizedX = Mathf.Clamp(
            (transform.position.x - originX) / horizontalLimit, -1f, 1f);
        float aimAngle = normalizedX * maxAimAngle * aimStrength;
        return (Quaternion.Euler(0f, aimAngle, 0f) * baseDir).normalized;
    }
}
