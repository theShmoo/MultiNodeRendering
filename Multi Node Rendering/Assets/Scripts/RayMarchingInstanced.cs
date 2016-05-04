using UnityEngine;

// // Debug function to instanciate molecules in edit mode
// public static class MyMenuCommands
// {
    // [MenuItem("My Commands/Add molecule texture &a")]
    // static void FirstCommand()
    // {
        // int n = 128;
        // int size = n * n * n;
        // Color[] volumeColors = new Color[size];

        // // Clear cols
        // for (int i = 0; i < size; i++) volumeColors[i].a = 0;
        
        // float scale = 1.5f;
        // float smoothness = 2.0f;

        // // Fill cols
        // foreach (var atom in PdbLoader.LoadPdbFile("1w6k"))
        // {
            // float radius = atom.w * scale;
            // float radiusSqr = radius * radius;
            // Vector3 pos = (Vector3)atom * scale + new Vector3(n, n, n) * 0.5f;

            // Vector3 pos_int = new Vector3((int)pos.x, (int)pos.y, (int)pos.z);
            // Vector3 round_offset = pos - pos_int;

            // int influenceRadius = (int)radius * 3;

            // for (int x = -influenceRadius; x <= influenceRadius; x++)
            // {
                // for (int y = -influenceRadius; y <= influenceRadius; y++)
                // {
                    // for (int z = -influenceRadius; z <= influenceRadius; z++)
                    // {
                        // Vector3 local = new Vector3(x, y, z);
                        // Vector3 global = pos_int + local;

                        // if (global.x < 0 || global.y < 0 || global.z < 0) continue;
                        // if (global.x >= n || global.y >= n || global.z >= n) continue;

                        // int idx = (int)global.x + (int)global.y * n + (int)global.z * n * n;

                        // // Gaussian surface formula from: https://bionano.cent.uw.edu.pl/Software/SurfaceDiver/UsersManual/Surface
                        // var r = Mathf.Pow(Vector3.Distance(local, round_offset), 2);
                        // float a = -Mathf.Log(2) * r / (radiusSqr);
                        // float gauss_f = 2 * Mathf.Exp(a);

                        // //float b = smoothness;
                        // //float a = -Mathf.Log(0.5f / b) / (radiusSqr);
                        // //float gauss_f = b * Mathf.Exp(-(r * a));

                        // volumeColors[idx].a += gauss_f;
                    // }
                // }
            // }
        // }	

        // var texture3D = new Texture3D(n, n, n, TextureFormat.Alpha8, true);
        // texture3D.SetPixels(volumeColors);
        // texture3D.wrapMode = TextureWrapMode.Clamp;
        // texture3D.anisoLevel = 0;
        // texture3D.Apply();

        // string path = "Assets/p3_tex.asset";        

        // Texture3D tmp = (Texture3D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture3D));
        // if (tmp)
        // {
            // AssetDatabase.DeleteAsset(path);
            // tmp = null;
        // }

        // AssetDatabase.CreateAsset(texture3D, path);
        // AssetDatabase.SaveAssets();

        // // Print the path of the created asset
        // Debug.Log(AssetDatabase.GetAssetPath(texture3D));
    // }
// }

[ExecuteInEditMode]
public class RayMarchingInstanced : MonoBehaviour
{
    [Range(0.1f, 1)]
    public float Threshold;

    [Range(10, 200)]
    public int NumRayStepMax;

    [Range(0, 4)]
    public int MipLevel;

    public int _numInstances = 2500;

    public Mesh CubeMesh;
    public Texture3D VolumeTexture;
    public Material DistanceFieldMaterial;

    /*****/

    private ComputeBuffer cubeIndices;
    private ComputeBuffer cubeVertices;
    private ComputeBuffer cubeMatrices;

    /*****/
    
    void OnEnable()
    {
        var vertices = CubeMesh.vertices;
        var indices = CubeMesh.triangles;
        var matrices = new Matrix4x4[_numInstances];

        for (int i = 0; i < _numInstances; i++)
        {
            float scale = 0.8f;
            Vector3 pos = Random.insideUnitSphere * 25;
            matrices[i] = Matrix4x4.TRS(pos, Random.rotationUniform, new Vector3(scale, scale, scale));
        }

        cubeIndices = new ComputeBuffer(indices.Length, sizeof(int));
        cubeVertices = new ComputeBuffer(vertices.Length, 3 * sizeof(float));
        cubeMatrices = new ComputeBuffer(_numInstances, 16 * sizeof(float));

        cubeIndices.SetData(indices);
        cubeVertices.SetData(vertices);
        cubeMatrices.SetData(matrices);
    }
    
    private void OnDisable()
    {
        if (cubeIndices != null) cubeIndices.Release();
        if (cubeVertices != null) cubeVertices.Release();
        if (cubeMatrices != null) cubeMatrices.Release();
    }

    [ImageEffectOpaque]
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // If volume texture null
        if (VolumeTexture == null) { Graphics.Blit(source, destination); return; }

        var colorBuffer = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGB32);
        var depthBuffer = RenderTexture.GetTemporary(source.width, source.height, 24, RenderTextureFormat.Depth);

        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);
        GL.Clear(true, true, new Color(0, 0, 0, 0));

        Graphics.Blit(source, colorBuffer);
        
        // Render volume
        Graphics.SetRenderTarget(colorBuffer.colorBuffer, depthBuffer.depthBuffer);

        DistanceFieldMaterial.SetInt("_MipLevel", MipLevel);
        DistanceFieldMaterial.SetFloat("_Threshold", Threshold);
        DistanceFieldMaterial.SetFloat("_NumRayStepMax", NumRayStepMax);
        DistanceFieldMaterial.SetTexture("_VolumeTex", VolumeTexture);
        DistanceFieldMaterial.SetVector("_TextureSize", new Vector4(VolumeTexture.width, VolumeTexture.height, VolumeTexture.depth));

        DistanceFieldMaterial.SetBuffer("_CubeIndices", cubeIndices);
        DistanceFieldMaterial.SetBuffer("_CubeVertices", cubeVertices);
        DistanceFieldMaterial.SetBuffer("_CubeMatrices", cubeMatrices);

        DistanceFieldMaterial.SetPass(0);
        
        Graphics.DrawProcedural(MeshTopology.Triangles, cubeIndices.count, _numInstances);

        Graphics.Blit(colorBuffer, destination);
        Shader.SetGlobalTexture("_CameraDepthTexture", depthBuffer);

        RenderTexture.ReleaseTemporary(colorBuffer);
        RenderTexture.ReleaseTemporary(depthBuffer);
    }
}