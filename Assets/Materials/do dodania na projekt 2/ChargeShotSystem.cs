using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChargeShotSystem : MonoBehaviour
{
    [Header("Referencje")]
    public PingPongBall ball;
    public TMP_Text resultText;
    public Image barFill;
    public RectTransform needle;
    public Image needleImage;

    [Header("Dystanse")]
    public float slowdownStartDistance = 3.5f;
    public float freezeDistance = 0.8f;

    [Header("Pasek – rozmiar")]
    public float barWidth = 600f;

    [Header("Wskaźnik – prędkość")]
    public float needleSpeed = 280f;

    [Header("Moc")]
    public float powerAtCenter = 0.7f;
    public float powerAtEdge = 3.0f;

    [Header("Timeout")]
    public float timeoutSeconds = 5f;

    private Rigidbody ballRb;
    private float baseBallSpeed;

    private bool isFrozen = false;
    private bool hasShot = false;
    private bool minigameActive = false;
    private float shootCooldown = 0f;   // blokada po strzale

    private Vector3 frozenPos;
    private Vector3 incomingDir;

    private float needlePos = 0f;
    private float needleDir = 1f;
    private float elapsed = 0f;

    void Start()
    {
        ballRb = ball.GetComponent<Rigidbody>();
        baseBallSpeed = ball.speed;

        ballRb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        ballRb.interpolation = RigidbodyInterpolation.Interpolate;

        SetMinigameVisible(false);
    }

    void Update()
    {
        if (ballRb == null) return;

        // po strzale odczekaj aż piłka odskoczy, żeby nie triggerować od razu nowego freeze
        if (shootCooldown > 0f)
        {
            shootCooldown -= Time.deltaTime;
            return;
        }

        if (!isFrozen)
            HandleApproach();
        else
            HandleFrozenState();
    }

    // FixedUpdate usunięty celowo – raycast w FixedUpdate + HandleApproach w Update
    // powodowały race condition i podwójny FreezeBall (stąd bug z 2x skill check)

    void HandleApproach()
    {
        Vector3 toPaddle = transform.position - ball.transform.position;
        float dist = toPaddle.magnitude;

        bool flyingHere = Vector3.Dot(ballRb.velocity, toPaddle) > 0f;
        if (!flyingHere) return;

        if (dist < slowdownStartDistance && dist > freezeDistance)
        {
            float t = 1f - ((dist - freezeDistance) / (slowdownStartDistance - freezeDistance));
            float targetSpeed = Mathf.Lerp(baseBallSpeed, baseBallSpeed * 0.06f, t);
            if (ballRb.velocity.magnitude > targetSpeed)
                ballRb.velocity = ballRb.velocity.normalized * targetSpeed;
        }

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

        // najpierw isKinematic żeby Physics nie nadpisał velocity z powrotem
        ball.controlledByChargeShot = true;
        ballRb.isKinematic = true;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        frozenPos = ball.transform.position;

        StartMinigame();
    }

    void StartMinigame()
    {
        needlePos = 0f;
        needleDir = 1f;
        elapsed = 0f;
        minigameActive = true;
        SetMinigameVisible(true);
        UpdateNeedleUI();
    }

    void HandleFrozenState()
    {
        // trzymaj piłkę w miejscu każdy frame – nadpisuje ewentualne dryfy
        ball.transform.position = frozenPos;
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;

        if (!minigameActive) return;

        elapsed += Time.deltaTime;

        if (elapsed >= timeoutSeconds)
        {
            Shoot(0f);
            return;
        }

        float step = (needleSpeed / barWidth) * Time.deltaTime;
        needlePos += needleDir * step;
        if (needlePos >= 1f) { needlePos = 1f; needleDir = -1f; }
        if (needlePos <= 0f) { needlePos = 0f; needleDir = 1f; }
        UpdateNeedleUI();

        bool pressed = Input.anyKeyDown
                    || Input.GetMouseButtonDown(0)
                    || Input.GetMouseButtonDown(1);

        if (pressed && !hasShot)
            Shoot(CalcPower(needlePos));
    }

    float CalcPower(float pos)
    {
        float distFromCenter = Mathf.Abs(pos - 0.5f) * 2f;
        return Mathf.Lerp(powerAtCenter, powerAtEdge, distFromCenter);
    }

    void Shoot(float powerMult)
    {
        if (hasShot) return;
        hasShot = true;
        isFrozen = false;
        minigameActive = false;
        shootCooldown = 0.4f;

        SetMinigameVisible(false);

        // zeruj przed włączeniem fizyki żeby nie było resztkowego impulsu
        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = false;
        ball.controlledByChargeShot = false;

        if (powerMult <= 0f)
        {
            ballRb.velocity = (-incomingDir).normalized * 0.05f;
            return;
        }

        Vector3 shotDir = -incomingDir;
        float angle = Random.Range(-10f, 10f);
        shotDir = Quaternion.Euler(0f, angle, 0f) * shotDir;
        ballRb.velocity = shotDir.normalized * baseBallSpeed * powerMult;

        SoundManager.Instance?.PlayPlayerHit();

        if (resultText)
            resultText.text = $"Moc: {powerMult:F2}×";
    }

    void UpdateNeedleUI()
    {
        if (needle)
            needle.anchoredPosition = new Vector2((needlePos - 0.5f) * barWidth, needle.anchoredPosition.y);
    }

    void SetMinigameVisible(bool v)
    {
        if (barFill) barFill.gameObject.SetActive(v);
        if (needle) needle.gameObject.SetActive(v);
        if (resultText) resultText.gameObject.SetActive(v);
    }
}