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

    [Header("Stage Data")]
    [SerializeField] private ChoiceData choiceA;
    [SerializeField] private ChoiceData choiceB;

    [Header("Intro")]
    [SerializeField] private float introSeconds = 0.7f;
    [SerializeField] private Animator introAnimator;
    [SerializeField] private string introTrigger = "PlayIntro";

    [Header("Options")]
    [SerializeField] private bool shuffleChoicesEachStage = true;

    // runtime ui refs (lấy từ PanelGamePlay)
    private Button btnA, btnB;
    private Image imgA, imgB;

    private Action<bool> onResult;
    private bool locked;
    private bool btnAIsWin, btnBIsWin;

    // GameManager gọi trước khi PrepareGameplay
    public void BindGameplayUI(PanelGamePlay panel)
    {
        if (panel == null)
        {
            Debug.LogError("[StageController] BindGameplayUI: panel is null.");
            return;
        }

        btnA = panel.btnOption1;
        imgA = panel.imgOption1;

        btnB = panel.btnOption2;
        imgB = panel.imgOption2;
    }

    public void PrepareGameplay(Action<bool> onResultCallback)
    {
        onResult = onResultCallback;
        locked = false;

        if (!btnA || !btnB || !imgA || !imgB)
        {
            Debug.LogError("[StageController] Missing btn/img refs. Did you call BindGameplayUI()?");
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

        SetInteractable(false);
    }

    public IEnumerator PlayIntroCR()
    {
        if (introAnimator != null && !string.IsNullOrEmpty(introTrigger))
            introAnimator.SetTrigger(introTrigger);

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
