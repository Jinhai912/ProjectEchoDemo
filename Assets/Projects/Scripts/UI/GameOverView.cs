using TMPro;
using UnityEngine;

/// <summary>
/// 结算面板的本局摘要。
/// </summary>
public class GameOverView : MonoBehaviour
{
    public TextMeshProUGUI statsText;

    private void OnEnable()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (statsText == null) return;

        PlayerData player = PlayerData.Instance;
        GameManager game = GameManager.Instance;
        int floor = game != null ? game.currentFloor : 1;
        int fragments = player != null ? player.currentCurrency : 0;
        string core = player != null && player.currentCore != null ? player.currentCore.DisplayName : "未安装";
        string barrel = player != null && player.currentBarrel != null ? player.currentBarrel.DisplayName : "未安装";
        string sink = player != null && player.currentHeatSink != null ? player.currentHeatSink.DisplayName : "未安装";

        statsText.text =
            $"抵达层数 / FLOOR      {floor}\n" +
            $"剩余碎片 / SHARDS     {fragments}\n" +
            $"核心炉 / CORE         {core}\n" +
            $"枪管 / BARREL         {barrel}\n" +
            $"散热器 / HEAT SINK    {sink}";
    }
}
