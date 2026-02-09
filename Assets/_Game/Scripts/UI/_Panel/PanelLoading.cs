using System.Collections;
using UnityEngine;

public class PanelLoading : UICanvas
{
    [Header("Refs")]
    [SerializeField] private RectTransform movingImage;

    [Header("Pos (anchoredPosition)")]
    [SerializeField] private Vector2 startPos;
    [SerializeField] private Vector2 endPos;

    Coroutine moveCR;

    bool isPlaying;

    public void Play(float seconds)
    {
        if (!movingImage)
        {
            Debug.LogError("[PanelLoading] missing movingImage.");
            return;
        }

        if (moveCR != null) StopCoroutine(moveCR);

        isPlaying = true;

        movingImage.anchoredPosition = startPos;

        moveCR = StartCoroutine(MoveOnceCR(Mathf.Max(0.01f, seconds)));
    }

    IEnumerator MoveOnceCR(float seconds)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / seconds;
            movingImage.anchoredPosition = Vector2.Lerp(startPos, endPos, t);
            yield return null;
        }

        movingImage.anchoredPosition = endPos;

        isPlaying = false;
        moveCR = null;
    }

    public override void CloseDirectly()
    {
        if (moveCR != null)
        {
            StopCoroutine(moveCR);
            moveCR = null;
        }

        isPlaying = false;

        if (movingImage) movingImage.anchoredPosition = startPos;

        base.CloseDirectly();
    }
}
