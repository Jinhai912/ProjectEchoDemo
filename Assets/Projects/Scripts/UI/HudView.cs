using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 战斗 HUD：热量、碎片、三个硬件槽。
/// 描述：生命/内存继续由 UIManager 驱动；这里负责新增的可见信息。
/// </summary>
public class HudView : MonoBehaviour
{
    public Image heatFill;
    public TextMeshProUGUI heatLabel;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI coreText;
    public TextMeshProUGUI barrelText;
    public TextMeshProUGUI sinkText;

    private int _lastCurrency = int.MinValue;
    private HardwareCoreSO _lastCore;
    private HardwareBarrelSO _lastBarrel;
    private HardwareHeatSinkSO _lastSink;

    public void SetHeat(float current, float max, bool overheated)
    {
        if (heatFill != null)
        {
            heatFill.fillAmount = max > 0f ? Mathf.Clamp01(current / max) : 0f;
            heatFill.color = overheated ? InkUI.Accent : InkUI.Ink;
        }

        if (heatLabel != null)
        {
            heatLabel.text = overheated
                ? "过热 / OVERHEAT"
                : $"热量 {Mathf.RoundToInt(current)} / {Mathf.RoundToInt(max)}";
            heatLabel.color = overheated ? InkUI.Accent : InkUI.Ink;
        }
    }

    private void Update()
    {
        PlayerData player = PlayerData.Instance;
        if (player == null) return;

        if (currencyText != null && player.currentCurrency != _lastCurrency)
        {
            _lastCurrency = player.currentCurrency;
            currencyText.text = $"碎片 {player.currentCurrency}";
        }

        if (coreText != null && player.currentCore != _lastCore)
        {
            _lastCore = player.currentCore;
            coreText.text = _lastCore != null ? _lastCore.DisplayName : "未安装";
        }

        if (barrelText != null && player.currentBarrel != _lastBarrel)
        {
            _lastBarrel = player.currentBarrel;
            barrelText.text = _lastBarrel != null ? _lastBarrel.DisplayName : "未安装";
        }

        if (sinkText != null && player.currentHeatSink != _lastSink)
        {
            _lastSink = player.currentHeatSink;
            sinkText.text = _lastSink != null ? _lastSink.DisplayName : "未安装";
        }
    }
}
