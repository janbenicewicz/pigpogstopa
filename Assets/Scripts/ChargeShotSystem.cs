using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChargeShotSystem : MonoBehaviour
{
    [Header("Referencje")]
    public PingPongBall ball;
    public TMP_Text chargeText;
    public Image chargeFill;

    [Header("Dystanse")]
    public float slowdownStartDistance = 3.5f;
    public float freezeDistance = 0.8f;

    [Header("Ladowanie")]
    public float chargeSpeed = 40f;

    [Header("Moc")]
    public float powerAtZero = 0.7f;
    public float powerAtFull = 3.0f;

    private float chargePercent = 0f;

    private bool isFrozen = false;
    private bool isChargingNow = false;
    private bool hasShot = false;

    private Rigidbody ballRb;
    private float baseBallSpeed;

    private Vector3 frozenPos;
    private Vector3 incomingDir;

    void Start()
    {
        ballRb = ball.GetComponent<Rigidbody>();
        baseBallSpeed = ball.speed;

        // 🔥 FIX PRZENIKANIA
        ballRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        ballRb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    void Update()
    {
        if (ballRb == null) return;

        if (!isFrozen)
            HandleApproach();
        else
            HandleFrozenState();

        UpdateUI();
    }

    void HandleApproach()
    {
        Vector3 toPaddle = transform.position - ball.transform.position;
        float dist = toPaddle.magnitude;

        bool flyingHere = Vector3.Dot(ballRb.velocity, toPaddle) > 0;
        if (!flyingHere) return;

        if (dist < slowdownStartDistance && dist > freezeDistance)
        {
            float t = 1f - ((dist - freezeDistance) / (slowdownStartDistance - freezeDistance));
            float targetSpeed = Mathf.Lerp(baseBallSpeed, baseBallSpeed * 0.06f, t);

            if (ballRb.velocity.magnitude > targetSpeed)
                ballRb.velocity = ballRb.velocity.normalized * targetSpeed;
        }

        // 🔥 FIX PODWÓJNEGO FREEZE
        if (dist <= freezeDistance && !isFrozen)
            FreezeBall();
    }

    void FreezeBall()
    {
        if (isFrozen) return;

        isFrozen = true;
        hasShot = false;

        incomingDir = ballRb.velocity.sqrMagnitude > 0.001f
            ? ballRb.velocity.normalized
            : (ball.transform.position - transform.position).normalized;

        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = true;

        frozenPos = ball.transform.position;
    }

    void HandleFrozenState()
    {
        ball.transform.position = frozenPos;

        bool keyHeld = Input.GetKey(KeyCode.LeftShift) ||
                       Input.GetKey(KeyCode.RightShift) ||
                       Input.GetKey(KeyCode.LeftControl) ||
                       Input.GetKey(KeyCode.RightControl);

        if (keyHeld)
        {
            isChargingNow = true;

            chargePercent += chargeSpeed * Time.deltaTime;
            chargePercent = Mathf.Clamp(chargePercent, 0f, 100f);
        }
        else if (isChargingNow && !hasShot)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        if (hasShot) return;

        hasShot = true;
        isFrozen = false;
        isChargingNow = false;

        float t = chargePercent / 100f;
        float powerMult = Mathf.Lerp(powerAtZero, powerAtFull, t);

        ballRb.isKinematic = false;

        Vector3 shotDir = -incomingDir;

        float randomAngle = Random.Range(-10f, 10f);
        shotDir = Quaternion.Euler(0f, randomAngle, 0f) * shotDir;

        ballRb.velocity = shotDir.normalized * baseBallSpeed * powerMult;

        chargePercent = 0f;
    }

    // 🔥 EXTRA FIX (anty-przenikanie PRO)
    void FixedUpdate()
    {
        if (!isFrozen && ballRb.velocity.magnitude > 0.1f)
        {
            RaycastHit hit;
            if (Physics.Raycast(ball.transform.position, ballRb.velocity.normalized, out hit, 0.5f))
            {
                if (hit.collider.transform == transform)
                {
                    FreezeBall();
                }
            }
        }
    }

    void UpdateUI()
    {
        if (chargeText != null)
            chargeText.text = Mathf.RoundToInt(chargePercent) + "%";

        if (chargeFill != null)
            chargeFill.fillAmount = chargePercent / 100f;
    }
}