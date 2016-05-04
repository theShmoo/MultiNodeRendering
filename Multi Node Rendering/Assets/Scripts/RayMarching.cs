using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class RayMarching : MonoBehaviour
{
    public Texture2D[] slices;

    [Range(0, 1)]
    public int pass = 0;

    [Range(0, 2)]
    public float opacity = 1;

    public Mesh CubeMesh;
    public Vector3 CubeScale;
    public Texture3D VolumeTexture;
    public Material RayMarchMaterial;
    public Material BackDepthMaterial;
    public Material FrontDepthMaterial;
    
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        // If volume texture null
        if (VolumeTexture == null) {
            //GenerateVolumeTexture();
            Graphics.Blit(source, destination); 
            return; }

        // Get temp render textures
        var backDepth = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBFloat);
        var frontDepth = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBFloat);

        // Clear back depth texture
        Graphics.SetRenderTarget(backDepth);
        GL.Clear(true, true, Color.black);

        // Draw back depth
        BackDepthMaterial.SetPass(0);
        Graphics.DrawMeshNow(CubeMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, CubeScale));
        
        // Clear front depth texture
        Graphics.SetRenderTarget(frontDepth);
        GL.Clear(true, true, Color.black);
        
        // Draw front depth
        FrontDepthMaterial.SetPass(0);
        Graphics.DrawMeshNow(CubeMesh, Matrix4x4.TRS(Vector3.zero, Quaternion.identity, CubeScale));

        //Graphics.Blit(frontDepth, destination);
        //// Release temp textures
        //RenderTexture.ReleaseTemporary(frontDepth);
        //RenderTexture.ReleaseTemporary(backDepth);
        //return;

        // Draw a fullscreen quad
        RayMarchMaterial.SetFloat("_Opacity", opacity); // Blending strength 
        RayMarchMaterial.SetTexture("_BackTex", backDepth);
        RayMarchMaterial.SetTexture("_FrontTex", frontDepth);
        RayMarchMaterial.SetTexture("_VolumeTex", VolumeTexture);
        RayMarchMaterial.SetVector("_TextureSize", new Vector4(VolumeTexture.width, VolumeTexture.height, VolumeTexture.depth));
        Graphics.Blit(null, source, RayMarchMaterial, pass);

        // Copy source texture to destination
        Graphics.Blit(source, destination);

        // Release temp textures
        RenderTexture.ReleaseTemporary(frontDepth);
        RenderTexture.ReleaseTemporary(backDepth);
    }

    //****** Offline texture generation ******//

    private void GenerateVolumeTexture()
    {
        // sort slices
        System.Array.Sort(slices, (x, y) => x.name.CompareTo(y.name));

        var w = 256;
        var h = 256;
        var d = 128;

        var sliceCount = 0;
        var volumeColors = new Color[w * h * d];
        
        for (var z = 1; z < d; z++)
        {
            for (var x = 0; x < w; x++)
            {
                for (var y = 0; y < h; y++)
                {
                    var idx = x + (y * w) + (z * (w * h));
                    var color = slices[sliceCount].GetPixel(x, y);
                    volumeColors[idx].a = color.r;
                }
            }

            sliceCount ++;
            if (sliceCount >= slices.Length) break;
        }

        var volumeTexture = new Texture3D(w, h, d, TextureFormat.Alpha8, true);
        volumeTexture.SetPixels(volumeColors);
        volumeTexture.Apply();

        var path = "Assets/VolumeTexture.asset";
        var tmp = (Texture3D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture3D));
        if (tmp){ AssetDatabase.DeleteAsset(path);}

        AssetDatabase.CreateAsset(volumeTexture, path);
        AssetDatabase.SaveAssets();

        // Print the path of the created asset
        Debug.Log(AssetDatabase.GetAssetPath(volumeTexture));

        VolumeTexture = volumeTexture;
    }
}
