using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public BubbleProperties properties;
    public bool lastBall;

    public void InitBall()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = properties.sprite;
    }

    private bool launched;
    public bool maxSpeed;

    public AnimationCurve snapAnimation;
    public float animationDuration;

    IEnumerator LaunchBallCouroutine(Vector3 direction, float speed)
    {
        Vector3 currentMovement = direction * speed;
        while (launched)
        {
            currentMovement.Set(currentMovement.x, currentMovement.y - GameSettings.instance.fallAcceleration * Time.deltaTime, 0);
            transform.Translate(currentMovement * Time.deltaTime);
            if (Mathf.Abs(transform.position.x) >= GameSettings.instance.halfScreenWidthInUnits)
            {
                currentMovement.Set(-currentMovement.x, currentMovement.y, 0);
            }
            if (transform.position.y >= GameSettings.instance.halfScreenHeightInUnits + HoneycombGrid.instance.yOffset)
            {
                currentMovement.Set(currentMovement.x, -currentMovement.y, 0);
            }
            yield return null;
        }
    }

    IEnumerator BubbleCollision(Vector3 position, float duration)
    {
        float curTime = 0;
        Vector3 startPos = transform.position;
        while (curTime < duration)
        {
            curTime += Time.deltaTime;
            float t = Mathf.Clamp01(curTime / duration);
            transform.position = Vector3.Lerp(startPos, position, t);
            yield return null;
        }
        StopBubble();
        HoneycombGrid.instance.bubblesToRemove.Clear();
        CheckNeighboursColors();
        if (HoneycombGrid.instance.bubblesToRemove.Count > 2)
        {
            while (HoneycombGrid.instance.bubblesToRemove.Count > 0)
            {
                HoneycombGrid.instance.RemoveBubbleFromMatrix(HoneycombGrid.instance.bubblesToRemove[0]);
                HoneycombGrid.instance.bubblesToRemove[0].BubbleSnap();
                HoneycombGrid.instance.bubblesToRemove.RemoveAt(0);
            }
            HoneycombGrid.instance.DropBubbles();
        }
        if (lastBall)
        {
            print("last");
            if (HoneycombGrid.instance.CheckForVictory())
            {
                GameSettings.instance.Win();
            }
            else
            {
                GameSettings.instance.Lose();
            }
        }
    }

    public void CheckNeighboursColors()
    {
        List<Bubble> neighbours = HoneycombGrid.instance.GetNeighbours(this);
        foreach (Bubble item in neighbours)
        {
            if (!HoneycombGrid.instance.bubblesToRemove.Contains(item))
            {
                if (properties.color == item.properties.color)
                {
                    HoneycombGrid.instance.bubblesToRemove.Add(item);
                    item.CheckNeighboursColors();
                }
            }
        }
    }

    public void Drop()
    {
        gameObject.layer = 9;
        Destroy(GetComponent<SpringJoint2D>());
    }

    public void BubbleSnap()
    {
        Drop();
        if (lastBall)
        {
            if (HoneycombGrid.instance.CheckForVictory())
            {
                GameSettings.instance.Win();
            }
            else
            {
                GameSettings.instance.Lose();
            }
        }
        GameSettings.instance.AddPoints();
        StartCoroutine(Snap());
        Destroy(gameObject, animationDuration);
    }

    private void StopBubble()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector3.zero;
        rb.isKinematic = false;
        SpringJoint2D joint = gameObject.AddComponent<SpringJoint2D>();
        joint.connectedAnchor = new Vector2(joint.transform.position.x, joint.transform.position.y);
        joint.dampingRatio = 10;
        joint.frequency = 10;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (launched)
        {
            if (maxSpeed)
            {
                launched = false;
                Bubble old = collision.gameObject.GetComponent<Bubble>();
                old.BubbleSnap();
                maxSpeed = false;
                Vector3 pos = HoneycombGrid.instance.ReplaceBubble(old, this);
                StartCoroutine(BubbleCollision(pos, .1f));
                return;
            }
            launched = false;
            Vector3 positionInGrid = HoneycombGrid.instance.AddBubbleToMatrix(this);
            StartCoroutine(BubbleCollision(positionInGrid, .1f));
        }
    }

    public void Launch(Vector3 direction, float speed, bool maxSpeed)
    {
        launched = true;
        StartCoroutine(LaunchBallCouroutine(direction, speed));
        this.maxSpeed = maxSpeed;
    }

    IEnumerator Snap()
    {
        float time = 0;
        float startScale = transform.localScale.x;
        while (time < animationDuration)
        {
            time += Time.deltaTime;
            float t = Mathf.InverseLerp(0, animationDuration, time);
            float scale = snapAnimation.Evaluate(t) * startScale;
            gameObject.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

}
