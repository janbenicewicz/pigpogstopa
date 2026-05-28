using UnityEngine;

/// <summary>
/// Wrzuć na trigger (np. ściana za graczem).
/// Ustaw kto traci punkt: Player czy Opponent.
/// </summary>
public class PointZone : MonoBehaviour
{
    public enum ZoneOwner { Player, Opponent }

    [Header("Kto traci punkt gdy piłka tu wpadnie")]
    public ZoneOwner loser = ZoneOwner.Opponent;

    void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<PingPongBall>(out _)) return;

        if (loser == ZoneOwner.Opponent)
        {
            // gracz zdobył punkt – tłum świętuje
            SoundManager.Instance?.PlayCrowdCheer();
        }
        else
        {
            // gracz stracił punkt – tłum wzdycha
            SoundManager.Instance?.PlayCrowdGroan();
        }
    }
}
