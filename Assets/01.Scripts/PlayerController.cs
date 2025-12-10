using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerController : MonoBehaviour
{
    //기본 이동 관련
    private float m_MoveSpeed = 10f;
    private float m_RotSpeed = 30f;
    private float m_JumpHeight = 12f;

    //대시 관련
    public float m_DashDistance = 5f;
    private float m_dashcool = 0f;
    private float m_dashduration = 0.7f;
    private float m_dashcoolTimer;

    //상태 관련
    private bool m_IsMove = false;
    private bool m_isDash = false;
    //땅을 밟고 있는 지 판별할 변수
    private bool m_IsGrounded = false;

    //컴포넌트 관련
    private Transform m_CameraTrans;
    //접촉한 오브젝트의 레이어를 판별할 레이어마스크
    public LayerMask m_layer;
    private Rigidbody m_rigidbody;
    //애니메이션 컨트롤러
    public PlayerAniController m_AnimController;
    //콤보 공격 컴포넌트
    public PlayerComboAtk m_ComboCom;

    //이동이나 카메라 방향에 대한 기능을 수행할 때 방향을 정할 변수
    private Vector3 m_dir = Vector3.zero;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = this.GetComponent<Rigidbody>();
        m_AnimController = GetComponent<PlayerAniController>();
        m_ComboCom = GetComponent<PlayerComboAtk>();

        //카메라가 할당되지 않았을 경우 자동으로 할당
        if (m_CameraTrans == null)
        {
            //main카메라의 부모 transform찾기
            Camera mainCam = Camera.main;
            if(mainCam != null)
            {  m_CameraTrans = mainCam.transform.parent;  }
        }
        
    }

    void Update()
    {
        //입력 처리
        InputHandle();

        //지면 체크
        CheckGrounded();

        //점프 - 지면을 밟고 대시 중이 아닐 때 사용 가능
        if (Input.GetButtonDown("Jump") && m_IsGrounded && !m_isDash)
        { Jump(); }

        //대시 쿨타임 타이머
        if (m_dashcoolTimer > 0)
        { m_dashcoolTimer -= Time.deltaTime;}

        if(Input.GetButtonDown("Dash") && m_dashcoolTimer <= 0 && !m_isDash )
        {
            StartCoroutine(Dash());
        }

        //애니메이터 업데이트 부분 구현
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        //대시 중이 아닐 때만 일반 이동 처리
        if (!m_isDash)
        {
            MovementHandle();
            JumpGravity();
        }
    }

    private void UpdateAnimation()
    {
        if (m_AnimController == null) return;
        //이동 애니메이션
        m_AnimController.SetMovement(m_IsMove, m_dir.magnitude);
        //지면 상태 애니메이션
        m_AnimController.SetGrounded(m_IsGrounded);
        //수직 속도 애니메이션
        m_AnimController.SetVerticalVelocity(m_rigidbody.linearVelocity.y);

    }
    //입력값으로 카메라 기준 이동 방향을 계산하는 함수
    private void InputHandle()
    {
        //대시 중엔 입력 무시
        if (m_isDash) return;

        //공격 중일 때 이동 입력 무시? 고민 중
        if (m_ComboCom != null && m_ComboCom.IsAttacking) return;

        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        //카메라가 없으면 월드좌표 기준으로 이동
        if (m_CameraTrans == null)
        {
            m_dir = new Vector3(horz, 0f, vert);
            m_dir.Normalize();      
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
        //공격 중일 때 이동 입력 무시? 고민 중
        if (m_ComboCom != null && m_ComboCom.IsAttacking) return;

        if (m_dir != Vector3.zero)
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

        //애니메이션 트리거 호출
        if (m_AnimController != null)
        {
            m_AnimController.TriggerJump();
        }
    }

    //점프 시 하강 속도가 너무 느려서 gravity값을 일시적으로 변경하여 더 빠르게 하강하도록 설정
    private void JumpGravity()
    {
        //y방향 선형속도가 0보다 작다면 정점을 찍고 내려오는 순간 -> 하강 시작
        if (m_rigidbody.linearVelocity.y < 0)
        {
            m_rigidbody.AddForce(Physics.gravity * 5f, ForceMode.Acceleration);
        }
        //상승 중일 땐 중력을 좀 더 약하게 설정
        else if (m_rigidbody.linearVelocity.y > 0)
        {
            m_rigidbody.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
        }
    }


    IEnumerator Dash()
    {
        m_isDash = true;
        m_dashcoolTimer = m_dashcool;

        //애니메이션 트리거 호출
        if (m_AnimController != null)
        {
            m_AnimController.TriggerDash();
            m_AnimController.SetDashing(true);
        }

        //대시 전 점프 시 y의 속도는 놔두고 수평속도만 초기화
        m_rigidbody.linearVelocity = new Vector3(0, m_rigidbody.linearVelocity.y, 0);
        //캐릭터 정면을 대시할 방향으로 설정
        Vector3 dashdir = transform.forward;
        float dashspeed = m_DashDistance * 10f;

        //대시 초반엔 엄청 빠르게 갔다가 순간 감속을 적용하기 위해서
        //초기 속도를 따로 구함. -> y축의 이동은 기존을 유지
        Vector3 dashVelocity = new Vector3(dashdir.x * dashspeed,
            m_rigidbody.linearVelocity.y, dashdir.z * dashspeed);
        m_rigidbody.linearVelocity = dashVelocity;

        //대시 후반에 순간적인 감속을 하기 전 원래 공기저항값?을 저장
        float originalDamping = m_rigidbody.linearDamping;
        m_rigidbody.linearDamping = 10f;

        //자연스러운 감속을 위해 대시 지속 시간 동안 저항을 다르게 할 때 사용할 타이머
        float temptime = 0f;

        while(temptime < m_dashduration)
        {
            temptime += Time.deltaTime;

            float t = temptime / m_dashduration;

            //대시 시작 시 
            Vector3 currDashVel = new Vector3(dashdir.x * dashspeed,
                m_rigidbody.linearVelocity.y, dashdir.z * dashspeed);
            //대시 종료 시
            Vector3 endDashVel = new Vector3(0f, m_rigidbody.linearVelocity.y, 0f);

            //lerp를 통해 중간값들 보간으로 자연스럽게 감속하되
            //t의 제곱을 통해 순간적인 감속의 느낌을 살림
            m_rigidbody.linearVelocity = Vector3.Lerp(currDashVel, endDashVel, t * t);
            yield return null;
        }

        //대시 종료 후 다음 방향키 입력으로 인한 이동에 영향을 끼치지 않도록 속도 조정
        m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x * 0.2f,
            m_rigidbody.linearVelocity.y, m_rigidbody.linearVelocity.z * 0.2f);

        //원래 저항값으로 복원
        m_rigidbody.linearDamping = originalDamping;
        m_isDash = false;

        if(m_AnimController != null)
        {
            m_AnimController.SetDashing(false);
        }
    }

}
//�Ʒ� �ּ�ó���� �ڵ��� ������
//[1] update, fixedupdate���� �ʹ� ���� ����� ������
//->����� �и��ϰ� update���� ȣ���ϴ� ������ �����ؾ��ҵ�
//[2]ī�޶� Ʈ���������� inspectorâ���� ���� �� ���ָ� ī�޶� �� ��
//->Ȥ�� �𸣴� start���� ī�޶� ã�Ƽ� �������ִ� �κ� �ʿ��ҵ�
//[3]ī�޶��� ������ �ٲ�� �װ����� �÷��̾��� ���鵵 �ٲ��� �ϴµ� �� �ٲ��� ��ǥ�� �̻�����
//->ī�޶� trans�߰��ؼ� ���� ���ϰ� ����ȭ�ؼ� �÷��̾� �̵� ���⿡ �����ؾ��ҵ�

// Update is called once per frame
//void Update()
//{
//    //Ű���忡�� a,d�� �Է� �� �¿� �̵��� �Է��� �� ����
//    m_dir.x = Input.GetAxis("Horizontal");
//    //Ű���忡�� a,d�� �Է� �� �յ� �̵��� �Է��� �� ����
//    m_dir.z = Input.GetAxis("Vertical");

//    //[����]���� �߻�1 - �����¿� �̵� �� �̵��ӵ��� �����ϳ� �밢�� �̵� �� �ӵ��� ������
//    //���� -> �밢�� �̵� �� ��Ÿ���� ������ ���� �̵� ���� *��Ʈ2�� ������
//    //�ذ� -> Nomalize();�� ���� ���� �׻� ���⸸ �����ǰ� ���� ����ȭ ���� �����ϵ��� ����
//    m_dir.Normalize();

//    //���� ��Ҵ� �� �˻�, �� �� ���� ��� �����鼭 �����̽��� �Է� �� ���� ����
//    CheckGrounded();
//    if (Input.GetButtonDown("Jump") && m_IsGrounded)
//    {
//        Jump();
//    }

//    //�뽬 ��ų ��Ÿ�� ���� Ÿ�̸�
//    if(m_dashcool > 0)
//    { m_dashcool -= Time.deltaTime; }

//    //left shitf �Է� �� �������� ������ �޷����� ���
//    if (Input.GetButtonDown("Dash") && m_dashcool <= 0 && !m_isDash)
//    {
//        //StartCoroutine(Dash());
//    }

//}


////�� update���� �������� ȣ�� �ֱⰡ ������ �������̳� ���ñ���� �������̳� ����
////������ �����Ӽӵ����� ȣ���ϴ� �Լ� -> ������Ģ�� ���� �ڵ尡 Update�� ������ ����� �����ӿ� ���� ������Ģ�� �ٸ��� ����� �� �����Ƿ� Fixed�� �־����
//private void FixedUpdate()
//{
//    //��� �Է��� �߻��ϸ� m_dir�� �����Ͱ� �ƴϰ� �ǹǷ�
//    if (m_dir != Vector3.zero)
//    {
//        //[����]���� �߻�2 - �ݴ�������� �̵� �� �ٷ� ȸ���� ���� �ʰ� �ݴ븦 �ٶ� ä �̵��ϴٰ� ���ڱ� ���ư�
//        //���� -> ��� �������� ȸ���� �ؾ��� ���� ��ȣ�� ����
//        //�ذ�1 -> �ݴ���� �̵� �� ���Ƿ� �������� ��¦ ȸ�����Ѽ� ������ ������
//        //�ذ�2(���� ��) -> m_RotSpeed�� ��û ũ�� �ص� �ذ��. �� �߿� ���� ���� ���� �𸣰����� �ذ� 1���� ó������ ���� �� ����.
//        //Mathf.Sign -> �Ű������� ����� 1, 0�̸� 0, ������ -1 ��ȯ
//        //�ݴ� ������ Ű �Է��� ���� ��� ��¦ ȸ��������
//        //if (Mathf.Sign(transform.forward.x) != Mathf.Sign(m_dir.x)
//        //    || Mathf.Sign(transform.forward.z) != Mathf.Sign(m_dir.z))
//        //{ transform.Rotate(0,1,0); }

//        //�� �� ���̸� t�� ���� �������ִ� �Լ�
//        //ȸ�� ���� ���� forward�� ���ο� �Է��� ���� ���� m_dir�� �������� ���� ȸ���ӵ��� �� ��° �Ű������� ����
//        //[����] ����4 - �÷��̾� �̵� �� �÷��̾� ������Ʈ�� �̼��ϰ� ����
//        //���� -> jittering�̶�� �ϴ� unity�� ���� ���׷� ���⼱ FixedUpdate���� Time.deltaTime Ÿ�� ������� ���� ����
//        //FixedUpdate�� �������ε� Time.deltaTime�� �������̶� �߻�����
//        //�ذ� -> playercamer�� playercontroller���� fixedupdate���� ����ϴ� deltatime���� ���� Time.fixedDeltaTime������� �ذ�
//        transform.forward = Vector3.Lerp(transform.forward, m_dir, Time.fixedDeltaTime * m_RotSpeed);
//    }

//    //�� ��ġ���� ���ư� ���� ���Ϳ� �̵� �ӵ�, ���ñ���� �������� ���� ���� ���ؼ� �̵� ó��
//    m_rigidbody.MovePosition(this.gameObject.transform.position + m_dir * m_MoveSpeed * Time.fixedDeltaTime);

//    //y�� �����ӵ� ���� 0���� �۴ٸ� ������ ��� �������� ��Ȳ
//    if(m_rigidbody.linearVelocity.y < 0)
//    {
//        m_rigidbody.AddForce(Physics.gravity * 5f, ForceMode.Acceleration);
//    }
//    //������ ���� ���� ��Ȳ
//    else if(m_rigidbody.linearVelocity.y > 0)
//    {
//        //�ϰ��� ������ �߷��� ���ϰ� ����
//        m_rigidbody.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
//    }
//}
////���� ��� �ִ� �� üũ�� �Լ�
//void CheckGrounded()
//{
//    //�������� ��� ����
//    RaycastHit hit;

//    //ĳ������ ���� ���ϴܿ��� ��¦ �÷��� �������κ���, �Ʒ���, ������ ���� �� ��ȯ���� hit��, 0.4�Ÿ���ŭ �������� �߻�, m_layer - ���� ���̾��
//    if (Physics.Raycast(transform.position + (Vector3.up * 0.2f), Vector3.down, out hit, 0.4f, m_layer))
//    {   m_IsGrounded = true;     }
//    else { m_IsGrounded = false; }

//}

//���� ���
//void Jump()
//{
//    Vector3 JumpPower = Vector3.up * m_JumpHeight;
//    m_rigidbody.AddForce(JumpPower, ForceMode.VelocityChange);
//}
//�뽬�� �ڷ�ƾ���� �����Ͽ� damping������ ���� ���������� ����
//IEnumerator Dash()
//{

//    m_isDash = true;
//    m_dashcool = 5.0f;

//    //���� ���λ��� �ӵ��� 0���� �ʱ�ȭ
//    m_rigidbody.linearVelocity = Vector3.zero;
//    //�뽬 ���� �� �ӵ��� ���� ����
//    Vector3 dashdir = transform.forward;
//    float dashSpeed = m_DashDistance * 10f;

//    //�� �� ������ ���� �ӵ� �����Ͽ� ����
//    m_rigidbody.linearVelocity = dashdir * dashSpeed;

//    //���Ӻκ��� ���� ���� ���λ��� �������װ��� ����
//    float originalDamping = m_rigidbody.linearDamping;
//    //������ ���� ����
//    m_rigidbody.linearDamping = 10f;

//    //�뽬�� ����Ǵ� �ð�
//    float dashTime = 0.15f;
//    float temptime = 0f;
//    while (temptime < dashTime)
//    {
//        temptime += Time.deltaTime;

//        //�������� ������ ���� ����ġ
//        float t = temptime / dashTime;
//        //�뽬�� �̵��� ��ġ�� ���� ��ġ ���̿� ������, ����ġ�� �����Ͽ� �� ������ ���ӽ�Ŵ
//        m_rigidbody.linearVelocity = Vector3.Lerp(dashdir * dashSpeed, Vector3.zero, t * t);

//        yield return null;

//    }
//    //�뽬 �� ���ְŸ��� ũ�� �߻����� �ʰ� �ٷ� ���ߵ��� �ӵ� ����
//    m_rigidbody.linearVelocity *= 0.2f;
//    //���λ��� ���װ� �ʱⰪ���� ����
//    m_rigidbody.linearDamping = originalDamping;
//    m_isDash = false;
//}


//�� �ּ�ó���� �ڵ��� ������
//[1] update, fixedupdate���� �ʹ� ���� ����� ������
//->����� �и��ϰ� update���� ȣ���ϴ� ������ �����ؾ��ҵ�
//[2]ī�޶� Ʈ���������� inspectorâ���� ���� �� ���ָ� ī�޶� �� ��
//->Ȥ�� �𸣴� start���� ī�޶� ã�Ƽ� �������ִ� �κ� �ʿ��ҵ�
//[3]ī�޶��� ������ �ٲ�� �װ����� �÷��̾��� ���鵵 �ٲ��� �ϴµ� �� �ٲ��� ��ǥ�� �̻�����
//->ī�޶� trans�߰��ؼ� ���� ���ϰ� ����ȭ�ؼ� �÷��̾� �̵� ���⿡ �����ؾ��ҵ�
