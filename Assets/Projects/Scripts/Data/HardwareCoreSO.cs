using UnityEngine;

[CreateAssetMenu(fileName = "HW_C_001", menuName = "Game/Hardware/Core")]
public class HardwareCoreSO : ScriptableObject
{
    public string hardwareID;
    public string hardwareName;
    public int memoryCapacity = 1024; // 提供多少 MB 内存
    public int healthBonus = 0;       // 额外血量加成
    public float energyRegen = 10f;   // 能量恢复速度
}