using UnityEngine;
using UnityEngine.UI;

public class PanelGamePlay : UICanvas
{
    public Button btnOption1;
    public Image imgOption1;

    public Button btnOption2;
    public Image imgOption2;

    public void OpenSettingBTN()
    {
        UIManager.Instance.OpenUI<PanelSetting>();
    }
}
