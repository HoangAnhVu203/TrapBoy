using System;
using System.Collections;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [Serializable]
    public class ChoiceData
    {
        public Sprite optionSprite;
        public bool isWin;
    }

    [Header("Stage Choices")]
    [SerializeField] private ChoiceData choiceA;
    [SerializeField] private ChoiceData choiceB;

    [Header("Button BG Sprites (Result)")]
    [SerializeField] private Sprite winButtonBg;
    [SerializeField] private Sprite failButtonBg;

    [Header("Intro")]
    [SerializeField] private float introSeconds = 1.0f;
    [SerializeField] private Animator introAnimator;
    [SerializeField] private string introTrigger = "PlayIntro";

    [Header("Options")]
    [SerializeField] private bool shuffleChoicesEachStage = true;

    private PanelGamePlay panel;
    private Action<bool> onResult;
    private bool locked;
    private bool mapped0IsWin, mapped1IsWin;

    public void BindGameplayUI(PanelGamePlay p) => panel = p;

    public void PrepareGameplay(Action<bool> onResultCallback)
    {
        onResult = onResultCallback;
        locked = false;

        if (panel == null)
        {
            Debug.LogError("[StageController] panel is null. Call BindGameplayUI first.");
            return;
        }
        if (choiceA == null || choiceB == null)
        {
            Debug.LogError("[StageController] choiceA/choiceB missing.");
            return;
        }

        panel.ResetOptionsUI();

        var c0 = choiceA;
        var c1 = choiceB;

        if (shuffleChoicesEachStage && UnityEngine.Random.value > 0.5f)
            (c0, c1) = (c1, c0);

        // ✅ set ảnh lựa chọn stage (giữ nguyên trong suốt quá trình)
        if (panel.optionImg1) panel.optionImg1.sprite = c0.optionSprite;
        if (panel.optionImg2) panel.optionImg2.sprite = c1.optionSprite;

        mapped0IsWin = c0.isWin;
        mapped1IsWin = c1.isWin;

        // bind click
        panel.BindChoiceButtons(
            onClick1: () => Choose(0, mapped0IsWin),
            onClick2: () => Choose(1, mapped1IsWin)
        );

        // khóa input trong intro
        if (panel.btnOption1) panel.btnOption1.interactable = false;
        if (panel.btnOption2) panel.btnOption2.interactable = false;
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
        locked = false;
        if (panel?.btnOption1) panel.btnOption1.interactable = true;
        if (panel?.btnOption2) panel.btnOption2.interactable = true;
    }

    private void Choose(int chosenIndex, bool isWin)
    {
        if (locked) return;
        locked = true;

        panel.PlayChoiceResultBG(
            chosenIndex: chosenIndex,
            isWin: isWin,
            winBgSprite: winButtonBg,
            failBgSprite: failButtonBg,
            onDone: () => onResult?.Invoke(isWin)
        );
    }
}
