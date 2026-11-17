using System.Collections.Generic;
using UnityEngine;

//此脚本提供gameobject可供射击的属性
public class Shootable : MonoBehaviour
{
    // 全局注册表：避免 ShootController 每帧 FindObjectsOfType
    public static readonly HashSet<Shootable> All = new HashSet<Shootable>();

    private void OnEnable() => All.Add(this);
    private void OnDisable() => All.Remove(this);
}
