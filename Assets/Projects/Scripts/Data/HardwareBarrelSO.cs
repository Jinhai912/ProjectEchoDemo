using UnityEngine;

[CreateAssetMenu(fileName = "HW_B_001", menuName = "Game/Hardware/Barrel")]
public class HardwareBarrelSO : ScriptableObject
{
    public string hardwareID;
    public string hardwareName;
    public GameObject bulletPrefab;   // 该枪管射出的子弹形态
    public float baseDamage = 10f;    // 基础伤害
    public float baseFireRate = 2f;   // 基础射速
    public float heatPerShot = 5f;    // 每发子弹产生的热量 (为后面过热做准备)
}