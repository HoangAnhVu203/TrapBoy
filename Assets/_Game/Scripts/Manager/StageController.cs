using System;
using System.Collections;
using UnityEngine;

public class StageController : MonoBehaviour
{
    [Serializable]
    public class RouteAnimSet
    {
        public string introAnim = "intro";
        public string idleAnim  = "idle";
        public string winAnim   = "win";
        public string loseAnim  = "lose";
    }

    [Serializable]
    public class ChoiceData
    {
        public Sprite optionSprite;
        public bool isWin;

        public BranchRoute nextRoute = BranchRoute.None;
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

    [Header("VFX")]
    [SerializeField] private StageVFXPlayer vfxPlayer;
    [SerializeField] private StageVFXConfig vfxConfig;
    [SerializeField] private RectTransform uiVfxRoot;

    [Header("Branch Route (optional)")]
    [SerializeField] private bool useBranchRoute = false;
    [SerializeField] private RouteAnimSet defaultSet = new RouteAnimSet();
    [SerializeField] private RouteAnimSet route1Set = new RouteAnimSet
    {
        introAnim = "intro1", idleAnim = "idle1", winAnim = "win1", loseAnim = "lose1"
    };
    [SerializeField] private RouteAnimSet route2Set = new RouteAnimSet
    {
        introAnim = "intro2", idleAnim = "idle2", winAnim = "win2", loseAnim = "lose2"
    };

    private StageContext ctx;
    public void SetContext(StageContext context) => ctx = context;


    [Header("Options")]
    [SerializeField] private bool shuffleChoicesEachStage = true;

    private PanelGamePlay panel;
    private Action<bool> onResult;
    private bool locked;
    private bool mapped0IsWin, mapped1IsWin;
    private BranchRoute mapped0Route, mapped1Route;

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

        // ===== VFX =====
        if (!vfxPlayer) vfxPlayer = GetComponentInChildren<StageVFXPlayer>(true);

        if (!uiVfxRoot) uiVfxRoot = panel.transform as RectTransform;

        if (vfxPlayer != null)
        {
            vfxPlayer.ClearAll();

            if (character != null && vfxConfig != null)
                vfxPlayer.Bind(character, vfxConfig, uiVfxRoot, null);
            else
                vfxPlayer.Unbind(); 
        }

        // ===== UI choices =====
        panel.ResetOptionsUI();

        var c0 = choiceA;
        var c1 = choiceB;

        if (shuffleChoicesEachStage && UnityEngine.Random.value > 0.5f)
            (c0, c1) = (c1, c0);

        if (panel.optionImg1) panel.optionImg1.sprite = c0.optionSprite;
        if (panel.optionImg2) panel.optionImg2.sprite = c1.optionSprite;

        mapped0IsWin = c0.isWin;
        mapped1IsWin = c1.isWin;

        mapped0Route = c0.nextRoute;
        mapped1Route = c1.nextRoute;

        panel.BindChoiceButtons(
            onClick1: () => Choose(0, mapped0IsWin, mapped0Route),
            onClick2: () => Choose(1, mapped1IsWin, mapped1Route)
        );


        // khóa input cho tới khi intro xong
        if (panel.btnOption1) panel.btnOption1.interactable = false;
        if (panel.btnOption2) panel.btnOption2.interactable = false;
    }

    // ĐỢI SPINE INTRO THẬT SỰ XONG
    public IEnumerator PlayIntroCR()
    {
        if (character != null)
        {
            var set = GetAnimSet();

            bool done = false;
            character.PlayIntroThenIdle(set.introAnim, set.idleAnim, () => done = true);
            yield return new WaitUntil(() => done);
            yield break;
        }

        float t = Mathf.Max(0f, introSeconds);
        if (t > 0f) yield return new WaitForSecondsRealtime(t);
    }


    public void StartGameplayInput()
    {
        locked = false;
        if (panel?.btnOption1) panel.btnOption1.interactable = true;
        if (panel?.btnOption2) panel.btnOption2.interactable = true;
    }

    //  UI anim xong -> chạy SPINE win/lose -> xong mới trả kết quả
    private void Choose(int chosenIndex, bool isWin, BranchRoute nextRoute)
    {
        if (locked || resultSent) return;
        locked = true;

        if (ctx != null)
        {
            ctx.route = nextRoute;
            ctx.lastChosenIndex = chosenIndex;
        }

        panel.PlayChoiceResultBG(
            chosenIndex: chosenIndex,
            isWin: isWin,
            winBgSprite: winButtonBg,
            failBgSprite: failButtonBg,
            onDone: () =>
            {
                if (character == null)
                {
                    StartCoroutine(ResultDelayCR(isWin));
                    return;
                }

                var set = GetAnimSet();
                bool bothWin = mapped0IsWin && mapped1IsWin;

                bool animWinFlag = isWin;

                if (bothWin && isWin)
                {
                    animWinFlag = (chosenIndex == 0);
                }

                string anim = animWinFlag ? set.winAnim : set.loseAnim;

                character.PlayResultNoReturn(anim, animWinFlag, () =>
                {
                    StartCoroutine(ResultDelayCR(isWin));
                });

            }
        );
    }

    private void SendResult(bool isWin)
    {
        if (resultSent) return;
        resultSent = true;
        onResult?.Invoke(isWin);
    }

    private IEnumerator ResultDelayCR(bool isWin)
    {
        yield return new WaitForSecondsRealtime(1.25f);
        SendResult(isWin);
    }

    RouteAnimSet GetAnimSet()
    {
        if (!useBranchRoute) return defaultSet;

        var r = ctx != null ? ctx.route : BranchRoute.None;

        return r switch
        {
            BranchRoute.Route1 => route1Set,
            BranchRoute.Route2 => route2Set,
            _ => defaultSet
        };
    }

}
