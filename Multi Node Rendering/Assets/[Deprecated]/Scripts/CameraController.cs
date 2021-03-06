﻿using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{

    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private float rotationX;
    private float rotationY;

    private Transform volumeTransform;

    private bool _updated = false;
    // Use this for initialization
    void Start()
    {
        volumeTransform = new GameObject().transform;
        volumeTransform.position = Vector3.zero;
        volumeTransform.rotation = Quaternion.identity;
            
        originalPosition = this.transform.position;
        originalRotation = this.transform.rotation;
        rotationX = originalRotation.eulerAngles.x;
        rotationY = originalRotation.eulerAngles.y;
    }

    // Update is called once per frame
    void Update()
    {


        if (TextureNetworkManager.Instance == null || !TextureNetworkManager.Instance.IsServer)
            return;

        // Rotate
        if (Input.GetButton("Fire1") && !Input.GetButton("Jump"))
        {
            rotationX += Input.GetAxis("Mouse X");
            rotationY += Input.GetAxis("Mouse Y");
            rotationX = ClampAngle(rotationX, -360F, 360F);
            rotationY = ClampAngle(rotationY, -85F, 85F);
            var rotX = Quaternion.AngleAxis(rotationX, Vector3.up);
            var rotY = Quaternion.AngleAxis(rotationY, -Vector3.right);

            transform.localRotation = originalRotation * rotX * rotY;
            _updated = true;
        }
        // ROTATE VOLUME
        else if (Input.GetButton("Fire1") && Input.GetButton("Jump"))
        {
            float angleX = Input.GetAxis("Mouse X");
            float angleY = Input.GetAxis("Mouse Y");

            volumeTransform.Rotate(Vector3.up, -angleX, Space.World);
            volumeTransform.Rotate(Vector3.right, angleY, Space.World);
            _updated = true;
        } 
        // Translate Forward/Backwards
        if (Input.GetButton("Fire2"))
        {
            this.transform.localPosition += (this.transform.forward * Input.GetAxis("Mouse Y") * 0.03f);
            _updated = true;
        }
        // Translate Forward/Backwards
        if (Input.GetButton("Fire3"))
        {
            this.transform.localPosition += (this.transform.right * Input.GetAxis("Mouse X") * 0.03f);
            this.transform.localPosition += (this.transform.up * Input.GetAxis("Mouse Y") * 0.03f);

            _updated = true;
        }
        

        if (Input.GetKeyDown(KeyCode.F))
        {
            this.transform.position = originalPosition;
            this.transform.rotation = originalRotation;
            _updated = true;
        }

        // Alert if changes
        if (_updated)
            OnSceneChanged();

    }

    private void OnSceneChanged()
    {
        TextureNetworkManager.Instance.OnSceneStateChanged(volumeTransform.localToWorldMatrix, Camera.main.worldToCameraMatrix, Camera.main.projectionMatrix, Camera.main.transform.position);

        _updated = false;
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle <= -360F)
            angle += 360F;
        if (angle >= 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }
}

