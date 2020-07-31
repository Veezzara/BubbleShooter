using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitMenu : MonoBehaviour
{
    public float animationDuration;
    public AnimationCurve animation;

    public Vector3 startScale;
    public Vector3 endScale;

    private IEnumerator AppearAnimation()
    {
        float currentTime = 0;
        while (currentTime < animationDuration)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / animationDuration);
            float scale = animation.Evaluate(t);
            transform.localScale = new Vector3(scale, scale, scale);
            yield return null;
        }
    }

    private IEnumerator DisappearAnimation()
    {
        float currentTime = 0;
        while (currentTime < animationDuration)
        {
            currentTime += Time.deltaTime;
            float t = Mathf.Clamp01(currentTime / animationDuration);
            transform.localScale = Vector3.Lerp(endScale, startScale, t);
            yield return null;
        }
    }

    public void Appear()
    {
        StartCoroutine(AppearAnimation());
    }

    public void Disappear()
    {
        StartCoroutine(DisappearAnimation());
    }

    public void Exit()
    {
        Application.Quit();
    }
}
