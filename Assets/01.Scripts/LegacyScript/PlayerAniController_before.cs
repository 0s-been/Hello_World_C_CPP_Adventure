using UnityEngine;

public class PlayerAniController_before: MonoBehaviour
//인터페이스가 없어서 player controller에서 직접 이 클래스의 함수를 호출하는 구조였습니다.
//그로 인해 서로 간 결합도가 매우 높고 playercontroller가 playeranicontroller의 내부 구현에 대해 너무 많이 알게 되는 구조였습니다.
//결합도가 높기에 한쪽에서 변경이 일어나면 다른 쪽도 영향을 받아서 유지보수가 어려워 OCP와 ISP 원칙을 위반하는 구조였습니다.
{
    public Animator m_Animator;

    private int m_HashSpeed = Animator.StringToHash("Speed");
    private int m_HashIsMoving = Animator.StringToHash("IsMoving");
    private int m_HashIsGrounded = Animator.StringToHash("IsGrounded");
    private int m_HashJump = Animator.StringToHash("Jump");
    private int m_HashDash = Animator.StringToHash("Dash");
    private int m_HashIsDashing = Animator.StringToHash("IsDashing");
    private int m_HashVerticalVelocity = Animator.StringToHash("VerticalVelocity");

    //skill 관련 해시
    private int m_HashSkillE = Animator.StringToHash("SkillE");
    private int m_HashSkillQ = Animator.StringToHash("SkillQ");

    //start 전에 실행되는 함수
    void Awake()
    {
        m_Animator = GetComponent<Animator>();
    }

    //현재 애니메이션의 정규화된 시간 (0~1) 반환
    public float GetNormalizedTime(int layer = 0)
    {
        if (m_Animator == null) return 0f;
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.normalizedTime % 1f;
    }

    //현재 특정 State가 재생 중인지 확인
    public bool IsPlayingState(string stateName, int layer = 0)
    {
        if (m_Animator == null) return false;
        AnimatorStateInfo stateInfo = m_Animator.GetCurrentAnimatorStateInfo(layer);
        return stateInfo.IsName(stateName);
    }

    //현재 애니메이션이 특정 % 이상 재생됐는지 확인
    public bool IsAnimationPast(float normalizedTime, int layer = 0)
    {
        return GetNormalizedTime(layer) >= normalizedTime;
    }

    //현재 애니메이션이 끝났는지 확인 (95% 이상)
    public bool IsAnimationFinished(int layer = 0)
    {
        return GetNormalizedTime(layer) >= 0.95f;
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

    //q,e 스킬 트리거
    public void TriggerSkillE()
    {
        if (m_Animator == null) return;
        m_Animator.SetTrigger(m_HashSkillE);
    }

    public void TriggerSkillQ()
    {
        if (m_Animator == null) return;
        m_Animator.SetTrigger(m_HashSkillQ);
    }
}
