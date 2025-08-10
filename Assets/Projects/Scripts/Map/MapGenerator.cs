// MapGenerator.cs
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator
{
    // --- 地图配置 ---
    private int totalLayers; // 地图总层数
    private int minNodesPerLayer; // 每层最少节点数
    private int maxNodesPerLayer; // 每层最多节点数
    
    // 我们可以把所有节点都存在一个列表里，方便管理
    private List<MapNode> allNodes = new List<MapNode>();

    /// <summary>
    /// 生成地图的主入口方法
    /// </summary>
    public List<MapNode> GenerateMap(int layers, int minNodes, int maxNodes)
    {
        totalLayers = layers;
        minNodesPerLayer = minNodes;
        maxNodesPerLayer = maxNodes;

        allNodes.Clear();

        // --- 算法核心流程 ---
        GenerateLayers();
        ConnectLayers();
        // (可选) AssignNodeTypes(); // 分配节点类型，比如精英、商店等
        // (可选) PruneDeadEnds(); // 修正地图，确保所有路径都能通往Boss

        return allNodes;
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