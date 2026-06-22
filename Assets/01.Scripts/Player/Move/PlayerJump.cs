using UnityEngine;

// 점프 실행과 점프 중력 보정만 담당
public class PlayerJump : MonoBehaviour
{
    [SerializeField] private float m_JumpHeight       = 12f;
    [SerializeField] private float m_FallGravityMult  = 5f;  // 하강 중력 배율
    [SerializeField] private float m_RiseGravityMult  = 2f;  // 상승 중력 배율

    private Rigidbody           m_Rigidbody;
    // concrete 타입 대신 인터페이스만 참조 — I 원칙 적용
    private ICombatAnimatable   m_AnimController;

    void Awake()
    {
        m_Rigidbody      = GetComponent<Rigidbody>();
        // PlayerAniController는 자식 모델 오브젝트에 붙어있으므로 InChildren으로 탐색
        m_AnimController = GetComponentInChildren<PlayerAniController>();
    }

    // PlayerController가 FixedUpdate에서 조건 충족 시 호출
    public void Jump()
    {
        m_Rigidbody.AddForce(Vector3.up * m_JumpHeight, ForceMode.VelocityChange);
        m_AnimController?.TriggerJump();
    }

    // 점프 중 중력 보정 — 대시 중이 아닐 때 FixedUpdate마다 호출
    public void ApplyJumpGravity()
    {
        float vy = m_Rigidbody.linearVelocity.y;

        if (vy < 0)
            // 하강: 중력을 강하게 → 빠른 낙하감
            m_Rigidbody.AddForce(Physics.gravity * m_FallGravityMult, ForceMode.Acceleration);
        else if (vy > 0)
            // 상승: 중력을 약하게 → 자연스러운 포물선
            m_Rigidbody.AddForce(Physics.gravity * m_RiseGravityMult, ForceMode.Acceleration);
    }
}
