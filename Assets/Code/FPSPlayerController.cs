using UnityEngine;
using UnityEngine.SceneManagement;

public class FPSPlayerController : MonoBehaviour
{
 

    float m_Yaw;
    float m_Pitch;
    public float m_YawRotationalSpeed;
    public float m_PitchRotationalSpeed;

    public float m_MinPitch;
    public float m_MaxPitch;
    public int m_PortalSize=1;

    public Transform m_PitchController;
    public bool m_UseYawInverted;
    public bool m_UsePitchInverted;

    public CharacterController m_CharacterController;
    public float m_Speed;
    public float m_FastSpeedMultiplier = 3f;

    Vector3 Direction;

    KeyCode m_LeftKeyCode = KeyCode.A;
    KeyCode m_RightKeyCode = KeyCode.D;
    KeyCode m_UpKeyCode = KeyCode.W;
    KeyCode m_DownKeyCode = KeyCode.S;
    KeyCode m_JumpKeyCode = KeyCode.Space;
    KeyCode m_RunKeycode = KeyCode.LeftShift;
    KeyCode m_RestartGameKeyCode = KeyCode.R;

    public Camera m_Camera;
    public float m_NormalMovementFOV = 60;
    public float m_RunMovementFOV = 70;

    float m_VerticalSpeed = 0.0f;
    bool m_OnGround = true;

    public float m_JumpSpeed = 10.0f;

    [Header("Portales")]
    public Portal BluePortal;
    public Portal OrangePortal;
    public DecallPortal DecallPortal;

    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    bool m_AngleLocked = false;
    bool m_AimLocked = true;

    Vector3 m_StartPosition;
    Quaternion m_StartRotation;

    public float OffsetTeleportPortal;
    [Range(0.0f, 90.0f)]public float AngleToEnterPortalInDegrees;

    [Header("AttachObject")]
    public Transform AttachingPosition;
    Rigidbody ObjectAttached;
    bool AttachingObject = false;
    public float AttachingObjectSpeed = 3.0f;
    Quaternion AttachingObjectStartRotation;
    public float MaxDistanceToAttachObject = 10.0f;
    public LayerMask AttachObjectLayerMask;
    KeyCode AttachObjectKeyCode = KeyCode.E;
    public float AttachedObjectThrowForce = 750.0f;

    [Header("Shoot")]
    public float m_MaxShootDistance = 50.0f;
    public LayerMask m_ShootingLayerMask;

    [Header("HUD")]
    public Canvas HUD;

    [Header("Keys")]
    public bool HasAKey = false;

    void Start()
    {
        m_Yaw = transform.rotation.y;
        m_Pitch = m_PitchController.localRotation.x;
        Cursor.lockState = CursorLockMode.Locked;
        m_AimLocked = Cursor.lockState == CursorLockMode.Locked;
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        BluePortal.gameObject.SetActive(false);
        OrangePortal.gameObject.SetActive(false);
        DecallPortal.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    void UpdateInputDebug()
    {
        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
        {
            m_AngleLocked = !m_AngleLocked;
            if (Cursor.lockState == CursorLockMode.Locked)
                    Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
            m_AimLocked = Cursor.lockState == CursorLockMode.Locked;
        }
    }
#endif

    // Update is called once per frame
    void Update()
    {

#if UNITY_EDITOR
        UpdateInputDebug();
#endif

        //Debug.Log(m_Life);
        //Debug.Log(m_Shield);
        Vector3 l_RightDirection = transform.right;
        Vector3 l_ForwardDirection = transform.forward;
        Direction = Vector3.zero;
        float l_Speed = m_Speed;

        if(Input.GetKey(m_UpKeyCode))
            Direction = l_ForwardDirection;
        if (Input.GetKey(m_DownKeyCode))
            Direction = -l_ForwardDirection;
        if (Input.GetKey(m_RightKeyCode))
            Direction += l_RightDirection;
        if (Input.GetKey(m_LeftKeyCode))
            Direction -= l_RightDirection;
        if (Input.GetKeyDown(m_JumpKeyCode) && m_OnGround)
            m_VerticalSpeed = m_JumpSpeed;
        if (Input.GetKeyDown(m_RestartGameKeyCode))
            RestartGame();
        float l_FOV = m_NormalMovementFOV;
        if (Input.GetKey(m_RunKeycode))
        {
            l_Speed = m_Speed * m_FastSpeedMultiplier;
            l_FOV = m_RunMovementFOV;
        }
        m_Camera.fieldOfView = l_FOV;

        Direction.Normalize();
        Vector3 l_Movement = Direction * l_Speed * Time.deltaTime;

        //Rotation
        float l_MouseX = Input.GetAxis("Mouse X");
        float l_MouseY = Input.GetAxis("Mouse Y");
        float l_YawRotationalSpeed = m_YawRotationalSpeed;
        float l_PitchRotationalSpeed = m_PitchRotationalSpeed;

        if(m_AngleLocked)
        {
            l_MouseX = 0.0f;
            l_MouseY = 0.0f;
        }

        m_Yaw = m_Yaw + l_MouseX * m_YawRotationalSpeed *Time.deltaTime * (m_UseYawInverted ? -1.0f : 1.0f);
        m_Pitch = m_Pitch + l_MouseY * m_PitchRotationalSpeed *Time.deltaTime * (m_UsePitchInverted ? -1.0f : 1.0f);
        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

        transform.rotation = Quaternion.Euler(0.0f, m_Yaw, 0.0f);
        m_PitchController.localRotation = Quaternion.Euler(m_Pitch, 0.0f, 0.0f);

        m_VerticalSpeed = m_VerticalSpeed + Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags =  m_CharacterController.Move(l_Movement);

        if ((l_CollisionFlags & CollisionFlags.Above) != 0 && m_VerticalSpeed > 0.0f)
        {
            m_VerticalSpeed = 0.0f;
        }
        if ((l_CollisionFlags & CollisionFlags.Below)!=0)
        {
            m_VerticalSpeed = 0.0f;
            m_OnGround = true;
        }
        else
        {
            m_OnGround = false;
        }

        if (Input.GetKeyDown(AttachObjectKeyCode) && CanAttachObject())
            AttachObject();
        if(ObjectAttached && !AttachingObject)
        {
            if(Input.GetMouseButtonDown(0))
            {
                ThrowAttachedObject(AttachedObjectThrowForce);
            }
            if(Input.GetMouseButtonDown(1))
            {
                ThrowAttachedObject(0.0f);
            }
        }
        else if(!AttachingObject)
        {
            if(Input.mouseScrollDelta.y > 0.0f && m_PortalSize < 2)
            {
                DecallPortal.transform.localScale = new Vector3(DecallPortal.transform.localScale.x / 0.5f, DecallPortal.transform.localScale.y / 0.5f, DecallPortal.transform.localScale.z);
                m_PortalSize++; 
            }
            else if (Input.mouseScrollDelta.y < 0.0f && m_PortalSize > 0)
            {
                DecallPortal.transform.localScale = new Vector3(DecallPortal.transform.localScale.x * 0.5f, DecallPortal.transform.localScale.y * 0.5f, DecallPortal.transform.localScale.z);
                m_PortalSize--;
            }
            if (Input.GetMouseButton(0))
            {
                ShootDecall(DecallPortal);
            }
            if (Input.GetMouseButtonUp(0))
            {
                DecallPortal.gameObject.SetActive(false);
                Shoot(BluePortal);
            }
            if (Input.GetMouseButton(1))
            {
                ShootDecall(DecallPortal);
            }
            if (Input.GetMouseButtonUp(1))
            {
                DecallPortal.gameObject.SetActive(false);
                Shoot(OrangePortal);
            }
        }

        if (AttachingObject)
            UpdateAttachObject();
    }
    bool CanAttachObject()
    {
        return ObjectAttached == null;
    }

    void AttachObject()
    {
        Debug.Log("hola");
        Ray l_Ray = m_Camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0.0f));
        RaycastHit l_RaycastHit;
        if(Physics.Raycast(l_Ray, out l_RaycastHit, MaxDistanceToAttachObject, AttachObjectLayerMask.value))
        {
            if(l_RaycastHit.collider.tag == "CompanionCube" || l_RaycastHit.collider.tag == "RefractionCube")
            {
                AttachingObject = true;
                ObjectAttached = l_RaycastHit.collider.GetComponent<Rigidbody>();
                ObjectAttached.GetComponent<CompanionScript>().SetAttached(true);
                ObjectAttached.isKinematic = true;
                AttachingObjectStartRotation = l_RaycastHit.collider.transform.rotation;
            }
        }
    }
    void ThrowAttachedObject(float Force)
    {
        if(ObjectAttached != null)
        {
            ObjectAttached.transform.SetParent(null);
            ObjectAttached.isKinematic = false;
            ObjectAttached.AddForce(m_PitchController.forward * Force);
            ObjectAttached.GetComponent<CompanionScript>().SetAttached(false);
            ObjectAttached = null;
        }
    }
    void UpdateAttachObject()
    {
        Vector3 l_EulerAngles = AttachingPosition.rotation.eulerAngles;
        Vector3 l_Direction = AttachingPosition.transform.position - ObjectAttached.transform.position;
        float l_Distance = l_Direction.magnitude;
        float l_Movement = AttachingObjectSpeed * Time.deltaTime;
        if(l_Movement >= l_Distance)
        {
            AttachingObject = false;
            ObjectAttached.transform.SetParent(AttachingPosition);
            ObjectAttached.transform.localPosition = Vector3.zero;
            ObjectAttached.transform.localRotation = Quaternion.identity;
        }
        else
        {
            l_Direction /= l_Distance;
            ObjectAttached.MovePosition(ObjectAttached.transform.position + l_Direction * l_Movement);
            ObjectAttached.MoveRotation(Quaternion.Lerp(AttachingObjectStartRotation, 
                Quaternion.Euler(0.0f, l_EulerAngles.y, l_EulerAngles.z), 1.0f - Mathf.Min(l_Distance / 1.5f, 1.0f)));
        }
    }
    void Shoot(Portal _Portal)
    {
        Vector3 l_Pos;
        Vector3 l_Normal;

        if (_Portal.IsValidPos(m_Camera.transform.position, m_Camera.transform.forward, m_MaxShootDistance, m_ShootingLayerMask, out l_Pos, out l_Normal))
        {
            _Portal.gameObject.SetActive(true);
        }
        else
        {
            _Portal.gameObject.SetActive(false);
        }
    }

    void ShootDecall(DecallPortal _DecallPortal)
    {
        Vector3 l_Pos;
        Vector3 l_Normal;

        if (_DecallPortal.IsValidPos(m_Camera.transform.position, m_Camera.transform.forward, m_MaxShootDistance, m_ShootingLayerMask, out l_Pos, out l_Normal))
        {
            _DecallPortal.gameObject.SetActive(true);
        }
        else
        {
            _DecallPortal.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Portal")
        {
            Portal l_Portal = other.GetComponent<Portal>();
            if(Vector3.Dot(l_Portal.transform.forward, -Direction)>Mathf.Cos(AngleToEnterPortalInDegrees * Mathf.Deg2Rad))
                Teleport(l_Portal);
        }
        if (other.tag == "DeadZone")
        {
            Die();
        }
    }

    void Teleport(Portal thePortal)
    {
        Vector3 l_LocalPos = thePortal.OtherPortalTransform.InverseTransformPoint(transform.position);
        Vector3 l_LocalDir = thePortal.OtherPortalTransform.transform.InverseTransformDirection(transform.forward);
        Vector3 l_LocalDirMovement = thePortal.OtherPortalTransform.transform.InverseTransformDirection(Direction);
        Vector3 l_WorldDirMovement = thePortal.MirrorPortal.transform.TransformDirection(l_LocalDirMovement);

        m_CharacterController.enabled = false;
        transform.forward = thePortal.MirrorPortal.transform.TransformDirection(l_LocalDir);
        m_Yaw = transform.rotation.eulerAngles.y;
        transform.position = thePortal.MirrorPortal.transform.TransformPoint(l_LocalPos) + l_WorldDirMovement * OffsetTeleportPortal;
        m_CharacterController.enabled = true;
    }

    public void Die()
    {
        SceneManager.LoadScene("GameOver");
    }
    public void RestartGame()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
}
