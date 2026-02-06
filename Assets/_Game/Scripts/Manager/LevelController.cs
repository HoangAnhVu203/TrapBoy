using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    [Header("Stage Prefabs (order)")]
    [SerializeField] private List<StageController> stagePrefabs = new();

    private StageController currentStage;

    public int StageCount => stagePrefabs != null ? stagePrefabs.Count : 0;

    public StageController SpawnStage(int index, RectTransform parent)
    {
        ClearStage();

        if (stagePrefabs == null || stagePrefabs.Count == 0)
        {
            Debug.LogError("[LevelController] stagePrefabs empty.");
            return null;
        }
        if (index < 0 || index >= stagePrefabs.Count)
        {
            Debug.LogError("[LevelController] SpawnStage invalid index.");
            return null;
        }
        if (parent == null)
        {
            Debug.LogError("[LevelController] parent is null.");
            return null;
        }

        currentStage = Instantiate(stagePrefabs[index], parent);

        // fit full rect
        if (currentStage.transform is RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        return currentStage;
    }

    public void ClearStage()
    {
        if (currentStage != null)
        {
            Destroy(currentStage.gameObject);
            currentStage = null;
        }
    }
}
