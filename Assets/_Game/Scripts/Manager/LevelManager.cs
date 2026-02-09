using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager>
{
    [Header("Levels (prefabs có LevelController)")]
    [SerializeField] private List<GameObject> levelPrefabs = new();

    [Header("Spawn Root (world/canvas)")]
    [SerializeField] private Transform levelRoot;

    [Header("Start Options")]
    [SerializeField] private int startLevelIndex = 0;

    private int currentLevelIndex;
    private GameObject currentLevelGO;
    private LevelController currentLevel;

    public int CurrentLevelIndex => currentLevelIndex;
    public LevelController CurrentLevel => currentLevel;
    public int TotalLevels => levelPrefabs != null ? levelPrefabs.Count : 0;

    void Awake()
    {
        currentLevelIndex = Mathf.Clamp(startLevelIndex, 0, Mathf.Max(0, levelPrefabs.Count - 1));
    }

    /// Load level theo index (spawn prefab level, KHÔNG tự StartLevel)
    public LevelController LoadLevel(int levelIndex)
    {
        if (levelPrefabs == null || levelPrefabs.Count == 0)
        {
            Debug.LogError("[LevelManager] levelPrefabs is empty.");
            return null;
        }

        levelIndex = Mathf.Clamp(levelIndex, 0, levelPrefabs.Count - 1);

        // clear level cũ
        ClearCurrentLevel();

        currentLevelIndex = levelIndex;

        // spawn level mới
        var prefab = levelPrefabs[currentLevelIndex];
        currentLevelGO = Instantiate(prefab, levelRoot != null ? levelRoot : transform);
        currentLevel = currentLevelGO.GetComponentInChildren<LevelController>(true);

        if (currentLevel == null)
        {
            Debug.LogError("[LevelManager] Level prefab missing LevelController component.");
            return null;
        }

        return currentLevel;
    }

    /// Load level hiện tại
    public LevelController LoadCurrentLevel()
    {
        return LoadLevel(currentLevelIndex);
    }

    /// Sang level tiếp theo (wrap về 0 nếu hết)
    public LevelController NextLevel()
    {
        if (levelPrefabs == null || levelPrefabs.Count == 0) return null;

        int next = currentLevelIndex + 1;
        if (next >= levelPrefabs.Count) next = 0;

        return LoadLevel(next);
    }

    /// Chơi lại level hiện tại (reload prefab level)
    public LevelController ReplayLevel()
    {
        return LoadLevel(currentLevelIndex);
    }

    public void ClearCurrentLevel()
    {
        if (currentLevelGO != null)
        {
            Destroy(currentLevelGO);
            currentLevelGO = null;
            currentLevel = null;
        }
    }

    public int NextLevelIndex
    {
        get
        {
            if (TotalLevels == 0) return 0;
            return (currentLevelIndex + 1) % TotalLevels;
        }
    }
}
