using UnityEngine;

public class PingPongBall : MonoBehaviour
{
    public Transform player;
    public float speed = 5f;

    private Vector3 startPosition;
    private Rigidbody rb;
    private Vector3 direction;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        startPosition = transform.position;

        // Początkowy kierunek w stronę gracza
        direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }

    void Update()
    {
        // Reset po wciśnięciu R
        if (Input.GetKeyDown(KeyCode.R))
        {
            ResetBall();
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        // Jeśli uderzy w gracza → odbij
        if (collision.transform == player)
        {
            direction = -direction;
            rb.velocity = direction * speed;
        }
    }

    void ResetBall()
    {
        rb.velocity = Vector3.zero;
        transform.position = startPosition;

        direction = (player.position - transform.position).normalized;
        rb.velocity = direction * speed;
    }
}
