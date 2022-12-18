using UnityEngine;

public class UVScroller : MonoBehaviour
{
    private Vector2 uvSpeed = new(0, 0.01f);
    private Vector2 uvOffset = Vector2.zero;
    private static readonly int MainTex = Shader.PropertyToID("_MainTex");

    private Renderer rend;

    private void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    private void LateUpdate()
    {
        uvOffset += uvSpeed * Time.deltaTime;

        if (uvOffset.x > 0.0625f)
        {
            uvOffset = new Vector2(0, uvOffset.y);
        }
        
        if (uvOffset.y > 0.0625f)
        {
            uvOffset = new Vector2(uvOffset.y, 0);
        }

        rend.materials[0].SetTextureOffset(MainTex, uvOffset);
    }
}
