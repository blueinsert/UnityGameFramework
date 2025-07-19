# UIDocumentManager 使用指南

## 概述

UIDocumentManager 是一个单例类，用于管理 Unity UI Toolkit 中的 UIDocument 组件。它提供了统一的接口来操作 UI 元素，包括文本设置、可见性控制、样式管理和事件监听等功能。

## 主要功能

### 1. 单例模式
- 全局访问点：`UIDocumentManager.Instance`
- 自动创建：如果不存在会自动创建实例
- 场景持久：使用 `DontDestroyOnLoad` 保持跨场景存在

### 2. UIDocument 管理
- 注册/注销 UIDocument
- 支持多个 UIDocument 同时管理
- 文档名称索引系统

### 3. UI 元素操作
- 获取 UI 元素
- 设置文本内容
- 控制可见性
- 管理样式类
- 事件监听

## 基本使用

### 1. 设置 UIDocumentManager

```csharp
// 在 Inspector 中设置主 UIDocument
[SerializeField] private UIDocument mainUIDocument;

// 在 Start 方法中注册
void Start()
{
    UIDocumentManager.Instance.RegisterUIDocument("Main", mainUIDocument);
}
```

### 2. 操作 UI 元素

```csharp
// 设置文本
UIDocumentManager.Instance.SetElementText("ScoreLabel", "Score: 100");

// 控制可见性
UIDocumentManager.Instance.SetElementVisibility("SettingsPanel", false);

// 添加样式
UIDocumentManager.Instance.SetElementStyle("HealthBar", "warning");

// 移除样式
UIDocumentManager.Instance.RemoveElementStyle("HealthBar", "warning");
```

### 3. 事件监听

```csharp
// 添加点击事件
UIDocumentManager.Instance.AddClickEventListener("StartButton", OnStartButtonClicked);

// 移除事件监听
UIDocumentManager.Instance.RemoveClickEventListener("StartButton", OnStartButtonClicked);

private void OnStartButtonClicked()
{
    Debug.Log("Button clicked!");
}
```

### 4. 事件订阅

```csharp
void Start()
{
    // 订阅事件
    UIDocumentManager.Instance.OnUIElementUpdated += OnUIElementUpdated;
    UIDocumentManager.Instance.OnUIDocumentLoaded += OnUIDocumentLoaded;
}

void OnDestroy()
{
    // 取消订阅
    UIDocumentManager.Instance.OnUIElementUpdated -= OnUIElementUpdated;
    UIDocumentManager.Instance.OnUIDocumentLoaded -= OnUIDocumentLoaded;
}

private void OnUIElementUpdated(string elementName)
{
    Debug.Log($"Element {elementName} updated");
}
```

## 高级功能

### 1. 多文档管理

```csharp
// 注册多个 UIDocument
UIDocumentManager.Instance.RegisterUIDocument("Main", mainDocument);
UIDocumentManager.Instance.RegisterUIDocument("Popup", popupDocument);

// 操作特定文档中的元素
UIDocumentManager.Instance.SetElementText("Title", "Popup Title", "Popup");
```

### 2. 直接元素访问

```csharp
// 获取元素并直接操作
VisualElement element = UIDocumentManager.Instance.GetUIElement("CustomElement");
if (element != null)
{
    element.style.backgroundColor = Color.red;
}
```

### 3. 缓存管理

```csharp
// 清空元素缓存（当 UI 结构发生变化时）
UIDocumentManager.Instance.ClearElementCache();
```

## 最佳实践

### 1. 命名规范
- 使用有意义的元素名称
- 保持文档名称的一致性
- 避免使用特殊字符

### 2. 错误处理
```csharp
VisualElement element = UIDocumentManager.Instance.GetUIElement("MyElement");
if (element == null)
{
    Debug.LogWarning("Element not found, check if UIDocument is loaded");
    return;
}
```

### 3. 性能优化
- 缓存频繁访问的元素
- 及时清理不需要的事件监听
- 避免在 Update 中频繁调用

### 4. 内存管理
```csharp
void OnDestroy()
{
    // 清理事件监听
    if (UIDocumentManager.Instance != null)
    {
        UIDocumentManager.Instance.RemoveClickEventListener("MyButton", OnButtonClicked);
    }
}
```

## 注意事项

1. **初始化时机**：确保 UIDocument 已加载后再进行操作
2. **元素名称**：确保 UI 元素名称与 UXML 文件中的名称一致
3. **线程安全**：UI 操作必须在主线程中进行
4. **内存泄漏**：及时取消事件订阅和清理缓存

## 示例场景

参考 `UIDocumentManagerExample.cs` 文件，其中包含了完整的使用示例，展示了：
- 基本的 UI 元素操作
- 事件监听和响应
- 动态样式管理
- 多文档协作

## 故障排除

### 常见问题

1. **元素找不到**
   - 检查元素名称是否正确
   - 确认 UIDocument 已加载
   - 验证 UXML 文件结构

2. **事件不响应**
   - 确认事件监听器已正确添加
   - 检查元素是否可交互
   - 验证事件类型是否匹配

3. **样式不生效**
   - 检查 USS 文件是否正确引用
   - 确认样式类名称正确
   - 验证样式优先级

## API 参考

详细的方法说明请参考 `UIDocumentManager.cs` 文件中的注释。 