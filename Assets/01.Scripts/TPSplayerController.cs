using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;
using System.Collections;
using System.Collections.Generic;



//----------삽질-------------------------
//플레이어 움직임과 카메라는 구분하는 게 좋을듯

public class TPSplayerController : MonoBehaviour
{
    public enum StateType
    { ST_Idle = 0,
      ST_Move = 1,
      ST_Jump = 2,
      ST_Dash = 3
    };

    [SerializeField]
    private Transform m_characterbody;
    [SerializeField]
    private Transform m_Camerabody;
    private StateType m_State = (StateType)1;
    Animator m_animator;

    private float m_MoveSpeed = 5f;
    private float m_RotSpeed = 5f;
    private bool m_IsGrounded = true;
    private bool m_isDash = false;
    public float m_dashcool = 0f;
    private float m_DashDistance = 8f;
    public LayerMask m_layer;
    private float m_JumpHeight = 5f;
    private Rigidbody m_rigidbody;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        LookAround();
        CheckGrounded();
        Move();
        Jump();
        //대쉬 스킬 쿨타임 감소 타이머
        if (m_dashcool > 0)
        { m_dashcool -= Time.deltaTime; }

        //left shitf 입력 시 전방으로 빠르게 달려가는 기능
        if (Input.GetButtonDown("Dash") && m_dashcool <= 0 && !m_isDash)
        {
            StartCoroutine(Dash());
        }
        
    }

    private void Move()
    {
        //Debug.DrawRay(m_Camerabody.position, new Vector3(m_Camerabody.forward.x, 0f, m_Camerabody.forward.z).normalized, Color.red);
        Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        bool isMove = moveInput.magnitude != 0;
        if (isMove)
        {
            Vector3 looForawrd = new Vector3(m_Camerabody.forward.x, 0f, m_Camerabody.forward.z).normalized;
            Vector3 lookRight = new Vector3(m_Camerabody.right.x, 0f, m_Camerabody.right.z).normalized;
            Vector3 moveDir = looForawrd * moveInput.y + lookRight * moveInput.x;

            m_characterbody.forward = moveDir;
            Vector3.Lerp(transform.forward, moveDir, Time.deltaTime * m_RotSpeed);
            transform.position += moveDir * Time.deltaTime * m_MoveSpeed;
        }
    }

    private void LookAround()
    {
        Vector2 mouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        Vector3 camAngle = m_Camerabody.rotation.eulerAngles;
        Debug.Log(camAngle.x);
        float ClampX = camAngle.x - mouseDelta.y;
        if (ClampX < 180f)
        {
            ClampX = Mathf.Clamp(ClampX, -1f, 40f);
        }
        else
        {
            ClampX = Mathf.Clamp(ClampX, 270f, 361f);
        }

        m_Camerabody.rotation = Quaternion.Euler(ClampX, camAngle.y + mouseDelta.x, camAngle.z);
    }

    //땅을 밟고 있는 지 체크할 함수
    void CheckGrounded()
    {
        //레이저를 쏘는 변수
        RaycastHit hit;

        //캐릭터의 가장 최하단에서 살짝 올려준 지점으로부터, 아래로, 조건이 충족 시 반환값을 hit에, 0.4거리만큼 레이저를 발사, m_layer - 비교할 레이어변수
        if (Physics.Raycast(transform.position + (Vector3.up * 0.2f), Vector3.down, out hit, 0.4f, m_layer))
        { m_IsGrounded = true; }
        else { m_IsGrounded = false; }

    }
    void Jump()
    {
        if (Input.GetButtonDown("Jump") && m_IsGrounded)
        {
            Vector3 JumpPower = Vector3.up * m_JumpHeight;
            m_rigidbody.AddForce(JumpPower, ForceMode.VelocityChange);
        }
        
    }
    //대쉬를 코루틴으로 구현하여 damping조절을 통해 순간가속을 조절
    IEnumerator Dash()
    {
        

        //기존 가로상의 속도를 0으로 초기화
        m_rigidbody.linearVelocity = Vector3.zero;
        //대쉬 방향 및 속도에 관한 변수
        Vector3 dashdir = new Vector3(m_characterbody.forward.x, 0f, m_characterbody.forward.z).normalized;
        float dashSpeed = m_DashDistance * 10f;

        //위 두 변수를 통해 속도 변경하여 가속
        m_rigidbody.linearVelocity = dashdir * dashSpeed;

        //감속부분을 위해 기존 가로상의 공기저항값을 저장
        float originalDamping = m_rigidbody.linearDamping;
        //저항을 높여 감속
        m_rigidbody.linearDamping = 10f;

        //대쉬가 진행되는 시간
        float dashTime = 0.15f;
        float temptime = 0f;
        while (temptime < dashTime)
        {
            temptime += Time.deltaTime;

            //점진적인 가속을 위한 가중치
            float t = temptime / dashTime;
            //대쉬로 이동할 위치와 제로 위치 사이에 보간값, 가중치를 제곱하여 더 따르게 감속시킴
            m_rigidbody.linearVelocity = Vector3.Lerp(dashdir * dashSpeed, Vector3.zero, t * t);

            yield return null;

        }
        //대쉬 후 공주거리가 크게 발생하지 않고 바로 멈추도록 속도 변경
        m_rigidbody.linearVelocity *= 0.2f;
        //가로상의 저항값 초기값으로 변경
        m_rigidbody.linearDamping = originalDamping;
        m_isDash = false;
    }
}
