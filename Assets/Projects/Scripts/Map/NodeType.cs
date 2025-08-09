// NodeType.cs

// 这是一个纯粹的枚举定义，所以不需要 using UnityEngine 等。
// 也不需要 class 或 MonoBehaviour。
public enum NodeType
{
    NormalCombat, // 普通战斗
    EliteCombat,  // 精英战斗
    Boss,         // Boss
    Store,        // 商店
    Event,        // 随机事件
    RestSite      // 休息点 (未来扩展)
}