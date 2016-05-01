﻿using UnityEngine;
using System.Collections;

/// <summary>
/// A screen tile represents a part of the screen of the MAIN camera. 
/// </summary>
public class ScreenTile : MonoBehaviour{
    

    /// <summary>
    /// The area of the tile relative to the size of the cameras viewport rectangle
    /// </summary>
    public Vector2 numTiles;
    public Vector2 tileIndex;


    /// <summary>
    /// Returns the tile size scaled from 0 to 1
    /// </summary>
    /// <returns></returns>
    public Vector2 TileSize
    {
        get {
            return new Vector2(1.0f / numTiles.x, 1.0f / numTiles.y);
        }
    }


	/// <summary>
	/// Returns the proper off center projection matrix for this tile of the given camera
	/// </summary>
    public Matrix4x4 getOffCenterProjectionMatrix(Camera cam)
    {
        // Compute correct view port rect values
        float top = cam.nearClipPlane * Mathf.Tan(cam.fieldOfView * Mathf.PI / 360.0f);
        float bottom = -top;
        float left = bottom * cam.aspect;
        float right = top * cam.aspect;

        // Scale viewport rect to the tile position
        float sl = 1.0f - (2.0f * tileIndex.x / numTiles.x);
        float sn = -(sl - 2.0f / numTiles.x);
        float sb = 1.0f - (2.0f * tileIndex.y / numTiles.y);
        float st = -(sb - 2.0f / numTiles.y);
        
        left    *= sl;
        right   *= sn;
        bottom  *= sb;
        top     *= st;



        Debug.Log("Left     : " + sl);
        Debug.Log("Right    : " + sn);
        //Debug.Log("Top      : " + top);
        //Debug.Log("bottom   : " + bottom);


        return PerspectiveOffCenter(left, right, bottom, top, cam.nearClipPlane, cam.farClipPlane);
    }


    /// <summary>
    /// Computes an off center perspective projection matrix
    /// </summary>
    public static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {

        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
        //Matrix4x4.Perspective()
        m[0, 0] = x;
        m[0, 1] = 0;
        m[0, 2] = a;
        m[0, 3] = 0;
        m[1, 0] = 0;
        m[1, 1] = y;
        m[1, 2] = b;
        m[1, 3] = 0;
        m[2, 0] = 0;
        m[2, 1] = 0;
        m[2, 2] = c;
        m[2, 3] = d;
        m[3, 0] = 0;
        m[3, 1] = 0;
        m[3, 2] = e;
        m[3, 3] = 0;
        return m;
    }
}
