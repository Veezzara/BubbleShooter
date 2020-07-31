using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FieldLoader : MonoBehaviour
{
    public TextAsset textFile;

    public GameObject bubblePrefab;

    public BubbleProperties red;
    public BubbleProperties green;
    public BubbleProperties blue;


    private void Start()
    {
        List<string> lines = new List<string>(textFile.text.Split('|'));
        for (int row = 0; row < lines.Count; row++)
        {
            for (int col = 0; col < lines[row].Length; col++)
            {
                if (lines[row][col] == ' ') continue;
                GameObject bubble = Instantiate(bubblePrefab, CalculateBubblePosition(col, row), Quaternion.identity);
                SetBubbleRadius(bubble);
                Bubble bubbleComponent = bubble.GetComponent<Bubble>();
                char color = lines[row][col];
                if (color == 'r')
                {
                    bubbleComponent.properties = red;
                    bubbleComponent.InitBall();
                }
                else if (color == 'g')
                {
                    bubbleComponent.properties = green;
                    bubbleComponent.InitBall();
                }
                else if (color == 'b')
                {
                    bubbleComponent.properties = blue;
                    bubbleComponent.InitBall();
                }
                if (row == 0) 
                {
                    SpringJoint2D joint = bubble.AddComponent<SpringJoint2D>();
                    joint.connectedAnchor = new Vector2(joint.transform.position.x, joint.transform.position.y);
                    joint.dampingRatio = 1;
                    joint.frequency = 10;
                }
                else
                {
                    Collider2D[] nearCols = Physics2D.OverlapCircleAll(bubble.transform.position, GameSettings.instance.bubbleRadius + .2f);
                    foreach (Collider2D collider in nearCols)
                    {
                        if (collider.gameObject == bubble) continue; 
                        SpringJoint2D joint = bubble.AddComponent<SpringJoint2D>();
                        joint.connectedBody = collider.gameObject.GetComponent<Rigidbody2D>();
                        joint.dampingRatio = 1;
                        joint.frequency = 10;
                    }
                }
                Rigidbody2D rb = bubble.GetComponent<Rigidbody2D>();
                rb.isKinematic = false;
                rb.gravityScale = 0;
            }
        }
    }

    private Vector3 CalculateBubblePosition(int col, int row)
    {
        float radius = GameSettings.instance.bubbleRadius;
        float x, y, xOffset = 0, yOffset;
        if (row % 2 != 0)
        {
            xOffset = GameSettings.instance.bubbleRadius;
        }
        yOffset = GameSettings.instance.bubbleRadius / Mathf.Sqrt(3) / 2 * row;
        x = radius + col * radius * 2 - GameSettings.instance.halfScreenWidthInUnits + xOffset;
        y = - radius - row * radius * 2 + GameSettings.instance.halfScreenHeightInUnits + yOffset;
        return new Vector3(x, y, 0);
    }

    private void SetBubbleRadius(GameObject bubble)
    {
        float radius = GameSettings.instance.bubbleRadius;
        float curRadius = 0.1f;
        float scale = radius / curRadius;
        bubble.transform.localScale = new Vector3(scale, scale, scale);
    }

}
