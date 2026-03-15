using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class MapView : MonoBehaviour
{
    [Header("UI 预制体")]
    public GameObject nodePrefab; 
    public GameObject linePrefab; 

    [Header("UI 引用")]
    public RectTransform container;

    [Header("图标资源")]
    public Sprite normalCombatIcon;
    public Sprite eliteCombatIcon;
    public Sprite bossIcon;
    public Sprite storeIcon;
    public Sprite eventIcon;

    [Header("路径线设置")]
    public float lineWidth = 10f;


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
            // 把它映射到 [0, 1] 的比例
            float xRatio = node.position.x / (MapManager.Instance.CurrentMapTotalLayers - 1);
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

        GameObject lineGO = Instantiate(linePrefab, container);
        RectTransform lineRect = lineGO.GetComponent<RectTransform>();
        lineGO.transform.SetAsFirstSibling();
        
        Vector2 startPos = fromGO.GetComponent<RectTransform>().anchoredPosition;
        Vector2 endPos = toGO.GetComponent<RectTransform>().anchoredPosition;
        
        Vector2 direction = endPos - startPos;
        float distance = direction.magnitude;
        
        lineRect.anchoredPosition = (startPos + endPos) / 2f;
        lineRect.sizeDelta = new Vector2(distance, lineWidth);
        lineRect.rotation = Quaternion.FromToRotation(Vector3.right, direction);
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