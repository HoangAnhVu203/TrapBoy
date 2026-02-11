using UnityEngine;

public class MoneyManager : Singleton<MoneyManager>
{
    private const string KEY_MONEY = "MONEY_BALANCE";
    private const string KEY_INIT  = "MONEY_INITED";

    [Header("Config")]
    [SerializeField] private int startMoney = 200000;
    [SerializeField] private int costPerChoice = 30000;
    [SerializeField] private int rewardPerLevel = 150000;

    public int Balance { get; private set; }

     void Awake()
    {
        LoadOrInit();
    }

    private void LoadOrInit()
    {
        if (!PlayerPrefs.HasKey(KEY_INIT) || PlayerPrefs.GetInt(KEY_INIT, 0) == 0)
        {
            Balance = startMoney;
            PlayerPrefs.SetInt(KEY_MONEY, Balance);
            PlayerPrefs.SetInt(KEY_INIT, 1);
            PlayerPrefs.Save();
            return;
        }

        Balance = PlayerPrefs.GetInt(KEY_MONEY, startMoney);
    }

    public void ResetMoney()
    {
        Balance = startMoney;
        PlayerPrefs.SetInt(KEY_MONEY, Balance);
        PlayerPrefs.SetInt(KEY_INIT, 1);
        PlayerPrefs.Save();
    }

    public bool CanSpend(int amount) => Balance >= amount;

    public bool SpendChoice()
    {
        return Spend(costPerChoice);
    }

    public bool Spend(int amount)
    {
        if (amount <= 0) return true;
        if (Balance < amount) return false;

        Balance -= amount;
        Save();
        return true;
    }

    public void RewardLevelComplete()
    {
        Add(rewardPerLevel);
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Balance += amount;
        Save();
    }

    private void Save()
    {
        PlayerPrefs.SetInt(KEY_MONEY, Balance);
        PlayerPrefs.Save();
    }
}
