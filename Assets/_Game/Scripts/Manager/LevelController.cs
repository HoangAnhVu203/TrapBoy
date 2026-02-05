using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelController : MonoBehaviour
{
    [Serializable]
    public class ChoiceData
    {
        public Sprite sprite;
        public bool isWin;
    }

    [Serializable]
    public class StageData
    {
        public ChoiceData choiceA;
        public ChoiceData choiceB;
        public string stageTitle; // optional
    }

    [Header("Level Data (1-4 stages)")]
    [SerializeField] private List<StageData> stages = new();

    [Header("Options")]
    [SerializeField] private bool shuffleChoicesEachStage = true;

    // runtime refs từ PanelGamePlay
    private PanelGamePlay panel;
    private Button buttonA;
    private Image buttonAImage;
    private Button buttonB;
    private Image buttonBImage;

    private int currentStageIndex = 0;
    private bool locked = false;

    // outcome ứng với button A/B tại stage hiện tại
    private bool runtimeChoice0IsWin;
    private bool runtimeChoice1IsWin;

    void OnEnable()
    {
        StartLevel();
    }

    public void StartLevel()
    {
        if (stages == null || stages.Count == 0)
        {
            Debug.LogError("[LevelController] stages is empty.");
            return;
        }

        currentStageIndex = 0;
        locked = false;

        // 1) Mở UI gameplay và lấy instance ngay (UIManager của bạn trả về instance)
        panel = UIManager.Instance.OpenUI<PanelGamePlay>();

        UIManager.Instance.CloseUIDirectly<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();

        // 2) Cache refs
        CacheUIRefs(panel);

        // 3) Bind click
        BindButtons();

        // 4) Show stage đầu
        ShowStage(currentStageIndex);
    }

    private void CacheUIRefs(PanelGamePlay p)
    {
        if (p == null)
        {
            Debug.LogError("[LevelController] PanelGamePlay instance is null.");
            return;
        }

        buttonA = p.btnA;
        buttonAImage = p.imgA;
        buttonB = p.btnB;
        buttonBImage = p.imgB;

        if (buttonA == null || buttonAImage == null || buttonB == null || buttonBImage == null)
        {
            Debug.LogError("[LevelController] Missing btn/img refs in PanelGamePlay prefab. Please assign in Inspector.");
        }
    }

    private void BindButtons()
    {
        if (buttonA != null)
        {
            buttonA.onClick.RemoveAllListeners();
            buttonA.onClick.AddListener(() => OnChoose(0));
        }

        if (buttonB != null)
        {
            buttonB.onClick.RemoveAllListeners();
            buttonB.onClick.AddListener(() => OnChoose(1));
        }
    }

    private void ShowStage(int stageIndex)
    {
        if (stageIndex < 0 || stageIndex >= stages.Count)
        {
            Debug.LogError($"[LevelController] Invalid stageIndex: {stageIndex}");
            return;
        }

        if (buttonAImage == null || buttonBImage == null) return;

        locked = false;

        var stage = stages[stageIndex];
        var c0 = stage.choiceA;
        var c1 = stage.choiceB;

        if (c0 == null || c1 == null)
        {
            Debug.LogError("[LevelController] Stage choice is null.");
            return;
        }

        // Random swap để tránh đoán vị trí
        if (shuffleChoicesEachStage && UnityEngine.Random.value > 0.5f)
            (c0, c1) = (c1, c0);

        // ĐỔI SPRITE LIÊN TỤC THEO STAGE
        buttonAImage.sprite = c0.sprite;
        buttonBImage.sprite = c1.sprite;

        // Lưu kết quả Win/Fail ứng với từng button
        runtimeChoice0IsWin = c0.isWin;
        runtimeChoice1IsWin = c1.isWin;

        if (buttonA) buttonA.interactable = true;
        if (buttonB) buttonB.interactable = true;
    }

    private void OnChoose(int buttonIndex)
    {
        if (locked) return;
        locked = true;

        bool isWin = (buttonIndex == 0) ? runtimeChoice0IsWin : runtimeChoice1IsWin;

        if (isWin) GoNextStageOrWin();
        else OpenFail();
    }

    private void GoNextStageOrWin()
    {
        int next = currentStageIndex + 1;

        if (next < stages.Count)
        {
            currentStageIndex = next;
            ShowStage(currentStageIndex); // <-- đây là chỗ đổi sprite stage tiếp
        }
        else
        {
            OpenWin();
        }
    }

    public void OpenWin()
    {
        UIManager.Instance.OpenUI<PanelWin>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
        UIManager.Instance.CloseUIDirectly<PanelFail>();
    }

    public void OpenFail()
    {
        UIManager.Instance.OpenUI<PanelFail>();
        UIManager.Instance.CloseUIDirectly<PanelGamePlay>();
        UIManager.Instance.CloseUIDirectly<PanelWin>();
    }

    public void Retry()
    {
        // Nếu LevelManager destroy & instantiate level lại thì gọi LevelManager.
        // Còn nếu muốn retry trong cùng instance:
        currentStageIndex = 0;
        ShowStage(currentStageIndex);
    }
}
