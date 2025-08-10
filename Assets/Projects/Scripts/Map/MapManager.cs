// MapManager.cs (最终的、逻辑清晰的正确版本)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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
    public List<EncounterData> storeEncounters; // (未来商店)
    public List<EncounterData> eventEncounters; // (未来事件)
    public EncounterData bossEncounter; // Boss 通常是固定的

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
    }


    // 4. 每次场景加载后，都会自动调用这个方法
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainFightScene")
        {
            StartCoroutine(PrepareAndDrawMapRoutine());
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
        mapNodes = mapGenerator.GenerateMap(
            totalLayers, 
            minNodesPerLayer, 
            maxNodesPerLayer,
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
        Debug.Log("玩家移动到新节点: " + nextNode.nodeType);
        currentNode = nextNode;

        // --- 核心修复：直接命令 RoomController 开始战斗 ---
        RoomController roomController = FindObjectOfType<RoomController>();
        if (roomController != null && nextNode.encounterData != null)
        {
            // 命令当前场景的 RoomController 用新数据开始战斗
            roomController.StartEncounter(nextNode.encounterData);
        }
        else
        {
            Debug.LogError("MapManager 找不到 RoomController 或关卡数据！");
        }
        
        // --- 最后，将游戏状态切换回 Playing ---
        // UIManager 会自动响应这个状态，隐藏地图，显示HUD
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
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
    
    // --- 核心修改：新增这个协程方法 ---
    private IEnumerator PrepareAndDrawMapRoutine()
    {
        // 尝试寻找 MapView
        mapView = FindObjectOfType<MapView>();
        
        // 如果在当前帧没找到，可能是它还没被激活，再等一帧
        if (mapView == null)
        {
            yield return null; // 等待下一帧
            mapView = FindObjectOfType<MapView>();
        }

        // 如果最终还是找不到，就报错并终止
        if (mapView == null)
        {
            Debug.LogError("MapManager 在 MainFightScene 中找不到 MapView 组件！");
            yield break;
        }

        // --- 解决“挤在一起”问题的【关键代码】 ---
        // 等待一帧，确保 Canvas 的布局系统已经完成了所有尺寸的计算
        yield return null; 

        // 现在，所有 UI 元素的尺寸都应该是正确的了，我们再开始绘制
        if (mapNodes == null || mapNodes.Count == 0)
        {
            GenerateNewMap();
        }
        
        // 调用你现有的、功能完备的 DrawMap 方法
        mapView.DrawMap(mapNodes);
        UpdateNodeStates();
    }




}