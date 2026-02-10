using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PanelGamePlay : UICanvas
{
    [Header("Option 1")]
    public Button btnOption1;
    public Image optionImg1;
    public Image buttonBg1;
    public Image iconCorrect1;
    public Image iconWrong1;

    [Header("Option 2")]
    public Button btnOption2;
    public Image optionImg2;
    public Image buttonBg2;
    public Image iconCorrect2;
    public Image iconWrong2;

    [Header("Price Tag (child obj)")]
    [SerializeField] private GameObject priceTag1; // object con (hoặc bất kỳ object trong subtree) có tag "price" của option 1
    [SerializeField] private GameObject priceTag2; // object con có tag "price" của option 2

    [Header("Center Target")]
    public RectTransform centerTarget;

    [Header("Top HUD")]
    [SerializeField] private Text currentLevelTxt;
    [SerializeField] private Text nextLevelTxt;
    [SerializeField] private Image progressFill;

    [Header("Anim - Choice Move")]
    public float moveTime = 0.25f;
    public float chosenScale = 1.15f;

    [Header("Anim - Pop")]
    [SerializeField] private float appearTime = 0.18f;       // 0->1
    [SerializeField] private float disappearTime = 0.15f;    // 1->0
    [SerializeField] private float clickPunchTime = 0.08f;   // down + up
    [SerializeField] private float clickPunchScale = 0.92f;

    [Header("Timing")]
    [SerializeField] private float waitBeforeRevealResult = 1.0f; // đợi sau khi vào giữa rồi mới hiện đúng/sai + đổi bg

    RectTransform rt1, rt2;
    Vector2 pos1, pos2;
    Vector3 scale1, scale2;

    Sprite bg1BaseSprite, bg2BaseSprite;

    bool locked;
    Coroutine animCR;
    bool pendingAppearAnim;

    void Awake()
    {
        if (buttonBg1 == null && btnOption1) buttonBg1 = btnOption1.targetGraphic as Image;
        if (buttonBg2 == null && btnOption2) buttonBg2 = btnOption2.targetGraphic as Image;

        rt1 = btnOption1 ? btnOption1.GetComponent<RectTransform>() : null;
        rt2 = btnOption2 ? btnOption2.GetComponent<RectTransform>() : null;

        if (rt1) { pos1 = rt1.anchoredPosition; scale1 = rt1.localScale; }
        if (rt2) { pos2 = rt2.anchoredPosition; scale2 = rt2.localScale; }

        if (buttonBg1) bg1BaseSprite = buttonBg1.sprite;
        if (buttonBg2) bg2BaseSprite = buttonBg2.sprite;

        // Auto-find price tag trong subtree của từng option nếu chưa gán trong Inspector
        if (priceTag1 == null && btnOption1) priceTag1 = FindTagInChildren(btnOption1.transform, "price");
        if (priceTag2 == null && btnOption2) priceTag2 = FindTagInChildren(btnOption2.transform, "price");

        ResetOptionsUI();
    }

    void OnEnable()
    {
        if (pendingAppearAnim)
        {
            pendingAppearAnim = false;
            PlayAppearAnim();
        }
    }

    // ===================== RESET =====================

    public void ResetOptionsUI()
    {
        locked = false;

        if (animCR != null) { StopCoroutine(animCR); animCR = null; }

        if (btnOption1) { btnOption1.gameObject.SetActive(true); btnOption1.interactable = true; }
        if (btnOption2) { btnOption2.gameObject.SetActive(true); btnOption2.interactable = true; }

        // reset vị trí
        if (rt1) rt1.anchoredPosition = pos1;
        if (rt2) rt2.anchoredPosition = pos2;

        // reset scale
        if (rt1) rt1.localScale = scale1;
        if (rt2) rt2.localScale = scale2;

        // bật lại price tag
        if (priceTag1) priceTag1.SetActive(true);
        if (priceTag2) priceTag2.SetActive(true);

        // tắt icon
        if (iconCorrect1) iconCorrect1.gameObject.SetActive(false);
        if (iconWrong1) iconWrong1.gameObject.SetActive(false);
        if (iconCorrect2) iconCorrect2.gameObject.SetActive(false);
        if (iconWrong2) iconWrong2.gameObject.SetActive(false);

        // reset BG
        if (buttonBg1) buttonBg1.sprite = bg1BaseSprite;
        if (buttonBg2) buttonBg2.sprite = bg2BaseSprite;

        // panel đang tắt -> đừng set scale=0 (kẻo mất nút)
        if (!isActiveAndEnabled)
        {
            pendingAppearAnim = true;
            return;
        }

        // panel đang bật -> chạy pop-in
        PlayAppearAnim();
    }

    // ===================== BIND =====================

    public void BindChoiceButtons(Action onClick1, Action onClick2)
    {
        if (btnOption1)
        {
            btnOption1.onClick.RemoveAllListeners();
            btnOption1.onClick.AddListener(() => { if (!locked) onClick1?.Invoke(); });
        }

        if (btnOption2)
        {
            btnOption2.onClick.RemoveAllListeners();
            btnOption2.onClick.AddListener(() => { if (!locked) onClick2?.Invoke(); });
        }
    }

    // ===================== CHOICE RESULT =====================

    public void PlayChoiceResultBG(int chosenIndex, bool isWin, Sprite winBgSprite, Sprite failBgSprite, Action onDone)
    {
        if (locked) return;
        locked = true;

        if (animCR != null) StopCoroutine(animCR);
        animCR = StartCoroutine(ChoiceResultBGCR(chosenIndex, isWin, winBgSprite, failBgSprite, onDone));
    }

    IEnumerator ChoiceResultBGCR(int idx, bool isWin, Sprite winBg, Sprite failBg, Action onDone)
    {
        Button chosenBtn = (idx == 0) ? btnOption1 : btnOption2;
        Button otherBtn  = (idx == 0) ? btnOption2 : btnOption1;

        Image chosenBg   = (idx == 0) ? buttonBg1 : buttonBg2;
        Image correctIco = (idx == 0) ? iconCorrect1 : iconCorrect2;
        Image wrongIco   = (idx == 0) ? iconWrong1 : iconWrong2;

        RectTransform chosenRT = chosenBtn ? chosenBtn.GetComponent<RectTransform>() : null;
        RectTransform otherRT  = otherBtn ? otherBtn.GetComponent<RectTransform>() : null;

        GameObject chosenPrice = (idx == 0) ? priceTag1 : priceTag2;

        if (!chosenBtn || chosenBg == null || centerTarget == null || chosenRT == null)
        {
            Debug.LogError("[PanelGamePlay] Missing refs (chosenBtn/chosenBg/centerTarget).");
            yield break;
        }

        // 1) click punch: scale nhỏ rồi về lại
        yield return ClickPunch(chosenRT, idx == 0 ? scale1 : scale2);

        // 2) ẩn price tag ngay sau khi click
        if (chosenPrice) chosenPrice.SetActive(false);

        // 3) ẩn nút còn lại bằng scale về 0 rồi SetActive(false)
        if (otherBtn && otherRT)
        {
            yield return ScaleTo(otherRT, otherRT.localScale, Vector3.zero, disappearTime, unscaled: true);
            otherBtn.gameObject.SetActive(false);
        }

        // 4) move chosen to center
        Vector2 start = chosenRT.anchoredPosition;
        Vector2 end = WorldToAnchored(chosenRT, centerTarget.position);

        float t = 0f;
        float dur = Mathf.Max(0.01f, moveTime);
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            chosenRT.anchoredPosition = Vector2.Lerp(start, end, t);
            yield return null;
        }

        chosenRT.localScale = Vector3.one * chosenScale;

        // 5) đợi 1s rồi mới đổi bg + hiện đúng/sai
        yield return new WaitForSecondsRealtime(waitBeforeRevealResult);

        if (isWin && winBg) chosenBg.sprite = winBg;
        if (!isWin && failBg) chosenBg.sprite = failBg;

        if (correctIco) correctIco.gameObject.SetActive(isWin);
        if (wrongIco)   wrongIco.gameObject.SetActive(!isWin);

        yield return new WaitForSecondsRealtime(0.5f);
        onDone?.Invoke();
    }

    IEnumerator ClickPunch(RectTransform rt, Vector3 baseScale)
    {
        // down
        Vector3 down = baseScale * clickPunchScale;
        yield return ScaleTo(rt, rt.localScale, down, clickPunchTime, unscaled: true);

        // up
        yield return ScaleTo(rt, rt.localScale, baseScale, clickPunchTime, unscaled: true);
    }

    IEnumerator ScaleTo(RectTransform rt, Vector3 from, Vector3 to, float duration, bool unscaled)
    {
        if (!rt) yield break;

        float t = 0f;
        float dur = Mathf.Max(0.01f, duration);

        rt.localScale = from;

        while (t < 1f)
        {
            t += (unscaled ? Time.unscaledDeltaTime : Time.deltaTime) / dur;
            rt.localScale = Vector3.LerpUnclamped(from, to, EaseOutBack(t));
            yield return null;
        }

        rt.localScale = to;
    }

    // Ease "pop" nhẹ
    float EaseOutBack(float x)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1 + c3 * Mathf.Pow(x - 1, 3) + c1 * Mathf.Pow(x - 1, 2);
    }

    Vector2 WorldToAnchored(RectTransform rt, Vector3 worldPos)
    {
        RectTransform parent = rt.parent as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent,
            RectTransformUtility.WorldToScreenPoint(null, worldPos),
            null,
            out Vector2 localPoint);
        return localPoint;
    }

    GameObject FindTagInChildren(Transform root, string tag)
    {
        if (!root) return null;

        var all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i] != null && all[i].CompareTag(tag))
                return all[i].gameObject;
        }
        return null;
    }

    // ===================== APPEAR =====================

    private void PlayAppearAnim()
    {
        if (rt1)
        {
            rt1.localScale = Vector3.zero;
            StartCoroutine(ScaleTo(rt1, Vector3.zero, scale1, appearTime, unscaled: true));
        }

        if (rt2)
        {
            rt2.localScale = Vector3.zero;
            StartCoroutine(ScaleTo(rt2, Vector3.zero, scale2, appearTime, unscaled: true));
        }
    }

    // ===================== HUD =====================

    public void SetLevelInfo(int currentLevelNumber, int nextLevelNumber)
    {
        if (currentLevelTxt) currentLevelTxt.text = currentLevelNumber.ToString();
        if (nextLevelTxt) nextLevelTxt.text = nextLevelNumber.ToString();
    }

    public void SetStageProgress(int stageIndex, int stageCount)
    {
        if (!progressFill) return;

        if (stageCount <= 0)
        {
            progressFill.fillAmount = 0f;
            return;
        }

        float amount = stageIndex / (float)stageCount;
        progressFill.fillAmount = Mathf.Clamp01(amount);
    }

    public void OpenSettingBTN()
    {
        UIManager.Instance.OpenUI<PanelSetting>();
    }
}
