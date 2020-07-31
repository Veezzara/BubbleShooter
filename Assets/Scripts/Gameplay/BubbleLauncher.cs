using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BubbleLauncher : MonoBehaviour
{
    public int bubblesToShoot;
    private int bubblesRemaining;

    public GameObject bubblePrefab;

    public float maxTension = 2;
    public float spread = 5;
    public float launchSpeedMultiplier;
    public float pathTime = 1;
    public AnimationCurve lineWidth;
    public float swapDuration = .3f;

    private bool readyToLaunch;
    private LineRenderer[] lines;
    private bool maxSpeed;

    private Queue<BubbleProperties> bubbles;
    private BubbleProperties currentBubble;
    private BubbleProperties nextBubble;
    public GameObject nextBubbleObject;
    private SpriteRenderer nextBubbleRenderer;

    private SpriteRenderer spriteRenderer;
    private float animatedAngle;

    Vector3 fingerPos;

    private void Start()
    {
        float radius = GameSettings.instance.bubbleRadius;
        float curRadius = 0.1f;
        float scale = radius / curRadius;
        transform.localScale = new Vector3(scale, scale, scale);
        nextBubbleObject.transform.localScale = new Vector3(scale, scale, scale);
        bubbles = new Queue<BubbleProperties>();
        for (int i = 0; i < bubblesToShoot; i++)
        {
            bubbles.Enqueue(HoneycombGrid.instance.properties[Random.Range(0, HoneycombGrid.instance.properties.Length)]);
        }
        currentBubble = bubbles.Dequeue();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = currentBubble.sprite;
        nextBubbleRenderer = nextBubbleObject.GetComponent<SpriteRenderer>();
        nextBubble = bubbles.Dequeue();
        nextBubbleRenderer.sprite = nextBubble.sprite;
        lines = GetComponentsInChildren<LineRenderer>();
        ResetLines();
        bubblesRemaining = bubblesToShoot - 1;
        GameSettings.instance.SetRemainingBallsText(bubblesRemaining);
    }

    private void SetNextBubble()
    {
        if (nextBubble == null)
        {
            gameObject.SetActive(false);
            return;
        }
        currentBubble = nextBubble;
        spriteRenderer.sprite = currentBubble.sprite;
        if (bubbles.Count == 0)
        {
            nextBubble = null;
            nextBubbleObject.SetActive(false);
            return;
        }
        nextBubble = bubbles.Dequeue();
        nextBubbleRenderer.sprite = nextBubble.sprite;
    }

    private void Update()
    {
        if (readyToLaunch)
        {
            if (Input.GetKey(KeyCode.Mouse0))
            {
                fingerPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                fingerPos.Set(fingerPos.x, fingerPos.y, 0);
                if (maxTension <= (fingerPos - transform.position).magnitude)
                {
                    fingerPos = (fingerPos - transform.position).normalized * maxTension + transform.position;
                    maxSpeed = true;
                    DrawLine(fingerPos, spread);
                }
                else
                {
                    maxSpeed = false;
                    ResetLines();
                    DrawLine(fingerPos, lines[0], lines[1]);
                }

            }
            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                readyToLaunch = false;
                ResetLines();
                if (maxSpeed)
                {
                    float spreadAngle = Random.Range(-spread, spread);
                    fingerPos = rotateVector(fingerPos, spreadAngle / 2);
                }
                --bubblesRemaining;
                GameSettings.instance.SetRemainingBallsText(bubblesRemaining);
                LaunchBubble(-(fingerPos - transform.position));
            }
        }
    }

    private void LaunchBubble(Vector3 direction)
    {
        direction.Set(direction.x, direction.y, 0);
        GameObject bubble = Instantiate(bubblePrefab);
        bubble.transform.position = transform.position;
        float curRadius = 0.1f;
        float scale = GameSettings.instance.bubbleRadius / curRadius;
        bubble.transform.localScale = new Vector3(scale, scale, scale);
        Bubble bubbleComp = bubble.GetComponent<Bubble>();
        bubbleComp.properties = currentBubble;
        SetNextBubble();
        if (bubblesRemaining == -1) bubbleComp.lastBall = true;
        bubbleComp.InitBall();
        bubbleComp.Launch(direction, launchSpeedMultiplier, maxSpeed);
        StartCoroutine(SwapBallAnimation());
    }

    private void OnMouseDown()
    {
        readyToLaunch = true;
    }

    private void DrawLine(Vector3 fingerPos, float offsetAngle)
    {
        fingerPos -= transform.position;
        fingerPos.Set(fingerPos.x, fingerPos.y, 0);
        Vector3 pos1 = rotateVector(fingerPos, offsetAngle) + transform.position;
        Vector3 pos2 = rotateVector(fingerPos, -offsetAngle) + transform.position;
        DrawLine(pos1, lines[0], lines[1]);
        DrawLine(pos2, lines[2], lines[3]);
    }

    private Vector3 rotateVector(Vector3 vector, float angle)
    {
        float radian = angle * Mathf.Deg2Rad;
        float x = vector.x * Mathf.Cos(radian) - vector.y * Mathf.Sin(radian);
        float y = vector.x * Mathf.Sin(radian) + vector.y * Mathf.Cos(radian);
        return new Vector3(x, y, 0);
    }

    private void DrawLine(Vector3 fingerPos, LineRenderer line1, LineRenderer line2)
    {
        Vector3 direction = -(fingerPos - transform.position) * launchSpeedMultiplier;
        float t = GameSettings.instance.halfScreenWidthInUnits / Mathf.Abs(direction.x);
        t = Mathf.Min(pathTime, t);
        int pathSteps = Mathf.RoundToInt(t * 10);
        line1.positionCount = pathSteps + 2;
        float stepPercent = 1f / (pathSteps + 1f);
        line1.startWidth = lineWidth.Evaluate(0);
        line1.endWidth = lineWidth.Evaluate(Mathf.InverseLerp(0, pathTime, t));
        for (int i = 0; i < pathSteps + 2; i++)
        {
            float curT = Mathf.Lerp(0, t, stepPercent * i);
            float x = curT * direction.x;
            float y = curT * direction.y;
            y -= GameSettings.instance.fallAcceleration * curT * curT / 2;
            y += transform.position.y;
            line1.SetPosition(i, new Vector3(x, y, 0));
        }
        if (t < pathTime)
        {
            line2.startWidth = lineWidth.Evaluate(Mathf.InverseLerp(0, pathTime, t));
            line2.endWidth = lineWidth.Evaluate(1);
            float remainingTime = pathTime - t;
            pathSteps = Mathf.RoundToInt(remainingTime * 10);
            line2.positionCount = pathSteps + 2;
            for (int i = 0; i < pathSteps + 2; i++)
            {
                float curT = Mathf.Lerp(t, pathTime, stepPercent * i);
                float sign = Mathf.Sign(fingerPos.x);
                float x = curT * -direction.x - GameSettings.instance.halfScreenWidthInUnits * 2 * sign;
                float y = curT * direction.y;
                y -= GameSettings.instance.fallAcceleration * curT * curT / 2;
                y += transform.position.y;
                line2.SetPosition(i, new Vector3(x, y, 0));
            }
        }
        else
        {
            line2.positionCount = 0;
        }
    }

    public void ResetLines()
    {
        foreach (LineRenderer line in lines)
        {
            line.positionCount = 0;
        }
    }

    IEnumerator SwapBallAnimation()
    {
        float time = 0;
        Vector3 startPos = transform.position;
        while (time < swapDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / swapDuration);
            transform.position = Vector3.Lerp(nextBubbleObject.transform.position, startPos, t);
            yield return null;
        }
    }

}
