using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TerrainInfo : MonoBehaviour
{
    TerrainData terrainData;

    // Use this for initialization
    void Start() {
        terrainData = GetComponent<TerrainCollider>().terrainData;
    }

    public TerrainData GetTerrainData() {
        return this.terrainData;
    }
}
