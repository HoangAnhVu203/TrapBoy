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

    [Header("Save")]
    [SerializeField] private bool useSavedLevelOnBoot = true;

    private const string PREF_LEVEL_INDEX = "CURRENT_LEVEL_INDEX";

    private int currentLevelIndex;
    private GameObject currentLevelGO;
    private LevelController currentLevel;

    public int CurrentLevelIndex => currentLevelIndex;
    public LevelController CurrentLevel => currentLevel;
    public int TotalLevels => levelPrefabs != null ? levelPrefabs.Count : 0;

    void Awake()
    {
        if (useSavedLevelOnBoot && HasSavedLevel())
        {
            currentLevelIndex = GetSavedLevelIndexSafe();
        }
        else
        {
            currentLevelIndex = ClampLevelIndex(startLevelIndex);
        }
    }

    void Start()
    {
        // TODO: 
        // LoadCurrentLevel();
    }

    // ===================== PUBLIC API =====================

    public LevelController LoadLevel(int levelIndex)
    {
        if (levelPrefabs == null || levelPrefabs.Count == 0)
        {
            Debug.LogError("[LevelManager] levelPrefabs is empty.");
            return null;
        }

        levelIndex = ClampLevelIndex(levelIndex);

        ClearCurrentLevel();

        currentLevelIndex = levelIndex;

        SaveCurrentLevelIndex(currentLevelIndex);

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

    public LevelController LoadCurrentLevel()
    {
        return LoadLevel(currentLevelIndex);
    }

    public LevelController NextLevel()
    {
        if (levelPrefabs == null || levelPrefabs.Count == 0) return null;

        int next = currentLevelIndex + 1;
        if (next >= levelPrefabs.Count) next = 0;

        return LoadLevel(next);
    }

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

    // ===================== SAVE / LOAD =====================

    public void SaveCurrentLevelIndex(int levelIndex)
    {
        PlayerPrefs.SetInt(PREF_LEVEL_INDEX, levelIndex);
        PlayerPrefs.Save();
    }

    public bool HasSavedLevel()
    {
        return PlayerPrefs.HasKey(PREF_LEVEL_INDEX);
    }

    public int GetSavedLevelIndexSafe()
    {
        if (!HasSavedLevel()) return ClampLevelIndex(startLevelIndex);

        int saved = PlayerPrefs.GetInt(PREF_LEVEL_INDEX, startLevelIndex);
        return ClampLevelIndex(saved);
    }

    public void ClearSavedLevel()
    {
        if (PlayerPrefs.HasKey(PREF_LEVEL_INDEX))
        {
            PlayerPrefs.DeleteKey(PREF_LEVEL_INDEX);
            PlayerPrefs.Save();
        }
    }

    // ===================== UTILS =====================

    private int ClampLevelIndex(int idx)
    {
        if (levelPrefabs == null || levelPrefabs.Count == 0) return 0;
        return Mathf.Clamp(idx, 0, levelPrefabs.Count - 1);
    }
}
