using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetNormals : MonoBehaviour
{
    [SerializeField]
    private RenderTexture writeTexture;
    [SerializeField]
    private ComputeShader shader;

    private int mapWidth;
    private int mapHeight;

    private float seaLevel;
    private float heightMultiplier;
    private float worldRadius;

    private void Awake()
    {
        this.writeTexture = new RenderTexture(1024, 512, 0);
        this.writeTexture.enableRandomWrite = true;
        this.writeTexture.useMipMap = false;
        this.writeTexture.Create();

        var id = this.shader.FindKernel("PlanetNormals");

        this.shader.SetTexture(id, "heightMap", this.writeTexture);
        this.shader.SetInt("mapWidth", mapWidth);
        this.shader.SetInt("mapHeight", mapHeight);
        this.shader.SetFloat("PI", Mathf.PI);
        this.shader.SetFloat("seaLevel", seaLevel);
        this.shader.SetFloat("heightMultiplier", heightMultiplier);
        this.shader.SetFloat("worldRadius", worldRadius);

        this.shader.Dispatch(id, this.writeTexture.width / 16, this.writeTexture.height / 16, 1);
    }
}
