using UnityEngine;
using System.Collections.Generic;

public class StatusContainer : MonoBehaviour
{
    [Header("状态存续时间 (秒)")]
    public float statusDuration = 5f;
    [Header("反应控制")]
    public float reactionICD = 1.0f; // 反应内置冷却时间：1秒
    private float lastReactionTime = -99f; 

    // 用字典来存每个属性剩下的时间
    private Dictionary<PhysicalMediaType, float> activeStatusTimers = new Dictionary<PhysicalMediaType, float>();

    void Update()
    {
        // 每一帧减少所有状态的计时
        List<PhysicalMediaType> keys = new List<PhysicalMediaType>(activeStatusTimers.Keys);
        foreach (var type in keys)
        {
            activeStatusTimers[type] -= Time.deltaTime;
            if (activeStatusTimers[type] <= 0)
            {
                activeStatusTimers.Remove(type);
                Debug.Log($"[状态] {gameObject.name} 的 {type} 效果消失了");
                // 这里以后可以加“状态消失”的视觉效果
            }
        }
    }

    // 外部调用：给敌人施加某种物理状态
    public void ApplyMediaStatus(PhysicalMediaType newMedia)
    {
        if (newMedia == PhysicalMediaType.None) return;

        // 核心逻辑：检查是否会产生反应
        if (CheckForReaction(newMedia))
        {
            // 如果触发了反应，通常会消耗掉原来的状态（根据你的文档）
            // 反应具体的逻辑我们下一步在 ReactionManager 里写
            return; 
        }

        // 如果没有反应，就纯挂载状态或重置计时
        if (activeStatusTimers.ContainsKey(newMedia))
        {
            activeStatusTimers[newMedia] = statusDuration; // 刷新时间
        }
        else
        {
            activeStatusTimers.Add(newMedia, statusDuration);
            Debug.Log($"[状态] {gameObject.name} 现在进入了 {newMedia} 状态");
            // 这里以后可以改变敌人材质颜色，比如变蓝(电)、变红(热)
        }
    }

    // 检查身上已有的状态是否与新进入的状态发生反应
    private bool CheckForReaction(PhysicalMediaType incoming)
    {
        // --- 新增：检查冷却时间 ---
        if (Time.time - lastReactionTime < reactionICD) return false;
        foreach (var existing in activeStatusTimers.Keys)
        {
            // 1. 电 + 水 = 传导 (Conduction)
            if (Match(existing, incoming, PhysicalMediaType.Electric, PhysicalMediaType.Liquid_Water))
            {
                ApplyReactionCooldown();
                TriggerReaction("Conduction", existing);
                return true;
            }

            // 2. 电 + 热 = 过载 (Overload)
            if (Match(existing, incoming, PhysicalMediaType.Electric, PhysicalMediaType.Heat))
            {
                ApplyReactionCooldown();
                TriggerReaction("Overload", existing);
                return true;
            }

            // 3. 热 + 水 = 气化 (Vaporization)
            if (Match(existing, incoming, PhysicalMediaType.Heat, PhysicalMediaType.Liquid_Water))
            {
                ApplyReactionCooldown();
                TriggerReaction("Vaporization", existing);
                return true;
            }

            // 4. 热 + 油 = 爆燃 (Ignition)
            if (Match(existing, incoming, PhysicalMediaType.Heat, PhysicalMediaType.Liquid_Oil))
            {
                ApplyReactionCooldown();
                TriggerReaction("Ignition", existing);
                return true;
            }
        }
        return false;
    }

    // 辅助工具：判断两个属性是否匹配目标组合（不分先后顺序）
    private bool Match(PhysicalMediaType a, PhysicalMediaType b, PhysicalMediaType target1, PhysicalMediaType target2)
    {
        return (a == target1 && b == target2) || (a == target2 && b == target1);
    }

    private void TriggerReaction(string reactionName, PhysicalMediaType consumedStatus)
    {
        // 1. 原有的日志保留，方便调试
        Debug.Log($"<color=yellow>[反应] {gameObject.name} 触发了 {reactionName}！</color>");
        
        // 2. 消耗掉参与反应的旧状态
        activeStatusTimers.Remove(consumedStatus);

        // 3. 【核心连接】告诉中枢执行实际效果
        if (PhysicalReactionManager.Instance != null)
        {
            // 传入反应名和自己（源头）
            PhysicalReactionManager.Instance.ExecuteReaction(reactionName, this.gameObject);
        }
    }

    private void ApplyReactionCooldown()
    {
        lastReactionTime = Time.time;
    }
    
    // 提供给外部查询是否有某种状态
    public bool HasStatus(PhysicalMediaType type) => activeStatusTimers.ContainsKey(type);
}