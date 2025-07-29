// 注意，它不是 class，是 interface，也不继承 MonoBehaviour
public interface IInteractable
{
    // 任何实现这个接口的类，都必须有一个名为 Interact 的、无参数、无返回值的方法
    void Interact();
    
    // (可选) 还可以定义一些用于显示提示的属性
    // string InteractionPrompt { get; } 
}