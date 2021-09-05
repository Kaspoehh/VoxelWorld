using UnityEngine;

public class Noise
{
    public static float Get2DPerlin (Vector2 position, float offset, float scale) {
        return Mathf.PerlinNoise((position.x + 0.1f) / 16 * scale + offset, (position.y + 0.1f) / 16 * scale + offset);
    }
    
    public static float Perlin3D(float x, float y, float z) {
        float ab = Mathf.PerlinNoise(x, y);
        float bc = Mathf.PerlinNoise(y, z);
        float ac = Mathf.PerlinNoise(x, z);

        float ba = Mathf.PerlinNoise(y, x);
        float cb = Mathf.PerlinNoise(z, y);
        float ca = Mathf.PerlinNoise(z, x);

        float abc = ab + bc + ac + ba + cb + ca;
        return abc / 6f;
    }
}
