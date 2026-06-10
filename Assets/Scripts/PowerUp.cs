using UnityEngine;

public class PowerUp : MonoBehaviour
{
   
    public Transform playerPaddle;
    public Transform aiPaddle;
    public PingPongBall ball;

    public float playerSizeMultiplier = 1.6f;
    public float aiSizeMultiplier = 0.6f;
    public float ballSpeedMultiplier = 1.4f;
  
    public float effectDuration = 5f;

    public float respawnDelay = 6f;

    public float radiusX = 3f;
    public float radiusY = 1.5f;
    public float radiusZ = 0f;

    Vector3 startPos;

    int currentEffect;
    float effectEnd;

    Transform changedPaddle;
    Vector3 changedPaddleScale;
    float originalBallSpeed;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        startPos = transform.position;
        originalBallSpeed = ball.speed;
    }

    void Update()
    {
        if (currentEffect != 0 && Time.time > effectEnd)
            ClearEffect();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PingPongBall>() == null) return;

        if (currentEffect != 0) ClearEffect();

        Rigidbody rb = ball.GetComponent<Rigidbody>();
        Vector3 fromPlayerToAI = aiPaddle.position - playerPaddle.position;
        bool playerHit = Vector3.Dot(rb.velocity, fromPlayerToAI) > 0f;

        Transform hitter = playerHit ? playerPaddle : aiPaddle;
        Transform opponent = playerHit ? aiPaddle : playerPaddle;

        currentEffect = Random.Range(1, 4);

        if (currentEffect == 1)
        {
            changedPaddle = opponent;
            changedPaddleScale = opponent.localScale;
            opponent.localScale = changedPaddleScale * playerSizeMultiplier;
        }
        else if (currentEffect == 2)
        {
            changedPaddle = hitter;
            changedPaddleScale = hitter.localScale;
            hitter.localScale = changedPaddleScale * aiSizeMultiplier;
        }
        else
        {
            ball.speed = originalBallSpeed * ballSpeedMultiplier;
            rb.velocity = rb.velocity.normalized * ball.speed;
        }

        effectEnd = Time.time + effectDuration;

        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        Invoke("Respawn", respawnDelay);
    }

    void ClearEffect()
    {
        if (currentEffect == 1 || currentEffect == 2)
        {
            if (changedPaddle != null)
                changedPaddle.localScale = changedPaddleScale;
        }
        else if (currentEffect == 3)
        {
            ball.speed = originalBallSpeed;
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.velocity = rb.velocity.normalized * ball.speed;
        }

        currentEffect = 0;
        changedPaddle = null;
    }

    void Respawn()
    {
        float x = startPos.x + Random.Range(-radiusX, radiusX);
        float y = startPos.y + Random.Range(-radiusY, radiusY);
        float z = startPos.z + Random.Range(-radiusZ, radiusZ);
        transform.position = new Vector3(x, y, z);

        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}
