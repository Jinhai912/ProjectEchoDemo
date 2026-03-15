using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 物理状态容器
/// 描述：挂载于单位身上，负责管理电、热、液态介质的附着计时及物理反应判定。
/// </summary>
public class StatusContainer : MonoBehaviour
{
    #region 配置与状态
    [Header("计时配置")]
    public float statusDuration = 5f;
    public float reactionICD = 1.0f; 

    private float _lastReactionTime = -99f; 
    private Dictionary<PhysicalMediaType, float> _activeStatusTimers = new Dictionary<PhysicalMediaType, float>();
    #endregion

    #region 生命周期
    void Update()
    {
        UpdateStatusTimers();
    }
    #endregion

    #region 外部接口
    public void ApplyMediaStatus(PhysicalMediaType newMedia)
    {
        if (newMedia == PhysicalMediaType.None) return;

        if (CheckForReaction(newMedia)) return; 

        if (_activeStatusTimers.ContainsKey(newMedia))
        {
            _activeStatusTimers[newMedia] = statusDuration;
        }
        else
        {
            _activeStatusTimers.Add(newMedia, statusDuration);
            Debug.Log($"[状态附着] {gameObject.name}: {newMedia}");
        }
    }

    public bool HasStatus(PhysicalMediaType type) => _activeStatusTimers.ContainsKey(type);
    #endregion

    #region 核心逻辑
    private void UpdateStatusTimers()
    {
        var keys = _activeStatusTimers.Keys.ToList();
        foreach (var type in keys)
        {
            _activeStatusTimers[type] -= Time.deltaTime;
            if (_activeStatusTimers[type] <= 0)
            {
                _activeStatusTimers.Remove(type);
            }
        }
    }

    private bool CheckForReaction(PhysicalMediaType incoming)
    {
        if (Time.time - _lastReactionTime < reactionICD) return false;

        foreach (var existing in _activeStatusTimers.Keys)
        {
            // 反应判定逻辑矩阵
            if (TryMatchReaction(existing, incoming, "Conduction", PhysicalMediaType.Electric, PhysicalMediaType.Liquid_Water)) return true;
            if (TryMatchReaction(existing, incoming, "Overload", PhysicalMediaType.Electric, PhysicalMediaType.Heat)) return true;
            if (TryMatchReaction(existing, incoming, "Vaporization", PhysicalMediaType.Heat, PhysicalMediaType.Liquid_Water)) return true;
            if (TryMatchReaction(existing, incoming, "Ignition", PhysicalMediaType.Heat, PhysicalMediaType.Liquid_Oil)) return true;
        }
        return false;
    }

    private bool TryMatchReaction(PhysicalMediaType a, PhysicalMediaType b, string reactionName, PhysicalMediaType t1, PhysicalMediaType t2)
    {
        if ((a == t1 && b == t2) || (a == t2 && b == t1))
        {
            _lastReactionTime = Time.time;
            TriggerReaction(reactionName, a);
            return true;
        }
        return false;
    }

    private void TriggerReaction(string reactionName, PhysicalMediaType consumedStatus)
    {
        _activeStatusTimers.Remove(consumedStatus);
        if (PhysicalReactionManager.Instance != null)
        {
            PhysicalReactionManager.Instance.ExecuteReaction(reactionName, this.gameObject);
        }
    }
    #endregion
}