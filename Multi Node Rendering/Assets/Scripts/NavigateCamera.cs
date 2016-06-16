using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// This class is used to navigate the camera from the composer
/// </summary>
[ExecuteInEditMode]
public class NavigateCamera : MonoBehaviour
{
    /// <summary>
    /// The default distance from the object to the camera
    /// </summary>
    public float DefaultDistance = 5.0f;

    /// <summary>
    /// The rotation speed of the arc ball rotation
    /// </summary>
    public float AcrBallRotationSpeed = 0.25f;

    /// <summary>
    /// The FPS Rotation Speed
    /// </summary>    
    public float FpsRotationSpeed = 0.25f;

    /// <summary>
    /// The translation speed
    /// </summary>    
    public float TranslationSpeed = 10.0f;

    /// <summary>
    /// The scrolling speed
    /// </summary>      
    public float ScrollingSpeed = 1.0f;

    /// <summary>
    /// The panning speed
    /// </summary>      
    public float PannigSpeed = 0.25f;

    /// <summary>
    /// The start position of the target object
    /// </summary>  
    public Vector3 TargetPosition;

    /// <summary>
    /// The target object
    /// </summary>      
    [HideInInspector]
    public GameObject TargetGameObject;

    /*****/

    private bool forward;
    private bool backward;
    private bool right;
    private bool left;

    /// <summary>
    /// The current distance of the camera to the game object
    /// </summary>      
    [HideInInspector]
    public float Distance;

    /// <summary>
    /// The euler angle in X direction
    /// </summary>     
    [HideInInspector]
    public float EulerAngleX;

    /// <summary>
    /// The euler angle in Y direction
    /// </summary> 
    [HideInInspector]
    public float EulerAngleY;

    private float deltaTime = 0;
    private float lastUpdateTime = 0;

    private bool _updated = false;

    /*****/

    void OnEnable()
    {
        #if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            EditorApplication.update += Update;
        }
        #endif
    }

    /// <summary>
    /// Update is called once per frame
    /// </summary>    
    void Update()
    {
        if ( TextureNetworkManager.Instance == null || !TextureNetworkManager.Instance.IsServer)
            return;

        deltaTime = Time.realtimeSinceStartup - lastUpdateTime;
        lastUpdateTime = Time.realtimeSinceStartup;
        
        if (forward)
        {
            TargetPosition += gameObject.transform.forward * TranslationSpeed * deltaTime; 
            transform.position += gameObject.transform.forward * TranslationSpeed * deltaTime; 
            _updated = true;
        }

        if (backward)
        {
            TargetPosition -= gameObject.transform.forward * TranslationSpeed * deltaTime;
            transform.position -= gameObject.transform.forward * TranslationSpeed * deltaTime; 
            _updated = true;
        }

        if (right)
        {
            TargetPosition += gameObject.transform.right * TranslationSpeed * deltaTime;
            transform.position += gameObject.transform.right * TranslationSpeed * deltaTime; 
            _updated = true;
        }

        if (left)
        {
            TargetPosition -= gameObject.transform.right * TranslationSpeed * deltaTime;
            transform.position -= gameObject.transform.right * TranslationSpeed * deltaTime; 
            _updated = true;
        }

        if(_updated)
        {
            OnCameraChanged();
        }
    }

    void DoArcBallRotation()
    {
        EulerAngleX += Event.current.delta.x * AcrBallRotationSpeed;
        EulerAngleY += Event.current.delta.y * AcrBallRotationSpeed;

        var rotation = Quaternion.Euler(EulerAngleY, EulerAngleX, 0.0f);
        var position = TargetPosition + rotation * Vector3.back * Distance;

        transform.rotation = rotation;
        transform.position = position;
        _updated = true;
    }

    void DoFpsRotation()
    {
        EulerAngleX += Event.current.delta.x * FpsRotationSpeed;
        EulerAngleY += Event.current.delta.y * FpsRotationSpeed;

        var rotation = Quaternion.Euler(EulerAngleY, EulerAngleX, 0.0f);

        transform.rotation = rotation;
        TargetPosition = transform.position + transform.forward * Distance;
        _updated = true;
    }

    void DoPanning()
    {
        TargetPosition += transform.up * Event.current.delta.y * PannigSpeed;
        transform.position += transform.up * Event.current.delta.y * PannigSpeed;

        TargetPosition -= transform.right * Event.current.delta.x * PannigSpeed;
        transform.position -= transform.right * Event.current.delta.x * PannigSpeed;
        _updated = true;
    }

    
    void DoScrolling()
    {
        Distance += Event.current.delta.y* ScrollingSpeed;
        transform.position = TargetPosition - transform.forward* Distance;

        if (Distance< 0)
        {
            TargetPosition = transform.position + transform.forward * DefaultDistance;
            Distance = Vector3.Distance(TargetPosition, transform.position);
        }
        _updated = true;
    }
    
    private void OnGUI()
    {

#if UNITY_EDITOR
        if (Event.current.type == EventType.Layout || Event.current.type == EventType.Repaint)
        {
            EditorUtility.SetDirty(this); // this is important, if omitted, "Mouse down" will not be display
        }
#endif

        if (Event.current.alt && Event.current.type == EventType.mouseDrag && Event.current.button == 0)
        {
            DoArcBallRotation();
        }

        if (Event.current.type == EventType.mouseDrag && Event.current.button == 1)
        {
            DoFpsRotation();
        }

        if (Event.current.type == EventType.mouseDrag && Event.current.button == 2)
        {
            DoPanning();
        }

        if (Event.current.type == EventType.ScrollWheel)
        {
            DoScrolling();
        }

        if (Event.current.keyCode == KeyCode.F)
        {
            if (TargetGameObject != null)
            {
                TargetPosition = TargetGameObject.transform.position;
            }

            Distance = DefaultDistance;
            transform.position = TargetPosition - transform.forward*Distance;
            _updated = true;
        }

        if (Event.current.keyCode == KeyCode.R)
        {
            Distance = DefaultDistance;
            TargetPosition = Vector3.zero;
            transform.position = TargetPosition - transform.forward*Distance;
            _updated = true;
        }

        if (Event.current.keyCode == KeyCode.W)
        {
            forward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.S)
        {
            backward = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.A)
        {
            left = Event.current.type == EventType.KeyDown;
        }

        if (Event.current.keyCode == KeyCode.D)
        {
            right = Event.current.type == EventType.KeyDown;
        }
    }

    private void OnCameraChanged()
    {
        if(_updated)
        {
            TextureNetworkManager.Instance.OnSceneStateChanged(Matrix4x4.identity, Camera.main.worldToCameraMatrix, Camera.main.projectionMatrix, Camera.main.transform.position);
            _updated = false;
        }
    }
}

