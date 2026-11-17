using TMPro;
using UnityEngine;

/// <summary>
/// 地图面板顶部信息。
/// </summary>
public class MapHeaderView : MonoBehaviour
{
    public TextMeshProUGUI floorText;
    public TextMeshProUGUI currencyText;

    private int _lastFloor = int.MinValue;
    private int _lastCurrency = int.MinValue;

    private void OnEnable()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    private void Refresh()
    {
        GameManager game = GameManager.Instance;
        PlayerData player = PlayerData.Instance;
        int floor = game != null ? game.currentFloor : 1;
        int currency = player != null ? player.currentCurrency : 0;

        if (floorText != null && floor != _lastFloor)
        {
            _lastFloor = floor;
            floorText.text = $"第 {floor} 层 / FLOOR {floor:00}";
        }

        if (currencyText != null && currency != _lastCurrency)
        {
            _lastCurrency = currency;
            currencyText.text = $"碎片 {currency}";
        }
    }
}
