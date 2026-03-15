using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 地图面板事件控制器
/// 负责绑定节点按钮点击事件至 GameManager。
/// </summary>
public class MapPanelController : MonoBehaviour
{
    [Header("关卡数据")]
    // 为每个按钮都指定一个关卡数据文件
    public EncounterData encounterForButton1;
    public EncounterData encounterForButton2;
    
    [Header("按钮引用")]
    public Button nextEncounterButton1;
    public Button nextEncounterButton2;
    

    void Start()
    {
        // --- 核心逻辑：在 Start 中动态绑定按钮事件 ---

        if (nextEncounterButton1 != null)
        {
            nextEncounterButton1.onClick.RemoveAllListeners();
            // 当按钮被点击时，调用 GameManager 的 StartEncounter，并把对应的关卡数据传过去
            nextEncounterButton1.onClick.AddListener(() => {
                GameManager.Instance.StartEncounter(encounterForButton1);
                UIManager.Instance.HideMapPanel(); // 这行可以移到 GameManager 中
            });
        }
        
        if (nextEncounterButton2 != null)
        {
            nextEncounterButton2.onClick.RemoveAllListeners();
            nextEncounterButton2.onClick.AddListener(() =>
            {
                GameManager.Instance.StartEncounter(encounterForButton2);
                UIManager.Instance.HideMapPanel();
            });
        }
    }
}