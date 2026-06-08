using UnityEngine;
using TMPro;

public class PointSystem : MonoBehaviour
{
    [Header("Kto dostaje punkt gdy pilka tu wpadnie")]
    public int scoringPlayer = 1;

    [Header("Pilka")]
    public PingPongBall ball;

    [Header("Ustawienia gry")]
    public int maxScore = 10;

    [Header("UI")]
    public TMP_Text playerScoreText;
    public TMP_Text enemyScoreText;

    private static int scorePlayer1 = 0;
    private static int scorePlayer2 = 0;

    private bool gameEnded = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;

        scorePlayer1 = 0;
        scorePlayer2 = 0;
        gameEnded = false;

        UpdateScoreUI();
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameEnded) return;

        if (other.GetComponent<PingPongBall>() == null) return;

        if (scoringPlayer == 1)
            scorePlayer1++;
        else
            scorePlayer2++;

        UpdateScoreUI();

        Debug.Log("Gracz 1: " + scorePlayer1 + " | Gracz 2: " + scorePlayer2);

        // Sprawdzenie czy ktoś wygrał
        if (scorePlayer1 >= maxScore)
        {
            EndGame(1);
            return;
        }
        else if (scorePlayer2 >= maxScore)
        {
            EndGame(2);
            return;
        }

        // Reset piłki po punkcie
        if (ball != null)
        {
            ball.ResetBall();

            EnemyAI enemyAI = FindObjectOfType<EnemyAI>();
            if (enemyAI != null)
                enemyAI.ResetSpace();
        }
    }

    void UpdateScoreUI()
    {
        if (playerScoreText != null)
            playerScoreText.text = scorePlayer1.ToString();

        if (enemyScoreText != null)
            enemyScoreText.text = scorePlayer2.ToString();
    }

    void EndGame(int winner)
    {
        gameEnded = true;

        Debug.Log("KONIEC GRY! Wygral gracz numer: " + winner);

        Time.timeScale = 0f;

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}