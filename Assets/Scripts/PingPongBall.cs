using UnityEngine;

public class PingPongBall : MonoBehaviour
{
    [Header("Referencje")]
    public Transform player;

    [Header("Ustawienia")]
    public float speed = 5f;

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
        // reset R
        if (Input.GetKeyDown(KeyCode.R))
            ResetBall();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.transform == player)
        {
            direction = -rb.velocity.normalized;

            float randomAngle = Random.Range(-15f, 15f);
            direction = Quaternion.Euler(0f, randomAngle, 0f) * direction;

            rb.velocity = direction * speed;
        }
    }

    // 🔥 NOWE: bounds reset
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Bounds"))
        {
            ResetBall();
        }
    }

    public void ResetBall()
    {
        rb.velocity = Vector3.zero;
        transform.position = startPosition;

        direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }
}