using UnityEngine;

/// <summary>
/// 对象池实例标记。
/// 描述：记录实例来源的预制体，让 ObjectPooler 不再依赖 GameObject.name 分池。
/// </summary>
public class PooledObject : MonoBehaviour
{
    public GameObject SourcePrefab { get; private set; }
    public bool IsInPool { get; private set; }

    public void Bind(GameObject sourcePrefab)
    {
        SourcePrefab = sourcePrefab;
        IsInPool = false;
    }

    public void MarkInPool() => IsInPool = true;
    public void MarkOutOfPool() => IsInPool = false;
}
