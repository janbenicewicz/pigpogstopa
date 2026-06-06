using UnityEngine;

public class PingPongBall : MonoBehaviour
{
    [Header("Referencje")]
    public Transform player;

    [Header("Ustawienia")]
    public float speed = 5f;

    // ChargeShotSystem ustawia to na true gdy przejmuje kontrolę
    [HideInInspector] public bool controlledByChargeShot = false;

    private Vector3 startPosition;
    private Rigidbody rb;
    private Vector3 direction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;
        direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
            ResetBall();
    }

    void OnCollisionEnter(Collision collision)
    {
        // jeśli ChargeShotSystem przejął kontrolę – nie nadpisuj velocity
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
        direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }
}