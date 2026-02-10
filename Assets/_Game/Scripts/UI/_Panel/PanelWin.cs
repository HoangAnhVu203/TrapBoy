using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelWin : UICanvas
{
    [Header("Refs")]
    [SerializeField] private RectTransform claimX2Button;

    [Header("Pulse Config")]
    [SerializeField] private float pulseScale = 1.15f;
    [SerializeField] private float pulseTime = 0.35f;

    Coroutine pulseCR;
    Vector3 baseScale;

    public override void Open()
    {
        base.Open();

        if (claimX2Button)
        {
            baseScale = claimX2Button.localScale;
            pulseCR = StartCoroutine(PulseCR());
        }
    }

    public override void CloseDirectly()
    {
        if (pulseCR != null)
        {
            StopCoroutine(pulseCR);
            pulseCR = null;
        }

        if (claimX2Button)
            claimX2Button.localScale = baseScale;

        base.CloseDirectly();
    }

    IEnumerator PulseCR()
    {
        Vector3 big = baseScale * pulseScale;

        while (true)
        {
            yield return ScaleTo(claimX2Button, baseScale, big, pulseTime);
            yield return ScaleTo(claimX2Button, big, baseScale, pulseTime);
        }
    }

    IEnumerator ScaleTo(RectTransform rt, Vector3 from, Vector3 to, float time)
    {
        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / time;
            rt.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        rt.localScale = to;
    }

#region Button

    public void NextLevelBTN()
    {
        GameManager.Instance.NextLevel();
    }

    public void ClaimX2MoneyBTN()
    {
        // TODO: reward ads / x2 money
    }

#endregion
}
