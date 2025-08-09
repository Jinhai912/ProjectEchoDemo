// MapView.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 引入 UI 命名空间

// 挂载在 MapPanel 上
public class MapView : MonoBehaviour
{
    [Header("UI 预制体")]
    public GameObject nodePrefab; // 我们之前创建的 MapNode_Prefab 按钮
    public GameObject linePrefab; // 一个用来画线的简单 Image 预制体

    [Header("UI 引用")]
    public RectTransform container;

    [Header("图标资源")]
    public Sprite normalCombatIcon;
    public Sprite eliteCombatIcon;
    public Sprite bossIcon;
    public Sprite storeIcon;
    public Sprite eventIcon;

    [Header("路径线设置")]
    public float lineWidth = 10f; // 默认粗细为 10

    [Header("布局微调")]
    public float lineEdgeOffset = 40f; // 线的边缘偏移量，大约等于图标宽度的一半

    // 存储已创建的节点UI，方便查找
    public Dictionary<MapNode, GameObject> nodeObjects = new Dictionary<MapNode, GameObject>();


    /// <summary>
    /// 绘制地图的主入口方法
    /// </summary>
    public void DrawMap(List<MapNode> allNodes)
    {
        if (!IsReadyToDraw() || allNodes == null || allNodes.Count == 0) return;

        ClearMap();
        nodeObjects.Clear();
        
        // 在循环外获取一次容器尺寸
        Rect containerRect = container.rect;
        float containerWidth = containerRect.width;
        float containerHeight = containerRect.height;

        // --- 1. 绘制所有节点 ---
        foreach (var node in allNodes)
        {
            // --- 核心修改 1：所有东西都实例化在【同一个】container下 ---
            GameObject nodeGO = Instantiate(nodePrefab, container);
            RectTransform rect = nodeGO.GetComponent<RectTransform>();
            
             // --- 核心修复：完全采纳你的居中算法 ---
        
            // a. 获取画幅的左右边界
            float leftBound = -containerWidth / 2f;
            float rightBound = containerWidth / 2f;
            
            // b. 计算可用的绘图宽度 (可以留一点边距)
            float padding = containerWidth * 0.1f; // 左右各留 10%
            float drawableWidth = (rightBound - padding) - (leftBound + padding);
            
            // c. 计算节点的 X 位置
            // node.position.x 的范围是 0 到 (totalLayers - 1)
            // 我们把它映射到 [0, 1] 的比例
            float xRatio = node.position.x / (MapManager.Instance.totalLayers - 1);
            // 将比例映射到可绘制宽度上，再加上左边界和边距
            float xPos = (leftBound + padding) + (xRatio * drawableWidth);

            // d. 计算节点的 Y 位置 (中心为0)
            float yPos = (node.position.y - 0.5f) * containerHeight;

            // e. 设置最终位置
            rect.anchoredPosition = new Vector2(xPos, yPos);
            
            // --- 你的图标和按钮绑定逻辑是完美的，保持不变 ---
            Image nodeIcon = nodeGO.GetComponent<Image>();
            Button nodeButton = nodeGO.GetComponent<Button>();
            nodeIcon.sprite = GetSpriteForNodeType(node.nodeType);
            nodeButton.onClick.RemoveAllListeners();
            nodeButton.onClick.AddListener(() => {
                if (MapManager.Instance != null) {
                    MapManager.Instance.MoveToNode(node);
                }
            });
            
            nodeObjects.Add(node, nodeGO);
        }

        // --- 2. 在所有节点都创建好之后，再绘制连线 ---
        foreach (var parentNode in allNodes)
        {
            foreach (var childIndex in parentNode.childrenIndices)
            {
                MapNode childNode = allNodes[childIndex];
                DrawLine(parentNode, childNode);
            }
        }
    }

    /// <summary>
    /// 在两个节点之间绘制一条线
    /// </summary>
    private void DrawLine(MapNode from, MapNode to)
    {
        GameObject fromGO = nodeObjects[from];
        GameObject toGO = nodeObjects[to];

        // --- 核心修改 3：线也实例化在【同一个】container下 ---
        GameObject lineGO = Instantiate(linePrefab, container);
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();
        
        // --- 核心修改 4：让线段显示在所有节点的【下方】 ---
        lineGO.transform.SetAsFirstSibling(); 
        
        Vector2 startPos = fromGO.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endPos = toGO.GetComponent<RectTransform>().anchoredPosition;
        
        startPos.x += lineEdgeOffset;
        endPos.x += lineEdgeOffset;

        Vector2 direction = (endPos - startPos).normalized;
        float distance = Vector2.Distance(startPos, endPos);
        
        lineRect.anchoredPosition = startPos; 
        lineRect.transform.right = direction;
        lineRect.sizeDelta = new Vector2(distance, lineWidth);
    }

    /// <summary>
    /// 辅助方法：根据节点类型返回对应的图标
    /// </summary>
    private Sprite GetSpriteForNodeType(NodeType type)
    {
        switch (type)
        {
            case NodeType.NormalCombat: return normalCombatIcon;
            case NodeType.EliteCombat: return eliteCombatIcon;
            case NodeType.Boss: return bossIcon;
            case NodeType.Store: return storeIcon;
            case NodeType.Event: return eventIcon;
            default: return null;
        }
    }

    /// <summary>
    /// 清空当前地图上的所有UI元素
    /// </summary>
    private void ClearMap()
    {
        foreach (Transform child in container) { 
            Destroy(child.gameObject); 
        }
    }
    
    /// <summary>
    /// 检查所有必要的引用是否都已设置
    /// </summary>
    private bool IsReadyToDraw()
    {
        bool isReady = true;
        if (nodePrefab == null) { Debug.LogError("MapView 错误: Node Prefab 未设置！"); isReady = false; }
        if (linePrefab == null) { Debug.LogError("MapView 错误: Line Prefab 未设置！"); isReady = false; }
        if (container == null) { Debug.LogError("MapView 错误: Container 未设置！"); isReady = false; }
        return isReady;
    }
}