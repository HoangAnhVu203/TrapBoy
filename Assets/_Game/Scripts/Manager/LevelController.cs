using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Stages in this Level (drag children here in order)")]
    [SerializeField] private List<StageController> stages = new();

    [Header("Auto")]
    [SerializeField] private bool autoResetToFirstStageOnEnable = false; 

    private int currentIndex = -1;

    public int StageCount => stages != null ? stages.Count : 0;
    public int CurrentStageIndex => currentIndex;

    void Awake()
    {
        SetAllStagesActive(false);
        currentIndex = -1;
    }

    void OnEnable()
    {
        if (autoResetToFirstStageOnEnable)
            ResetToStage1();
    }

    public StageController ActivateStage(int index)
    {
        if (stages == null || stages.Count == 0)
        {
            Debug.LogError("[LevelController] stages list is empty. Drag StageControllers into the list.");
            return null;
        }

        if (index < 0 || index >= stages.Count)
        {
            Debug.LogError($"[LevelController] ActivateStage invalid index: {index}");
            return null;
        }

        // tắt stage hiện tại
        if (currentIndex >= 0 && currentIndex < stages.Count && stages[currentIndex] != null)
            stages[currentIndex].gameObject.SetActive(false);

        // bật stage mới
        currentIndex = index;

        var stage = stages[currentIndex];
        if (stage == null)
        {
            Debug.LogError("[LevelController] StageController is null in list.");
            return null;
        }

        stage.gameObject.SetActive(true);
        stage.transform.SetAsLastSibling();

        return stage;
    }

    public StageController ActivateFirstStage()
    {
        return ActivateStage(0);
    }

    public StageController ResetToStage1()
    {
        DeactivateAllStages();
        return ActivateFirstStage();
    }

    public void DeactivateAllStages()
    {
        SetAllStagesActive(false);
        currentIndex = -1;
    }

    private void SetAllStagesActive(bool active)
    {
        if (stages == null) return;
        for (int i = 0; i < stages.Count; i++)
        {
            if (stages[i] != null)
                stages[i].gameObject.SetActive(active);
        }
    }
}
