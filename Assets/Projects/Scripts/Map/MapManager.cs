// MapManager.cs (最终的、逻辑清晰的正确版本)
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MapManager : MonoBehaviour
{
    // --- 单例模式 (这是必须的！) ---
    public static MapManager Instance { get; private set; }

    [Header("地图配置")]
    public int totalLayers = 5;
    public int minNodesPerLayer = 2;
    public int maxNodesPerLayer = 4;

    [Header("关卡数据池")]
    public List<EncounterData> normalCombatEncounters;
    public List<EncounterData> eliteCombatEncounters;
    public EncounterData bossEncounter; // Boss 通常是固定的

    [Header("特殊节点池")]
    [Tooltip("留空时会自动从 Resources/Events 加载全部 EventData")]
    public List<EventData> eventPool = new List<EventData>();

    [Header("地图基础配置 (第一层)")] // 修改了 Header 名字，更清晰
    public int baseTotalLayers = 5;      // 基础层数
    public int baseMinNodesPerLayer = 2; // 基础最少节点
    public int baseMaxNodesPerLayer = 3; // 基础最多节点

    public int CurrentMapTotalLayers { get; private set; }//存储实际层数

    [Header("难度成长系数")]
    [Tooltip("每进一层，地图增加多少层")]
    public int layersIncreasePerFloor = 1;
    [Tooltip("每进一层，每层最少/最多节点数增加多少")]
    public int nodesIncreasePerFloor = 1;

    // --- 运行时数据 ---
    private List<MapNode> mapNodes;
    private MapNode currentNode;

    // --- 引用 ---
    private MapView mapView;
    private MapGenerator mapGenerator = new MapGenerator();

    void Awake()
    {
        // 1. 实现单例模式
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadEventsIfNeeded();
    }

    private void LoadEventsIfNeeded()
    {
        if (eventPool == null) eventPool = new List<EventData>();
        if (eventPool.Count > 0) return;

        EventData[] loadedEvents = Resources.LoadAll<EventData>("Events");
        if (loadedEvents != null && loadedEvents.Length > 0)
        {
            eventPool.AddRange(loadedEvents);
            Debug.Log($"MapManager: 从 Resources/Events 加载了 {loadedEvents.Length} 个事件。");
        }
    }

    /// <summary>
    /// 准备并绘制地图。这是所有地图绘制逻辑的唯一入口。
    /// </summary>
    // 【新增】一个公共方法，由 UIManager 调用
    public void PrepareAndDrawMap()
    {
        Debug.Log("MapManager 收到绘制地图的命令...");

        // --- 核心修改：在这里，实时地去寻找 MapView ---
        mapView = FindObjectOfType<MapView>(true); // true: 包括非激活的对象

        if (mapView != null)
        {
            Debug.Log("成功找到了 MapView！准备绘制。");
            if (mapNodes == null || mapNodes.Count == 0)
            {
                GenerateNewMap();
            }
            mapView.DrawMap(mapNodes);
            UpdateNodeStates();
        }
        else
        {
            Debug.LogError("MapManager 在 PrepareAndDrawMap 时，依然找不到 MapView 组件！这太奇怪了！");
        }
    }

    private void GenerateNewMap()
    {
        // --- 在这里计算动态参数 ---
        int floor = 1;
        if (GameManager.Instance != null)
        {
            // 从 GameManager 读取当前是第几层
            floor = GameManager.Instance.currentFloor;
        }

        // 1. 根据当前层数，计算本次生成的地图参数
        // (floor - 1) 确保第一层使用基础值
        CurrentMapTotalLayers = baseTotalLayers + (floor - 1) * layersIncreasePerFloor;
        int minNodes = baseMinNodesPerLayer + (floor - 1) * nodesIncreasePerFloor;
        int maxNodes = baseMaxNodesPerLayer + (floor - 1) * nodesIncreasePerFloor;

        Debug.Log("正在为第 " + floor + " 层生成地图... 参数: " +
                  "总层数=" + CurrentMapTotalLayers + ", " +
                  "最少节点=" + minNodes + ", " +
                  "最多节点=" + maxNodes);
        
        mapNodes = mapGenerator.GenerateMap(
            CurrentMapTotalLayers, 
            minNodes, 
            maxNodes,
            this // <-- 把 MapManager 自己传过去
        );
        // 设置起点
        List<MapNode> startNodes = GetNodesInLayer(0);
        if (startNodes.Count > 0)
        {
            currentNode = startNodes[0];
        }
    }

    // 公共方法：清空地图，由 GameManager 在开始新游戏时调用
    public void ClearMap()
    {
        mapNodes = null;
        currentNode = null;
    }

    // 公共方法：当玩家选择了一个新节点后，由 MapView 的按钮调用
    public void MoveToNode(MapNode nextNode)
    {
        Debug.Log("玩家选择了新节点，类型为: " + nextNode.nodeType);
    currentNode = nextNode;

    // --- 核心修改：使用 switch 语句来处理不同的节点类型 ---
    switch (nextNode.nodeType)
    {
        case NodeType.NormalCombat:
        case NodeType.EliteCombat:
        case NodeType.Boss:
            // 如果是任何一种战斗类型
            
            // 1. 切换到战斗状态，UIManager 会自动隐藏地图、显示HUD
            GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
            
            // 2. 找到 RoomController 并命令它开始战斗
            RoomController roomController = FindObjectOfType<RoomController>();
            if (roomController != null && nextNode.encounterData != null)
            {
                roomController.StartEncounter(nextNode.encounterData);
            }
            else
            {
                Debug.LogError("MapManager 找不到 RoomController 或战斗数据！");
            }
            break; // 结束这个 case 的处理

        case NodeType.Store:
            OpenStoreNode();
            break;

        case NodeType.Event:
            OpenEventNode(nextNode);
            break;

        case NodeType.RestSite:
            Debug.LogWarning("休息点尚未实现，返回地图。");
            UIManager.Instance.ShowMapPanel();
            break;

        default:
            // 如果遇到未知的节点类型
            Debug.LogError("未知的节点类型: " + nextNode.nodeType);
            // 安全起见，返回地图
            GameManager.Instance.UpdateGameState(GameManager.GameState.MapSelection);
            break;
    }
    }

    private void OpenStoreNode()
    {
        AbilityPool pool = LevelUpManager.Instance != null
            ? LevelUpManager.Instance.abilityPool
            : FindObjectOfType<AbilityPool>();

        if (pool == null)
        {
            Debug.LogError("MapManager: 找不到 AbilityPool，无法打开商店节点。");
            UIManager.Instance.ShowMapPanel();
            return;
        }

        List<AbilityData> stock = pool.GetRandomAbilities(3);
        UIManager.Instance.OpenStorePanel(stock, true);
    }

    private void OpenEventNode(MapNode node)
    {
        if (node.eventData == null)
        {
            Debug.LogError("MapManager: 事件节点没有绑定 EventData，返回地图。");
            UIManager.Instance.ShowMapPanel();
            return;
        }

        UIManager.Instance.OpenEventPanel(node.eventData, null, true);
    }

    private void UpdateNodeStates()
    {
        if (mapView == null || mapNodes == null || currentNode == null) return;
        
        foreach (var nodeObjectPair in mapView.nodeObjects)
        {
            MapNode node = nodeObjectPair.Key;
            GameObject nodeGO = nodeObjectPair.Value;
            Button nodeButton = nodeGO.GetComponent<Button>();
            Image nodeIcon = nodeGO.GetComponent<Image>(); // 我们需要获取 Image 来改变颜色
            
            if(nodeButton == null || nodeIcon == null) continue;

            // --- 核心修改 ---

            // 规则 1：判断是否是当前节点
            if (node == currentNode)
            {
                nodeButton.interactable = false; // 当前节点不可点击
                nodeIcon.color = Color.yellow; // 用醒目的颜色（比如黄色）来高亮当前节点
                continue; // 处理完这个节点，就跳到下一个
            }

            // 规则 2：判断是否是可达的子节点
            int nodeIndex = mapNodes.IndexOf(node);
            bool isInteractable = currentNode.childrenIndices.Contains(nodeIndex);
            
            nodeButton.interactable = isInteractable;
            
            // 规则 3：设置其他节点的外观
            if (isInteractable)
            {
                nodeIcon.color = Color.white; // 可交互的节点是白色
            }
            else
            {
                nodeIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 不可交互的节点是半透明的灰色
            }
        }
    }

    // 辅助方法，保持不变
    private List<MapNode> GetNodesInLayer(int layerIndex)
    {
        List<MapNode> layerNodes = new List<MapNode>();
        if (mapNodes == null) return layerNodes;
        foreach (var node in mapNodes)
        {
            if ((int)node.position.x == layerIndex)
            {
                layerNodes.Add(node);
            }
        }
        return layerNodes;
    }
}
