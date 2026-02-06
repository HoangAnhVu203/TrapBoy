using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class StageController : MonoBehaviour
{
    [Serializable]
    public class ChoiceData
    {
        public Sprite sprite;
        public bool isWin;
    }

    [Header("UI Refs")]
    [SerializeField] private Button btnA;
    [SerializeField] private Image imgA;
    [SerializeField] private Button btnB;
    [SerializeField] private Image imgB;

    [Header("Stage Data (set per stage prefab)")]
    [SerializeField] private ChoiceData choiceA;
    [SerializeField] private ChoiceData choiceB;

    [Header("Intro")]
    [SerializeField] private float introSeconds = 0.7f;          // intro đơn giản (delay)
    [SerializeField] private Animator introAnimator;             // nếu có anim, kéo Animator vào
    [SerializeField] private string introTrigger = "PlayIntro";  // trigger anim intro

    [Header("Options")]
    [SerializeField] private bool shuffleChoicesEachStage = true;

    private Action<bool> onResult;
    private bool locked;

    private bool btnAIsWin;
    private bool btnBIsWin;

    public void PrepareGameplay(Action<bool> onResultCallback)
    {
        onResult = onResultCallback;
        locked = false;

        if (!btnA || !btnB || !imgA || !imgB)
        {
            Debug.LogError("[StageController] Missing btn/img refs.");
            return;
        }
        if (choiceA == null || choiceB == null)
        {
            Debug.LogError("[StageController] Missing choiceA/choiceB.");
            return;
        }

        var c0 = choiceA;
        var c1 = choiceB;

        if (shuffleChoicesEachStage && UnityEngine.Random.value > 0.5f)
            (c0, c1) = (c1, c0);

        imgA.sprite = c0.sprite;
        imgB.sprite = c1.sprite;

        btnAIsWin = c0.isWin;
        btnBIsWin = c1.isWin;

        btnA.onClick.RemoveAllListeners();
        btnB.onClick.RemoveAllListeners();
        btnA.onClick.AddListener(() => Choose(btnAIsWin));
        btnB.onClick.AddListener(() => Choose(btnBIsWin));

        // Chưa cho bấm cho tới khi intro xong
        SetInteractable(false);
    }

    public IEnumerator PlayIntroCR()
    {
        // nếu có anim intro thì play trigger
        if (introAnimator != null && !string.IsNullOrEmpty(introTrigger))
            introAnimator.SetTrigger(introTrigger);

        // intro kiểu delay (bạn có thể thay bằng chờ anim event)
        float t = Mathf.Max(0f, introSeconds);
        if (t > 0f) yield return new WaitForSecondsRealtime(t);
    }

    public void StartGameplayInput()
    {
        SetInteractable(true);
        locked = false;
    }

    private void Choose(bool isWin)
    {
        if (locked) return;
        locked = true;
        SetInteractable(false);
        onResult?.Invoke(isWin);
    }

    private void SetInteractable(bool on)
    {
        if (btnA) btnA.interactable = on;
        if (btnB) btnB.interactable = on;
    }
}
