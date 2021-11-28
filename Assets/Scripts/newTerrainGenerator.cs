using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class newTerrainGenerator : MonoBehaviour
{
	[Header("Heights & Threshholds")]
	[SerializeField]private float beachThreshold = 0f;
	[SerializeField]private float grassThreshold = 0.125f;
	[SerializeField]private float noiseThreshold = 0.2f;
	[SerializeField]private float f1Threshold = 0.5f;
	[SerializeField]private float f2Threshold = 0.7f;
	public float seaFloorHeight = 0f;
	public float beachHeight = 0.195f;
	public float grassHeight = 0.2f;
	public float f1Height = 0.3f;
	public float f2Height = 0.4f;

	[Header("Perlin Settings")]
	[SerializeField]private bool randomizeHills = true;
    [SerializeField]private int islandNoiseScale = 10;
	[SerializeField]private int hillNoiseScale = 25;
    [SerializeField]private float offsetX = 0f;
    [SerializeField]private float offsetY = 0f;
	[SerializeField]private float powHandler = 1.7f;
	[SerializeField]private float sqrtHandler = 0.1f;

    [Header("General Settings")]
	public int height = 50;
    [HideInInspector]public Terrain terrain;
	[Tooltip("Has to be a ^2 number (i.e. 64, 128, 256, 512, etc)")]
    [SerializeField]private int terrainDimension = 512;
	[SerializeField]private int recheckCount = 5;


	[HideInInspector]public int[,] textureArray;

    public void Start() 
    {
        InitiateTerrain();
    }

	public IEnumerator WaitForLoadingscreen()
    {
        yield return new WaitForSeconds(0.1f);
        Debug.Log("Waited for loadingscreen");
        InitiateTerrain();
        StopCoroutine(WaitForLoadingscreen());
    }

    public void InitiateTerrain()
    {
        terrain = GetComponent<Terrain>();

		textureArray = new int[terrainDimension - 1, terrainDimension - 1];

        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        if (randomizeHills)
        {
            offsetX = Random.Range(-99999f, 99999f);
            offsetY = Random.Range(-99999f, 99999f);
        }

        terrainData.heightmapResolution = terrainDimension + 1;
		terrainData.alphamapResolution = terrainDimension;

        terrainData.size = new Vector3(terrainDimension, height, terrainDimension);

        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[terrainDimension , terrainDimension];

        for(int x = 1; x < terrainDimension - 1; x++)
        {
            for(int y = 1; y < terrainDimension - 1; y++)
            {
				float d = CalculateCenterDistance(x, y);
				float e = Mathf.Pow(CalculateElevation(x, y), powHandler);

				e = (1 + e - d) / 2;

				e = ManageElevations(e, x, y);

                heights[x,y] = e;
            }
        }

		heights = ReCheckTerrain(heights);

        return heights;
    }

	float GetNeutralNosieHeight(int x, int y){
		float xCoord = ((float)x / terrainDimension - 0.5f) * hillNoiseScale - offsetX;
        float yCoord = ((float)y / terrainDimension - 0.5f) * hillNoiseScale - offsetY;

		float perlinHeight = Mathf.PerlinNoise(xCoord, yCoord);

		if(perlinHeight >= f2Threshold) {
			textureArray[x,y] = 4;
			return f2Height; 
		} else if(perlinHeight >= f1Threshold) {
			textureArray[x,y] = 3;
			return f1Height;
		} else {
			textureArray[x,y] = 2;
			return grassHeight;
		}
	}

	float ManageElevations(float e, int x, int y) {

		if(e < beachThreshold) {
			textureArray[x,y] = 1;
			return seaFloorHeight;
		}
		else if(e >= noiseThreshold) {
			return GetNeutralNosieHeight(x, y);
		}
		else if(e >= grassThreshold) {
			textureArray[x,y] = 2;
			return grassHeight;
		}
		else {
			textureArray[x,y] = 1;
			return beachHeight;
		}

	}

    float CalculateElevation(int x,int y)
    {
        float xCoord = ((float)x / terrainDimension - 0.5f) * islandNoiseScale + offsetX;
        float yCoord = ((float)y / terrainDimension - 0.5f) * islandNoiseScale + offsetY;

		return Mathf.PerlinNoise(xCoord, yCoord);
    }

	float CalculateCenterDistance(float x, float y) {
		float nx = x / terrainDimension - 0.5f;
		float ny = y / terrainDimension - 0.5f;

		return Mathf.Sqrt(nx * nx + ny * ny) / Mathf.Sqrt(sqrtHandler); //Euclidean distance
	}

    public float GetInterpolatedNormal(Vector2 origin)
    {
        return terrain.terrainData.GetInterpolatedNormal((float)origin.y / terrain.terrainData.alphamapResolution, (float)origin.x / terrain.terrainData.alphamapResolution).y;
    }

	float[,] ReCheckTerrain(float[,] heights)
    {
        int siblings;
		for(int i = 0; i<recheckCount;i++){
			for (int x = 1; x < terrainDimension - 1; x++)
			{
				for (int y = 1; y < terrainDimension - 1; y++)
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

						if (heights[x, y] == grassHeight)
						{
							heights[x, y] = seaFloorHeight;
							textureArray[x,y] = 1;
						}
						else if (heights[x, y] == f1Height)
						{
							heights[x, y] = beachHeight;
							textureArray[x,y] = 1;
						}
						else if (heights[x, y] == f2Height)
						{
							heights[x, y] = f1Height;
							textureArray[x,y] = 3;
						}
					}
				}
			}
		}
        return heights;
    }

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
}
