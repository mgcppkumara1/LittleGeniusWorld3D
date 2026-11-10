using System;
using UnityEngine;
using UnityEngine.UI;

// Tiny code-only uGUI helpers (no prefabs/assets needed).
public static class UIKit
{
    static Font uiFont;
    static Sprite round;

    public static Font UIFont
    {
        get
        {
            if (uiFont == null)
                uiFont = Font.CreateDynamicFontFromOSFont(
                    new[] { "Roboto", "Noto Sans", "Noto Sans Sinhala", "Noto Sans Sinhala UI", "Iskoola Pota", "Nirmala UI", "Arial" }, 48);
            return uiFont;
        }
    }

    public static Sprite Round
    {
        get
        {
            if (round == null)
            {
                const int s = 64, r = 24;
                var t = new Texture2D(s, s, TextureFormat.RGBA32, false);
                var px = new Color[s * s];
                for (int y = 0; y < s; y++)
                    for (int x = 0; x < s; x++)
                    {
                        float dx = Mathf.Max(r - x - 0.5f, x + 0.5f - (s - r), 0f);
                        float dy = Mathf.Max(r - y - 0.5f, y + 0.5f - (s - r), 0f);
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        px[y * s + x] = new Color(1, 1, 1, Mathf.Clamp01(r - d + 0.5f));
                    }
                t.SetPixels(px);
                t.filterMode = FilterMode.Bilinear;
                t.Apply();
                round = Sprite.Create(t, new Rect(0, 0, s, s), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
            }
            return round;
        }
    }

    public static RectTransform Rect(string name, Transform parent, Vector2 aMin, Vector2 aMax)
    {
        var go = new GameObject(name, typeof(RectTransform));
        var rt = go.GetComponent<RectTransform>();
        rt.SetParent(parent, false);
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        return rt;
    }

    public static Image Panel(Transform parent, Vector2 aMin, Vector2 aMax, Color c)
    {
        var rt = Rect("Panel", parent, aMin, aMax);
        var im = rt.gameObject.AddComponent<Image>();
        im.sprite = Round; im.type = Image.Type.Sliced; im.color = c;
        return im;
    }

    public static Text Label(Transform parent, string s, int size, Color c, Vector2 aMin, Vector2 aMax, TextAnchor a = TextAnchor.MiddleCenter)
    {
        var rt = Rect("Text", parent, aMin, aMax);
        var t = rt.gameObject.AddComponent<Text>();
        t.font = UIFont; t.text = s; t.fontSize = size; t.color = c; t.alignment = a;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.raycastTarget = false;
        return t;
    }

    public static Button Btn(Transform parent, string s, int size, Color bg, Color fg, Vector2 aMin, Vector2 aMax, Action onClick)
    {
        var im = Panel(parent, aMin, aMax, bg);
        im.name = "Button";
        var b = im.gameObject.AddComponent<Button>();
        b.targetGraphic = im;
        var cb = b.colors; cb.pressedColor = new Color(.8f, .8f, .8f); b.colors = cb;
        Label(im.transform, s, size, fg, Vector2.zero, Vector2.one);
        b.onClick.AddListener(() => onClick());
        return b;
    }

    public static void Clear(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var c = t.GetChild(i).gameObject;
            c.SetActive(false);
            UnityEngine.Object.Destroy(c);
        }
    }

    // Normalised cell inside a cols x rows grid (row 0 = top).
    public static void Cell(int i, int cols, int rows, float pad, out Vector2 mn, out Vector2 mx)
    {
        int col = i % cols, row = i / cols;
        float w = 1f / cols, h = 1f / rows;
        mn = new Vector2(col * w + pad * w, 1f - (row + 1) * h + pad * h);
        mx = new Vector2((col + 1) * w - pad * w, 1f - row * h - pad * h);
    }
}
