using UnityEngine;

public class PointSystem : MonoBehaviour
{
    [Header("Kto dostaje punkt gdy pilka tu wpadnie")]
    public int scoringPlayer = 1;

    [Header("Pilka")]
    public PingPongBall ball;

    [Header("Ustawienia gry")]
    public int maxScore = 10;

    private static int scorePlayer1 = 0;
    private static int scorePlayer2 = 0;

    private bool gameEnded = false;

    void Start()
    {
        GetComponent<Collider>().isTrigger = true;
        scorePlayer1 = 0;
        scorePlayer2 = 0;
        gameEnded = false;
    }

    void OnTriggerEnter(Collider other)
    {
        if (gameEnded) return;

        if (other.GetComponent<PingPongBall>() == null) return;

        if (scoringPlayer == 1)
            scorePlayer1++;
        else
            scorePlayer2++;

        Debug.Log("Gracz 1: " + scorePlayer1 + " | Gracz 2: " + scorePlayer2);

        // Sprawdzenie czy ktoœ wygra³
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

        // Reset pi³ki po punkcie
        if (ball != null)
            ball.ResetBall();
    }

    void EndGame(int winner)
    {
        gameEnded = true;

        Debug.Log("KONIEC GRY! Wygral czarnuch numer: " + winner);

        // Zatrzymanie gry
        Time.timeScale = 0f;

        // Opcjonalnie zatrzymanie edytora Unity
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}