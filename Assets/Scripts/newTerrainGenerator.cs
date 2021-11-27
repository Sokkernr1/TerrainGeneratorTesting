using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class newTerrainGenerator : MonoBehaviour
{
	[Header("Heights & Threshholds")]
	[SerializeField]private float seaFloorHeight = 0f;
	[SerializeField]private float beachThreshold = 0f;
	[SerializeField]private float beachHeight = 0.195f;
	[SerializeField]private float grassThreshold = 0.1f;
	[SerializeField]private float grassHeight = 0.2f;
	[SerializeField]private float noiseThreshold = 0.15f;
	[SerializeField]private float f1Threshold = 0.5f;
	[SerializeField]private float f1Height = 0.3f;
	[SerializeField]private float f2Threshold = 0.7f;
	[SerializeField]private float f2Height = 0.4f;

    [Header("General Settings")]
    [SerializeField]private int islandNoiseScale = 10;
	[SerializeField]private int hillNoiseScale = 20;
	[SerializeField]private int height = 50;
	[SerializeField]private bool randomizeHills = true;
    [SerializeField]private float offsetX = 0f;
    [SerializeField]private float offsetY = 0f;

    [HideInInspector]public Terrain terrain;
    [SerializeField]private int width = 512;
    [SerializeField]private int lenght = 512;

	[SerializeField]private float powHandler = 1.7f;
	[SerializeField]private float sqrtHandler = 0.1f;
	[SerializeField]private int terrainRecheckRuns = 5;

	[HideInInspector]public int[,] textureArray;

    public void Start() 
    {
        InitiateTerrain();
    }

    public void InitiateTerrain()
    {
        terrain = GetComponent<Terrain>();

		textureArray = new int[width - 1, lenght - 1];

        terrain.terrainData = GenerateTerrain(terrain.terrainData);
    }

    TerrainData GenerateTerrain(TerrainData terrainData)
    {
        if (randomizeHills)
        {
            offsetX = Random.Range(-99999f, 99999f);
            offsetY = Random.Range(-99999f, 99999f);
        }

        terrainData.heightmapResolution = width + 1;

        terrainData.size = new Vector3(width, height, lenght);

        terrainData.SetHeights(0, 0, GenerateHeights());

        return terrainData;
    }

    float[,] GenerateHeights()
    {
        float[,] heights = new float[width , lenght];

        for(int x = 0; x < width - 1; x++)
        {
            for(int y = 0; y < lenght - 1; y++)
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
		float xCoord = ((float)x / width - 0.5f) * hillNoiseScale - offsetX;
        float yCoord = ((float)y / lenght - 0.5f) * hillNoiseScale - offsetY;

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
        float xCoord = ((float)x / width - 0.5f) * islandNoiseScale + offsetX;
        float yCoord = ((float)y / lenght - 0.5f) * islandNoiseScale + offsetY;

		return Mathf.PerlinNoise(xCoord, yCoord);
    }

	float CalculateCenterDistance(float x, float y) {
		float nx = x / width - 0.5f;
		float ny = y / lenght - 0.5f;

		return Mathf.Sqrt(nx * nx + ny * ny) / Mathf.Sqrt(sqrtHandler); //Euclidean distance
	}

    public float GetInterpolatedNormal(Vector2 origin)
    {
        return terrain.terrainData.GetInterpolatedNormal((float)origin.y / terrain.terrainData.alphamapResolution, (float)origin.x / terrain.terrainData.alphamapResolution).y;
    }

	float[,] ReCheckTerrain(float[,] heights)
    {
        int siblings;
		for(int i =0; i < terrainRecheckRuns; i++){
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
