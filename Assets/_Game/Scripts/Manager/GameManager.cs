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

    private int stageIndex;

    private PanelGamePlay panelGamePlay;
    private LevelController level;

    private Coroutine stageCR;

    #region BOOT

    void Start()
    {
        StartCoroutine(BootCR());
    }

    private IEnumerator BootCR()
    {
        SetState(GameFlowState.Loading);

        UIManager.Instance.OpenUI<PanelLoading>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        yield return null;

        // Load level
        level = LevelManager.Instance.LoadCurrentLevel();
        if (level == null)
        {
            Debug.LogError("[GameManager] Cannot load level.");
            yield break;
        }

        panelGamePlay = UIManager.Instance.GetUI<PanelGamePlay>();
        if (panelGamePlay == null || panelGamePlay.stageRoot == null)
        {
            Debug.LogError("[GameManager] PanelGamePlay or stageRoot missing.");
            yield break;
        }

        UIManager.Instance.CloseUIDirectly<PanelLoading>();

        stageIndex = 0;
        PlayStage(stageIndex);
    }

    #endregion

    #region STAGE FLOW

    private void PlayStage(int index)
    {
        StopStageCoroutine();
        stageCR = StartCoroutine(StageFlowCR(index));
    }

    private IEnumerator StageFlowCR(int index)
    {
        SetState(GameFlowState.StageIntro);

        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();

        var stage = level.SpawnStage(index, panelGamePlay.stageRoot);
        if (stage == null) yield break;

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

        stageIndex++;

        if (stageIndex >= level.StageCount)
        {
            SetState(GameFlowState.Win);
            UIManager.Instance.OpenUI<PanelWin>();
            UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
            return;
        }

        PlayStage(stageIndex);
    }

    #endregion

    #region BUTTON API (UI CALLS)

    public void ReplayStage()
    {
        if (state != GameFlowState.Fail) return;

        UIManager.Instance.CloseUIDirectly<PanelFail>();
        PlayStage(stageIndex);
    }

    public void ReplayLevel()
    {
        UIManager.Instance.CloseUIDirectly<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();

        level = LevelManager.Instance.ReplayLevel();
        stageIndex = 0;

        PlayStage(stageIndex);
    }

    public void NextLevel()
    {
        if (state != GameFlowState.Win) return;

        UIManager.Instance.CloseUIDirectly<PanelWin>();

        level = LevelManager.Instance.NextLevel();
        stageIndex = 0;

        PlayStage(stageIndex);
    }

    #endregion

    #region STATE / UTILS

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

    #endregion
}
