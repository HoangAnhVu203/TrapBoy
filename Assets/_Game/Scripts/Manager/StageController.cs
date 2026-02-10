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

    [Header("Character (Spine)")]
    [SerializeField] private CharacterController character;

    [Header("Intro (fallback nếu không có character)")]
    [SerializeField] private float introSeconds = 1.0f;

    [Header("Options")]
    [SerializeField] private bool shuffleChoicesEachStage = true;

    private PanelGamePlay panel;
    private Action<bool> onResult;
    private bool locked;
    private bool mapped0IsWin, mapped1IsWin;
    private bool resultSent;

    public void BindGameplayUI(PanelGamePlay p) => panel = p;

    public void PrepareGameplay(Action<bool> onResultCallback)
    {
        onResult = onResultCallback;
        locked = false;
        resultSent = false;

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

        if (panel.optionImg1) panel.optionImg1.sprite = c0.optionSprite;
        if (panel.optionImg2) panel.optionImg2.sprite = c1.optionSprite;

        mapped0IsWin = c0.isWin;
        mapped1IsWin = c1.isWin;

        panel.BindChoiceButtons(
            onClick1: () => Choose(0, mapped0IsWin),
            onClick2: () => Choose(1, mapped1IsWin)
        );

        // khóa input cho tới khi intro xong
        if (panel.btnOption1) panel.btnOption1.interactable = false;
        if (panel.btnOption2) panel.btnOption2.interactable = false;
    }

    // ✅ ĐỢI SPINE INTRO THẬT SỰ XONG
    public IEnumerator PlayIntroCR()
    {
        if (character != null)
        {
            bool done = false;
            character.PlayIntroThenIdle(() => done = true);
            yield return new WaitUntil(() => done);
            yield break;
        }

        // fallback nếu không có character
        float t = Mathf.Max(0f, introSeconds);
        if (t > 0f) yield return new WaitForSecondsRealtime(t);
    }

    public void StartGameplayInput()
    {
        locked = false;
        if (panel?.btnOption1) panel.btnOption1.interactable = true;
        if (panel?.btnOption2) panel.btnOption2.interactable = true;
    }

    // ✅ UI anim xong -> chạy SPINE win/lose -> xong mới trả kết quả
    private void Choose(int chosenIndex, bool isWin)
    {
        if (locked || resultSent) return;
        locked = true;

        if (panel == null)
        {
            Debug.LogError("[StageController] Choose: panel is null.");
            locked = false;
            return;
        }

        panel.PlayChoiceResultBG(
            chosenIndex: chosenIndex,
            isWin: isWin,
            winBgSprite: winButtonBg,
            failBgSprite: failButtonBg,
            onDone: () =>
            {
                // sau UI anim, chạy spine result
                if (character == null)
                {
                    SendResult(isWin);
                    return;
                }

                if (isWin)
                    character.PlayWinThenIdle(() => SendResult(true));
                else
                    character.PlayLoseThenIdle(() => SendResult(false));
            }
        );
    }

    private void SendResult(bool isWin)
    {
        if (resultSent) return;
        resultSent = true;
        onResult?.Invoke(isWin);
    }
}
