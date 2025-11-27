using UnityEngine;

public class PlayerAniController : MonoBehaviour
{
    public Animator m_Animator;

    private int m_HashSpeed = Animator.StringToHash("Speed");
    private int m_HashIsMoving = Animator.StringToHash("IsMoving");
    private int m_HashIsGrounded = Animator.StringToHash("IsGrounded");
    private int m_HashIsAttack = Animator.StringToHash("IsAttack");
    private int m_HashJump = Animator.StringToHash("Jump");
    private int m_HashDash = Animator.StringToHash("Dash");
    private int m_HashIsDashing = Animator.StringToHash("IsDashing");
    private int m_HashVerticalVelocity = Animator.StringToHash("VerticalVelocity");

    //start ���� ����Ǵ� �Լ�
    void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    //�̵� ���� ������Ʈ PlayerController�� MovementHandle���� ȣ��
    //isMoving -> �̵� ������ ����, speed -> �̵��ӵ�
    public void SetMovement(bool isMoving, float speed)
    {
        speed = 1f;
        if (m_Animator == null) return;

        m_Animator.SetBool(m_HashIsMoving, isMoving);
        m_Animator.SetFloat(m_HashSpeed, speed);
    }

    public void TriggerAttack()
    {
        if(m_Animator == null) return;

        m_Animator.SetTrigger(m_HashIsAttack);
    }

    //���� ���� ������Ʈ PlayerController�� CheckGrounded���� ȣ��
    public void SetGrounded(bool isGrounded)
    {
        if (m_Animator == null) return;

        m_Animator.SetBool(m_HashIsGrounded, isGrounded);
    }

    //���� Ʈ���� PlayerController�� Jump���� ȣ��
    public void TriggerJump()
    {
        if (m_Animator == null) return;

        m_Animator.SetTrigger(m_HashJump);
    }

    //�뽬 Ʈ���� PlayerController�� Dash �ڷ�ƾ���� ȣ��
    public void TriggerDash()
    {
        if (m_Animator == null) return;

        m_Animator.SetTrigger(m_HashDash);
    }

    //�뽬 ���� ������Ʈ
    public void SetDashing(bool isDashing)
    {
        if (m_Animator == null) return;

        m_Animator.SetBool(m_HashIsDashing, isDashing);
    }
    //���� �� ���,�ϰ� �ӵ��� �ٸ��� �׿� ���� ���� �ӵ� ������Ʈ
    public void SetVerticalVelocity(float vel)
    {
        if (m_Animator == null) return;

        m_Animator.SetFloat (m_HashVerticalVelocity, vel);
    }
}
