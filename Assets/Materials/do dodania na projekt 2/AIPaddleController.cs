using UnityEngine;

public class AIPaddleController : MonoBehaviour
{
    [Header("Referencje")]
    public Transform ball;
    private Rigidbody ballRb;

    [Header("Ruch")]
    public float followSpeed = 12f;
    public float maxOffset = 0.5f;

    [Header("Granice")]
    public float horizontalLimit = 4f;
    public float verticalLimit = 3f;
    private float fixedZPosition;

    private float originX;
    private float originY;

    [Header("=== SKILL (0=noob, 1=pro) ===")]
    [Range(0f, 1f)]
    public float aiSkill = 0.4f;

    [Header("--- Debuff: Błędy obrony ---")]
    [Tooltip("Szansa że AI zignoruje lecącą piłkę. Auto: (1-skill)*0.08")]
    [Range(0f, 1f)]
    public float defenseMistakeChance = 0.05f;

    [Header("--- Debuff: Celowy unik ---")]
    [Tooltip("Szansa na ucieczkę zamiast odbicia. Auto: (1-skill)*0.10")]
    [Range(0f, 1f)]
    public float dodgeChance = 0.05f;
    public float dodgeDistance = 1.5f;

    [Tooltip("Minimalny czas między unikami")]
    public float dodgeCooldown = 2f;
    private float lastDodgeTime = -99f;

    [Header("--- Debuff: Opóźnienie reakcji ---")]
    [Tooltip("Sekundy opóźnienia. Auto: (1-skill)*0.15")]
    public float reactionDelay = 0.1f;
    private float lastReactTime = 0f;

    // trackingError usunięty — śledzenie zawsze perfekcyjne

    [Header("--- Debuff: Słaby strzał ---")]
    [Tooltip("Szansa że AI strzeli losowo. Auto: (1-skill)*0.50")]
    [Range(0f, 1f)]
    public float badShotChance = 0.25f;

    [Header("Czy auto-synchronizować debuffy ze aiSkill?")]
    public bool autoSyncDebuffs = true;

    [Header("Charge")]
    public bool useCharge = true;
    public float chargeSpeed = 30f;
    public float maxCharge = 100f;

    private float chargePercent = 0f;
    private bool isCharging = false;
    private bool hasDodgedThisApproach = false;
    private bool hasMistakeThisApproach = false;
    private bool hasDecidedShot = false;
    private bool willChargeShot = false;
    private float lastShotTime = -99f;
    public float shotCooldown = 0.5f;

    [Header("Moc")]
    public float powerAtZero = 0.9f;
    public float powerAtFull = 2.2f;

    [Header("Zasięg odbicia")]
    public float freezeDistance = 0.8f;

    private bool ballWasComing = false;

    void Start()
    {
        if (ball != null)
            ballRb = ball.GetComponent<Rigidbody>();

        // Zapisz startową pozycję paletki jako punkt odniesienia
        originX = transform.position.x;
        originY = transform.position.y;
        fixedZPosition = transform.position.z;

        SyncDebuffs();
    }

    void Update()
    {
        if (ball == null) return;

        if (autoSyncDebuffs)
            SyncDebuffs();

        MoveAI();
        HandleRotation();
        HandleCharge();
    }

    // =====================================================
    // SYNC DEBUFFÓW
    // trackingError celowo NIE jest tutaj — śledzenie
    // jest zawsze idealne, tylko delay i błędy są debuffem
    // =====================================================
    void SyncDebuffs()
    {
        float weakness = 1f - aiSkill;

        // Im słabsze AI, tym częściej ignoruje piłkę
        defenseMistakeChance = weakness * 0.08f;

        // Im słabsze AI, tym częściej ucieka zamiast odbijać
        dodgeChance = weakness * 0.10f;

        // Im słabsze AI, tym wolniej reaguje na ruch piłki
        reactionDelay = weakness * 0.15f;

        // Im słabsze AI, tym częściej strzela krzywo
        badShotChance = weakness * 0.50f;

        // followSpeed NIE jest skalowane — paletka zawsze porusza się
        // z pełną prędkością, tylko reaguje z opóźnieniem
    }

    // =====================================================
    // RUCH AI
    // Paletka zawsze śledzi dokładną pozycję piłki (bez błędu),
    // ale robi to z opóźnieniem i czasem celowo ignoruje piłkę
    // =====================================================
    void MoveAI()
    {
        if (ballRb == null) return;

        // Ustal czy piłka leci w stronę tej paletki
        // dirZ = -1 jeśli paletka jest na ujemnym Z, +1 jeśli na dodatnim
        float dirZ = transform.position.z > 0 ? -1f : 1f;
        bool ballComing = Mathf.Sign(ballRb.velocity.z) == Mathf.Sign(dirZ);

        // Gdy piłka odlatuje — resetuj flagi żeby przy następnym
        // podejściu decyzje (unik, błąd) były podejmowane od nowa
        if (!ballComing)
        {
            hasDodgedThisApproach = false;
            hasMistakeThisApproach = false;
            ballWasComing = false;
        }

        // --- UNIK ---
        // Decyzja podejmowana JEDEN RAZ na podejście piłki.
        // AI celowo ucieka w bok żeby nie odbić.
        if (ballComing
            && !hasDodgedThisApproach
            && Time.time - lastDodgeTime > dodgeCooldown
            && Random.value < dodgeChance)
        {
            hasDodgedThisApproach = true;
            hasMistakeThisApproach = false;
            lastDodgeTime = Time.time;

            float side = Random.value > 0.5f ? 1f : -1f;
            Vector3 dodgePos = new Vector3(
                Mathf.Clamp(transform.position.x + side * dodgeDistance,
                            originX - horizontalLimit, originX + horizontalLimit),
                transform.position.y,
                fixedZPosition
            );
            transform.position = Vector3.Lerp(transform.position, dodgePos, followSpeed * Time.deltaTime);
            return;
        }

        // --- OPÓŹNIENIE REAKCJI ---
        // AI "widzi" piłkę dopiero po reactionDelay sekundach.
        // Im niższy skill, tym dłużej czeka zanim zacznie się ruszać.
        if (Time.time - lastReactTime < reactionDelay)
            return;

        // --- BŁĄD OBRONY ---
        // Decyzja podejmowana JEDEN RAZ na podejście piłki.
        // AI stoi w miejscu i "nie patrzy" na piłkę.
        if (ballComing && !hasDodgedThisApproach)
        {
            if (!hasMistakeThisApproach)
                hasMistakeThisApproach = Random.value < defenseMistakeChance;

            if (hasMistakeThisApproach)
            {
                // Delikatne kołysanie w miejscu zamiast śledzenia
                Vector3 idle = new Vector3(
                    originX + Mathf.Sin(Time.time * 1.2f) * 0.4f,
                    originY + Mathf.Sin(Time.time * 0.8f) * 0.3f,
                    fixedZPosition
                );
                transform.position = Vector3.Lerp(transform.position, idle, Time.deltaTime * 2f);
                lastReactTime = Time.time;
                return;
            }
        }

        // --- PERFEKCYJNE ŚLEDZENIE (zawsze, bez błędu pozycji) ---
        // Predykcja gdzie piłka będzie za 0.12s — dzięki temu paletka
        // nie goni gdzie piłka JEST, tylko gdzie BĘDZIE
        Vector3 predicted = ball.position + ballRb.velocity * 0.12f;

        // Zablokuj ruch poza granicami stołu
        float x = Mathf.Clamp(predicted.x, originX - horizontalLimit, originX + horizontalLimit);
        float y = Mathf.Clamp(predicted.y, originY - verticalLimit, originY + verticalLimit);

        Vector3 target = new Vector3(x, y, fixedZPosition);

        // Lerp zapewnia płynny ruch bez teleportowania
        transform.position = Vector3.Lerp(transform.position, target, followSpeed * Time.deltaTime);

        lastReactTime = Time.time;
        ballWasComing = ballComing;
    }

    // =====================================================
    // ROTACJA
    // Paletka obraca się żeby "patrzeć" w stronę piłki (X)
    // =====================================================
    void HandleRotation()
    {
        transform.rotation = ball.position.x >= transform.position.x
            ? Quaternion.Euler(0f, 0f, 0f)
            : Quaternion.Euler(0f, 180f, 0f);
    }

    // =====================================================
    // CHARGE + ODBICIE
    // Gdy piłka wejdzie w zasięg freezeDistance:
    // - dobry gracz: ładuje charge i strzela mocniej
    // - słaby gracz: strzela od razu bez ładowania (losowy kierunek)
    // =====================================================
    void HandleCharge()
    {
        if (!useCharge || ballRb == null) return;
        if (Time.time - lastShotTime < shotCooldown) return;

        float distance = Vector3.Distance(transform.position, ball.position);

        if (distance <= freezeDistance)
        {
            // Decyzja raz przy wejściu w zasięg — czy ładować czy strzelić od razu
            if (!hasDecidedShot)
            {
                hasDecidedShot = true;
                willChargeShot = Random.value > badShotChance;

                if (!willChargeShot)
                {
                    // Słaby strzał — natychmiastowy, bez charge
                    isCharging = true;
                    chargePercent = 0f;
                    Shoot();
                }
            }

            // Dobry strzał — ładuj do pełna, potem strzel
            if (willChargeShot)
            {
                isCharging = true;
                chargePercent += chargeSpeed * Time.deltaTime;
                chargePercent = Mathf.Clamp(chargePercent, 0f, maxCharge);

                if (chargePercent >= maxCharge)
                    Shoot();
            }
        }
        else
        {
            // Piłka wyszła z zasięgu — strzel co ma i zresetuj
            if (isCharging)
                Shoot();

            hasDecidedShot = false;
        }
    }

    // =====================================================
    // STRZAŁ
    // Dobry strzał: piłka leci prosto w stronę przeciwnika
    // Słaby strzał: losowy kierunek, mała moc
    // =====================================================
    void Shoot()
    {
        if (!isCharging) return;

        float t = chargePercent / maxCharge;
        float power = Mathf.Lerp(powerAtZero, powerAtFull, t);

        // Z zawsze w kierunku przeciwnika
        float dirZ = transform.position.z > 0 ? -1f : 1f;

        Vector3 dir;

        if (Random.value > badShotChance)
        {
            // Celny strzał — minimalny rozrzut
            dir = new Vector3(
                Random.Range(-0.2f, 0.2f),
                Random.Range(-0.1f, 0.1f),
                dirZ
            ).normalized;
        }
        else
        {
            // Słaby strzał — duży rozrzut, może polecieć bokiem
            dir = new Vector3(
                Random.Range(-0.6f, 0.6f),
                Random.Range(-0.4f, 0.4f),
                dirZ
            ).normalized;
        }

        ballRb.velocity = dir * 10f * power;
        ballRb.angularVelocity = Vector3.zero;

        chargePercent = 0f;
        isCharging = false;
        hasDecidedShot = false;
        lastShotTime = Time.time;
    }
}
