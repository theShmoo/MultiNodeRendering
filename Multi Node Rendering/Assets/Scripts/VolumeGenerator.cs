using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Collections;
using System.IO;

/// <summary>
/// A class to load a 3D Volume into unity. Only for the use in the unity editor
/// </summary>
[ExecuteInEditMode]
public class VolumeGenerator : MonoBehaviour {

    /// <summary>
    /// the slices of the volume dataset
    /// </summary>
    public Texture2D[] slices;

    /// <summary>
    /// the volume texture
    /// </summary>
    public Texture3D generatedVolumeTexture = null;

    /// <summary>
    /// The directory where to search for Image files
    /// </summary>
    public string dir = "BigMRI";

    private string currentDir = "";
	
    /// <summary>
    /// Initialize the Script 
    /// </summary>
	void Start ()
    {
        currentDir = dir;      
	}
	
	/// <summary>
    /// Update is called once per frame
    /// </summary>
	void Update () 
    {

        if (dir == "" || dir == null)
        {
            Debug.Log("No valid directory for volume creation");
            return;
        }
        // Directory changed -> generate Volume Texture
        if(currentDir != dir)
        {
            currentDir = dir;
            Debug.Log("Directory changed. Generating Volume Texture...");
            GenerateVolumeTexture();
        }       
	}

    private void GenerateVolumeTexture()
    {

        Texture2D[] objects = Resources.LoadAll<Texture2D>(dir);
        

        int numSlices = objects.Length;
        if (objects.Length > 0)
        {
            slices = objects;
            Debug.Log("Creating VolumeTexture from " + slices.Length + " slices.");
            
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
                if (sliceCount >= numSlices) break;
            }

            var volumeTexture = new Texture3D(w, h, d, TextureFormat.Alpha8, true);
            volumeTexture.SetPixels(volumeColors);
            volumeTexture.Apply();

            var path = "Assets/Generated/" + dir + ".asset";

            // Create Generated Folder if it does not exist
            if (!Directory.Exists("Assets/Generated/"))
            {
                Directory.CreateDirectory("Assets/Generated/");
            }


            var tmp = (Texture3D)AssetDatabase.LoadAssetAtPath(path, typeof(Texture3D));
            if (tmp) { AssetDatabase.DeleteAsset(path); }

            AssetDatabase.CreateAsset(volumeTexture, path);
            AssetDatabase.SaveAssets();

            // Print the path of the created asset
            Debug.Log("Generated 3D Volume Texture in " + AssetDatabase.GetAssetPath(volumeTexture));

            generatedVolumeTexture = volumeTexture;
        }
        else
        {
            Debug.Log("No Textures found in directory /" + dir + "/");
        }
    }
}
#endif