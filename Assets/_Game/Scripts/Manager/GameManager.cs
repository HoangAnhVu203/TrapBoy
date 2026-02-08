using System.Collections;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    enum GameFlowState
    {
        None,
        Loading,
        StageIntro,
        Gameplay,
        Win,
        Fail
    }

    private GameFlowState state = GameFlowState.None;

    [Header("Runtime")]
    [SerializeField] private int stageIndex;

    private LevelController level;
    private Coroutine stageCR;

    void Start()
    {
    }

    private IEnumerator BootCR()
    {
        SetState(GameFlowState.Loading);

        // Loading on top
        UIManager.Instance.OpenUI<PanelLoading>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        yield return null;

        // Load current level (LevelManager chỉ spawn prefab, không tự start)
        level = LevelManager.Instance.LoadCurrentLevel();
        if (level == null)
        {
            Debug.LogError("[GameManager] Cannot load level.");
            yield break;
        }

        // Đảm bảo gameplay panel đã có instance để lát nữa Open nhanh (optional)
        UIManager.Instance.GetUI<PanelGamePlay>();

        // Tắt loading
        UIManager.Instance.CloseUIDirectly<PanelLoading>();

        // Start stage 0
        stageIndex = 0;
        PlayStage(stageIndex);
    }

    private void PlayStage(int index)
    {
        StopStageCoroutine();
        stageCR = StartCoroutine(StageFlowCR(index));
    }

    private IEnumerator StageFlowCR(int index)
    {
        if (level == null) yield break;

        SetState(GameFlowState.StageIntro);

        // Đóng gameplay trước khi intro stage
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();

        // Bật stage con trong level prefab
        var stage = level.ActivateStage(index);
        if (stage == null) yield break;

        var gameplayPanel = UIManager.Instance.GetUI<PanelGamePlay>();
        stage.BindGameplayUI(gameplayPanel);

        // bind choices nhưng chưa cho bấm
        stage.PrepareGameplay(OnStageResult);

        // chạy intro stage
        yield return stage.PlayIntroCR();

        // mở gameplay và cho input
        SetState(GameFlowState.Gameplay);
        UIManager.Instance.OpenUI<PanelGamePlay>();
        stage.StartGameplayInput();
    }

    private void OnStageResult(bool isWin)
    {
        // chỉ nhận kết quả khi đang gameplay
        if (state != GameFlowState.Gameplay) return;

        if (!isWin)
        {
            SetState(GameFlowState.Fail);
            UIManager.Instance.OpenUI<PanelFail>();
            UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
            return;
        }

        stageIndex++;

        // hết stage -> win level
        if (level == null || stageIndex >= level.StageCount)
        {
            SetState(GameFlowState.Win);
            UIManager.Instance.OpenUI<PanelWin>();
            UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
            return;
        }

        // stage tiếp theo
        PlayStage(stageIndex);
    }

    // ===== UI buttons =====

    public void ReplayStage()
    {
        if (state != GameFlowState.Fail) return;

        UIManager.Instance.CloseUIDirectly<PanelFail>();
        PlayStage(stageIndex);
    }

    public void ReplayLevel()
    {
        // cho phép gọi từ Win hoặc Fail đều được
        UIManager.Instance.CloseUIDirectly<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        StopStageCoroutine();

        level = LevelManager.Instance.ReplayLevel();
        if (level == null)
        {
            Debug.LogError("[GameManager] ReplayLevel failed: level is null.");
            return;
        }

        stageIndex = 0;
        PlayStage(stageIndex);
    }

    public void NextLevel()
    {
        if (state != GameFlowState.Win) return;

        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        StopStageCoroutine();

        level = LevelManager.Instance.NextLevel();
        if (level == null)
        {
            Debug.LogError("[GameManager] NextLevel failed: level is null.");
            return;
        }

        stageIndex = 0;
        PlayStage(stageIndex);
    }

    // ===== utils =====

    private void SetState(GameFlowState newState)
    {
        state = newState;
        // Debug.Log($"[GameManager] State = {state}");
    }

    private void StopStageCoroutine()
    {
        if (stageCR != null)
        {
            StopCoroutine(stageCR);
            stageCR = null;
        }
    }
}
