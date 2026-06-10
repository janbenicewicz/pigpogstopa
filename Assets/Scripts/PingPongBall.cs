using UnityEngine;

public class PingPongBall : MonoBehaviour
{
    [Header("Referencje")]
    public Transform player;

    [Header("Ustawienia")]
    public float speed = 5f;

    [HideInInspector] public bool controlledByChargeShot = false;

    private Vector3 startPosition;
    private Rigidbody rb;
    private Vector3 direction;

    private bool directionLocked = true; // ball starts locked

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        direction = (player.position - transform.position).normalized;
        rb.velocity = Vector3.zero; // start locked
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetBall();
    }

    void FixedUpdate()
    {
        if (directionLocked)
        {
            Vector3 vel = rb.velocity;

            vel.x = 0f;
            vel.z = 0f;

            if (Mathf.Abs(vel.y) < 0.1f)
                vel.y = 0f;

            rb.velocity = vel;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (controlledByChargeShot) return;

        if (collision.transform == player)
        {
            direction = -rb.velocity.normalized;
            float randomAngle = Random.Range(-15f, 15f);
            direction = Quaternion.Euler(0f, randomAngle, 0f) * direction;
            rb.velocity = direction * speed;

            SoundManager.Instance?.PlayBallBounce();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bounds"))
            ResetBall();
    }

    public void ResetBall()
    {
        controlledByChargeShot = false;
        rb.isKinematic = false;

        rb.velocity = Vector3.zero;
        transform.position = startPosition;

        directionLocked = true; // LOCK BALL AFTER RESET
    }

    public void UnlockDirection()
    {
        directionLocked = false;

        direction = Vector3.up;
        rb.velocity = direction * speed;
    }
}