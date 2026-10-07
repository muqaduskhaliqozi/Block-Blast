using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace BlockBlast.Visuals
{
    public static class ProceduralSpriteGenerator
    {
        private static Sprite _roundedSquareSprite;
        private static Sprite _glossyBlockSprite;
        private static Sprite _borderSquareSprite;
        private static Sprite _softShadowSprite;
        private static Sprite _circleSprite;
        private static Sprite _softCircleSprite;
        private static Sprite _gradientSprite;
        private static Sprite _crownSprite;

        public static Sprite GetRoundedSquareSprite(int size = 128, float cornerRadiusRatio = 0.18f)
        {
            if (_roundedSquareSprite != null) return _roundedSquareSprite;
#if UNITY_EDITOR
            _roundedSquareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/RoundedSquare.png");
            if (_roundedSquareSprite != null) return _roundedSquareSprite;
#endif
            _roundedSquareSprite = CreateFallbackRoundedSquare(size, cornerRadiusRatio);
            return _roundedSquareSprite;
        }

        public static Sprite GetGlossyBlockSprite(int size = 128, float cornerRadiusRatio = 0.18f, float highlightIntensity = 0.25f, float shadowIntensity = 0.25f)
        {
            if (_glossyBlockSprite != null) return _glossyBlockSprite;
#if UNITY_EDITOR
            _glossyBlockSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/GlossyTile.png");
            if (_glossyBlockSprite != null) return _glossyBlockSprite;
#endif
            _glossyBlockSprite = CreateFallbackGlossyBlock(size, cornerRadiusRatio, highlightIntensity, shadowIntensity);
            return _glossyBlockSprite;
        }

        public static Sprite GetBorderSquareSprite(int size = 128, float cornerRadiusRatio = 0.18f, float borderWidthRatio = 0.08f)
        {
            if (_borderSquareSprite != null) return _borderSquareSprite;
#if UNITY_EDITOR
            _borderSquareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/CellBorder.png");
            if (_borderSquareSprite != null) return _borderSquareSprite;
#endif
            _borderSquareSprite = CreateFallbackBorderSquare(size, cornerRadiusRatio, borderWidthRatio);
            return _borderSquareSprite;
        }

        public static Sprite GetSoftShadowSprite(int size = 128, float cornerRadiusRatio = 0.22f)
        {
            if (_softShadowSprite != null) return _softShadowSprite;
#if UNITY_EDITOR
            _softShadowSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/SoftShadow.png");
            if (_softShadowSprite != null) return _softShadowSprite;
#endif
            _softShadowSprite = CreateFallbackSoftShadow(size, cornerRadiusRatio);
            return _softShadowSprite;
        }

        public static Sprite GetVerticalGradientSprite(Color topColor, Color bottomColor, int width = 32, int height = 256)
        {
            if (_gradientSprite != null) return _gradientSprite;
#if UNITY_EDITOR
            _gradientSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/BackgroundGradient.png");
            if (_gradientSprite != null) return _gradientSprite;
#endif
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] colors = new Color[width * height];
            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color c = Color.Lerp(bottomColor, topColor, t);
                for (int x = 0; x < width; x++) colors[y * width + x] = c;
            }
            tex.SetPixels(colors);
            tex.Apply();
            _gradientSprite = Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            return _gradientSprite;
        }

        public static Sprite GetSoftCircleSprite(int size = 128)
        {
            if (_softCircleSprite != null) return _softCircleSprite;
#if UNITY_EDITOR
            _softCircleSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/SoftCircle.png");
            if (_softCircleSprite != null) return _softCircleSprite;
#endif
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.SmoothStep(1.0f, 0.0f, Mathf.Clamp01(dist / radius));
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            _softCircleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            return _softCircleSprite;
        }

        public static Sprite GetCrownSprite(int size = 64)
        {
            if (_crownSprite != null) return _crownSprite;
#if UNITY_EDITOR
            _crownSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/Sprites/CrownIcon.png");
            if (_crownSprite != null) return _crownSprite;
#endif
            return GetRoundedSquareSprite();
        }

        private static Sprite CreateFallbackRoundedSquare(int size, float cornerRadiusRatio)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            float radius = size * cornerRadiusRatio;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = GetDistanceToRoundedCorner(x, y, size, size, radius);
                    float alpha = 1.0f - Mathf.Clamp01(dist - radius + 1f);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateFallbackGlossyBlock(int size, float cornerRadiusRatio, float highlightIntensity, float shadowIntensity)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            float radius = size * cornerRadiusRatio;
            for (int y = 0; y < size; y++)
            {
                float verticalGradient = (float)y / (size - 1);
                for (int x = 0; x < size; x++)
                {
                    float dist = GetDistanceToRoundedCorner(x, y, size, size, radius);
                    float alpha = 1.0f - Mathf.Clamp01(dist - radius + 1f);
                    if (alpha > 0.01f)
                    {
                        float lightFactor = Mathf.Lerp(1.0f - shadowIntensity, 1.0f + highlightIntensity, verticalGradient);
                        colors[y * size + x] = new Color(lightFactor, lightFactor, lightFactor, alpha);
                    }
                    else colors[y * size + x] = Color.clear;
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateFallbackBorderSquare(int size, float cornerRadiusRatio, float borderWidthRatio)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            float radius = size * cornerRadiusRatio;
            float borderWidth = size * borderWidthRatio;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float outerDist = GetDistanceToRoundedCorner(x, y, size, size, radius);
                    float outerAlpha = 1.0f - Mathf.Clamp01(outerDist - radius + 1f);
                    float innerRadius = Mathf.Max(0, radius - borderWidth);
                    int innerSizeX = size - (int)(borderWidth * 2);
                    int innerSizeY = size - (int)(borderWidth * 2);
                    float innerDist = GetDistanceToRoundedCorner(x - (int)borderWidth, y - (int)borderWidth, innerSizeX, innerSizeY, innerRadius);
                    float innerAlpha = Mathf.Clamp01(innerDist - innerRadius + 1f);
                    colors[y * size + x] = new Color(1f, 1f, 1f, outerAlpha * (1.0f - innerAlpha) * 0.35f);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static Sprite CreateFallbackSoftShadow(int size, float cornerRadiusRatio)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            float radius = size * cornerRadiusRatio;
            float blurMargin = size * 0.2f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = GetDistanceToRoundedCorner(x, y, size, size, radius);
                    float alpha = 1.0f - Mathf.Clamp01((dist - (radius - blurMargin)) / blurMargin);
                    colors[y * size + x] = new Color(0f, 0f, 0f, alpha * alpha);
                }
            }
            tex.SetPixels(colors);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
        }

        private static float GetDistanceToRoundedCorner(int x, int y, int width, int height, float radius)
        {
            float cx = Mathf.Clamp(x, radius, width - radius);
            float cy = Mathf.Clamp(y, radius, height - radius);
            return Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
        }
    }
}
