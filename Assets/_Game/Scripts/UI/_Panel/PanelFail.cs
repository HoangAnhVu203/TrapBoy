using UnityEngine;

public class PanelFail : UICanvas
{
    public void RollBackStageBTN()
    {
        // LevelManager.Instance.ReplayStage();
    }

    public void RePlayLevelBTN()
    {
        LevelManager.Instance.ReplayLevel();
    }
}
