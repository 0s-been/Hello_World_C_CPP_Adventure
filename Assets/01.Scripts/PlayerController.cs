using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;

public class PlayerController : MonoBehaviour
{
    //�⺻ �̵� ����
    private float m_MoveSpeed = 10f;
    private float m_RotSpeed = 30f;
    private float m_JumpHeight = 10f;

    //�뽬 ����
    public float m_DashDistance = 10f;
    private float m_dashcool = 0f;
    private float m_dashduration = 0.15f;
    private float m_dashcoolTimer;

    //���� ����
    private bool m_IsMove = false;
    private bool m_isDash = false;
    //���� ��� �ִ� �� �Ǻ��� ����
    private bool m_IsGrounded = false;

    //������Ʈ ����
    private Transform m_CameraTrans;
    //������ ������Ʈ�� ���̾ �Ǻ��� ���̾��ũ
    public LayerMask m_layer;
    private Rigidbody m_rigidbody;
    //�ִϸ��̼� ��Ʈ�ѷ�
    public PlayerAniController m_AnimController;

    public PlayerComboAtk m_ComboCom;



    //��� ����� ������ �� ������ ���� ����
    private Vector3 m_dir = Vector3.zero;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rigidbody = this.GetComponent<Rigidbody>();
        m_AnimController = GetComponent<PlayerAniController>();
        m_ComboCom = GetComponent<PlayerComboAtk>();

        //ī�޶� �Ҵ���� �ʾ��� ��� �ڵ����� �Ҵ�
        if(m_CameraTrans == null)
        {
            //mainī�޶��� �θ� transformã��
            Camera mainCam = Camera.main;
            if(mainCam != null)
            {  m_CameraTrans = mainCam.transform.parent;  }
        }
        
    }

   ///////////////////�����ڵ�///////////////////////////////////////251123ver
   //���� �ִϸ��̼� �κ��� ����
  

    void Update()
    {
        //�Է� ó��
        InputHandle();

        //���� üũ
        CheckGrounded();

        //���� - ������ ��� �뽬 ���� �ƴ� �� ��� ����
        if(Input.GetButtonDown("Jump") && m_IsGrounded && !m_isDash)
        { Jump(); }

        //�뽬 ��Ÿ�� Ÿ�̸�
        if(m_dashcoolTimer > 0)
        { m_dashcoolTimer -= Time.deltaTime;}

        if(Input.GetButtonDown("Dash") && m_dashcoolTimer <= 0 && !m_isDash )
        {
            StartCoroutine(Dash());
        }

        if(Input.GetMouseButton(0))
        {
             Debug.Log($"in update 공격 실행 : {m_ComboCom.currCombo}");
            Attack();
        }
        
        //�ִϸ����� ������Ʈ �κ� ����
        UpdateAnimation();
    }

    void FixedUpdate()
    {
        //�뽬 ���� �ƴ� ���� �Ϲ� �̵� ó��
        if(!m_isDash)
        {
            MovementHandle();
            JumpGravity();
        }
    }

    private void UpdateAnimation()
    {
        if (m_AnimController == null) return;
        //�̵� �ִϸ��̼�
        m_AnimController.SetMovement(m_IsMove, m_dir.magnitude);
        //���� ���� �ִϸ��̼�
        m_AnimController.SetGrounded(m_IsGrounded);
        //���� �ӵ� �ִϸ��̼�
        m_AnimController.SetVerticalVelocity(m_rigidbody.linearVelocity.y);

    }
    //�Է°����� ī�޶� ���� �̵� ������ ����ϴ� �Լ�
    private void InputHandle()
    {
        //��� �߿� �Է� ����
        //��� �߿� ������ �����ϵ��� �ϰų� Ư������ �߰� ���� ��
        if (m_isDash) return;

        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        //ī�޶� ������ ������ǥ �������� �̵�
        if(m_CameraTrans == null)
        {
            m_dir = new Vector3(horz, 0f, vert);
            m_dir.Normalize();
            //!=������ ���۷����� ������ ���Ͱ� �����Ͱ� �ƴ϶�� true�� �̵� ��
            //�����Ͷ��  false�� �̵� ���� �ƴ�
            m_IsMove = m_dir != Vector3.zero;
            return;
        }

        //ī�޶��� forward�� right���͸� ���������� ����̵��� �� ���̹Ƿ� y�� 0
        //�׸��� ũ�Ⱑ �ƴ� ���⸸ �ʿ��ϹǷ� ����ȭ
        Vector3 CameraForward = m_CameraTrans.forward;
        CameraForward.y = 0f;
        CameraForward.Normalize();
        Vector3 CameraRight = m_CameraTrans.right;
        CameraRight.y = 0f;
        CameraRight.Normalize();
        
        //ī�޶� �������� �̵� ���� ���
        m_dir = (CameraForward * vert + CameraRight * horz).normalized;
        m_IsMove = m_dir != Vector3.zero;
    }

    //�������� ĳ������ �̵��� ȸ���� ó���ϴ� �Լ�->Fixedupdate���� ȣ��
    private void MovementHandle()
    {
        if(m_dir != Vector3.zero)
        {
            //slerp�� ���� ���������� �������� ������ ������ �����Ͽ� �ε巴�� ȸ��
            Quaternion targetRot = Quaternion.LookRotation(m_dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot,
                Time.fixedDeltaTime * m_RotSpeed);

            //������ transform.position�� ���� �����ߴµ� ������ٵ��� MovePosition�� �ϴ� �� 
            //�� �����ϴٰ� �ؼ� ����
            Vector3 targetPos = transform.position + m_dir * m_MoveSpeed * Time.fixedDeltaTime;
            m_rigidbody.MovePosition(targetPos);
        }
    }

    //���� ��� �ִ� �� üũ�� �Լ�
    void CheckGrounded()
    {
        //����ĳ��Ʈ�� �� ���������� �÷��̾��� ��ǥ���� ��¦ ���� ����
        Vector3 rayStart = transform.position + Vector3.up * 0.2f;

        //Raycast�� ��ȯŸ���� bool�̹Ƿ� if else���� �ʿ������ ����
        //���۽���, �Ʒ���, 1.0f�Ÿ���ŭ �߻�, ���̾��ũ�� ��
        m_IsGrounded = Physics.Raycast(rayStart, Vector3.down, 1.0f, m_layer);
    }

    private void Jump()
    {
        //�� �������� �Ǽ����� ���͸� ���� �̵� ���� ���
        Vector3 jumpforce = Vector3.up * m_JumpHeight;
        //�� �̵����ͷ� ���� ����
        m_rigidbody.AddForce(jumpforce, ForceMode.VelocityChange);

        //�ִϸ��̼� Ʈ���� ȣ��
        if(m_AnimController != null)
        {
            m_AnimController.TriggerJump();
        }
    }

    //���� �� �ϰ� �ӵ��� �ʹ� ������ gravity���� �Ͻ������� �����Ͽ� �� ������ �ϰ��ϵ��� ����
    private void JumpGravity()
    {
        //y���� �����ӵ��� 0���� �۴ٸ� ������ ��� �������� ���� -> �ϰ� ����
        if(m_rigidbody.linearVelocity.y < 0)
        {
            m_rigidbody.AddForce(Physics.gravity * 5f, ForceMode.Acceleration);
        }
        //��� ���� �� �߷��� �� �� ���ϰ� ����
        else if(m_rigidbody.linearVelocity.y > 0)
        {
            m_rigidbody.AddForce(Physics.gravity * 2f, ForceMode.Acceleration);
        }
    }


    IEnumerator Dash()
    {
        m_isDash = true;
        m_dashcoolTimer = m_dashcool;

        //�ִϸ��̼� Ʈ���� ȣ��
        if (m_AnimController != null)
        {
            m_AnimController.TriggerDash();
            m_AnimController.SetDashing(true);
        }

        //�뽬 �� ���� �� y�� �ӵ��� ���ΰ� ����ӵ��� �ʱ�ȭ
        m_rigidbody.linearVelocity = new Vector3(0, m_rigidbody.linearVelocity.y, 0);
        //ĳ���� ������ �뽬�� �������� ����
        Vector3 dashdir = transform.forward;
        float dashspeed = m_DashDistance * 10f;

        //�뽬 �ʹݿ� ��û ������ ���ٰ� ���� ������ �����ϱ� ���ؼ�
        //�ʱ� �ӵ��� ���� ����. -> y���� �̵��� ������ ����
        Vector3 dashVelocity = new Vector3(dashdir.x * dashspeed,
            m_rigidbody.linearVelocity.y, dashdir.z * dashspeed);
        m_rigidbody.linearVelocity = dashVelocity;

        //�뽬 �Ĺݿ� �������� ������ �ϱ� �� ���� �������װ�?�� ����
        float originalDamping = m_rigidbody.linearDamping;
        m_rigidbody.linearDamping = 10f;

        //�ڿ������� ������ ���� �뽬 ���� �ð� ���� ������ �ٸ��� �� �� ����� Ÿ�̸�
        float temptime = 0f;

        while(temptime < m_dashduration)
        {
            temptime += Time.deltaTime;

            float t = temptime / m_dashduration;

            //�뽬 ���� �� 
            Vector3 currDashVel = new Vector3(dashdir.x * dashspeed,
                m_rigidbody.linearVelocity.y, dashdir.z * dashspeed);
            //�뽬 ���� ��
            Vector3 endDashVel = new Vector3(0f, m_rigidbody.linearVelocity.y, 0f);

            //lerp�� ���� �߰����� �������� �ڿ������� �����ϵ�
            //t�� ������ ���� �������� ������ ������ �츲
            m_rigidbody.linearVelocity = Vector3.Lerp(currDashVel, endDashVel, t * t);
            yield return null;
        }

        //�뽬 ���� �� ���� ����Ű �Է����� ���� �̵��� ������ ��ġ�� �ʵ��� �ӵ� ����
        m_rigidbody.linearVelocity = new Vector3(m_rigidbody.linearVelocity.x * 0.2f,
            m_rigidbody.linearVelocity.y, m_rigidbody.linearVelocity.z * 0.2f);

        //���� ���װ����� ����
        m_rigidbody.linearDamping = originalDamping;
        m_isDash = false;

        if(m_AnimController != null)
        {
            m_AnimController.SetDashing(false);
        }
    }

    private void Attack()
    {
        Debug.Log($"in attack func공격 실행 : {m_ComboCom.currCombo}");
        if(m_AnimController != null)
        {
            m_AnimController.TriggerAttack();
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
