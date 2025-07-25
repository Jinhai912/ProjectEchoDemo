// SimpleUITester.cs
using UnityEngine;
using TMPro;

public class SimpleUITester : MonoBehaviour
{
    [Header("请在这里拖入预制体和Canvas")]
    public GameObject textPrefab; // 你的浮动文字预制体
    public Transform canvasTransform; // 你的主 Canvas 的 Transform
    public GameObject damageNumberPrefab;

    // Update is called once per frame
    void Update()
    {
        // 按下空格键时执行测试
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Debug.Log("--- 空格键被按下，开始测试 ---");

            // // --- 健壮性检查 ---
            // if (canvasTransform == null)
            // {
            //     Debug.LogError("测试失败：没有在 Inspector 中设置 canvasTransform！");
            //     return;
            // }
            // if (textPrefab == null)
            // {
            //     Debug.LogError("测试失败：没有在 Inspector 中设置 textPrefab！");
            //     return;
            // }

            // Debug.Log("预制体和Canvas引用都正常，准备 Instantiate...");

            // // 1. 实例化
            // GameObject textGO = Instantiate(textPrefab, canvasTransform);
            // Debug.Log("Instantiate 已执行。新对象名：" + textGO.name);

            // // 2. 设置位置 (先固定在屏幕中央)
            // textGO.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
            // Debug.Log("已将对象位置设置在屏幕中央。");

            // // 3. 获取脚本并调用 Setup
            // PickupFeedbackAnimator animator = textGO.GetComponent<PickupFeedbackAnimator>();
            // if (animator == null)
            // {
            //     Debug.LogError("测试失败：预制体上没有挂载 PickupFeedbackAnimator 脚本！");
            //     Destroy(textGO); // 销毁无用的对象
            //     return;
            // }

            // Debug.Log("成功获取 animator 脚本，准备调用 Setup...");
            // animator.Setup("测试成功!", true); // 使用特殊样式，效果更明显
            // Debug.Log("--- 测试结束 ---");
            // 我们现在要用 DamageNumberAnimator 来显示经验值！
            // 1. 实例化 DamageNumber_Prefab
            GameObject textGO = Instantiate(damageNumberPrefab, canvasTransform); // 注意，这里用的是 damageNumberPrefab
    
            // 2. 设置位置
            textGO.transform.position = new Vector2(Screen.width / 2, Screen.height / 2);
    
            // 3. 获取 DamageNumberAnimator 脚本
            DamageNumberAnimator animator = textGO.GetComponent<DamageNumberAnimator>();
            if (animator != null)
            {
                // 调用它的 Setup 方法，但传入经验值的信息
                animator.Setup(12345, true); // 传入一个数字和暴击状态
            }
        }
    }
}