// MapGenerator.cs
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    // --- 地图配置 ---
    private int totalLayers; // 地图总层数
    private int minNodesPerLayer; // 每层最少节点数
    private int maxNodesPerLayer; // 每层最多节点数
    private MapManager mapManager;
    
    // 我们可以把所有节点都存在一个列表里，方便管理
    private List<MapNode> allNodes = new List<MapNode>();

    /// <summary>
    /// 生成地图的主入口方法
    /// </summary>
    public List<MapNode> GenerateMap(int layers, int minNodes, int maxNodes, MapManager manager)
    {
        totalLayers = layers;
        minNodesPerLayer = minNodes;
        maxNodesPerLayer = maxNodes;
        mapManager = manager; // <-- 保存引用

        allNodes.Clear();

        GenerateLayers();
        // --- 核心修改 2：在生成节点后，分配类型和数据 ---
        AssignNodeTypesAndData();
        ConnectLayers();

        return allNodes;
    }

    /// <summary>
    /// 为已生成的节点分配类型和具体的关卡数据
    /// </summary>
    private void AssignNodeTypesAndData()
    {
        bool storeAssigned = false;
        bool eventAssigned = false;

        // 遍历所有中间层 (不包括起点和终点Boss)
        for (int i = 1; i < totalLayers - 1; i++)
        {
            List<MapNode> layerNodes = GetNodesInLayer(i);
            if (layerNodes.Count == 0) continue;

            // 1) 特殊节点：第 1 层保证商店，第 2 层保证事件，其余层按概率
            NodeType? specialType = ChooseSpecialType(i, storeAssigned, eventAssigned);
            if (specialType.HasValue)
            {
                EventData eventData = specialType.Value == NodeType.Event ? PickEventData() : null;
                if (specialType.Value != NodeType.Event || eventData != null)
                {
                    AssignNode(layerNodes, specialType.Value, eventData);
                    if (specialType.Value == NodeType.Store) storeAssigned = true;
                    if (specialType.Value == NodeType.Event) eventAssigned = true;
                }
            }

            // 2) 在剩余节点里安插一个精英
            if (mapManager.eliteCombatEncounters != null &&
                mapManager.eliteCombatEncounters.Count > 0 &&
                layerNodes.Count > 0)
            {
                MapNode eliteNode = AssignNode(layerNodes, NodeType.EliteCombat, null);
                eliteNode.encounterData = mapManager.eliteCombatEncounters[Random.Range(0, mapManager.eliteCombatEncounters.Count)];
                Debug.Log("在第 " + i + " 层生成了一个精英节点。");
            }
        }

        // 3) 兜底：层数较少时，尽量保证商店和事件各有一个
        if (!storeAssigned) TryAssignFallbackSpecial(NodeType.Store, null);
        if (!eventAssigned) TryAssignFallbackSpecial(NodeType.Event, PickEventData());

        // --- 最后，为所有剩下的普通节点和Boss节点分配具体的关卡数据 ---
        foreach (var node in allNodes)
        {
            // 如果这个节点还没有被分配关卡数据
            if (node.encounterData == null)
            {
                switch (node.nodeType)
                {
                    case NodeType.NormalCombat:
                        if (mapManager.normalCombatEncounters.Count > 0)
                        {
                            node.encounterData = mapManager.normalCombatEncounters[Random.Range(0, mapManager.normalCombatEncounters.Count)];
                        }
                        break;
                    case NodeType.Boss:
                        node.encounterData = mapManager.bossEncounter;
                        break;
                }
            }
        }
    }

    private NodeType? ChooseSpecialType(int layer, bool storeAssigned, bool eventAssigned)
    {
        if (layer == 1 && !storeAssigned) return NodeType.Store;
        if (layer == 2 && !eventAssigned && HasEventPool()) return NodeType.Event;

        float roll = Random.value;
        if (roll < 0.2f) return NodeType.Store;
        if (roll < 0.4f && HasEventPool()) return NodeType.Event;
        return null;
    }

    private bool HasEventPool()
    {
        return mapManager.eventPool != null && mapManager.eventPool.Count > 0;
    }

    private EventData PickEventData()
    {
        if (!HasEventPool()) return null;
        return mapManager.eventPool[Random.Range(0, mapManager.eventPool.Count)];
    }

    private MapNode AssignNode(List<MapNode> layerNodes, NodeType type, EventData eventData)
    {
        int index = Random.Range(0, layerNodes.Count);
        MapNode node = layerNodes[index];
        node.nodeType = type;
        node.eventData = eventData;
        layerNodes.RemoveAt(index);
        Debug.Log("在第 " + (int)node.position.x + " 层生成了一个 " + type + " 节点。");
        return node;
    }

    private void TryAssignFallbackSpecial(NodeType type, EventData eventData)
    {
        if (type == NodeType.Event && eventData == null) return;

        for (int i = 1; i < totalLayers - 1; i++)
        {
            List<MapNode> candidates = GetNodesInLayer(i).FindAll(n => n.nodeType == NodeType.NormalCombat);
            if (candidates.Count == 0) continue;

            MapNode node = candidates[Random.Range(0, candidates.Count)];
            node.nodeType = type;
            node.eventData = eventData;
            Debug.Log("兜底生成了一个 " + type + " 节点。");
            return;
        }
    }

    /// <summary>
    /// 步骤 1: 一层一层地创建节点
    /// </summary>
    private void GenerateLayers()
    {
        // 创建起点 (第0层只有一个节点)
        MapNode startNode = new MapNode(NodeType.NormalCombat, new Vector2(0, 0.5f));
        allNodes.Add(startNode);

        // 创建中间层
        for (int layerIndex = 1; layerIndex < totalLayers - 1; layerIndex++)
        {
            int nodesInThisLayer = Random.Range(minNodesPerLayer, maxNodesPerLayer + 1);
            for (int i = 0; i < nodesInThisLayer; i++)
            {
                // yPosition 在 0 和 1 之间随机分布，代表在层内的垂直位置
                float yPosition = (i + 1f) / (nodesInThisLayer + 1f);
                MapNode newNode = new MapNode(NodeType.NormalCombat, new Vector2(layerIndex, yPosition));
                allNodes.Add(newNode);
            }
        }

        // 创建终点Boss (最后一层只有一个节点)
        MapNode bossNode = new MapNode(NodeType.Boss, new Vector2(totalLayers - 1, 0.5f));
        allNodes.Add(bossNode);
    }
    
    /// <summary>
    /// 步骤 2: 将创建好的节点连接起来
    /// </summary>
    private void ConnectLayers()
    {
        // 从第0层开始，连接到最后一层的前一层
        for (int i = 0; i < totalLayers - 1; i++)
        {
            List<MapNode> currentLayerNodes = GetNodesInLayer(i);
            List<MapNode> nextLayerNodes = GetNodesInLayer(i + 1);
            
            // --- 核心修复：使用新的、更可靠的连接算法 ---
            
            // 1. 确保下一层的每个节点都至少有一个父节点
            foreach (var nextNode in nextLayerNodes)
            {
                // 从当前层随机选择一个节点作为它的父节点
                MapNode parentNode = currentLayerNodes[Random.Range(0, currentLayerNodes.Count)];
                
                int childIndex = allNodes.IndexOf(nextNode);
                if (!parentNode.childrenIndices.Contains(childIndex))
                {
                    parentNode.childrenIndices.Add(childIndex);
                }
            }
            
            // 2. 确保当前层的每个节点都至少有一个子节点
            foreach (var currentNode in currentLayerNodes)
            {
                // 如果这个节点在第一步之后，还没有任何子节点
                if (currentNode.childrenIndices.Count == 0)
                {
                    // 就从下一层随机选一个作为它的子节点
                    MapNode childNode = nextLayerNodes[Random.Range(0, nextLayerNodes.Count)];
                    int childIndex = allNodes.IndexOf(childNode);
                    currentNode.childrenIndices.Add(childIndex);
                }
            }
        }
    }

    /// <summary>
    /// 一个辅助方法，用于获取指定层的所有节点
    /// </summary>
    private List<MapNode> GetNodesInLayer(int layerIndex)
    {
        List<MapNode> layerNodes = new List<MapNode>();
        foreach (var node in allNodes)
        {
            if ((int)node.position.x == layerIndex)
            {
                layerNodes.Add(node);
            }
        }
        return layerNodes;
    }
}
