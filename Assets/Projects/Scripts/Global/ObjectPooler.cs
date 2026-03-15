using UnityEngine;
using System.Collections.Generic;

public class ObjectPooler : MonoBehaviour
{
    public static ObjectPooler Instance;

    // 对象池字典：Key是预制体的名字，Value是存放该类物体的队列
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    void Awake()
    {
        Instance = this;
    }

    // 从池子中获取物体
    public GameObject SpawnFromPool(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        string tag = prefab.name;

        if (!poolDictionary.ContainsKey(tag))
        {
            poolDictionary.Add(tag, new Queue<GameObject>());
        }

        GameObject objectToSpawn;

        // 如果池子里没货了，创建一个新的
        if (poolDictionary[tag].Count == 0)
        {
            objectToSpawn = Instantiate(prefab);
            objectToSpawn.name = tag; // 保持名字统一，方便作为Key
        }
        else
        {
            // 如果池子里有，拿出一个来用
            objectToSpawn = poolDictionary[tag].Dequeue();
        }

        objectToSpawn.SetActive(true);
        objectToSpawn.transform.position = position;
        objectToSpawn.transform.rotation = rotation;

        return objectToSpawn;
    }

    // 将物体归还给池子
    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        string tag = obj.name;
        
        if (poolDictionary.ContainsKey(tag))
        {
            poolDictionary[tag].Enqueue(obj);
        }
        else
        {
            // 兜底：万一池子里没这个Tag，直接销毁
            Destroy(obj);
        }
    }
}