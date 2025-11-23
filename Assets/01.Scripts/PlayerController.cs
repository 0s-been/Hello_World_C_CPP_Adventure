using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    private float m_MoveSpeed = 10f;
    private float m_RotSpeed = 30f;
    private float m_JumpHeight = 10f;

    [Header("Dash")]
    public float m_DashDistance = 10f;
    private float m_dashcool = 0f;
    private float m_dashduration = 0.15f;
    private float m_dashcoolTimer;

    [Header("State")]
    private bool m_IsMove = false;
    private bool m_isDash = false;
    //땅을 밟고 있는 지 판별할 변수
    private bool m_IsGrounded = false;

    [Header("Component")]
    private Transform m_CameraTrans;
    //접촉한 오브젝트의 레이어를 판별할 레이어마스크
    public LayerMask m_layer;
    private Rigidbody m_rigidbody;
    private Animator m_Animator;

    //어떠한 기능을 수행할 때 방향을 정할 변수
    private Vector3 m_dir = Vector3.zero;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = this.GetComponent<Rigidbody>();
        m_Animator = this.GetComponent <Animator>();

        //카메라가 할당되지 않았을 경우 자동으로 할당
        if(m_CameraTrans == null)
        {
            //main카메라의 부모 transform찾기
            Camera mainCam = Camera.main;
            if(mainCam != null)
            {  m_CameraTrans = mainCam.transform.parent;  }
        }
        
    }

   ///////////////////개선코드///////////////////////////////////////251123ver
   //아직 애니메이션 부분은 못함
  
   
    void Update()
    {
        //입력 처리
        InputHandle();

        //지면 체크
        CheckGrounded();

        //점프 - 지면을 밟고 대쉬 중이 아닐 때 사용 가능
        if(Input.GetButtonDown("Jump") && m_IsGrounded && !m_isDash)
        { Jump(); }

        //대쉬 쿨타임 타이머
        if(m_dashcoolTimer > 0)
        { m_dashcoolTimer -= Time.deltaTime;}

        if(Input.GetButtonDown("Dash") && m_dashcoolTimer <= 0 && !m_isDash )
        {
            StartCoroutine(Dash());
        }
        
        //애니메이터 업데이트 부분 구현
    }

    void FixedUpdate()
    {
        //대쉬 중이 아닐 때만 일반 이동 처리
        if(!m_isDash)
        {
            MovementHandle();
            JumpGravity();
        }
    }
    //입력값으로 카메라 기준 이동 방향을 계산하는 함수
    private void InputHandle()
    {
        //대시 중엔 입력 무시
        //대시 중에 공격을 가능하도록 하거나 특수공격 추가 고민 중
        if (m_isDash) return;

        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        //카메라가 없으면 월드좌표 기준으로 이동
        if(m_CameraTrans == null)
        {
            m_dir = new Vector3(horz, 0f, vert);
            m_dir.Normalize();
            //!=연산자 오퍼레이터 목적지 벡터가 영벡터가 아니라면 true로 이동 중
            //영벡터라면  false로 이동 중이 아님
            m_IsMove = m_dir != Vector3.zero;
            return;
        }

        //카메라의 forward와 right벡터를 가져오지만 평면이동을 할 것이므로 y는 0
        //그리고 크기가 아닌 방향만 필요하므로 정규화
        Vector3 CameraForward = m_CameraTrans.forward;
        CameraForward.y = 0f;
        CameraForward.Normalize();
        Vector3 CameraRight = m_CameraTrans.right;
        CameraRight.y = 0f;
        CameraRight.Normalize();
        
        //카메라 기준으로 이동 방향 계산
        m_dir = (CameraForward * vert + CameraRight * horz).normalized;
        m_IsMove = m_dir != Vector3.zero;
    }

    //실질적인 캐릭터의 이동과 회전을 처리하는 함수->Fixedupdate에서 호출
    private void MovementHandle()
    {
        if(m_dir != Vector3.zero)
        {
            //slerp를 통해 시작지점과 목적지점 사이의 값들을 보간하여 부드럽게 회전
            Quaternion targetRot = Quaternion.LookRotation(m_dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                Time.fixedDeltaTime * m_RotSpeed);

            //원래는 transform.position을 직접 수정했는데 리지드바디의 MovePosition로 하는 게 
            //더 안전하다고 해서 수정
            Vector3 targetPos = transform.position + m_dir * m_MoveSpeed * Time.fixedDeltaTime;
            m_rigidbody.MovePosition(targetPos);
        }
    }

    //땅을 밟고 있는 지 체크할 함수
    void CheckGrounded()
    {
        //레이캐스트를 쏠 시작지점을 플레이어의 좌표보다 살짝 위로 설정
        Vector3 rayStart = transform.position + Vector3.up * 0.2f;

        //Raycast의 반환타입이 bool이므로 if else문이 필요없도록 개선
        //시작시점, 아래로, 1.0f거리만큼 발사, 레이어마스크와 비교
        m_IsGrounded = Physics.Raycast(rayStart, Vector3.down, 1.0f, m_layer);
    }

    private void Jump()
    {
        //위 방향으로 실수배한 벡터를 통해 이동 벡터 계산
        Vector3 jumpforce = Vector3.up * m_JumpHeight;
        //그 이동벡터로 힘을 가함
        m_rigidbody.AddForce(jumpforce, ForceMode.VelocityChange);
        if(m_Animator != null)
        {
            m_Animator.SetTrigger("Jump");
        }
    }

    //점프 시 하강 속도가 너무 느려서 gravity값을 일시적으로 변경하여 더 빠르게 하강하도록 설정
    private void JumpGravity()
    {
        //y방향 선형속도가 0보다 작다면 정점을 찍고 내려오는 순간 -> 하강 시작
        if(m_rigidbody.linearVelocity.y < 0)
        {
            m_rigidbody.AddForce(Physics.gravity * 5f, ForceMode.Acceleration);
        }
        //상승 중일 땐 중력을 좀 더 약하게 설정
        else if(m_rigidbody.linearVelocity.y > 0)
        {
            m_rigidbody.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
        }
    }


    IEnumerator Dash()
    {
        m_isDash = true;
        m_dashcoolTimer = m_dashcool;

        if (m_Animator != null)
        {
            m_Animator.SetTrigger("Dash");
        }

        //대쉬 전 점프 시 y의 속도는 놔두고 수평속도만 초기화
        m_rigidbody.linearVelocity = new Vector3(0, m_rigidbody.linearVelocity.y, 0);
        //캐릭터 정면을 대쉬할 방향으로 설정
        Vector3 dashdir = transform.forward;
        float dashspeed = m_DashDistance * 10f;

        //대쉬 초반엔 엄청 빠르게 갔다가 순간 감속을 적용하기 위해서
        //초기 속도를 따로 구함. -> y축의 이동은 기존을 유지
        Vector3 dashVelocity = new Vector3(dashdir.x * dashspeed,
            m_rigidbody.linearVelocity.y, dashdir.z * dashspeed);
        m_rigidbody.linearVelocity = dashVelocity;

        //대쉬 후반에 순간적인 감속을 하기 전 원래 공기저항값?을 저장
        float originalDamping = m_rigidbody.linearDamping;
        m_rigidbody.linearDamping = 10f;

        //자연스러운 감속을 위해 대쉬 지속 시간 동안 저항을 다르게 할 때 사용할 타이머
        float temptime = 0f;

        while(temptime < m_dashduration)
        {
            temptime += Time.deltaTime;

            float t = temptime / m_dashduration;

            //대쉬 시작 시 
            Vector3 currDashVel = new Vector3(dashdir.x * dashspeed,
                m_rigidbody.linearVelocity.y, dashdir.z * dashspeed);
            //대쉬 종료 시
            Vector3 endDashVel = new Vector3(0f, m_rigidbody.linearVelocity.y, 0f);

            //lerp를 통해 중간값들 보간으로 자연스럽게 감속하되
            //t의 제곱을 통해 순간적인 감속의 느낌을 살림
            m_rigidbody.linearVelocity = Vector3.Lerp(currDashVel, endDashVel, t * t);
            yield return null;
        }

        //대쉬 종료 후 다음 방향키 입력으로 인한 이동에 영향을 끼치지 않도록 속도 조정
        m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x * 0.2f,
            m_rigidbody.linearVelocity.y, m_rigidbody.linearVelocity.z * 0.2f);

        //원래 저항값으로 복원
        m_rigidbody.linearDamping = originalDamping;
        m_isDash = false;
    }


}
//아래 주석처리된 코드의 문제점
//[1] update, fixedupdate에서 너무 많은 기능을 수행함
//->기능을 분리하고 update에선 호출하는 식으로 개선해야할듯
//[2]카메라 트랜스폼등을 inspector창에서 연결 안 해주면 카메라가 안 뜸
//->혹시 모르니 start에서 카메라를 찾아서 연결해주는 부분 필요할듯
//[3]카메라의 방향이 바뀌면 그곳으로 플레이어의 정면도 바뀌어야 하는데 안 바껴서 좌표가 이상해짐
//->카메라 trans추가해서 방향 구하고 정규화해서 플레이어 이동 방향에 적용해야할듯

// Update is called once per frame
//void Update()
//{
//    //키보드에서 a,d를 입력 시 좌우 이동을 입력할 수 있음
//    m_dir.x = Input.GetAxis("Horizontal");
//    //키보드에서 a,d를 입력 시 앞뒤 이동을 입력할 수 있음
//    m_dir.z = Input.GetAxis("Vertical");

//    //[버그]버그 발생1 - 상하좌우 이동 시 이동속도는 일정하나 대각선 이동 시 속도가 빨라짐
//    //원인 -> 대각선 이동 시 피타고라스 정리에 의해 이동 값이 *루트2배 증가함
//    //해결 -> Nomalize();를 통해 값이 항상 방향만 유지되고 값은 정규화 시켜 일정하도록 설정
//    m_dir.Normalize();

//    //땅을 밟았는 지 검사, 그 후 땅을 밟고 있으면서 스페이스바 입력 시 점프 수행
//    CheckGrounded();
//    if (Input.GetButtonDown("Jump") && m_IsGrounded)
//    {
//        Jump();
//    }

//    //대쉬 스킬 쿨타임 감소 타이머
//    if(m_dashcool > 0)
//    { m_dashcool -= Time.deltaTime; }

//    //left shitf 입력 시 전방으로 빠르게 달려가는 기능
//    if (Input.GetButtonDown("Dash") && m_dashcool <= 0 && !m_isDash)
//    {
//        //StartCoroutine(Dash());
//    }

//}


////위 update와의 차이점은 호출 주기가 고정된 프레임이냐 로컬기기의 프레임이냐 차이
////고정된 프레임속도마다 호출하는 함수 -> 물리법칙에 관한 코드가 Update에 있으면 기기의 프레임에 따라 물리법칙이 다르게 적용될 수 있으므로 Fixed에 있어야함
//private void FixedUpdate()
//{
//    //어떠한 입력이 발생하면 m_dir은 영벡터가 아니게 되므로
//    if (m_dir != Vector3.zero)
//    {
//        //[버그]버그 발생2 - 반대방향으로 이동 시 바로 회전이 되지 않고 반대를 바라본 채 이동하다가 갑자기 돌아감
//        //원인 -> 어느 방향으로 회전을 해야할 지의 모호성 때문
//        //해결1 -> 반대방향 이동 시 임의로 한쪽으로 살짝 회전시켜서 방향을 정해줌
//        //해결2(적용 중) -> m_RotSpeed를 엄청 크게 해도 해결됨. 둘 중에 뭐가 좋은 지는 모르겠으나 해결 1보단 처리량은 적은 것 같음.
//        //Mathf.Sign -> 매개변수가 양수면 1, 0이면 0, 음수면 -1 반환
//        //반대 방향의 키 입력이 들어올 경우 살짝 회전시켜줌
//        //if (Mathf.Sign(transform.forward.x) != Mathf.Sign(m_dir.x)
//        //    || Mathf.Sign(transform.forward.z) != Mathf.Sign(m_dir.z))
//        //{ transform.Rotate(0,1,0); }

//        //두 점 사이를 t에 따라 보간해주는 함수
//        //회전 전의 방향 forward와 새로운 입력을 통한 방향 m_dir을 기준으로 수행 회전속도는 세 번째 매개변수로 제어
//        //[버그] 버그4 - 플레이어 이동 시 플레이어 오브젝트만 미세하게 떨림
//        //원인 -> jittering이라고 하는 unity의 흔한 버그로 여기선 FixedUpdate에서 Time.deltaTime 타임 사용으로 인한 문제
//        //FixedUpdate는 고정적인데 Time.deltaTime은 가변적이라서 발생했음
//        //해결 -> playercamer와 playercontroller에서 fixedupdate에서 사용하는 deltatime들을 전부 Time.fixedDeltaTime사용으로 해결
//        transform.forward = Vector3.Lerp(transform.forward, m_dir, Time.fixedDeltaTime * m_RotSpeed);
//    }

//    //내 위치에서 나아갈 방향 벡터와 이동 속도, 로컬기기의 프레임을 곱한 값을 더해서 이동 처리
//    m_rigidbody.MovePosition(this.gameObject.transform.position + m_dir * m_MoveSpeed * Time.fixedDeltaTime);

//    //y의 선형속도 값이 0보다 작다면 정점을 찍고 내려오는 상황
//    if(m_rigidbody.linearVelocity.y < 0)
//    {
//        m_rigidbody.AddForce(Physics.gravity * 5f, ForceMode.Acceleration);
//    }
//    //양수라면 점프 중인 상황
//    else if(m_rigidbody.linearVelocity.y > 0)
//    {
//        //하강할 때보다 중력을 약하게 설정
//        m_rigidbody.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
//    }
//}
////땅을 밟고 있는 지 체크할 함수
//void CheckGrounded()
//{
//    //레이저를 쏘는 변수
//    RaycastHit hit;

//    //캐릭터의 가장 최하단에서 살짝 올려준 지점으로부터, 아래로, 조건이 충족 시 반환값을 hit에, 0.4거리만큼 레이저를 발사, m_layer - 비교할 레이어변수
//    if (Physics.Raycast(transform.position + (Vector3.up * 0.2f), Vector3.down, out hit, 0.4f, m_layer))
//    {   m_IsGrounded = true;     }
//    else { m_IsGrounded = false; }

//}

//점프 기능
//void Jump()
//{
//    Vector3 JumpPower = Vector3.up * m_JumpHeight;
//    m_rigidbody.AddForce(JumpPower, ForceMode.VelocityChange);
//}
//대쉬를 코루틴으로 구현하여 damping조절을 통해 순간가속을 조절
//IEnumerator Dash()
//{

//    m_isDash = true;
//    m_dashcool = 5.0f;

//    //기존 가로상의 속도를 0으로 초기화
//    m_rigidbody.linearVelocity = Vector3.zero;
//    //대쉬 방향 및 속도에 관한 변수
//    Vector3 dashdir = transform.forward;
//    float dashSpeed = m_DashDistance * 10f;

//    //위 두 변수를 통해 속도 변경하여 가속
//    m_rigidbody.linearVelocity = dashdir * dashSpeed;

//    //감속부분을 위해 기존 가로상의 공기저항값을 저장
//    float originalDamping = m_rigidbody.linearDamping;
//    //저항을 높여 감속
//    m_rigidbody.linearDamping = 10f;

//    //대쉬가 진행되는 시간
//    float dashTime = 0.15f;
//    float temptime = 0f;
//    while (temptime < dashTime)
//    {
//        temptime += Time.deltaTime;

//        //점진적인 가속을 위한 가중치
//        float t = temptime / dashTime;
//        //대쉬로 이동할 위치와 제로 위치 사이에 보간값, 가중치를 제곱하여 더 따르게 감속시킴
//        m_rigidbody.linearVelocity = Vector3.Lerp(dashdir * dashSpeed, Vector3.zero, t * t);

//        yield return null;

//    }
//    //대쉬 후 공주거리가 크게 발생하지 않고 바로 멈추도록 속도 변경
//    m_rigidbody.linearVelocity *= 0.2f;
//    //가로상의 저항값 초기값으로 변경
//    m_rigidbody.linearDamping = originalDamping;
//    m_isDash = false;
//}


//위 주석처리된 코드의 문제점
//[1] update, fixedupdate에서 너무 많은 기능을 수행함
//->기능을 분리하고 update에선 호출하는 식으로 개선해야할듯
//[2]카메라 트랜스폼등을 inspector창에서 연결 안 해주면 카메라가 안 뜸
//->혹시 모르니 start에서 카메라를 찾아서 연결해주는 부분 필요할듯
//[3]카메라의 방향이 바뀌면 그곳으로 플레이어의 정면도 바뀌어야 하는데 안 바껴서 좌표가 이상해짐
//->카메라 trans추가해서 방향 구하고 정규화해서 플레이어 이동 방향에 적용해야할듯
