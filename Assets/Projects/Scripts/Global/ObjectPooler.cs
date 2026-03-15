using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 通用对象池管理器
/// 描述：采用 Dictionary + Queue 架构，有效减少运行时 Instantiate 和 Destroy 产生的内存碎片。
/// </summary>
public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // 对象池容器：Key 为 Prefab 的名称，Value 为非激活对象队列
    private Dictionary<string, Queue<GameObject>> _poolDictionary = new Dictionary<string, Queue<GameObject>>();

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
        string poolKey = prefab.name;

        if (!_poolDictionary.ContainsKey(poolKey))
        {
            _poolDictionary.Add(poolKey, new Queue<GameObject>());
        }

        GameObject objectToSpawn;

        if (_poolDictionary[poolKey].Count == 0)
        {
            // 池子为空时创建新实例
            objectToSpawn = Instantiate(prefab);
            objectToSpawn.name = poolKey; 
        }
        else
        {
            // 复用池中已有的对象
            objectToSpawn = _poolDictionary[poolKey].Dequeue();
        }

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
        obj.SetActive(false);
        string poolKey = obj.name;
        
        if (_poolDictionary.ContainsKey(poolKey))
        {
            _poolDictionary[poolKey].Enqueue(obj);
        }
        else
        {
            // 容错处理：若该对象不属于任何池子则直接销毁
            Destroy(obj);
        }
    }
}