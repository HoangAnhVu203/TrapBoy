using UnityEngine;

public class PanelFail : UICanvas
{
    public void RollBackStageBTN()
    {
        GameManager.Instance.ReplayStage();
    }

    public void RePlayLevelBTN()
    {
        GameManager.Instance.ReplayLevel();
    }
}
