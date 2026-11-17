using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    // --- 单例 ---
    public static PlayerData Instance { get; private set; }

    // 参数1: 当前已用内存, 参数2: 总上限
    public static event System.Action<int, int> OnMemoryChanged;
    // 参数1: 当前热量, 参数2: 热容上限, 参数3: 是否过热
    public static event System.Action<float, float, bool> OnHeatChanged;

    #region 1. 硬件插槽 (Hardware Slots)
    [Header("--- 当前硬件挂载 ---")]
    public HardwareCoreSO currentCore;     // 核心炉：决定内存和血量加成
    public HardwareBarrelSO currentBarrel; // 枪管：决定攻击形态、伤害、射速
    public HardwareHeatSinkSO currentHeatSink; // 散热器：决定热容和散热

    [Header("--- 初始硬件 (新开局使用) ---")]
    public HardwareCoreSO startingCore;
    public HardwareBarrelSO startingBarrel;
    public HardwareHeatSinkSO startingHeatSink;
    #endregion

    #region 2. 基础属性与加成 (数值层)
    [Header("--- 运行时加成 (Firmware & Softwares) ---")]
    public int bonusMaxHealth;
    public int bonusDefense;
    public float bonusAttack;
    public float moveSpeedMultiplier = 1.0f;
    public float fireRateMultiplier = 1.0f;
    public float critRateBonus;
    public float critDamageBonus;
    public float damageBonusMultiplier = 1.0f;
    public float pickupRadiusMultiplier = 1.0f;

    [Header("--- 默认/保底数值 (无硬件时使用) ---")]
    public int baseMaxHealth = 100;
    public float baseAttack = 10f;
    public float baseMoveSpeed = 4f;
    public float baseFireRate = 1f;
    public float initialPickupRadius = 5f;

    [Header("--- 初始资源 ---")]
    [Tooltip("每局开始时获得的回声碎片，用于商店购买")]
    public int startingCurrency = 300;

    [Header("--- 默认散热参数 (无散热器时) ---")]
    public float defaultHeatCapacity = 100f;
    public float defaultCoolingRate = 25f;
    public float defaultCoolingDelay = 0.5f;
    public float defaultOverheatRecoveryTime = 2f;
    public float defaultHeatPerShot = 5f;

    [Header("--- 机制状态 ---")]
    public int projectileCount = 1;     
    public int piercingCount = 0; 

    // --- 最终属性计算 (由硬件基础值驱动) ---

    // 内存上限：由核心炉决定
    public int maxMemoryMB => currentCore != null ? currentCore.memoryCapacity : 1024;

    // 最大生命值：基础 + 固件加成 + 核心加成
    public int MaxHealth => baseMaxHealth + bonusMaxHealth + (currentCore != null ? currentCore.healthBonus : 0);
    
    // 最终攻击力：核心枪管伤害 + 软件加成
    public float FinalAttack => (currentBarrel != null ? currentBarrel.baseDamage : baseAttack) + bonusAttack;
    
    // 最终射速：核心枪管射速 * 软件倍率
    public float FireRate => (currentBarrel != null ? currentBarrel.baseFireRate : baseFireRate) * fireRateMultiplier;

    public float HeatCapacity => currentHeatSink != null ? currentHeatSink.heatCapacity : defaultHeatCapacity;
    public float CoolingRate => currentHeatSink != null ? currentHeatSink.coolingRate : defaultCoolingRate;
    public float CoolingDelay => currentHeatSink != null ? currentHeatSink.coolingDelay : defaultCoolingDelay;
    public float OverheatRecoveryTime => currentHeatSink != null ? currentHeatSink.overheatRecoveryTime : defaultOverheatRecoveryTime;
    public float HeatPerShot => currentBarrel != null ? currentBarrel.heatPerShot : defaultHeatPerShot;
    public int FinalProjectileCount => Mathf.Max(1, projectileCount + (currentBarrel != null ? currentBarrel.projectilesPerShot - 1 : 0));
    public int FinalPierce => piercingCount + (currentBarrel != null ? currentBarrel.pierce : 0);

    public int Defense => bonusDefense;
    public float MoveSpeed => baseMoveSpeed * moveSpeedMultiplier;
    public float FinalCritRate => critRateBonus;
    public float FinalCritDamage => 1.5f + critDamageBonus; // 1.5为基础倍率
    public float FinalDamageBonus => 1.0f * damageBonusMultiplier;
    public float PickupRadius => initialPickupRadius * pickupRadiusMultiplier;
    #endregion

    #region 3. 运行时数据 (Runtime Data)
    [Header("--- 实时状态 ---")]
    public int currentHealth;
    public int currentExperience;
    public int currentCurrency; // 回声碎片
    public int currentUsedMemoryMB = 0;
    public float currentHeat;
    public bool isOverheated;

    private float _overheatTimer;
    private float _lastHeatTime = -999f;

    public bool CanFire => !isOverheated;
    public bool IsOverheated => isOverheated;

    public List<AbilityData> installedProtocols = new List<AbilityData>();
    public List<AbilityData> permanentFirmwares = new List<AbilityData>();
    public PhysicalMediaType currentBulletMedia = PhysicalMediaType.None;

    public int RemainingMemoryMB => maxMemoryMB - currentUsedMemoryMB;
    #endregion

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (isOverheated)
        {
            _overheatTimer -= Time.deltaTime;
            if (_overheatTimer <= 0f)
            {
                isOverheated = false;
                currentHeat = 0f;
                OnHeatChanged?.Invoke(currentHeat, HeatCapacity, false);
            }
            return;
        }

        if (currentHeat <= 0f) return;
        if (Time.time - _lastHeatTime < CoolingDelay) return;

        float newHeat = Mathf.Max(0f, currentHeat - CoolingRate * Time.deltaTime);
        if (!Mathf.Approximately(newHeat, currentHeat))
        {
            currentHeat = newHeat;
            OnHeatChanged?.Invoke(currentHeat, HeatCapacity, false);
        }
    }

    // --- 硬件更换接口 ---
    public void EquipCore(HardwareCoreSO newCore)
    {
        if (newCore == null) return;
        currentCore = newCore;
        if (currentHealth > MaxHealth) currentHealth = MaxHealth;
        // 核心更换后，内存上限变了，必须通知UI
        OnMemoryChanged?.Invoke(currentUsedMemoryMB, maxMemoryMB);
        PlayerStates.NotifyHealthChanged();
        Debug.Log($"<color=gold>[硬件] 核心炉已更新: {newCore.hardwareName}</color>");
    }

    public void EquipBarrel(HardwareBarrelSO newBarrel)
    {
        if (newBarrel == null) return;
        currentBarrel = newBarrel;
        Debug.Log($"<color=orange>[硬件] 枪管已更新: {newBarrel.hardwareName}</color>");
    }

    public void EquipHeatSink(HardwareHeatSinkSO newHeatSink)
    {
        if (newHeatSink == null) return;
        currentHeatSink = newHeatSink;
        ResetHeat();
        Debug.Log($"<color=cyan>[硬件] 散热器已更新: {newHeatSink.hardwareName}</color>");
    }

    public bool EquipHardware(HardwareData hardware)
    {
        if (hardware == null) return false;

        switch (hardware)
        {
            case HardwareCoreSO core:
                if (currentCore == core) return false;
                EquipCore(core);
                return true;

            case HardwareBarrelSO barrel:
                if (currentBarrel == barrel) return false;
                EquipBarrel(barrel);
                return true;

            case HardwareHeatSinkSO heatSink:
                if (currentHeatSink == heatSink) return false;
                EquipHeatSink(heatSink);
                return true;

            default:
                return false;
        }
    }

    public bool IsHardwareEquipped(HardwareData hardware)
    {
        if (hardware == null) return false;
        return hardware == currentCore || hardware == currentBarrel || hardware == currentHeatSink;
    }

    public void AddHeat(float amount)
    {
        if (amount <= 0f) return;

        currentHeat = Mathf.Min(currentHeat + amount, HeatCapacity);
        _lastHeatTime = Time.time;

        if (!isOverheated && currentHeat >= HeatCapacity - 0.001f)
        {
            isOverheated = true;
            _overheatTimer = OverheatRecoveryTime;
            Debug.Log($"<color=red>[过热] 散热保护启动，{OverheatRecoveryTime:0.0}s 后恢复。</color>");
        }

        OnHeatChanged?.Invoke(currentHeat, HeatCapacity, isOverheated);
    }

    public void ResetHeat()
    {
        currentHeat = 0f;
        isOverheated = false;
        _overheatTimer = 0f;
        _lastHeatTime = -999f;
        OnHeatChanged?.Invoke(currentHeat, HeatCapacity, false);
    }

    // --- 重置逻辑 ---
    public void InitializeForNewRun()
    {
        bonusMaxHealth = 0; bonusDefense = 0; bonusAttack = 0;
        moveSpeedMultiplier = 1.0f; fireRateMultiplier = 1.0f;
        critRateBonus = 0f; critDamageBonus = 0f; damageBonusMultiplier = 1.0f; pickupRadiusMultiplier = 1.0f;
        
        projectileCount = 1; piercingCount = 0;
        currentExperience = 0;
        currentCurrency = startingCurrency;
        currentUsedMemoryMB = 0;

        currentCore = startingCore;
        currentBarrel = startingBarrel;
        currentHeatSink = startingHeatSink;
        
        installedProtocols.Clear();
        permanentFirmwares.Clear();
        currentBulletMedia = PhysicalMediaType.None;
        ResetHeat();

        // 最后初始化血量和UI
        currentHealth = MaxHealth; 
        OnMemoryChanged?.Invoke(currentUsedMemoryMB, maxMemoryMB);
    }

    public void AddExperience(int amount) => currentExperience += amount;
    public void AddCurrency(int amount) => currentCurrency += amount;

    public bool TrySpendCurrency(int amount)
    {
        if (currentCurrency >= amount) { currentCurrency -= amount; return true; }
        return false;
    }

    // --- 能力应用分流逻辑 ---
    public void ApplyAbility(AbilityData ability)
    {
        if (ability == null) return;
        if (ability.isMemoryBound) InstallProtocol(ability);
        else InstallFirmware(ability);
    }

    private void InstallProtocol(AbilityData ability)
    {
        if (currentUsedMemoryMB + ability.memoryCost > maxMemoryMB)
        {
            Debug.LogWarning($"<color=red>[警告] 内存不足: {ability.abilityName}</color>");
            return;
        }
        currentUsedMemoryMB += ability.memoryCost;
        installedProtocols.Add(ability);
        OnMemoryChanged?.Invoke(currentUsedMemoryMB, maxMemoryMB);
        ActivateEffects(ability);
    }

    private void InstallFirmware(AbilityData ability)
    {
        permanentFirmwares.Add(ability);
        ActivateEffects(ability);
    }

    private void ActivateEffects(AbilityData ability)
    {
        foreach (var effect in ability.effects)
            if (effect != null) effect.OnEquip(this);
    }

    public void UninstallProtocol(AbilityData ability)
    {
        if (!installedProtocols.Contains(ability)) return;
        foreach (var effect in ability.effects)
            if (effect != null) effect.OnRemove(this);
        currentUsedMemoryMB -= ability.memoryCost;
        installedProtocols.Remove(ability);
        OnMemoryChanged?.Invoke(currentUsedMemoryMB, maxMemoryMB);
    }
}
