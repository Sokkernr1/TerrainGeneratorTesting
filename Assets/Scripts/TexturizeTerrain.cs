using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TexturizeTerrain : MonoBehaviour
{
    public Texture2D wallTexture;
    public Texture2D beachTexture;
    public Texture2D groundTextureF1;
    public Texture2D groundTextureF2;
    public Texture2D groundTextureF3;
	public Texture2D testTexture;

	private TerrainGenerator myGen;
	private Terrain terrain;
	private TerrainData terrainData;
	private int alphamapRes;

    public void StartTexturizing()
    {
        myGen = GetComponent<TerrainGenerator>();
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;

        ImplementTextures(terrainData);

        alphamapRes = terrainData.alphamapResolution - 1;
        Vector2[] heightDistr = new Vector2[terrainData.alphamapLayers - 1];
        FillDistrubution(heightDistr);

        float[,,] splatMapData = terrainData.GetAlphamaps(0, 0, alphamapRes, alphamapRes);

        for (int y = 0; y < alphamapRes; y++)
        {
            for (int x = 0; x < alphamapRes; x++)
            {
                Vector3 nrm = terrainData.GetInterpolatedNormal((float)x / alphamapRes, (float)y / alphamapRes);

                if (!(nrm.y == 1f))
                {
                    if (myGen.map[y * myGen.width + x].grayscale <= myGen.thresholdForcedSand) 
                    {
                        SetSplatValue(splatMapData, y, x, 1);
                    }
                    else
                    {
                        SetSplatValue(splatMapData, y, x, 0);
                    }
                }
                else 
                if(myGen.map[y * myGen.width + x].grayscale <= myGen.thresholdMapNoise)
                {
                    SetSplatValue(splatMapData, y, x, 1);
                }
                else
                {
                    float currentPosHeight = terrainData.GetInterpolatedHeight((float)x / alphamapRes, (float)y / alphamapRes);

                    if (currentPosHeight > (myGen.heightMapFlat - ((myGen.height1f - myGen.heightMapFlat) / 2)) * myGen.height && 
                        currentPosHeight < (myGen.height1f - ((myGen.height1f - myGen.heightMapFlat) / 2)) * myGen.height)
                    {
                        SetSplatValue(splatMapData, y, x, 2);
                    }
                    else if (currentPosHeight > (myGen.height1f - ((myGen.height2f - myGen.height1f) / 2)) * myGen.height && 
                             currentPosHeight < (myGen.height2f - ((myGen.height2f - myGen.height1f) / 2)) * myGen.height)
                    {
                        SetSplatValue(splatMapData, y, x, 3);
                    }
                    else if (currentPosHeight > (myGen.height2f - ((myGen.height2f - myGen.height1f) / 2)) * myGen.height)
                    {
                        SetSplatValue(splatMapData, y, x, 4);
                    }
                }
            }
        }
        BlurSplatMap(splatMapData);
        terrainData.SetAlphamaps(0, 0, splatMapData);
    }

    private void ImplementTextures(TerrainData terrainData)
    {
        TerrainLayer[] newTextures = new TerrainLayer[6];

        newTextures[0] = new TerrainLayer();
        newTextures[0].diffuseTexture = wallTexture;

        newTextures[1] = new TerrainLayer();
        newTextures[1].diffuseTexture = beachTexture;

        newTextures[2] = new TerrainLayer();
        newTextures[2].diffuseTexture = groundTextureF1;

        newTextures[3] = new TerrainLayer();
        newTextures[3].diffuseTexture = groundTextureF2;

        newTextures[4] = new TerrainLayer();
        newTextures[4].diffuseTexture = groundTextureF3;

		newTextures[5] = new TerrainLayer();
        newTextures[5].diffuseTexture = testTexture;

        terrainData.terrainLayers = newTextures;
    }

    void SetSplatValue(float[,,] splats, int y, int x, int splat)
    {
        for (int i = 0; i < splats.GetLength(2); i++)
        {
            if (i == splat)
            {
                splats[y, x, i] = 1;
            }
            else
            {
                splats[y, x, i] = 0;
            }
        }
    }

    void FillDistrubution(Vector2[] distrubution)
    {
        float step = 1f / distrubution.Length;
        for (int i = 0; i < distrubution.Length; i++)
        {
            distrubution[i].x = i * step;
            distrubution[i].y = (i + 1) * step;
        }
    }

    void BlurSplatMap(float[,,] splats)
    {
		int ic = 0;
        if (!(FindObjectOfType<TerrainGenerator>().blurRadius == 0))
        {
            for (int y = 0; y < splats.GetLength(0); y++)
            {
                for (int x = 0; x < splats.GetLength(1); x++)
                {
					if(	terrainData.GetInterpolatedHeight((float)x / alphamapRes, (float)y / alphamapRes) > ((myGen.heightMapFlat / 2) * myGen.height) &&
						terrainData.GetInterpolatedHeight((float)x / alphamapRes, (float)y / alphamapRes) < (((myGen.height2f - myGen.height1f) / 2) + myGen.height1f) * myGen.height) {

                    	float[] c = new float[splats.GetLength(2)];
                    	float[] cr = new float[splats.GetLength(2)];
                    	float[] cl = new float[splats.GetLength(2)];
                    	float[] cu = new float[splats.GetLength(2)];
                    	float[] cd = new float[splats.GetLength(2)];

                    	for (int i = 0; i < c.Length; i++)
                    	{
							ic++;
                        	c[i] = splats[y, x, i];
                        	cr[i] = splats[y, Mathf.Clamp(x + FindObjectOfType<TerrainGenerator>().blurRadius, 0, splats.GetLength(1) - 1), i];
                        	cl[i] = splats[y, Mathf.Clamp(x - FindObjectOfType<TerrainGenerator>().blurRadius, 0, splats.GetLength(1) - 1), i];
                        	cu[i] = splats[Mathf.Clamp(y - FindObjectOfType<TerrainGenerator>().blurRadius, 0, splats.GetLength(0) - 1), x, i];
                        	cd[i] = splats[Mathf.Clamp(y + FindObjectOfType<TerrainGenerator>().blurRadius, 0, splats.GetLength(0) - 1), x, i];
                        	c[i] = (c[i] + cr[i] + cl[i] + cu[i] + cd[i]) / 5;
                        	splats[y, x, i] = c[i];
                    	}
					}
                }
            }
        }
		Debug.Log(ic + " calculations done for texture smoothing");
    }

    //Unused currently + outdated
    public int GetTerrainTexture(Vector2 origin)
    {
        origin.x--;
        origin.y--;

        if (origin.x>511)
        {
            origin.x = 511;
        }
        if (origin.y > 511)
        {
            origin.y = 511;
        }

        if (origin.x < 0)
        {
            origin.x = 0;
        }
        if (origin.y < 0)
        {
            origin.y = 0;
        }

        float[,,] alphaMap = GetComponent<Terrain>().terrainData.GetAlphamaps(Mathf.RoundToInt(origin.x), Mathf.RoundToInt(origin.y), 1, 1);
        int textureAmount = GetComponent<Terrain>().terrainData.terrainLayers.Length;

        for (int i = 0; i < textureAmount; i++)
        {
            if (alphaMap[0, 0, i] == 1)
            {
                return i;
            }
        }

        return 420;
    }
}