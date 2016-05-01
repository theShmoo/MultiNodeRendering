using UnityEngine;
using System.Collections;


public class CameraControlScript : MonoBehaviour
{
    public float sensitivityX = 1F;
    public float sensitivityY = 1F;
    float mHdg = 0F;
    float mPitch = 0F;

    void Start()
    {
        // owt?
    }

    void Update()
    {
        if (!(Input.GetMouseButton(0) || Input.GetMouseButton(1)))
            return;
        float deltaX = Input.GetAxis("Mouse X") * sensitivityX;
        float deltaY = Input.GetAxis("Mouse Y") * sensitivityY;
        if (Input.GetMouseButton(0) && Input.GetMouseButton(1))
        {
            Strafe(deltaX);
            ChangeHeight(deltaY);
        }
        else
        {
            if (Input.GetMouseButton(0))
            {
                MoveForwards(deltaY);
                ChangeHeading(deltaX);
            }
            else if (Input.GetMouseButton(1))
            {
                ChangeHeading(deltaX);
                ChangePitch(-deltaY);
            }
        }
    }

    void MoveForwards(float aVal)
    {
        Vector3 fwd = transform.forward;
        fwd.y = 0;
        fwd.Normalize();
        transform.position += aVal * fwd;
    }

    void Strafe(float aVal)
    {
        transform.position += aVal * transform.right;
    }

    void ChangeHeight(float aVal)
    {
        transform.position += aVal * Vector3.up;
    }

    void ChangeHeading(float aVal)
    {
        mHdg += aVal;
        WrapAngle(ref mHdg);
        transform.localEulerAngles = new Vector3(mPitch, mHdg, 0);
    }

    void ChangePitch(float aVal)
    {
        mPitch += aVal;
        WrapAngle(ref mPitch);
        transform.localEulerAngles = new Vector3(mPitch, mHdg, 0);
    }

    public static void WrapAngle(ref float angle)
    {
        if (angle <= -360F)
            angle += 360F;
        if (angle >= 360F)
            angle -= 360F;
    }

}