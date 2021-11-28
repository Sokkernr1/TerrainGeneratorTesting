using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [Range(0,3)]public int blurRadius = 2;

    [Header("General Settings")]
    public Texture2D heightmap;
    public int height = 100;
    public float scale = 20;
    public bool randomizeHills = false;
    public float offsetX = 0f;
    public float offsetY = 0f;

    [Header("Threshhold settings")]
    [Range(0f, 1f)] public float thresholdMapFlat = 0.6f;
    [Range(0f, 1f)] public float thresholdMapNoise = 0.9f;
    [Range(0f, 1f)] public float threshold01 = 0.5f;
    [Range(0f, 1f)] public float threshold02 = 0.65f;
    [Range(0f, 1f)] public float thresholdForcedSand = 0.81f;

    [Header("Height settings")]
    [Range(0f, 1f)] public float heightMapGround0 = 0.0f;
    [Range(0f, 1f)] public float heightMapFlat = 0.4f;
    [Range(0f,1f)]public float height1f = 0.5f;
    [Range(0f, 1f)] public float height2f = 0.6f;
    [Range(0f, 1f)] public float WaterHeight = 0.96f;

    [HideInInspector] public Terrain terrain;
    [HideInInspector] public int width;
    [HideInInspector] public int lenght;

    [HideInInspector] public Color[] map;

    public void Start() 
    {
        InitiateTerrain();
    }

    public void InitiateTerrain()
    {
        terrain = GetComponent<Terrain>();

        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    //Unused currently
    public IEnumerator WaitForLoadingscreen()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Waited for loadingscreen");
        InitiateTerrain();
        StopCoroutine(WaitForLoadingscreen());
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        if (randomizeHills)
        {
            offsetX = Random.Range(-99999f, 99999f);
            offsetY = Random.Range(-99999f, 99999f);
        }

        width = heightmap.width;
        lenght = heightmap.height;

        terrainData.heightmapResolution = width + 1;

        terrainData.size = new Vector3(width, height, lenght);

        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    float[,] GenerateHeights()
    {
        map = heightmap.GetPixels();

        float[,] heights = new float[width, lenght];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < lenght; y++)
            {
                float nextHeight = (map[x * width + y].grayscale);

                if (nextHeight >= thresholdMapNoise)
                {
                    heights[x, y] = CalculateHeight(x, y);
                }
                else if (nextHeight >= thresholdMapFlat)
                {
                    heights[x, y] = heightMapFlat;
                }
                else
                {
                    heights[x, y] = heightMapGround0;
                }
            }
        }

        return heights;
    }

    float CalculateHeight(int x,int y)
    {
        float xCoord = (float)x / width * scale + offsetX;
        float yCoord = (float)y / lenght * scale + offsetY;

        if((Mathf.PerlinNoise(xCoord, yCoord)) > threshold02)
        {
            return height2f;
        }
        else if ((Mathf.PerlinNoise(xCoord, yCoord)) > threshold01)
        {
            return height1f;
        }
        else
        {
            return heightMapFlat;
        }
    }

    //Unused currently + outdated
    float[,] ReCheckTerrain(float[,] heights)
    {
        int siblings;
        for (int x = 1; x < width - 1; x++)
        {
            for (int y = 1; y < lenght - 1; y++)
            {
                siblings = 8;

                if (heights[x, y] > heights[x - 1, y])
                {
                    siblings--;
                }
                if (heights[x, y] > heights[x - 1, y - 1])
                {
                    siblings--;
                }
                if (heights[x, y] > heights[x, y - 1])
                {
                    siblings--;
                }
                if (heights[x, y] > heights[x + 1, y - 1])
                {
                    siblings--;
                }
                if (heights[x, y] > heights[x + 1, y])
                {
                    siblings--;
                }
                if (heights[x, y] > heights[x + 1, y + 1])
                {
                    siblings--;
                }
                if (heights[x, y] > heights[x, y + 1])
                {
                    siblings--;
                }
                if (heights[x, y] > heights[x - 1, y + 1])
                {
                    siblings--;
                }
                if (siblings <= 2)
                {

                    if (heights[x, y] == heightMapFlat)
                    {
                        heights[x, y] = heightMapGround0;
                    }
                    else if (heights[x, y] == height1f)
                    {
                        heights[x, y] = heightMapFlat;
                    }
                    else if (heights[x, y] == height2f)
                    {
                        heights[x, y] = height1f;
                    }
                }
            }
        }
        return heights;
    }

    //Unused currently + outdated
    public Vector2 GetTerrainCords(Vector3 origin)
    {
        origin -= FindObjectOfType<Terrain>().transform.position;
        origin.x /= terrain.terrainData.size.x;
        origin.z /= terrain.terrainData.size.z;
        origin *= terrain.terrainData.heightmapResolution;
        origin.x = Mathf.RoundToInt(origin.x);
        origin.z = Mathf.RoundToInt(origin.z);

        return new Vector2(origin.x, origin.z);
    }

    //Unused currently + outdated
    public float GetPixelGrayscale(Vector2 origin)
    {
        try
        {
            return map[Mathf.RoundToInt(origin.y) * width + Mathf.RoundToInt(origin.x)].grayscale;
        }
        catch
        {
            return 0f;
        }
    }

    public float GetInterpolatedNormal(Vector2 origin)
    {
        return terrain.terrainData.GetInterpolatedNormal((float)origin.y / terrain.terrainData.alphamapResolution, (float)origin.x / terrain.terrainData.alphamapResolution).y;
    }

    //Needed
    public List<Vector3> getTerrainList(int layer123, bool secondLayer,int layer23)
    {
        List<Vector3> myList = new List<Vector3>();
        float thresholdAbove;
        float thresholdBelow;

        switch (layer123)
        {
            case 1:
                thresholdAbove = 0f;
                thresholdBelow = thresholdMapFlat / 2;
                break;
            case 2:
                thresholdAbove = thresholdMapFlat;
                thresholdBelow = thresholdMapNoise;
                break;
            case 3:
                thresholdAbove = thresholdMapNoise;
                thresholdBelow = 999f;
                break;
            default:
                Debug.Log("Wrong layer number at getTerrainArray()1");
                return(myList);
        }

        if (secondLayer)
        {
            switch (layer23)
            {
                case 2:
                    thresholdBelow = (thresholdMapNoise + thresholdMapFlat) / 2;
                    break;
                case 3:
                    thresholdBelow = 999f;
                    break;
                default:
                    Debug.Log("Wrong layer number at getTerrainArray()2");
                    return (myList);
            }
        }

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < lenght; y++)
            {
                if (map[x * width + y].grayscale >= thresholdAbove && map[x * width + y].grayscale <= thresholdBelow)
                {
                    myList.Add(new Vector3(y, 40f, x));
                }
            }
        }

        return (myList);
    }
}
