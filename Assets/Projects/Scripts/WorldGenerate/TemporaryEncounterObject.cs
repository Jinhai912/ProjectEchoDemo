// TemporaryEncounterObject.cs
using UnityEngine;

/// <summary>
/// 这是一个标记组件。
/// 任何挂载了这个脚本的 GameObject，都会被视作在战斗结束后生成的临时物件，
/// 并在下一次关卡开始前被 RoomController 自动清理。
/// </summary>
public class TemporaryEncounterObject : MonoBehaviour
{
    // 这个脚本不需要任何代码，它的存在本身就是一种标记。
}