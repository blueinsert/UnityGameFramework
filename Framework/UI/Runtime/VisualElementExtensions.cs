using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;
using UnityEngine.UIElements;
using Button = UnityEngine.UIElements.Button;

public static class VisualElementExtensions
{
    /// <summary>
    /// 深度复制一个VisualElement及其所有子元素
    /// </summary>
    /// <param name="original">原始VisualElement</param>
    /// <returns>复制后的新VisualElement</returns>
    public static VisualElement DeepCopy(this VisualElement original)
    {
        if (original == null)
            return null;

        // 创建同类型的新元素
        VisualElement copiedElement = CreateNewInstance(original);

        // 复制基本属性
        CopyBasicProperties(original, copiedElement);

        // 复制样式
        CopyStyle(original, copiedElement);

        // 复制样式类
        CopyStyleClasses(original, copiedElement);

        // 复制布局数据 - 移除了 SetLayout 调用
        CopyLayoutProperties(original, copiedElement);

        // 复制变换属性
        CopyTransformProperties(original, copiedElement);

        // 递归复制子元素
        CopyChildren(original, copiedElement);

        return copiedElement;
    }

    private static VisualElement CreateNewInstance(VisualElement original)
    {
        // 根据元素类型创建新实例
        switch (original)
        {
            case Label label:
                return new Label();
            case UnityEngine.UIElements.Button button:
                return new UnityEngine.UIElements.Button();
            case TextField textField:
                return new TextField();
            case Toggle toggle:
                return new Toggle();
            case ScrollView scrollView:
                return new ScrollView();
            case Slider slider:
                return new Slider();
            case IMGUIContainer imguiContainer:
                return new IMGUIContainer(imguiContainer.onGUIHandler);
            case VisualElement visualElement:
                return new VisualElement();
            default:
                // 对于未知类型，尝试使用默认构造函数
                try
                {
                    return System.Activator.CreateInstance(original.GetType()) as VisualElement;
                }
                catch
                {
                    Debug.LogWarning($"无法创建 {original.GetType().Name} 类型的实例，使用基类 VisualElement 代替");
                    return new VisualElement();
                }
        }
    }

    private static void CopyBasicProperties(VisualElement original, VisualElement copy)
    {
        copy.name = original.name;
        copy.tooltip = original.tooltip;
        copy.viewDataKey = original.viewDataKey;
        copy.pickingMode = original.pickingMode;
        copy.focusable = original.focusable;
        copy.tabIndex = original.tabIndex;

        // 复制可见性和交互性
        copy.visible = original.visible;
        copy.SetEnabled(original.enabledSelf);
    }

    private static void CopyStyle(VisualElement original, VisualElement copy)
    {
        // 手动复制所有样式值
        CopyStyleProperties(original, copy);
    }

    private static void CopyStyleProperties(VisualElement original, VisualElement copy)
    {
        // 布局相关样式
        CopyLayoutStyleProperties(original, copy);

        // 外观相关样式
        CopyAppearanceStyleProperties(original, copy);

        // 变换相关样式
        CopyTransformStyleProperties(original, copy);

        // 文本相关样式
        CopyTextStyleProperties(original, copy);

        // 边框相关样式
        CopyBorderStyleProperties(original, copy);
    }

    private static void CopyLayoutStyleProperties(VisualElement original, VisualElement copy)
    {
        // 尺寸属性
        copy.style.width = original.style.width;

        copy.style.height = original.style.height;

        copy.style.minWidth = original.style.minWidth;

        copy.style.minHeight = original.style.minHeight;

        copy.style.maxWidth = original.style.maxWidth;

        copy.style.maxHeight = original.style.maxHeight;

        // 弹性布局属性
        copy.style.flexGrow = original.style.flexGrow;

        copy.style.flexShrink = original.style.flexShrink;

        copy.style.flexBasis = original.style.flexBasis;

        copy.style.flexDirection = original.style.flexDirection;

        // 对齐属性
        copy.style.alignItems = original.style.alignItems;

        copy.style.alignContent = original.style.alignContent;

        copy.style.justifyContent = original.style.justifyContent;

        // 位置属性
        copy.style.position = original.style.position;

        copy.style.left = original.style.left;

        copy.style.top = original.style.top;

        copy.style.right = original.style.right;

        copy.style.bottom = original.style.bottom;
    }

    private static void CopyAppearanceStyleProperties(VisualElement original, VisualElement copy)
    {
        // 背景颜色
            copy.style.backgroundColor = original.style.backgroundColor;

        // 透明度
            copy.style.opacity = original.style.opacity;

        // 显示/隐藏
            copy.style.display = original.style.display;

        // 可见性
            copy.style.visibility = original.style.visibility;
    }

    private static void CopyTransformStyleProperties(VisualElement original, VisualElement copy)
    {
        // 变换属性
            copy.style.translate = original.style.translate;

            copy.style.rotate = original.style.rotate;

            copy.style.scale = original.style.scale;

            copy.style.transformOrigin = original.style.transformOrigin;
    }

    private static void CopyTextStyleProperties(VisualElement original, VisualElement copy)
    {
        // 文本颜色
            copy.style.color = original.style.color;

        // 字体大小
            copy.style.fontSize = original.style.fontSize;

        // 字体样式 - 使用安全的反射方法
        TryCopyFontStyleProperty(original, copy);

        // 文本对齐
            copy.style.unityTextAlign = original.style.unityTextAlign;

        // 字体属性（如果可用）
        TryCopyFontProperty(original, copy);

        // 字间距
            copy.style.letterSpacing = original.style.letterSpacing;

        // 文本溢出
            copy.style.textOverflow = original.style.textOverflow;

        // 文本阴影
            copy.style.textShadow = original.style.textShadow;
    }

    private static void TryCopyFontStyleProperty(VisualElement original, VisualElement copy)
    {
        // 尝试不同的字体样式属性名称
        // 方法1：使用反射检查属性是否存在
        var styleType = original.style.GetType();
        var fontStyleProperty = styleType.GetProperty("unityFontStyle");

        if (fontStyleProperty != null)
        {
            var fontStyleValue = fontStyleProperty.GetValue(original.style);
            var keywordProperty = fontStyleValue.GetType().GetProperty("keyword");

            if (keywordProperty != null)
            {
                var keyword = (StyleKeyword)keywordProperty.GetValue(fontStyleValue);
                if (keyword != StyleKeyword.Undefined)
                {
                    fontStyleProperty.SetValue(copy.style, fontStyleValue);
                }
            }
        }
        else
        {
            // 方法2：尝试使用 unityFont 属性（如果可用）
            var unityFontProperty = styleType.GetProperty("unityFont");
            if (unityFontProperty != null)
            {
                var fontValue = unityFontProperty.GetValue(original.style);
                var keywordProperty = fontValue.GetType().GetProperty("keyword");

                if (keywordProperty != null)
                {
                    var keyword = (StyleKeyword)keywordProperty.GetValue(fontValue);
                    if (keyword != StyleKeyword.Undefined)
                    {
                        unityFontProperty.SetValue(copy.style, fontValue);
                    }
                }
            }
        }
    }

    private static void TryCopyFontProperty(VisualElement original, VisualElement copy)
    {
        // 尝试复制字体属性
        var styleType = original.style.GetType();
        var fontProperty = styleType.GetProperty("unityFont");

        if (fontProperty != null)
        {
            var fontValue = fontProperty.GetValue(original.style);
            var keywordProperty = fontValue.GetType().GetProperty("keyword");

            if (keywordProperty != null)
            {
                var keyword = (StyleKeyword)keywordProperty.GetValue(fontValue);
                if (keyword != StyleKeyword.Undefined)
                {
                    fontProperty.SetValue(copy.style, fontValue);
                }
            }
        }
    }

    private static void CopyBorderStyleProperties(VisualElement original, VisualElement copy)
    {
        // 边距
        if (original.style.marginLeft.keyword != StyleKeyword.Undefined)
            copy.style.marginLeft = original.style.marginLeft;

        if (original.style.marginRight.keyword != StyleKeyword.Undefined)
            copy.style.marginRight = original.style.marginRight;

        if (original.style.marginTop.keyword != StyleKeyword.Undefined)
            copy.style.marginTop = original.style.marginTop;

        if (original.style.marginBottom.keyword != StyleKeyword.Undefined)
            copy.style.marginBottom = original.style.marginBottom;

        // 填充
        if (original.style.paddingLeft.keyword != StyleKeyword.Undefined)
            copy.style.paddingLeft = original.style.paddingLeft;

        if (original.style.paddingRight.keyword != StyleKeyword.Undefined)
            copy.style.paddingRight = original.style.paddingRight;

        if (original.style.paddingTop.keyword != StyleKeyword.Undefined)
            copy.style.paddingTop = original.style.paddingTop;

        if (original.style.paddingBottom.keyword != StyleKeyword.Undefined)
            copy.style.paddingBottom = original.style.paddingBottom;

        // 边框宽度
        if (original.style.borderLeftWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderLeftWidth = original.style.borderLeftWidth;

        if (original.style.borderRightWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderRightWidth = original.style.borderRightWidth;

        if (original.style.borderTopWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderTopWidth = original.style.borderTopWidth;

        if (original.style.borderBottomWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderBottomWidth = original.style.borderBottomWidth;

        // 边框颜色
        if (original.style.borderLeftColor.keyword != StyleKeyword.Undefined)
            copy.style.borderLeftColor = original.style.borderLeftColor;

        if (original.style.borderRightColor.keyword != StyleKeyword.Undefined)
            copy.style.borderRightColor = original.style.borderRightColor;

        if (original.style.borderTopColor.keyword != StyleKeyword.Undefined)
            copy.style.borderTopColor = original.style.borderTopColor;

        if (original.style.borderBottomColor.keyword != StyleKeyword.Undefined)
            copy.style.borderBottomColor = original.style.borderBottomColor;

        // 边框圆角
        if (original.style.borderTopLeftRadius.keyword != StyleKeyword.Undefined)
            copy.style.borderTopLeftRadius = original.style.borderTopLeftRadius;

        if (original.style.borderTopRightRadius.keyword != StyleKeyword.Undefined)
            copy.style.borderTopRightRadius = original.style.borderTopRightRadius;

        if (original.style.borderBottomLeftRadius.keyword != StyleKeyword.Undefined)
            copy.style.borderBottomLeftRadius = original.style.borderBottomLeftRadius;

        if (original.style.borderBottomRightRadius.keyword != StyleKeyword.Undefined)
            copy.style.borderBottomRightRadius = original.style.borderBottomRightRadius;
    }

    private static void CopyStyleClasses(VisualElement original, VisualElement copy)
    {
        // 清除可能存在的默认类
        copy.ClearClassList();

        // 复制所有样式类
        foreach (string styleClass in original.GetClasses())
        {
            copy.AddToClassList(styleClass);
        }
    }

    private static void CopyLayoutProperties(VisualElement original, VisualElement copy)
    {
        // VisualElement 没有 SetLayout 方法，因此需要手动复制布局属性
        // 布局属性已经在 CopyLayoutStyleProperties 中处理

        // 如果需要复制计算后的布局信息，可以使用以下方法：
        // 注意：layout 属性是只读的，不能直接设置

        // 可以尝试通过样式系统复制布局效果
        CopyComputedLayoutProperties(original, copy);
    }

    private static void CopyComputedLayoutProperties(VisualElement original, VisualElement copy)
    {
        // 尝试通过样式复制计算后的布局效果
        // 这种方法不是完美的，但可以在一定程度上复制视觉布局

        // 设置位置和尺寸
        copy.style.left = original.layout.x;
        copy.style.top = original.layout.y;
        copy.style.width = original.layout.width;
        copy.style.height = original.layout.height;
    }

    private static void CopyTransformProperties(VisualElement original, VisualElement copy)
    {
        // 变换属性已经在 CopyTransformStyleProperties 中处理
    }

    private static void CopyChildren(VisualElement original, VisualElement copy)
    {
        foreach (var child in original.Children())
        {
            var copiedChild = child.DeepCopy();
            copy.Add(copiedChild);
        }
    }

    // 特殊类型的复制方法（可选扩展）
    public static Label DeepCopy(this Label original)
    {
        var copy = original.DeepCopy() as Label;
        if (copy != null)
        {
            copy.text = original.text;
        }
        return copy;
    }

    public static UnityEngine.UIElements.Button DeepCopy(this UnityEngine.UIElements.Button original)
    {
        var copy = original.DeepCopy() as UnityEngine.UIElements.Button;
        if (copy != null)
        {
            copy.text = original.text;
            // 注意：点击事件不会被复制，需要重新绑定
        }
        return copy;
    }

    public static TextField DeepCopy(this TextField original)
    {
        var copy = original.DeepCopy() as TextField;
        if (copy != null)
        {
            copy.value = original.value;
            copy.isPasswordField = original.isPasswordField;
        }
        return copy;
    }

    public static Toggle DeepCopy(this Toggle original)
    {
        var copy = original.DeepCopy() as Toggle;
        if (copy != null)
        {
            copy.value = original.value;
            copy.text = original.text;
        }
        return copy;
    }
}