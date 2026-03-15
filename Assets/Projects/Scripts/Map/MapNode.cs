// MapNode.cs
using System.Collections.Generic;
using UnityEngine; // 我们需要 Vector2 来存储位置

// [System.Serializable] 让这个类的实例可以在 Unity Inspector 中显示（比如在 MapGenerator 里）
[System.Serializable]
public class MapNode
{
    public NodeType nodeType;
    public Vector2 position;
    

    public List<int> childrenIndices = new List<int>();

    public EncounterData encounterData;

    public MapNode(NodeType type, Vector2 pos)
    {
        nodeType = type;
        position = pos;
    }
}