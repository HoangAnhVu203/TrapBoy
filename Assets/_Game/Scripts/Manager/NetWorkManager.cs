using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class NetWorkManager : Singleton<NetWorkManager>
{

    [Header("Network Check")]
    public float checkInterval = 2f;

    bool isOfflinePanelShown = false;

    void Awake()
    {
        
    }

    void Start()
    {
        StartCoroutine(CheckNetworkLoop());
    }

    IEnumerator CheckNetworkLoop()
    {
        var wait = new WaitForSecondsRealtime(checkInterval);

        while (true)
        {
            bool hasInternet = HasInternet();

            if (!hasInternet && !isOfflinePanelShown)
            {
                isOfflinePanelShown = true;
                UIManager.Instance.OpenUI<PanelMessage>();
                GameManager.Instance.PauseGame();
                // Time.timeScale = 0f;
            }
            else if (hasInternet && isOfflinePanelShown)
            {
                isOfflinePanelShown = false;
                UIManager.Instance.CloseUIDirectly<PanelMessage>();
                GameManager.Instance.ResumeGame();

                // Time.timeScale = 1f;
            }

            yield return wait;
        }
    }

    bool HasInternet()
    {
        return Application.internetReachability != NetworkReachability.NotReachable;
    }
}