using UnityEngine;

public static class IconSprites
{
    public static Sprite GenerateCloudBubble()
    {
        int w = 48, h = 40;
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;

        var pixels = new Color[w * h];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        Color white = new Color(0.98f, 0.98f, 0.98f, 0.95f);
        Color outline = new Color(0.4f, 0.4f, 0.45f, 0.95f);

        void DrawCircle(float cx, float cy, float r)
        {
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float dx = x + 0.5f - cx, dy = y + 0.5f - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= r) pixels[y * w + x] = white;
                else if (dist <= r + 1.3f) pixels[y * w + x] = outline;
            }
        }

        DrawCircle(24, 26, 11);
        DrawCircle(13, 23, 8);
        DrawCircle(35, 23, 8);
        DrawCircle(18, 31, 9);
        DrawCircle(30, 31, 9);
        DrawCircle(24, 34, 8);

        // thought-bubble tail, pointing down toward the object below
        DrawCircle(16, 12, 4f);
        DrawCircle(11, 5, 2.3f);

        // small "!" mark inside the cloud
        Color mark = new Color(0.25f, 0.25f, 0.3f, 1f);
        for (int y = 27; y <= 34; y++)
        for (int x = 22; x <= 25; x++)
            pixels[y * w + x] = mark;
        for (int y = 20; y <= 23; y++)
        for (int x = 22; x <= 25; x++)
            pixels[y * w + x] = mark;

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(11f / w, 3f / h), 100f);
    }
}
