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
    /// ��ȸ���һ��VisualElement����������Ԫ��
    /// </summary>
    /// <param name="original">ԭʼVisualElement</param>
    /// <returns>���ƺ����VisualElement</returns>
    public static VisualElement DeepCopy(this VisualElement original)
    {
        if (original == null)
            return null;

        // ����ͬ���͵���Ԫ��
        VisualElement copiedElement = CreateNewInstance(original);

        // ���ƻ�������
        CopyBasicProperties(original, copiedElement);

        // ������ʽ
        CopyStyle(original, copiedElement);

        // ������ʽ��
        CopyStyleClasses(original, copiedElement);

        // ���Ʋ������� - �Ƴ��� SetLayout ����
        CopyLayoutProperties(original, copiedElement);

        // ���Ʊ任����
        CopyTransformProperties(original, copiedElement);

        // �ݹ鸴����Ԫ��
        CopyChildren(original, copiedElement);

        return copiedElement;
    }

    private static VisualElement CreateNewInstance(VisualElement original)
    {
        // ����Ԫ�����ʹ�����ʵ��
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
                // ����δ֪���ͣ�����ʹ��Ĭ�Ϲ��캯��
                try
                {
                    return System.Activator.CreateInstance(original.GetType()) as VisualElement;
                }
                catch
                {
                    Debug.LogWarning($"�޷����� {original.GetType().Name} ���͵�ʵ����ʹ�û��� VisualElement ����");
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

        // ���ƿɼ��Ժͽ�����
        copy.visible = original.visible;
        copy.SetEnabled(original.enabledSelf);
    }

    private static void CopyStyle(VisualElement original, VisualElement copy)
    {
        // �ֶ�����������ʽֵ
        CopyStyleProperties(original, copy);
    }

    private static void CopyStyleProperties(VisualElement original, VisualElement copy)
    {
        // ���������ʽ
        CopyLayoutStyleProperties(original, copy);

        // ��������ʽ
        CopyAppearanceStyleProperties(original, copy);

        // �任�����ʽ
        CopyTransformStyleProperties(original, copy);

        // �ı������ʽ
        CopyTextStyleProperties(original, copy);

        // �߿������ʽ
        CopyBorderStyleProperties(original, copy);
    }

    private static void CopyLayoutStyleProperties(VisualElement original, VisualElement copy)
    {
        // �ߴ�����
        copy.style.width = original.style.width;

        copy.style.height = original.style.height;

        copy.style.minWidth = original.style.minWidth;

        copy.style.minHeight = original.style.minHeight;

        copy.style.maxWidth = original.style.maxWidth;

        copy.style.maxHeight = original.style.maxHeight;

        // ���Բ�������
        copy.style.flexGrow = original.style.flexGrow;

        copy.style.flexShrink = original.style.flexShrink;

        copy.style.flexBasis = original.style.flexBasis;

        copy.style.flexDirection = original.style.flexDirection;

        // ��������
        copy.style.alignItems = original.style.alignItems;

        copy.style.alignContent = original.style.alignContent;

        copy.style.justifyContent = original.style.justifyContent;

        // λ������
        copy.style.position = original.style.position;

        copy.style.left = original.style.left;

        copy.style.top = original.style.top;

        copy.style.right = original.style.right;

        copy.style.bottom = original.style.bottom;
    }

    private static void CopyAppearanceStyleProperties(VisualElement original, VisualElement copy)
    {
        // ������ɫ
            copy.style.backgroundColor = original.style.backgroundColor;

        // ͸����
            copy.style.opacity = original.style.opacity;

        // ��ʾ/����
            copy.style.display = original.style.display;

        // �ɼ���
            copy.style.visibility = original.style.visibility;
    }

    private static void CopyTransformStyleProperties(VisualElement original, VisualElement copy)
    {
        // �任����
            copy.style.translate = original.style.translate;

            copy.style.rotate = original.style.rotate;

            copy.style.scale = original.style.scale;

            copy.style.transformOrigin = original.style.transformOrigin;
    }

    private static void CopyTextStyleProperties(VisualElement original, VisualElement copy)
    {
        // �ı���ɫ
            copy.style.color = original.style.color;

        // �����С
            copy.style.fontSize = original.style.fontSize;

        // ������ʽ - ʹ�ð�ȫ�ķ��䷽��
        TryCopyFontStyleProperty(original, copy);

        // �ı�����
            copy.style.unityTextAlign = original.style.unityTextAlign;

        // �������ԣ�������ã�
        TryCopyFontProperty(original, copy);

        // �ּ��
            copy.style.letterSpacing = original.style.letterSpacing;

        // �ı����
            copy.style.textOverflow = original.style.textOverflow;

        // �ı���Ӱ
            copy.style.textShadow = original.style.textShadow;
    }

    private static void TryCopyFontStyleProperty(VisualElement original, VisualElement copy)
    {
        // ���Բ�ͬ��������ʽ��������
        // ����1��ʹ�÷����������Ƿ����
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
            // ����2������ʹ�� unityFont ���ԣ�������ã�
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
        // ���Ը�����������
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
        // �߾�
        if (original.style.marginLeft.keyword != StyleKeyword.Undefined)
            copy.style.marginLeft = original.style.marginLeft;

        if (original.style.marginRight.keyword != StyleKeyword.Undefined)
            copy.style.marginRight = original.style.marginRight;

        if (original.style.marginTop.keyword != StyleKeyword.Undefined)
            copy.style.marginTop = original.style.marginTop;

        if (original.style.marginBottom.keyword != StyleKeyword.Undefined)
            copy.style.marginBottom = original.style.marginBottom;

        // ���
        if (original.style.paddingLeft.keyword != StyleKeyword.Undefined)
            copy.style.paddingLeft = original.style.paddingLeft;

        if (original.style.paddingRight.keyword != StyleKeyword.Undefined)
            copy.style.paddingRight = original.style.paddingRight;

        if (original.style.paddingTop.keyword != StyleKeyword.Undefined)
            copy.style.paddingTop = original.style.paddingTop;

        if (original.style.paddingBottom.keyword != StyleKeyword.Undefined)
            copy.style.paddingBottom = original.style.paddingBottom;

        // �߿���
        if (original.style.borderLeftWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderLeftWidth = original.style.borderLeftWidth;

        if (original.style.borderRightWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderRightWidth = original.style.borderRightWidth;

        if (original.style.borderTopWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderTopWidth = original.style.borderTopWidth;

        if (original.style.borderBottomWidth.keyword != StyleKeyword.Undefined)
            copy.style.borderBottomWidth = original.style.borderBottomWidth;

        // �߿���ɫ
        if (original.style.borderLeftColor.keyword != StyleKeyword.Undefined)
            copy.style.borderLeftColor = original.style.borderLeftColor;

        if (original.style.borderRightColor.keyword != StyleKeyword.Undefined)
            copy.style.borderRightColor = original.style.borderRightColor;

        if (original.style.borderTopColor.keyword != StyleKeyword.Undefined)
            copy.style.borderTopColor = original.style.borderTopColor;

        if (original.style.borderBottomColor.keyword != StyleKeyword.Undefined)
            copy.style.borderBottomColor = original.style.borderBottomColor;

        // �߿�Բ��
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
        // ������ܴ��ڵ�Ĭ����
        copy.ClearClassList();

        // ����������ʽ��
        foreach (string styleClass in original.GetClasses())
        {
            copy.AddToClassList(styleClass);
        }
    }

    private static void CopyLayoutProperties(VisualElement original, VisualElement copy)
    {
        // VisualElement û�� SetLayout �����������Ҫ�ֶ����Ʋ�������
        // ���������Ѿ��� CopyLayoutStyleProperties �д���

        // �����Ҫ���Ƽ����Ĳ�����Ϣ������ʹ�����·�����
        // ע�⣺layout ������ֻ���ģ�����ֱ������

        // ���Գ���ͨ����ʽϵͳ���Ʋ���Ч��
        CopyComputedLayoutProperties(original, copy);
    }

    private static void CopyComputedLayoutProperties(VisualElement original, VisualElement copy)
    {
        // ����ͨ����ʽ���Ƽ����Ĳ���Ч��
        // ���ַ������������ģ���������һ���̶��ϸ����Ӿ�����

        // ����λ�úͳߴ�
        copy.style.left = original.layout.x;
        copy.style.top = original.layout.y;
        copy.style.width = original.layout.width;
        copy.style.height = original.layout.height;
    }

    private static void CopyTransformProperties(VisualElement original, VisualElement copy)
    {
        // �任�����Ѿ��� CopyTransformStyleProperties �д���
    }

    private static void CopyChildren(VisualElement original, VisualElement copy)
    {
        foreach (var child in original.Children())
        {
            var copiedChild = child.DeepCopy();
            copy.Add(copiedChild);
        }
    }

    // �������͵ĸ��Ʒ�������ѡ��չ��
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
            // ע�⣺����¼����ᱻ���ƣ���Ҫ���°�
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