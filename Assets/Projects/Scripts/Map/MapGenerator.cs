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
        // 遍历所有中间层的节点 (不包括起点和终点Boss)
        for (int i = 1; i < totalLayers - 1; i++)
        {
            List<MapNode> layerNodes = GetNodesInLayer(i);
            
            // --- 在这一层安插一个精英怪 ---
            // 确保精英怪池不为空，并且节点数量足够
            if (mapManager.eliteCombatEncounters.Count > 0 && layerNodes.Count > 0)
            {
                // 随机选择一个倒霉的节点，把它变成精英房
                int eliteIndex = Random.Range(0, layerNodes.Count);
                MapNode eliteNode = layerNodes[eliteIndex];
                eliteNode.nodeType = NodeType.EliteCombat;
                // 同样，从精英关卡池里随机选一个具体的关卡配置
                eliteNode.encounterData = mapManager.eliteCombatEncounters[Random.Range(0, mapManager.eliteCombatEncounters.Count)];
                
                // 把这个节点从待处理列表里移除，避免它又被分配成其他类型
                layerNodes.RemoveAt(eliteIndex);

                Debug.Log("在第 " + i + " 层生成了一个精英节点。");
            }
            
            // --- (未来) 在这里可以添加安插商店、事件房的逻辑 ---
            // if (mapManager.eventEncounters.Count > 0 && layerNodes.Count > 0) { ... }
        }

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