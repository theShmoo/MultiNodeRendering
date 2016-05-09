using UnityEngine;
using System.Collections;

public class DummyTileManager : MonoBehaviour
{
    Texture2D tileImage;

    // Use this for initialization
    void Start()
    {
        DeferredRenderer renderer = Camera.main.GetComponent<DeferredRenderer>();

        PerspectiveTile tile = new PerspectiveTile();
        tile.numTiles = new Vector2(1, 1);
        tile.tileIndex = new Vector2(0, 0);

        tile.fov = Camera.main.fieldOfView;
        tile.np = Camera.main.nearClipPlane;
        tile.fp = Camera.main.farClipPlane;
        tile.aspect = Camera.main.aspect;

        tile.screenHeight = Screen.width;
        tile.screenWidth = Screen.height;


        RayCastState state = new RayCastState();
        state.volumeWorldMatrix = this.transform.localToWorldMatrix;
        state.viewMatrix = Camera.main.transform.localToWorldMatrix;
        state.projectionMatrix = Camera.main.projectionMatrix;
                  
        TileRaycaster rayCaster = Camera.main.GetComponent<TileRaycaster>();
        rayCaster.SetTile(tile);
        rayCaster.SetSceneState(state);        
    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        //TileRaycaster rayCaster = Camera.main.GetComponent<TileRaycaster>();
        //rayCaster.RenderTile();

        //Graphics.Blit(rayCaster.compositeTexture, destination);
    }

    // Update is called once per frame
    void Update()
    {
        //this.transform.Rotate(Vector3.up, 90.0f * Time.deltaTime);

        //RayCastState state = new RayCastState();
        
        //state.volumeWorldMatrix = this.transform.localToWorldMatrix;
        //state.viewMatrix = Camera.main.transform.localToWorldMatrix;
        //state.projectionMatrix = Camera.main.projectionMatrix;
        

        //TileRaycaster rayCaster = Camera.main.GetComponent<TileRaycaster>();
        //rayCaster.SetSceneState(state);
        //tileImages = rayCaster.GetRenderedImages();
    }


}
