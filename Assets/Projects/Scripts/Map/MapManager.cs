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

    // --- 运行时数据 ---
    private List<MapNode> mapNodes;
    private MapNode currentNode;

    // --- 引用 ---
    private MapView mapView;
    private MapGenerator mapGenerator = new MapGenerator();

    void Awake()
    {
        Debug.LogError("!!!!!!!!!!!! MAP MANAGER AWAKE IS RUNNING !!!!!!!!!!!!");
        // 1. 实现单例模式
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 2. 订阅场景加载事件
    }

    void OnDestroy()
    {
        // 3. 取消订阅
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
        mapNodes = mapGenerator.GenerateMap(totalLayers, minNodesPerLayer, maxNodesPerLayer);
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
        currentNode = nextNode;
        // （未来）nextNode.encounterData 应该在这里被赋值给 GameManager
        // GameManager.Instance.nextEncounter = nextNode.encounterData;

        // 命令 GameManager 切换到战斗状态
        GameManager.Instance.UpdateGameState(GameManager.GameState.Playing);
    }

    private void UpdateNodeStates()
    {
        if (mapView == null || mapNodes == null) return;

        foreach (var nodeObjectPair in mapView.nodeObjects)
        {
            MapNode node = nodeObjectPair.Key;
            Button nodeButton = nodeObjectPair.Value.GetComponent<Button>();
            if (nodeButton == null) continue;

            bool isInteractable = false;
            // 确保 currentNode 存在，并且它的子节点列表也存在
            if (currentNode != null && currentNode.childrenIndices != null)
            {
                int nodeIndex = mapNodes.IndexOf(node);
                // 检查当前节点是否是 currentNode 的子节点之一
                isInteractable = currentNode.childrenIndices.Contains(nodeIndex);
            }
            nodeButton.interactable = isInteractable;
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