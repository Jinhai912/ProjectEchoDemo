using UnityEngine;
using System.Collections.Generic;

public class PhysicalReactionManager : MonoBehaviour
{
    public static PhysicalReactionManager Instance { get; private set; }
    public static System.Action<string, Vector3> OnAnyReaction;

    [Header("反应参数")]
    public float conductionRadius = 5f;
    public float overloadExplosionRadius = 3f;
    public float overloadForce = 500f;

    [Header("视觉特效 (拖入 Cartoon FX 预制体)")]
    public GameObject vfxConduction; // 电弧/闪电
    public GameObject vfxOverload;   // 大爆炸
    public GameObject vfxVaporization; // 烟雾喷发
    public GameObject vfxIgnition;     // 地面火花

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void ExecuteReaction(string reactionName, GameObject originSource)
    {
        Vector3 pos = originSource.transform.position;
        TriggerFeedback(reactionName, pos); // 1. 触发跳字和特效

        switch (reactionName)
        {
            case "Conduction": HandleConduction(pos, originSource); break;
            case "Overload": HandleOverload(pos, originSource); break;
            case "Vaporization": HandleVaporization(pos, originSource); break;
            case "Ignition": HandleIgnition(pos, originSource); break;
        }
        
        OnAnyReaction?.Invoke(reactionName, pos);
    }

    private void TriggerFeedback(string name, Vector3 pos)
    {
        string cnName = "";
        GameObject vfxPrefab = null;

        switch (name)
        {
            case "Conduction": cnName = "【传导】"; vfxPrefab = vfxConduction; break;
            case "Overload": cnName = "【过载】"; vfxPrefab = vfxOverload; break;
            case "Vaporization": cnName = "【气化】"; vfxPrefab = vfxVaporization; break;
            case "Ignition": cnName = "【爆燃】"; vfxPrefab = vfxIgnition; break;
        }

        // 弹出大号跳字 (假设你的反馈管理器叫 PlayerFeedbackManager)
        PlayerFeedbackManager.OnFeedbackRequested?.Invoke(cnName, true, pos + Vector3.up * 1.5f);

        // 生成视觉特效
        if (vfxPrefab != null)
        {
            GameObject vfx = Instantiate(vfxPrefab, pos, Quaternion.identity);
            Destroy(vfx, 2f); // 2秒后自动清理
        }
    }

    // --- 以下 Handle 方法保持你之前的逻辑，仅在最后删除原有的 OnAnyReaction 触发（因为已挪到开头） ---
    private void HandleConduction(Vector3 position, GameObject source)
    {
        float baseDmg = PlayerData.Instance.FinalAttack;
        Collider[] colliders = Physics.OverlapSphere(position, conductionRadius);
        foreach (var hit in colliders)
        {
            if (hit.gameObject == source) continue;
            StatusContainer targetStatus = hit.GetComponent<StatusContainer>();
            if (targetStatus != null && targetStatus.HasStatus(PhysicalMediaType.Liquid_Water))
            {
                EnemyState enemy = hit.GetComponent<EnemyState>();
                if (enemy != null) enemy.TakeDamage(baseDmg, false);
            }
        }
    }

    private void HandleOverload(Vector3 position, GameObject source)
    {
        float baseDmg = PlayerData.Instance.FinalAttack;
        Collider[] colliders = Physics.OverlapSphere(position, overloadExplosionRadius);
        foreach (var hit in colliders)
        {
            EnemyState enemy = hit.GetComponent<EnemyState>();
            if (enemy != null) enemy.TakeDamage(baseDmg * 1.5f, false);
            Rigidbody rb = hit.GetComponent<Rigidbody>();
            if (rb != null) rb.AddExplosionForce(overloadForce, position, overloadExplosionRadius, 0.5f);
        }
    }

    private void HandleVaporization(Vector3 position, GameObject source)
    {
        GameObject steamZone = new GameObject("Logic_SteamZone");
        steamZone.transform.position = new Vector3(position.x, 0.7f, position.z);
        steamZone.AddComponent<TemporaryEncounterObject>();
        var col = steamZone.AddComponent<SphereCollider>();
        col.isTrigger = true; col.radius = 4f;
        steamZone.AddComponent<TemporaryZoneLogic>().zoneType = "Steam";
    }

    private void HandleIgnition(Vector3 position, GameObject source)
    {
        Collider[] existingZones = Physics.OverlapSphere(position, 2f);
        foreach (var foundCol in existingZones) if (foundCol.gameObject.name == "Logic_FireZone") return;

        GameObject fireZone = new GameObject("Logic_FireZone");
        fireZone.transform.position = new Vector3(position.x, 0.7f, position.z);
        fireZone.AddComponent<TemporaryEncounterObject>();
        var newCol = fireZone.AddComponent<SphereCollider>();
        newCol.isTrigger = true; newCol.radius = 3f;
        var logic = fireZone.AddComponent<TemporaryZoneLogic>();
        logic.zoneType = "Fire"; logic.damagePerTick = 5f; logic.duration = 8f;
    }
}