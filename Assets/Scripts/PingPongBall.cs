using UnityEngine;

public class PingPongBall : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;

    [Header("AI - Trudnosc (0 = glupie, 10 = genialne)")]
    [Range(0, 10)]
    public int difficulty = 5;

    private Vector3 startPosition;
    private Rigidbody rb;
    private Vector3 direction;
    private int hitCount = 0;

    private Vector3 curveForce = Vector3.zero;
    private bool isCurving = false;
    private bool curveDecided = false;
    private bool ballGoingToPlayer = false; // true = leci do gracza, false = wraca od gracza

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
        ballGoingToPlayer = true; // startowo leci do gracza
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetBall();

        // Sprawdz kierunek lotu wzgledem gracza
        bool flyingTowardsPlayer = Vector3.Dot(rb.velocity, (player.position - transform.position)) > 0;

        // Pilka wlasnie zaczela leciec w strone gracza (zmiana kierunku)
        if (flyingTowardsPlayer && !ballGoingToPlayer)
        {
            ballGoingToPlayer = true;
            curveDecided = false; // nowy lot = nowa decyzja
        }

        // Pilka leci do gracza i jeszcze nie zdecydowalismy o krzywej
        if (flyingTowardsPlayer && !curveDecided)
        {
            curveDecided = true;
            float aiStrength = difficulty / 10f;
            float curveChance = Mathf.Lerp(0.05f, 0.6f, aiStrength);

            if (Random.value < curveChance)
            {
                isCurving = true;
                float curveStrength = Mathf.Lerp(3f, 16f, aiStrength);
                float side = Random.value > 0.5f ? 1f : -1f;
                curveForce = new Vector3(side * curveStrength, 0f, 0f);
            }
            else
            {
                isCurving = false;
                curveForce = Vector3.zero;
            }
        }

        // Pilka odleciala od gracza - wylacz curve natychmiast
        if (!flyingTowardsPlayer)
        {
            ballGoingToPlayer = false;
            isCurving = false;       // <-- tu jest fix: curve wylacza sie gdy pilka leci OD gracza
            curveForce = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        if (isCurving)
        {
            rb.AddForce(curveForce, ForceMode.Acceleration);

            if (rb.velocity.magnitude > speed * 2f)
                rb.velocity = rb.velocity.normalized * speed * 2f;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == player)
        {
            hitCount++;
            float aiStrength = difficulty / 10f;

            // Wylacz curve - gracz odbil
            isCurving = false;
            curveForce = Vector3.zero;
            curveDecided = false;
            ballGoingToPlayer = false;

            // Bazowe odbicie
            direction = -direction;

            // AI celuje i dodaje blad
            Vector3 toPlayer = (player.position - transform.position).normalized;
            direction = Vector3.Lerp(direction, toPlayer, aiStrength * 0.5f).normalized;

            float maxError = Mathf.Lerp(35f, 3f, aiStrength);
            float errorX = Random.Range(-maxError, maxError);
            direction = Quaternion.Euler(0f, errorX, 0f) * direction;
            direction.Normalize();

            float speedBoost = 1f + (aiStrength * 0.12f * hitCount);
            speedBoost = Mathf.Clamp(speedBoost, 1f, 2.2f);

            rb.velocity = direction * (speed * speedBoost);
        }
    }

    public void ResetBall()
    {
        hitCount = 0;
        isCurving = false;
        curveForce = Vector3.zero;
        curveDecided = false;
        ballGoingToPlayer = true;
        rb.velocity = Vector3.zero;
        transform.position = startPosition;
        direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }
}