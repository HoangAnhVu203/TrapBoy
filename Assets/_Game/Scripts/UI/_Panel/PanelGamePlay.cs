using UnityEngine;
using UnityEngine.UI;

public class PanelGamePlay : UICanvas
{
    public RectTransform stageRoot;

    public void OpenSettingBTN()
    {
        UIManager.Instance.OpenUI<PanelSetting>();
    }
}
