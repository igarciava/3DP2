using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPSPlayerController : MonoBehaviour
{
    float m_Yaw;
    float m_Pitch;
    public float m_YawRotationalSpeed;
    public float m_PitchRotationalSpeed;

    public float m_MinPitch;
    public float m_MaxPitch;

    public Transform m_PitchController;
    public bool m_UseYawInverted;
    public bool m_UsePitchInverted;

    public CharacterController m_CharacterController;
    public float m_Speed;
    public float m_FastSpeedMultiplier = 3f;
    KeyCode m_LeftKeyCode = KeyCode.A;
    KeyCode m_RightKeyCode = KeyCode.D;
    KeyCode m_UpKeyCode = KeyCode.W;
    KeyCode m_DownKeyCode = KeyCode.S;
    KeyCode m_JumpKeyCode = KeyCode.Space;
    KeyCode m_RunKeycode = KeyCode.LeftShift;

    public Camera m_Camera;
    public float m_NormalMovementFOV = 60;
    public float m_RunMovementFOV = 70;

    float m_VerticalSpeed = 0.0f;
    bool m_OnGround = true;

    public float m_JumpSpeed = 10.0f;

    [Header("Portales")]
    public Portal BluePortal;
    public Portal OrangePortal;

    public KeyCode m_DebugLockAngleKeyCode = KeyCode.I;
    public KeyCode m_DebugLockKeyCode = KeyCode.O;
    bool m_AngleLocked = false;
    bool m_AimLocked = true;

    Vector3 m_StartPosition;
    Quaternion m_StartRotation;
    public Slider m_HealthSlider;
    public Slider m_ShieldSlider;

    [Header("Shoot")]
    public float m_MaxShootDistance = 50.0f;
    public LayerMask m_ShootingLayerMask;


    [Header("Life")]
    public float m_Life;
    float m_MaxLife = 1.0f;
    public float m_Shield;
    float m_MaxShield = 1.0f;
    public float m_DroneDamage;
    public float m_CurrentHealth;
    public float m_CurrentShield;

    [Header("HUD")]
    public Canvas HUD;

    [Header("Keys")]
    public bool HasAKey = false;

    void Start()
    {
        Debug.Log(m_Life);
        m_Yaw = transform.rotation.y;
        m_Pitch = m_PitchController.localRotation.x;
        Cursor.lockState = CursorLockMode.Locked;
        m_AimLocked = Cursor.lockState == CursorLockMode.Locked;
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        
    }

#if UNITY_EDITOR
    void UpdateInputDebug()
    {
        if (Input.GetKeyDown(m_DebugLockAngleKeyCode))
            m_AngleLocked = !m_AngleLocked;
        if (Input.GetKeyDown(m_DebugLockKeyCode))
        {
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
        Vector3 l_Direction = Vector3.zero;
        float l_Speed = m_Speed;

        if(Input.GetKey(m_UpKeyCode))
            l_Direction = l_ForwardDirection;
        if (Input.GetKey(m_DownKeyCode))
            l_Direction = -l_ForwardDirection;
        if (Input.GetKey(m_RightKeyCode))
            l_Direction += l_RightDirection;
        if (Input.GetKey(m_LeftKeyCode))
            l_Direction -= l_RightDirection;
        if (Input.GetKeyDown(m_JumpKeyCode) && m_OnGround)
            m_VerticalSpeed = m_JumpSpeed;
        float l_FOV = m_NormalMovementFOV;
        if (Input.GetKey(m_RunKeycode))
        {
            l_Speed = m_Speed * m_FastSpeedMultiplier;
            l_FOV = m_RunMovementFOV;
        }
        m_Camera.fieldOfView = l_FOV;

        l_Direction.Normalize();
        Vector3 l_Movement = l_Direction * l_Speed * Time.deltaTime;

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

        if(Input.GetMouseButtonDown(0))
        {
            Shoot(BluePortal);
        }
        if(Input.GetMouseButtonDown(1))
        {
            Shoot(OrangePortal);
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

    void Kill()
    {
        m_Life = 0.0f;
        //GameController.GetGameController().RestartGame();
    }
    public void RestartGame()
    {
        m_Life = 1.0f;
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }   
}
