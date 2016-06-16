using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;

/// <summary>
/// A class to load a 3D Volume into unity. Only for the use in the unity editor
/// </summary>
[ExecuteInEditMode]
public class VolumeLoader : MonoBehaviour {

    /// <summary>
    /// the slices of the volume dataset
    /// </summary>
    public Texture2D[] slices;

    /// <summary>
    /// the volume texture
    /// </summary>
    public Texture3D VolumeTexture = null;

	/// <summary>
    /// Initialize the Script 
    /// </summary>
	void Start () {
	    if (VolumeTexture == null)
        {
            GenerateVolumeTexture();
        }
	}
	
	/// <summary>
    /// Update is called once per frame
    /// </summary>
	void Update () 
    {
	
	}

    private void GenerateVolumeTexture()
    {
        var dir = "BigMRI";
        Object[] objects = Resources.LoadAll(dir);

        int numObjects = objects.Length;
        if (numObjects > 0)
        {
            slices = new Texture2D[objects.Length];

            for (int i = 0; i < objects.Length; i++)
            {
                slices[i] = (Texture2D)objects[i];
            }

            // sort slices
            System.Array.Sort(slices, (x, y) => x.name.CompareTo(y.name));

            var w = 512;
            var h = 512;
            var d = 512;

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

                sliceCount++;
                if (sliceCount >= numObjects) break;
            }

            var volumeTexture = new Texture3D(w, h, d, TextureFormat.Alpha8, true);
            volumeTexture.SetPixels(volumeColors);
            volumeTexture.Apply();

            var path = "Assets/BigVolumeTexture.asset";
            var tmp = (Texture3D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture3D));
            if (tmp) { AssetDatabase.DeleteAsset(path); }

            AssetDatabase.CreateAsset(volumeTexture, path);
            AssetDatabase.SaveAssets();

            // Print the path of the created asset
            Debug.Log(AssetDatabase.GetAssetPath(volumeTexture));

            VolumeTexture = volumeTexture;
        }
    }
}
#endif