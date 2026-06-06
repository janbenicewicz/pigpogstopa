using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChargeShotSystem : MonoBehaviour
{
    [Header("References")]
    public PingPongBall ball;
    public TMP_Text resultText;
    public Image barFill;
    public RectTransform needle;

    [Header("Freeze Distance")]
    public float freezeDistance = 1.2f;
    public float rearmDistance = 3.5f;

    [Header("Needle")]
    public float needleSpeed = 1.2f;
    public float barWidth = 600f;

    [Header("Power")]
    public float centerPower = 1.2f;
    public float edgePower = 0.05f;

    private Rigidbody ballRb;

    private bool isFrozen = false;
    private bool canFreeze = true;

    private Vector3 frozenPosition;
    private Vector3 incomingDirection;

    private float needlePos = 0f;
    private float needleDir = 1f;

    void Start()
    {
        if (ball != null)
            ballRb = ball.GetComponent<Rigidbody>();

        SetUIVisible(false);
    }

    void Update()
    {
        if (ballRb == null)
            return;

        if (!isFrozen)
        {
            CheckFreeze();
        }
        else
        {
            RunMinigame();
        }
    }

    // ---------------------------------------------------
    // FREEZE LOGIC
    // ---------------------------------------------------

    void CheckFreeze()
    {
        float dist = Vector3.Distance(
            transform.position,
            ball.transform.position);

        // rearm system when ball is far away
        if (dist > rearmDistance)
        {
            canFreeze = true;
        }

        // freeze once per approach
        if (dist <= freezeDistance && canFreeze)
        {
            FreezeBall();
        }
    }

    void FreezeBall()
    {
        canFreeze = false;
        isFrozen = true;

        incomingDirection =
            ballRb.velocity.normalized;

        frozenPosition =
            ball.transform.position;

        ball.controlledByChargeShot = true;

        ballRb.velocity = Vector3.zero;
        ballRb.angularVelocity = Vector3.zero;
        ballRb.isKinematic = true;

        needlePos = 0.5f;
        needleDir = 1f;

        SetUIVisible(true);
    }

    // ---------------------------------------------------
    // MINIGAME
    // ---------------------------------------------------

    void RunMinigame()
    {
        ball.transform.position = frozenPosition;

        float move =
            needleSpeed * Time.deltaTime;

        needlePos += move * needleDir;

        // smooth bounce (NO teleporting)
        if (needlePos >= 1f)
        {
            needlePos = 1f;
            needleDir = -1f;
        }
        else if (needlePos <= 0f)
        {
            needlePos = 0f;
            needleDir = 1f;
        }

        UpdateNeedle();

        if (Input.GetMouseButtonDown(0))
        {
            ShootBall();
        }
    }

    // ---------------------------------------------------
    // SHOOT
    // ---------------------------------------------------

    void ShootBall()
    {
        float power = CalculatePower();

        Vector3 dir = -incomingDirection;

        ballRb.isKinematic = false;
        ball.controlledByChargeShot = false;

        

        ballRb.velocity =
            dir.normalized *
            ball.speed *
            power;

        isFrozen = false;

        SetUIVisible(false);

        if (resultText != null)
        {
            resultText.text =
                "Power: " +
                power.ToString("F2") +
                "x";
        }
    }

    // ---------------------------------------------------
    // POWER CALCULATION
    // ---------------------------------------------------

    float CalculatePower()
    {
        float distFromCenter =
            Mathf.Abs(needlePos - 0.5f) * 2f;

        return Mathf.Lerp(
            centerPower,
            edgePower,
            distFromCenter);
    }

    // ---------------------------------------------------
    // UI
    // ---------------------------------------------------

    void UpdateNeedle()
    {
        if (needle == null)
            return;

        needle.anchoredPosition =
            new Vector2(
                (needlePos - 0.5f) * barWidth,
                needle.anchoredPosition.y);
    }

    void SetUIVisible(bool v)
    {
        if (barFill) barFill.gameObject.SetActive(v);
        if (needle) needle.gameObject.SetActive(v);
        if (resultText) resultText.gameObject.SetActive(v);
    }
}