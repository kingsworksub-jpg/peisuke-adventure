using UnityEngine;
using UnityEngine.UI;

public class RadarChartRenderer : MonoBehaviour
{
    public RawImage target;
    public int textureSize = 320;

    Texture2D tex;

    public Vector2 AxisDirection(int index, int count)
    {
        float angle = -90f + index * (360f / count);
        float rad = angle * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
    }

    public float OuterRadius => textureSize * 0.38f;

    public void Render(float[] values, float maxValue, Color fillColor, Color lineColor, Color gridColor)
    {
        int size = textureSize;
        if (tex == null || tex.width != size)
        {
            tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            target.texture = tex;
        }

        int n = values.Length;
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float outerR = OuterRadius;

        var dataPts = new Vector2[n];
        for (int i = 0; i < n; i++)
        {
            float t = Mathf.Clamp01(values[i] / maxValue);
            dataPts[i] = center + AxisDirection(i, n) * outerR * t;
        }

        var pixels = new Color[size * size];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = new Color(0, 0, 0, 0);

        bool PointInPolygon(Vector2 p, Vector2[] poly)
        {
            bool inside = false;
            int j = poly.Length - 1;
            for (int i = 0; i < poly.Length; i++)
            {
                if (((poly[i].y > p.y) != (poly[j].y > p.y)) &&
                    (p.x < (poly[j].x - poly[i].x) * (p.y - poly[i].y) / (poly[j].y - poly[i].y) + poly[i].x))
                    inside = !inside;
                j = i;
            }
            return inside;
        }

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            var p = new Vector2(x + 0.5f, y + 0.5f);
            if (PointInPolygon(p, dataPts))
                pixels[y * size + x] = fillColor;
        }

        void DrawLine(Vector2 a, Vector2 b, Color c, float thickness)
        {
            float dist = Vector2.Distance(a, b);
            int steps = Mathf.CeilToInt(dist);
            int r = Mathf.CeilToInt(thickness / 2f);
            for (int s = 0; s <= steps; s++)
            {
                Vector2 p = steps == 0 ? a : Vector2.Lerp(a, b, (float)s / steps);
                int px = Mathf.RoundToInt(p.x), py = Mathf.RoundToInt(p.y);
                for (int oy = -r; oy <= r; oy++)
                for (int ox = -r; ox <= r; ox++)
                {
                    int nx = px + ox, ny = py + oy;
                    if (nx < 0 || nx >= size || ny < 0 || ny >= size) continue;
                    if (ox * ox + oy * oy <= r * r)
                        pixels[ny * size + nx] = c;
                }
            }
        }

        int rings = 4;
        for (int ring = 1; ring <= rings; ring++)
        {
            float ringT = (float)ring / rings;
            var ringPts = new Vector2[n];
            for (int i = 0; i < n; i++) ringPts[i] = center + AxisDirection(i, n) * outerR * ringT;
            for (int i = 0; i < n; i++)
                DrawLine(ringPts[i], ringPts[(i + 1) % n], gridColor, 1.5f);
        }

        for (int i = 0; i < n; i++)
            DrawLine(center, center + AxisDirection(i, n) * outerR, gridColor, 1.5f);

        for (int i = 0; i < n; i++)
            DrawLine(dataPts[i], dataPts[(i + 1) % n], lineColor, 3f);

        foreach (var pt in dataPts)
            DrawLine(pt, pt, lineColor, 6f);

        tex.SetPixels(pixels);
        tex.Apply();
    }
}
