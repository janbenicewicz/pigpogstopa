using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Paddle Boundires")]
    public float max_paddleY = 7;
    public float min_paddleY = 2;
    public float max_paddleX = 3;
    public float min_paddleX = -11;

    private bool shouldMove = false;

    [Header("Max Values")]
    public float max_delta = 0.3f;
    public float max_ballVelocityRangeX = 0.5f;
    public float max_ballBaseVelocityZ = 20;
    public float max_ballVelocityRangeZ = 6;

    [Header("Min Values")]
    public float min_delta = 0.05f;
    public float min_ballVelocityRangeX = 0.3f;
    public float min_ballBaseVelocityZ = 10;
    public float min_ballVelocityRangeZ = 2;

    [Header("Difficulty")]
    [Range(0, 1)]
    public float difficulty = 0.5f;

    [Header("References")]
    public Rigidbody ball;

    private bool canUseSpace = true;

    private void Start()
    {
        StartMoving();
        Bounce();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canUseSpace)
        {
            ball.GetComponent<PingPongBall>()?.UnlockDirection();
            Bounce();

            canUseSpace = false;
        }
    }

    private void FixedUpdate()
    {
        if (shouldMove)
            Move();
    }

    public void StartMoving()
    {
        shouldMove = true;
    }

    public void EndMoving()
    {
        shouldMove = false;
    }

    private void Move()
    {
        Vector3 ballposition = ball.transform.position;
        Vector3 desiredPalletPosition = ballposition - transform.position;

        desiredPalletPosition.z = 0;

        transform.Translate(desiredPalletPosition * Mathf.Lerp(min_delta, max_delta, difficulty));

        transform.position = new Vector3(
            Mathf.Clamp(transform.position.x, min_paddleX, max_paddleX),
            Mathf.Clamp(transform.position.y, min_paddleY, max_paddleY),
            transform.position.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.rigidbody == ball)
        {
            Bounce();
        }
    }

    public void Bounce()
    {
        float velocityX = Random.Range(
            Mathf.Lerp(min_ballVelocityRangeX, max_ballVelocityRangeX, difficulty),
            -Mathf.Lerp(min_ballVelocityRangeX, max_ballVelocityRangeX, difficulty));

        float velocityY = 1;

        float velocityZ = Mathf.Lerp(min_ballBaseVelocityZ, max_ballBaseVelocityZ, difficulty)
            + Random.Range(
                Mathf.Lerp(min_ballVelocityRangeZ, max_ballVelocityRangeZ, difficulty),
                -Mathf.Lerp(min_ballVelocityRangeZ, max_ballVelocityRangeZ, difficulty));

        ball.velocity = new Vector3(velocityX, velocityY, velocityZ);
    }

    public void ResetSpace()
    {
        canUseSpace = true;
    }
}