using UnityEngine;
using System.Collections;

public class RenderNode : MonoBehaviour {

    public Matrix4x4 projectionMatrix;

    private float left;
    private float right;
    private float top;
    private float bottom;

    /// <summary>
    /// The tile of the screen for this node represents
    /// </summary>
    public Rect screenTile;

    public float scaleLeft;
    public float scaleRight;
    public float scaleTop;
    public float scaleBottom;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        projectionMatrix = PerspectiveOffCenter(left * scaleLeft, right * scaleRight, top * scaleTop, bottom * scaleBottom, Camera.main.nearClipPlane, Camera.main.farClipPlane);
	}

    /// <summary>
    /// Creates an off center perspective Transformation. 
    /// </summary>
    /// <param name="left"></param>
    /// <param name="right"></param>
    /// <param name="bottom"></param>
    /// <param name="top"></param>
    /// <param name="near"></param>
    /// <param name="far"></param>
    /// <returns></returns>
    static Matrix4x4 PerspectiveOffCenter(float left, float right, float bottom, float top, float near, float far)
    {

        float x = 2.0F * near / (right - left);
        float y = 2.0F * near / (top - bottom);
        float a = (right + left) / (right - left);
        float b = (top + bottom) / (top - bottom);
        float c = -(far + near) / (far - near);
        float d = -(2.0F * far * near) / (far - near);
        float e = -1.0F;
        Matrix4x4 m = new Matrix4x4();
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
