using UnityEngine;

using System;
using System.Runtime.InteropServices;
using UnityEngine.Bindings;
using UnityEngine.Rendering;
using UnityEngine.Scripting;

public class TerrrainController : MonoBehaviour
{
    public Terrain terrain; // Reference to the Terrain
    public Vector3 craterCenter; // Center of the crater in world space
    public float craterRadius = 10f; // Radius of the crater
    public float craterDepth = 5f; // Depth of the crater
    public float textureBlendDistance = 5f; // Distance to blend new texture

    public Texture2D craterTexture; // Texture to apply to the crater area
    private TerrainData terrainData;

    void Start()
    {
        if (terrain == null)
        {
            terrain = Terrain.activeTerrain; // If no terrain is assigned, use the active terrain in the scene
        }

        terrainData = terrain.terrainData;

        CreateCrater();
    }

    private void Update()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            if (Input.GetMouseButtonDown(0))
            {
                craterCenter = hit.point;
                CreateCrater();
            }
                
        }
    }

    void CreateCrater()
    {
        // Get the terrain's size and heightmap resolution
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;

        // Convert the center point from world space to terrain heightmap space
        Vector3 terrainPos = terrain.transform.position;
        Vector2 craterCenterHeightmap = new Vector2(
            (craterCenter.x - terrainPos.x) / terrainData.size.x * width,
            (craterCenter.z - terrainPos.z) / terrainData.size.z * height
        );

        // Create the crater effect by lowering the terrain height in a circular region
        float[,] heights = terrainData.GetHeights(0, 0, width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate the distance from the current point to the crater center
                float distance = Vector2.Distance(new Vector2(x, y), craterCenterHeightmap);

                // If within the crater radius, lower the height
                if (distance < craterRadius)
                {
                    // Apply a falloff based on distance to the center
                    float falloff = Mathf.Clamp01(1 - (distance / craterRadius));
                    heights[y, x] -= falloff * craterDepth;
                }
            }
        }

        terrainData.SetHeights(0, 0, heights);

        // Apply texture to the crater region (optional)
        ApplyCraterTexture(craterCenterHeightmap, craterRadius, craterTexture);
    }

    void ApplyCraterTexture(Vector2 craterCenterHeightmap, float craterRadius, Texture2D newTexture)
    {
        // Get the terrain's texture layers
        TerrainLayer[] terrainLayers = terrainData.terrainLayers;

        // If there are no textures, we can add the new texture
        if (terrainLayers.Length == 0)
        {
            terrainData.SetTerrainLayersRegisterUndo(new TerrainLayer[] { new TerrainLayer() }, null);
            terrainLayers = terrainData.terrainLayers;
        }

        // Create a new alpha map (this represents how textures blend at each point)
        float[,,] alphaMap = terrainData.GetAlphamaps(0, 0, terrainData.alphamapResolution, terrainData.alphamapResolution);

        // Iterate through all the points in the terrain to apply the texture
        int width = terrainData.alphamapResolution;
        int height = terrainData.alphamapResolution;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Calculate the distance from the current point to the crater center
                float distance = Vector2.Distance(new Vector2(x, y), craterCenterHeightmap);

                if (distance < craterRadius + textureBlendDistance)
                {
                    // Normalize the distance to get a falloff value
                    float falloff = Mathf.Clamp01(1 - (distance / (craterRadius + textureBlendDistance)));

                    // Set the alpha values for the texture at this point
                    // Assuming the crater texture is at index 0
                    alphaMap[y, x, 0] = Mathf.Max(alphaMap[y, x, 0], falloff); // Use the max to avoid overwriting existing texture
                }
            }
        }

        // Apply the modified alpha map to the terrain
        terrainData.SetAlphamaps(0, 0, alphaMap);
    }
}
