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
    private StageContext ctx = new StageContext();

    [Header("Runtime")]
    [SerializeField] private int stageIndex;

    private LevelController level;
    private Coroutine stageCR;

    void Start()
    {
        StartCoroutine(BootCR());
    }

    private IEnumerator BootCR()
    {
        SetState(GameFlowState.Loading);

        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        level = LevelManager.Instance.LoadCurrentLevel();
        if (level == null)
        {
            Debug.LogError("[GameManager] Cannot load level.");
            yield break;
        }

        UIManager.Instance.GetUI<PanelGamePlay>();

        stageIndex = 0;
        UpdateHUD();

        PlayStageWithLoading(stageIndex, 5f);
    }

    private void PlayStage(int index)
    {
        StopStageCoroutine();
        stageCR = StartCoroutine(StageFlowCR(index));
    }

    private IEnumerator StageFlowCR(int index)
    {
        if (level == null) yield break;

        stageIndex = index;
        UpdateHUD();

        SetState(GameFlowState.StageIntro);

        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        var stage = level.ActivateStage(index);
        stage.SetContext(ctx);
        if (stage == null) yield break;

        var gameplayPanel = UIManager.Instance.GetUI<PanelGamePlay>();
        if (gameplayPanel == null)
        {
            Debug.LogError("[GameManager] Missing PanelGamePlay prefab/instance.");
            yield break;
        }

        stage.BindGameplayUI(gameplayPanel);
        stage.PrepareGameplay(OnStageResult);

        yield return stage.PlayIntroCR();

        SetState(GameFlowState.Gameplay);
        UIManager.Instance.OpenUI<PanelGamePlay>();

        stage.StartGameplayInput();

    }

    private void OnStageResult(bool isWin)
    {
        if (state != GameFlowState.Gameplay) return;

        if (!isWin)
        {
            SetState(GameFlowState.Fail);
            UIManager.Instance.OpenUI<PanelFail>();
            UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
            return;
        }

        // win stage => tăng stage
        stageIndex++;

        // update progress theo stage vừa hoàn thành
        UpdateHUD();

        if (level == null || stageIndex >= level.StageCount)
        {
            SetState(GameFlowState.Win);
            UIManager.Instance.OpenUI<PanelWin>();
            UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
            return;
        }

        PlayStageWithLoading(stageIndex, 1.0f);
    }

    // ===== UI buttons =====

    public void ReplayStage()
    {
        if (state != GameFlowState.Fail) return;

        UIManager.Instance.CloseUIDirectly<PanelFail>();
        PlayStageWithLoading(stageIndex, 1.0f);
    }


    public void ReplayLevel()
    {
        UIManager.Instance.CloseUIDirectly<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        StopStageCoroutine();

        level = LevelManager.Instance.ReplayLevel();
        if (level == null)
        {
            Debug.LogError("[GameManager] ReplayLevel failed.");
            return;
        }

        stageIndex = 0;
        UpdateHUD();

        PlayStageWithLoading(stageIndex, 1.0f);
    }

    public void NextLevel()
    {
        if (state != GameFlowState.Win) return;

        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        StopStageCoroutine();

        MoneyManager.Instance.RewardLevelComplete();
        level = LevelManager.Instance.NextLevel();
        if (level == null)
        {
            Debug.LogError("[GameManager] NextLevel failed.");
            return;
        }

        stageIndex = 0;
        UpdateHUD();

        PlayStageWithLoading(stageIndex, 1.0f);
    }


    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    // ===== utils =====

    private void SetState(GameFlowState newState)
    {
        state = newState;
    }

    private void StopStageCoroutine()
    {
        if (stageCR != null)
        {
            StopCoroutine(stageCR);
            stageCR = null;
        }
    }

    private void UpdateHUD()
    {
        var panel = UIManager.Instance.GetUI<PanelGamePlay>();
        if (panel == null || level == null) return;

        int totalLevels = LevelManager.Instance.TotalLevels;
        int cur = LevelManager.Instance.CurrentLevelIndex + 1;

        int next = cur;
        if (totalLevels > 0)
        {
            int nextIndex = (LevelManager.Instance.CurrentLevelIndex + 1) % totalLevels;
            next = nextIndex + 1;
        }

        panel.SetLevelInfo(cur, next);
        panel.SetStageProgress(stageIndex, level.StageCount);
    }

    private IEnumerator ShowLoadingCR(float seconds)
    {
        var loading = UIManager.Instance.OpenUI<PanelLoading>();
        loading.Play(seconds);

        yield return new WaitForSecondsRealtime(seconds);

        UIManager.Instance.CloseUIDirectly<PanelLoading>();
    }



    private void PlayStageWithLoading(int index, float loadingSeconds)
    {
        StopStageCoroutine();
        stageCR = StartCoroutine(PlayStageWithLoadingCR(index, loadingSeconds));
    }

    private IEnumerator PlayStageWithLoadingCR(int index, float loadingSeconds)
    {
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();

        yield return StartCoroutine(ShowLoadingCR(loadingSeconds));

        yield return StartCoroutine(StageFlowCR(index));
    }

}
