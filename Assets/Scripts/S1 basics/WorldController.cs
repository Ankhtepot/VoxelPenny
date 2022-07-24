using System.Collections;
using UnityEngine;

public class WorldController : MonoBehaviour
{
    [SerializeField] private GameObject block;
    [SerializeField] private Vector3Int worldSize;
    [SerializeField] private int layersToRandomizeFromTop;
    [Range(0f, 1.0f)] [SerializeField] private float randomFillRatio; 
    

    private void Start()
    {
        StartCoroutine(BuildWorld());
    }

    public IEnumerator BuildWorld()
    {
        for (int z = 0; z < worldSize.z; z++)
        {
            for (int y = 0; y < worldSize.y; y++)
            {
                for (int x = 0; x < worldSize.x; x++)
                {
                    if (y >= Mathf.Max(0, worldSize.y - layersToRandomizeFromTop) 
                        && Random.Range(0f, 1.0f) > randomFillRatio) continue;
                    
                    Vector3 pos = new Vector3(x, y, z);
                    GameObject cube = Instantiate(block, pos, Quaternion.identity);
                    cube.name = $"{x}_{y}_{z}";
                    // cube.GetComponent<Renderer>().material = new Material(Shader.Find("Standard"));
                }

                yield return null;
            }
        }
    }
}
