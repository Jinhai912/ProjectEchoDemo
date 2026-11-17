using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通用对象池管理器
/// 描述：按“预制体引用”分池，避免依赖 GameObject.name 导致回收错池。
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // 对象池容器：Key 为预制体引用，Value 为非激活对象队列
    private readonly Dictionary<GameObject, Queue<GameObject>> _poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 从对象池中索取或创建一个对象
    /// </summary>
    /// <param name="prefab">目标预制体</param>
    /// <param name="position">生成位置</param>
    /// <param name="rotation">生成旋转</param>
    /// <returns>可用的 GameObject 实例</returns>
    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        if (prefab == null) return null;

        if (!_poolDictionary.TryGetValue(prefab, out Queue<GameObject> pool))
        {
            pool = new Queue<GameObject>();
            _poolDictionary.Add(prefab, pool);
        }

        GameObject objectToSpawn = null;
        while (pool.Count > 0)
        {
            objectToSpawn = pool.Dequeue();
            if (objectToSpawn != null) break; // 跳过已被场景销毁的旧实例
        }

        if (objectToSpawn == null)
        {
            objectToSpawn = Instantiate(prefab);

            PooledObject pooled = objectToSpawn.GetComponent<PooledObject>();
            if (pooled == null) pooled = objectToSpawn.AddComponent<PooledObject>();
            pooled.Bind(prefab);
        }

        PooledObject spawned = objectToSpawn.GetComponent<PooledObject>();
        if (spawned != null) spawned.MarkOutOfPool();

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.SetPositionAndRotation(position, rotation);

        return objectToSpawn;
    }

    /// <summary>
    /// 将对象回收至对应的对象池中
    /// </summary>
    /// <param name="obj">需要回收的 GameObject</param>
    public void ReturnToPool(GameObject obj)
    {
        if (obj == null) return;

        PooledObject pooled = obj.GetComponent<PooledObject>();
        if (pooled == null || pooled.SourcePrefab == null ||
            !_poolDictionary.TryGetValue(pooled.SourcePrefab, out Queue<GameObject> pool))
        {
            // 容错处理：若该对象不属于任何池子则直接销毁
            Destroy(obj);
            return;
        }

        if (pooled.IsInPool) return; // 防止同一实例被重复入池

        pooled.MarkInPool();
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}
