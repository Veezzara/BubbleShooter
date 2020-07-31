using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameSettings : MonoBehaviour
{

    private float _halfScreenWidthInUnits;
    private float _halfScreenHeightInUnits;
    private float _bubbleRadius;
    private int _fieldXLength;
    private int _fieldYLength;
    private BubbleProperties _properties;

    private int score;

    public TextAsset field;
    public float fallAcceleration;

    public float halfScreenWidthInUnits { get { return _halfScreenWidthInUnits; } }
    public float halfScreenHeightInUnits { get { return _halfScreenHeightInUnits; } }
    public float bubbleRadius { get { return _bubbleRadius; } }
    public int fieldXLength { get { return _fieldXLength; } }
    public int fieldYLength { get { return _fieldYLength; } }
    public BubbleProperties properties { get { return _properties; } }

    private static GameSettings _instance;
    public static GameSettings instance { get { return _instance; } }

    public TMP_Text scoreText;
    public TMP_Text ballsRemainingText;
    public GameObject exitWindow;
    public GameObject resultWindow;
    public float animationDuration;
    public AnimationCurve animationCurve;
    public GameObject winText;
    public GameObject loseText;
    public TMP_Text resultScoreText;
    public TMP_Text highscoreText;

    private void Awake()
    {
        _instance = this;
        _halfScreenWidthInUnits = Camera.main.orthographicSize * Screen.width / Screen.height;
        _halfScreenHeightInUnits = halfScreenWidthInUnits / Screen.width * Screen.height;
        List<string> lines = new List<string>(field.text.Split('|'));
        _bubbleRadius = halfScreenWidthInUnits / lines[0].Length;
        _fieldXLength = Mathf.FloorToInt(halfScreenWidthInUnits / _bubbleRadius);
        _fieldYLength = Mathf.FloorToInt(halfScreenHeightInUnits / _bubbleRadius);
        scoreText.text = $"Score: {score}";
    }

    public void ShowExitWindow()
    {
        StartCoroutine(ExitWindowsAppear());
    }

    public void HideExitWindow()
    {
        StartCoroutine(ExitWindowsHide());
    }

    IEnumerator ExitWindowsAppear()
    {
        float currentTime = 0;
        while (currentTime < animationDuration)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / animationDuration);
            float scale = animationCurve.Evaluate(t);
            exitWindow.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

    IEnumerator ExitWindowsHide()
    {
        float currentTime = 0;
        while (currentTime < animationDuration)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / animationDuration);
            t = 1 - t;
            float scale = animationCurve.Evaluate(t);
            exitWindow.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

    public void Win()
    {
        StartCoroutine(ShowWinResult());
    }

    public void Lose()
    {
        StartCoroutine(ShowLoseResult());
    }

    IEnumerator ShowLoseResult()
    {
        yield return new WaitForSecondsRealtime(1.5f);
        int highscore = PlayerPrefs.GetInt("Highscore");
        if (score > highscore)
        {
            PlayerPrefs.SetInt("Highscore", score);
            highscore = score;
        }
        resultWindow.SetActive(true);
        loseText.SetActive(true);
        highscoreText.text = $"Рекорд: {highscore}";
        resultScoreText.text = $"Счет: {score}";
        float currentTime = 0;
        while (currentTime < animationDuration)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / animationDuration);
            float scale = animationCurve.Evaluate(t);
            resultWindow.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

    IEnumerator ShowWinResult()
    {
        yield return new WaitForSecondsRealtime(1.6f);
        int highscore = PlayerPrefs.GetInt("Highscore");
        if (score > highscore)
        {
            PlayerPrefs.SetInt("Highscore", score);
            highscore = score;
        }
        resultWindow.SetActive(true);
        winText.SetActive(true);
        highscoreText.text = $"Рекорд: {highscore}";
        resultScoreText.text = $"Счет: {score}";
        float currentTime = 0;
        while (currentTime < animationDuration)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / animationDuration);
            float scale = animationCurve.Evaluate(t);
            resultWindow.transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

    public void AddPoints()
    {
        ++score;
        scoreText.text = $"Score: {score}";
    }

    public void SetRemainingBallsText(int balls)
    {
        if (balls == 0)
        {
            ballsRemainingText.gameObject.SetActive(false);
            return;
        }
        ballsRemainingText.text = balls.ToString();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Bubble bubble = collision.gameObject.GetComponent<Bubble>();
        if (bubble != null)
        {
            bubble.BubbleSnap();
        }
    }

}
