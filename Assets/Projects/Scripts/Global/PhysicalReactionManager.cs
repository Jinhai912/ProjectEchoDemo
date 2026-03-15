using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// 全局物理反应管理器
/// 描述：核心“反应堆”，负责计算介质间的交互结果、派发区域伤害、触发视觉反馈。
/// </summary>
public class PhysicalReactionManager : MonoBehaviour
{
    public static PhysicalReactionManager Instance { get; private set; }
    
    /// <summary> 全局物理反应事件广播（反应名称, 发生坐标） </summary>
    public static System.Action<string, Vector3> OnAnyReaction;

    #region 配置参数
    [Header("反应数值配置")]
    public float conductionRadius = 5f;
    public float overloadExplosionRadius = 3f;
    public float overloadForce = 500f;

    [Header("反应反馈特效 (VFX)")]
    public GameObject vfxConduction; 
    public GameObject vfxOverload;   
    public GameObject vfxVaporization; 
    public GameObject vfxIgnition;     
    #endregion

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    /// <summary>
    /// 响应受击触发反应的唯一入口
    /// </summary>
    public void ExecuteReaction(string reactionName, GameObject originSource)
    {
        Vector3 pos = originSource.transform.position;
        
        // 触发 UI 提示与粒子反馈
        HandleVisualFeedback(reactionName, pos);

        // 分发具体的物理逻辑
        switch (reactionName)
        {
            case "Conduction":   HandleConduction(pos, originSource); break;
            case "Overload":     HandleOverload(pos, originSource); break;
            case "Vaporization": HandleVaporization(pos, originSource); break;
            case "Ignition":     HandleIgnition(pos, originSource); break;
        }
        
        // 广播信号供 软件协议（Protocol）系统监听
        OnAnyReaction?.Invoke(reactionName, pos);
    }

    #region 私有逻辑处理器
    private void HandleVisualFeedback(string name, Vector3 pos)
    {
        string cnLabel = name switch {
            "Conduction" => "【传导】",
            "Overload" => "【过载】",
            "Vaporization" => "【气化】",
            "Ignition" => "【爆燃】",
            _ => name
        };

        // 触发战斗飘字
        PlayerFeedbackManager.OnFeedbackRequested?.Invoke(cnLabel, true, pos + Vector3.up * 1.5f);

        // 生成粒子特效（建议接入对象池，Demo版暂用Instantiate）
        GameObject vfxPrefab = name switch {
            "Conduction" => vfxConduction,
            "Overload" => vfxOverload,
            "Vaporization" => vfxVaporization,
            "Ignition" => vfxIgnition,
            _ => null
        };

        if (vfxPrefab)
        {
            GameObject vfx = Instantiate(vfxPrefab, pos, Quaternion.identity);
            Destroy(vfx, 2f);
        }
    }

    private void HandleConduction(Vector3 position, GameObject source)
    {
        float damage = PlayerData.Instance.FinalAttack;
        Collider[] hits = Physics.OverlapSphere(position, conductionRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == source) continue;
            if (hit.TryGetComponent<StatusContainer>(out var status) && status.HasStatus(PhysicalMediaType.Liquid_Water))
            {
                if (hit.TryGetComponent<EnemyState>(out var enemy)) enemy.TakeDamage(damage, false);
            }
        }
    }

    private void HandleOverload(Vector3 position, GameObject source)
    {
        float damage = PlayerData.Instance.FinalAttack * 1.5f;
        Collider[] hits = Physics.OverlapSphere(position, overloadExplosionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<EnemyState>(out var enemy)) enemy.TakeDamage(damage, false);
            if (hit.TryGetComponent<Rigidbody>(out var rb)) rb.AddExplosionForce(overloadForce, position, overloadExplosionRadius, 0.5f);
        }
    }

    private void HandleVaporization(Vector3 position, GameObject source)
    {
        GameObject zone = new GameObject("Logic_SteamZone");
        zone.transform.position = new Vector3(position.x, 0.7f, position.z);
        zone.AddComponent<TemporaryEncounterObject>();
        zone.AddComponent<SphereCollider>().isTrigger = true;
        zone.GetComponent<SphereCollider>().radius = 4f;
        zone.AddComponent<TemporaryZoneLogic>().zoneType = "Steam";
    }

    private void HandleIgnition(Vector3 position, GameObject source)
    {
        GameObject zone = new GameObject("Logic_FireZone");
        zone.transform.position = new Vector3(position.x, 0.7f, position.z);
        zone.AddComponent<TemporaryEncounterObject>();
        zone.AddComponent<SphereCollider>().isTrigger = true;
        zone.GetComponent<SphereCollider>().radius = 3f;
        var logic = zone.AddComponent<TemporaryZoneLogic>();
        logic.zoneType = "Fire"; logic.damagePerTick = 5f; logic.duration = 8f;
    }
    #endregion
}