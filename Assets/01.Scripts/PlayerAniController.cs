using UnityEngine;

public class PlayerAniController : MonoBehaviour
{
    public Animator m_Animator;

    private int m_HashSpeed = Animator.StringToHash("Speed");
    private int m_HashIsMoving = Animator.StringToHash("IsMoving");
    private int m_HashIsGrounded = Animator.StringToHash("IsGrounded");

    private int m_HashJump = Animator.StringToHash("Jump");
    private int m_HashDash = Animator.StringToHash("Dash");
    private int m_HashIsDashing = Animator.StringToHash("IsDashing");
    private int m_HashVerticalVelocity = Animator.StringToHash("VerticalVelocity");

    //start 전에 실행되는 함수
    void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    //이동 상태 업데이트 PlayerController의 MovementHandle에서 호출
    //isMoving -> 이동 중인지 여부, speed -> 이동속도
    public void SetMovement(bool isMoving, float speed)
    {
        speed = 1f;
        if (m_Animator == null) return;

        m_Animator.SetBool(m_HashIsMoving, isMoving);
        m_Animator.SetFloat(m_HashSpeed, speed);
    }

    //지면 상태 업데이트 PlayerController의 CheckGrounded에서 호출
    public void SetGrounded(bool isGrounded)
    {
        if (m_Animator == null) return;

        m_Animator.SetBool(m_HashIsGrounded, isGrounded);
    }

    //점프 트리거 PlayerController의 Jump에서 호출
    public void TriggerJump()
    {
        if (m_Animator == null) return;

        m_Animator.SetTrigger(m_HashJump);
    }

    //대시 트리거 PlayerController의 Dash 코루틴에서 호출
    public void TriggerDash()
    {
        if (m_Animator == null) return;

        m_Animator.SetTrigger(m_HashDash);
    }

    //대시 상태 업데이트
    public void SetDashing(bool isDashing)
    {
        if (m_Animator == null) return;

        m_Animator.SetBool(m_HashIsDashing, isDashing);
    }
    //점프 시 상승,하강 속도가 다르니 그에 따른 수직 속도 업데이트
    public void SetVerticalVelocity(float vel)
    {
        if (m_Animator == null) return;

        m_Animator.SetFloat (m_HashVerticalVelocity, vel);
    }
}
