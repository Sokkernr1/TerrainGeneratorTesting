using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class newTerrainGenerator : MonoBehaviour
{

    [Header("General Settings")]
    public int scale = 10;
	public int height = 50;
	public bool randomizeHills = false;
    public float offsetX = 0f;
    public float offsetY = 0f;
	public float updateSpeed = 1f;
	public bool scrolling = true;

    [HideInInspector] public Terrain terrain;
    public int width = 512;
    public int lenght = 512;

	public float powHandler = 2f;
	public float sqrtHandler = 0.5f;

    public void Start() 
    {
        InitiateTerrain();
    }

	public void Update() {
		if(scrolling) {
			offsetX= offsetX + ((0.1f * Time.deltaTime) * updateSpeed);
			offsetY= offsetY + ((0.1f * Time.deltaTime) * updateSpeed);

			InitiateTerrain();
		}
	}

    public void InitiateTerrain()
    {
        terrain = GetComponent<Terrain>();

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
        float[,] heights = new float[width, lenght];

        for(int x = 0; x < width; x++)
        {
            for(int y = 0; y < lenght; y++)
            {
				float d = CalculateCenterDistance(x, y);
				float e = Mathf.Pow(CalculateElevation(x, y), powHandler);

				if(e >= 1f) {
					//Debug.Log("Elevation: " + e + " || CenterDistance: " + d);
				}

				heights[x,y] = (1 + e - d) / 2;
            }
        }

        return heights;
    }

    float CalculateElevation(int x,int y)
    {
        float xCoord = ((float)x / width - 0.5f) * scale + offsetX;
        float yCoord = ((float)y / lenght - 0.5f) * scale + offsetY;

		return Mathf.PerlinNoise(xCoord, yCoord);
    }

	float CalculateCenterDistance(float x, float y) {
		float nx = x / width - 0.5f;
		float ny = y / lenght - 0.5f;

		return Mathf.Sqrt(nx * nx + ny * ny) / Mathf.Sqrt(sqrtHandler); //Euclidean
		//return 2 * Mathf.Max(Mathf.Abs(nx), Mathf.Abs(ny)); //Diagonal
		//return  Mathf.Abs(nx) + Mathf.Abs(ny); //Manhatten
	}

    public float GetInterpolatedNormal(Vector2 origin)
    {
        return terrain.terrainData.GetInterpolatedNormal((float)origin.y / terrain.terrainData.alphamapResolution, (float)origin.x / terrain.terrainData.alphamapResolution).y;
    }
}
