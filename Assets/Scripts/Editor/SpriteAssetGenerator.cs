using System.IO;
using UnityEngine;
using UnityEditor;

namespace BlockBlast.EditorTools
{
    public static class SpriteAssetGenerator
    {
        [MenuItem("Tools/Block Blast/Generate Sprite Assets")]
        public static void GenerateAllSpriteAssets()
        {
            string spriteFolder = Path.Combine(Application.dataPath, "Art/Sprites");
            if (!Directory.Exists(spriteFolder))
            {
                Directory.CreateDirectory(spriteFolder);
            }

            // 1. RoundedSquare.png
            Texture2D texRounded = CreateRoundedSquareTexture(128, 0.18f);
            SaveTextureAsPNG(texRounded, Path.Combine(spriteFolder, "RoundedSquare.png"));

            // 2. GlossyTile.png
            Texture2D texGlossy = CreateGlossyBlockTexture(128, 0.18f, 0.25f, 0.25f);
            SaveTextureAsPNG(texGlossy, Path.Combine(spriteFolder, "GlossyTile.png"));

            // 3. CellBorder.png
            Texture2D texBorder = CreateBorderSquareTexture(128, 0.18f, 0.08f);
            SaveTextureAsPNG(texBorder, Path.Combine(spriteFolder, "CellBorder.png"));

            // 4. SoftShadow.png
            Texture2D texShadow = CreateSoftShadowTexture(128, 0.22f);
            SaveTextureAsPNG(texShadow, Path.Combine(spriteFolder, "SoftShadow.png"));

            // 5. BackgroundGradient.png
            Texture2D texGradient = CreateVerticalGradientTexture(new Color(0.106f, 0.165f, 0.361f, 1f), new Color(0.055f, 0.078f, 0.200f, 1f), 32, 256);
            SaveTextureAsPNG(texGradient, Path.Combine(spriteFolder, "BackgroundGradient.png"));

            // 6. SoftCircle.png
            Texture2D texCircle = CreateSoftCircleTexture(128);
            SaveTextureAsPNG(texCircle, Path.Combine(spriteFolder, "SoftCircle.png"));

            // 7. CrownIcon.png
            Texture2D texCrown = CreateCrownTexture(64);
            SaveTextureAsPNG(texCrown, Path.Combine(spriteFolder, "CrownIcon.png"));

            AssetDatabase.Refresh();

            SetSpriteImporter("Assets/Art/Sprites/RoundedSquare.png");
            SetSpriteImporter("Assets/Art/Sprites/GlossyTile.png");
            SetSpriteImporter("Assets/Art/Sprites/CellBorder.png");
            SetSpriteImporter("Assets/Art/Sprites/SoftShadow.png");
            SetSpriteImporter("Assets/Art/Sprites/BackgroundGradient.png");
            SetSpriteImporter("Assets/Art/Sprites/SoftCircle.png");
            SetSpriteImporter("Assets/Art/Sprites/CrownIcon.png");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[SpriteAssetGenerator] All PNG sprite assets generated in Assets/Art/Sprites/");
        }

        public static Sprite GetSpriteAsset(string spriteName)
        {
            string assetPath = "Assets/Art/Sprites/" + spriteName + ".png";
            Sprite s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            if (s == null)
            {
                GenerateAllSpriteAssets();
                s = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            }
            return s;
        }

        private static void SaveTextureAsPNG(Texture2D tex, string fullPath)
        {
            byte[] bytes = tex.EncodeToPNG();
            File.WriteAllBytes(fullPath, bytes);
            Object.DestroyImmediate(tex);
        }

        private static void SetSpriteImporter(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spriteImportMode = SpriteImportMode.Single;
                importer.alphaIsTransparency = true;
                importer.mipmapEnabled = false;
                importer.SaveAndReimport();
            }
        }

        private static Texture2D CreateRoundedSquareTexture(int size, float cornerRadiusRatio)
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
            return tex;
        }

        private static Texture2D CreateGlossyBlockTexture(int size, float cornerRadiusRatio, float highlightIntensity, float shadowIntensity)
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
                        float edgeDist = Mathf.Min(Mathf.Min(x, size - 1 - x), Mathf.Min(y, size - 1 - y));
                        if (edgeDist < size * 0.08f && y > size * 0.4f)
                        {
                            lightFactor += 0.15f * (1f - edgeDist / (size * 0.08f));
                        }
                        colors[y * size + x] = new Color(lightFactor, lightFactor, lightFactor, alpha);
                    }
                    else
                    {
                        colors[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateBorderSquareTexture(int size, float cornerRadiusRatio, float borderWidthRatio)
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

                    float borderAlpha = outerAlpha * (1.0f - innerAlpha);
                    colors[y * size + x] = new Color(1f, 1f, 1f, borderAlpha * 0.35f);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSoftShadowTexture(int size, float cornerRadiusRatio)
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
            return tex;
        }

        private static Texture2D CreateVerticalGradientTexture(Color topColor, Color bottomColor, int width, int height)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; y++)
            {
                float t = (float)y / (height - 1);
                Color c = Color.Lerp(bottomColor, topColor, t);
                for (int x = 0; x < width; x++)
                {
                    colors[y * width + x] = c;
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateSoftCircleTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];
            Vector2 center = new Vector2(size / 2f, size / 2f);
            float radius = size / 2f - 2f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), center);
                    float t = Mathf.Clamp01(dist / radius);
                    float alpha = Mathf.SmoothStep(1.0f, 0.0f, t);
                    colors[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static Texture2D CreateCrownTexture(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float nx = (float)x / size;
                    float ny = (float)y / size;

                    bool isBase = ny >= 0.15f && ny <= 0.35f && nx >= 0.15f && nx <= 0.85f;
                    bool isLeftSpike = ny > 0.35f && ny <= 0.75f && Mathf.Abs(nx - 0.22f) < (1.0f - (ny - 0.35f) / 0.4f) * 0.15f;
                    bool isMidSpike = ny > 0.35f && ny <= 0.85f && Mathf.Abs(nx - 0.50f) < (1.0f - (ny - 0.35f) / 0.5f) * 0.18f;
                    bool isRightSpike = ny > 0.35f && ny <= 0.75f && Mathf.Abs(nx - 0.78f) < (1.0f - (ny - 0.35f) / 0.4f) * 0.15f;

                    if (isBase || isLeftSpike || isMidSpike || isRightSpike)
                    {
                        colors[y * size + x] = new Color(1.0f, 0.84f, 0.0f, 1.0f);
                    }
                    else
                    {
                        colors[y * size + x] = Color.clear;
                    }
                }
            }

            tex.SetPixels(colors);
            tex.Apply();
            return tex;
        }

        private static float GetDistanceToRoundedCorner(int x, int y, int width, int height, float radius)
        {
            float cx = Mathf.Clamp(x, radius, width - radius);
            float cy = Mathf.Clamp(y, radius, height - radius);
            return Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
        }
    }
}
