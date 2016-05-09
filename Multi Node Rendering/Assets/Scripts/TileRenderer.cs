using UnityEngine;



/// <summary>
/// This class represents a state structure for the scene/animation for distributed systems. Extend this to communicate other parameters to the Renderer. 
/// </summary>
public class StateObject
{
    /// <summary>
    /// The time since the last update. 
    /// </summary>
    public long deltaTime;

}



/// <summary>
/// A Tile represents a part of the screen
/// </summary>
public class Tile
{
    

}



/// <summary>
/// This interface offers the functionality for a Tile Renderer
/// </summary>
interface TileRenderer
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tile"></param>
    void enqueueTile(Tile tile);

    /// <summary>
    /// Sets the current state of the scene/animation
    /// </summary>
    /// <param name="state"></param>
    void SetSceneState(StateObject state);

    /// <summary>
    /// Renders the 
    /// </summary>
    void RenderTile();

    /// <summary>
    /// Returns the rendered image as byte array
    /// TODO: CHANGE?
    /// </summary>
    Texture2D GetRenderedImage();
}
