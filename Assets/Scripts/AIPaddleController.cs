using UnityEngine;

public class AIPaddleController : MonoBehaviour
{
    [Header("Referencje")]
    public Transform ball;
    private Rigidbody ballRb;

    [Header("Ruch")]
    public float followSpeed = 4f;
    public float maxOffset = 0.5f;

    [Header("Granice")]
    public float horizontalLimit = 4f;
    public float verticalLimit = 3f;
    private float fixedZPosition;

    private float originX;
    private float originY;

    [Header("Skill")]
    [Range(0f, 1f)]
    public float aiSkill = 0.4f;

    [Header("Słaba obrona")]
    [Range(0f, 1f)]
    public float defenseMistakeChance = 0.6f;

    [Header("Unikanie piłki (celowe)")]
    [Range(0f, 1f)]
    public float dodgeChance = 0.5f;
    public float dodgeDistance = 2f;

    [Header("Opóźnienie reakcji")]
    public float reactionDelay = 0.25f;
    private float lastReactTime = 0f;

    [Header("Charge")]
    public bool useCharge = true;
    public float chargeSpeed = 30f;
    public float maxCharge = 100f;

    private float chargePercent = 0f;
    private bool isCharging = false;

    [Header("Moc")]
    public float powerAtZero = 0.8f;
    public float powerAtFull = 2.0f;

    [Header("Zasięg odbicia")]
    public float freezeDistance = 0.8f;

    void Start()
    {
        if (ball != null)
            ballRb = ball.GetComponent<Rigidbody>();

        originX = transform.position.x;
        originY = transform.position.y;
        fixedZPosition = transform.position.z;
    }

    void Update()
    {
        if (ball == null) return;

        MoveAI();
        HandleRotation();
        HandleCharge();
    }

    // =========================
    // 🔥 RUCH AI
    // =========================
    void MoveAI()
    {
        if (ballRb == null) return;

        float dirZ = transform.position.z > 0 ? -1f : 1f;

        // czy piłka leci do AI
        bool ballComing = Mathf.Sign(ballRb.velocity.z) == Mathf.Sign(dirZ);

        // 🔥 UNIK (najważniejsze)
        if (ballComing && Random.value < dodgeChance)
        {
            float side = Random.value > 0.5f ? 1f : -1f;

            Vector3 dodgePos = new Vector3(
                transform.position.x + side * dodgeDistance,
                transform.position.y,
                fixedZPosition
            );

            dodgePos.x = Mathf.Clamp(dodgePos.x, originX - horizontalLimit, originX + horizontalLimit);

            transform.position = Vector3.Lerp(transform.position, dodgePos, followSpeed * Time.deltaTime);
            return;
        }

        // 🔥 OPÓŹNIENIE REAKCJI
        if (Time.time - lastReactTime < reactionDelay)
            return;

        // 🔥 CZASAMI IGNORUJE PIŁKĘ
        if (ballComing && Random.value < defenseMistakeChance)
        {
            Vector3 idle = new Vector3(
                originX + Mathf.Sin(Time.time) * 0.5f,
                originY,
                fixedZPosition
            );

            transform.position = Vector3.Lerp(transform.position, idle, Time.deltaTime * 2f);
            return;
        }

        // 🟢 NORMALNE ALE SŁABE ŚLEDZENIE
        Vector3 predicted = ball.position + ballRb.velocity * 0.1f;

        float error = (1f - aiSkill) * maxOffset;

        predicted.x += Random.Range(-error, error);
        predicted.y += Random.Range(-error, error);

        float x = Mathf.Clamp(predicted.x, originX - horizontalLimit, originX + horizontalLimit);
        float y = Mathf.Clamp(predicted.y, originY - verticalLimit, originY + verticalLimit);

        Vector3 target = new Vector3(x, y, fixedZPosition);

        transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);

        lastReactTime = Time.time;
    }

    // =========================
    // ROTACJA
    // =========================
    void HandleRotation()
    {
        if (ball.position.x >= transform.position.x)
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        else
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
    }

    // =========================
    // ODBICIE
    // =========================
    void HandleCharge()
    {
        if (!useCharge || ballRb == null) return;

        float distance = Vector3.Distance(transform.position, ball.position);

        if (distance <= freezeDistance)
        {
            // 🔥 czasem NIE odbija
            if (Random.value < defenseMistakeChance)
                return;

            ballRb.velocity *= 0.3f;
            ballRb.angularVelocity = Vector3.zero;

            isCharging = true;

            if (Random.value < aiSkill)
            {
                chargePercent += chargeSpeed * Time.deltaTime;
                chargePercent = Mathf.Clamp(chargePercent, 0f, maxCharge);
            }
            else
            {
                Shoot();
            }
        }
    }

    // =========================
    // STRZAŁ (DOBRY)
    // =========================
    void Shoot()
    {
        if (!isCharging) return;

        float t = chargePercent / maxCharge;
        float power = Mathf.Lerp(powerAtZero, powerAtFull, t);

        float dirZ = transform.position.z > 0 ? -1f : 1f;

        Vector3 dir = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(-0.2f, 0.2f),
            dirZ
        ).normalized;

        ballRb.velocity = dir * 10f * power;
        ballRb.angularVelocity = Vector3.zero;

        chargePercent = 0f;
        isCharging = false;
    }
}