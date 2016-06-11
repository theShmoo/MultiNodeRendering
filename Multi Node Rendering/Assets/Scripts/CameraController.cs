using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

    private Camera cam;
    private Vector2 prevMousePos;

    private Vector3 resetPos;
    private Quaternion resetRotation;

    private bool _updated = false;
	// Use this for initialization
	void Start () 
    {
        cam = this.GetComponent<Camera>();
        prevMousePos = Input.mousePosition;

        resetPos = cam.transform.position;
        resetRotation = cam.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () 
    {
        Vector2 currentMousePos = Input.mousePosition;
        Vector2 mouseDelta = currentMousePos - prevMousePos;

        //if (TextureNetworkManager.Instance == null || !TextureNetworkManager.Instance.IsServer)
        //    return;        

        // Rotate
        if (Input.GetButton("Fire1"))
        {
            cam.transform.Rotate(Vector3.right, -mouseDelta.y * 0.3f );
            cam.transform.Rotate(Vector3.up, mouseDelta.x * 0.3f, Space.World);
            _updated = true;
        }
        // Translate Forward/Backwards
        if (Input.GetButton("Fire2"))
        {
            cam.transform.Translate(cam.transform.forward * mouseDelta.y * 0.01f);
            _updated = true;
        }
        // Translate Forward/Backwards
        if (Input.GetButton("Fire3"))
        {
            cam.transform.Translate(cam.transform.right * mouseDelta.x * 0.01f);
            cam.transform.Translate(cam.transform.up * mouseDelta.y * 0.01f);
            _updated = true;
        }
        if(Input.GetKeyDown(KeyCode.F))
        {
            cam.transform.position = resetPos;
            cam.transform.rotation = resetRotation;
            _updated = true;
        }
        prevMousePos = currentMousePos;

        if (_updated)
            OnCameraChanged();

	}

    private void OnCameraChanged()
    {

            TextureNetworkManager.Instance.OnCameraParameterChanged();
            _updated = false;       
    }
}
