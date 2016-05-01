using UnityEngine;
using System.Collections;

public class DummyTileManager : MonoBehaviour {


	// Use this for initialization
	void Start () {
        DeferredRenderer renderer = Camera.main.GetComponent<DeferredRenderer>();

        ScreenTile tile = this.gameObject.AddComponent<ScreenTile>();
        tile.numTiles = new Vector2(2, 2);
        tile.tileIndex = new Vector2(0, 0);

        

        renderer.SetScreenTile(tile);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
