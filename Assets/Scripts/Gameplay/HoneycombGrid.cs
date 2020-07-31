using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HoneycombGrid : MonoBehaviour
{
    public Bubble[,] matrix;
    private static HoneycombGrid _instance;
    public static HoneycombGrid instance { get { return _instance; } }
    public float yOffset;

    private void Awake()
    {
        _instance = this;
    }

    public GameObject bubblePrefab;

    public BubbleProperties[] properties;

    public List<Bubble> bubblesToRemove;
    List<Bubble> connected = new List<Bubble>();
    private int lastRowCount;

    private void Start()
    {
        matrix = new Bubble[GameSettings.instance.fieldYLength, GameSettings.instance.fieldXLength];
        List<string> lines = new List<string>(GameSettings.instance.field.text.Split('|'));
        for (int row = 0; row < lines.Count; row++)
        {
            for (int col = 0; col < lines[row].Length; col++)
            {
                if (lines[row][col] == ' ') continue;
                if (row == 0) ++lastRowCount;
                GameObject bubbleObject = Instantiate(bubblePrefab, CalculateBubblePosition(col, row), Quaternion.identity);
                bubbleObject.transform.parent = transform;
                bubbleObject.name = $"Bubble_{row}_{col}";
                SetBubbleRadius(bubbleObject);
                Bubble bubbleComponent = bubbleObject.GetComponent<Bubble>();
                matrix[row, col] = bubbleComponent;
                char color = lines[row][col];
                foreach (BubbleProperties property in properties)
                {
                    if (color == property.textChar)
                    {
                        bubbleComponent.properties = property;
                        bubbleComponent.InitBall();
                    }
                }
                SpringJoint2D joint = bubbleObject.AddComponent<SpringJoint2D>();
                joint.connectedAnchor = new Vector2(joint.transform.position.x, joint.transform.position.y);
                joint.dampingRatio = 10;
                joint.frequency = 10;
                Rigidbody2D rb = bubbleObject.GetComponent<Rigidbody2D>();
                rb.isKinematic = false;
            }
        }
    }

    public Vector3 CalculateBubblePosition(int col, int row)
    {
        float radius = GameSettings.instance.bubbleRadius;
        float x, y, xOffset = 0, yOffset;
        if (row % 2 != 0)
        {
            xOffset = radius;
        }
        yOffset = radius / Mathf.Sqrt(3) / 2 * row;
        x = radius + col * radius * 2 - GameSettings.instance.halfScreenWidthInUnits + xOffset;
        y = -radius - row * radius * 2 + GameSettings.instance.halfScreenHeightInUnits + yOffset + this.yOffset;
        return new Vector3(x, y, 0);
    }

    public void SetBubbleRadius(GameObject bubble)
    {
        float radius = GameSettings.instance.bubbleRadius;
        float curRadius = 0.1f;
        float scale = radius / curRadius;
        bubble.transform.localScale = new Vector3(scale, scale, scale);
    }

    public Vector3 AddBubbleToMatrix(Bubble bubble)
    {
        Vector3 closetsPos = Vector3.positiveInfinity;
        float prevDist = Mathf.Infinity;
        int r = 0;
        int c = 0;
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                if (row % 2 != 0 && col == matrix.GetLength(1) - 1) continue;
                if (matrix[row, col] != null) continue;
                Vector3 currentPos = CalculateBubblePosition(col, row);
                float dist = (bubble.transform.position - currentPos).magnitude;
                if (dist < prevDist)
                {
                    prevDist = dist;
                    closetsPos = currentPos;
                    r = row;
                    c = col;
                }
            }
        }
        matrix[r, c] = bubble;
        bubble.gameObject.name = $"Bubble_{r}_{c}";
        bubble.transform.parent = transform;
        return closetsPos;
    }

    public Vector3 ReplaceBubble(Bubble bubbleOld, Bubble bubbleNew)
    {
        Vector2 pos = bubbleOld.transform.position;
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                if (matrix[row, col] == bubbleOld)
                {
                    matrix[row, col] = bubbleNew;
                    pos = CalculateBubblePosition(col, row);
                }
            }
        }
        return pos;
    }

    public bool RemoveBubbleFromMatrix(Bubble bubble)
    {
        for (int row = 0; row < matrix.GetLength(0); row++)
        {
            for (int col = 0; col < matrix.GetLength(1); col++)
            {
                if (matrix[row, col] == bubble)
                {
                    matrix[row, col] = null;
                    return true;
                }
            }
        }
        return false;
    }


    public List<Bubble> GetNeighbours(Bubble bubble)
    {
        int row = 0;
        int col = 0;
        for (int r = 0; r < matrix.GetLength(0); r++)
        {
            for (int c = 0; c < matrix.GetLength(1); c++)
            {
                if (matrix[r, c] == bubble)
                {
                    row = r;
                    col = c;
                }
            }
        }
        List<Bubble> neighbours = new List<Bubble>();
        int colOffset = 1;
        if (row % 2 == 0)
        {
            colOffset = -1;
        }
        if (row > 0)
        {
            if (col >= 0 && col < matrix.GetLength(1)) neighbours.Add(matrix[row - 1, col]);
            if (col + colOffset > 0 && col + colOffset <= matrix.GetLength(1) - 1) neighbours.Add(matrix[row - 1, col + colOffset]);
        }
        if (row < matrix.GetLength(0) - 1)
        {
            if (col >= 0 && col < matrix.GetLength(1)) neighbours.Add(matrix[row + 1, col]);
            if (col + colOffset > 0 && col + colOffset <= matrix.GetLength(1) - 1) neighbours.Add(matrix[row + 1, col + colOffset]);
        }
        if (col > 0) neighbours.Add(matrix[row, col - 1]);
        if (col < matrix.GetLength(1) - 1) neighbours.Add(matrix[row, col + 1]);
        while (neighbours.Contains(null))
        {
            neighbours.Remove(null);
        }
        return neighbours;
    }

    public void DropBubbles()
    {
        connected.Clear();
        for (int i = 0; i < matrix.GetLength(1); i++)
        {
            if (matrix[0, i] != null)
            {
                if (connected.Contains(matrix[0, i])) continue;
                AddConnectedWith(matrix[0, i]);
            }
        }
        for (int r = 0; r < matrix.GetLength(0); r++)
        {
            for (int c = 0; c < matrix.GetLength(1); c++)
            {
                if (!connected.Contains(matrix[r, c]) && matrix[r, c] != null)
                {
                    matrix[r, c].Drop();
                    RemoveBubbleFromMatrix(matrix[r, c]);
                }
            }
        }
        if (CheckForVictory()) GameSettings.instance.Win();
    }

    public bool CheckForVictory()
    {
        int bubblesRemaining = 0;
        for (int i = 0; i < matrix.GetLength(1); i++)
        {
            if (matrix[0, i] != null) ++bubblesRemaining;
        }
        float percent = (float)bubblesRemaining / lastRowCount * 100f;
        if (percent < 30)
        {
            return true;
        }
        else
        {
            return false;
        };
    }

    public void AddConnectedWith(Bubble bubble)
    {
        if (!connected.Contains(bubble)) connected.Add(bubble);
        List<Bubble> neighbours = GetNeighbours(bubble);
        foreach (Bubble neighbour in neighbours)
        {
            if (!connected.Contains(neighbour))
            {
                connected.Add(neighbour);
                AddConnectedWith(neighbour);
            }
        }
    }

}
