using UnityEngine;

// Dodaj ten skrypt na obiekt Forehand I na obiekt Backhand osobno
// Na Forehand ustaw hitType = "FOREHAND"
// Na Backhand ustaw hitType = "BACKHAND"
// Kazdy musi miec Collider (isTrigger = false)
public class HitDetector : MonoBehaviour
{
    public string hitType = "FOREHAND";

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<PingPongBall>() != null)
        {
            Debug.Log(hitType + "!");
        }
    }
}