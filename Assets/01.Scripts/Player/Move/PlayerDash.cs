using UnityEngine;
using System;
using System.Collections;

// 대시 실행·쿨타임·감속 처리만 담당
public class PlayerDash : MonoBehaviour
{
    [SerializeField] private float m_DashDistance = 5f;
    [SerializeField] private float m_DashDuration = 0.7f;
    [SerializeField] private float m_Cooldown     = 0f;

    // 대시 종료 시 PlayerController가 구독해서 상태 해제
    public event Action OnDashEnd;

    public bool IsDashing { get; private set; } = false;

    private float               m_CooldownTimer;
    private Rigidbody           m_Rigidbody;
    // concrete 타입 대신 인터페이스만 참조 — I 원칙 적용
    private ICombatAnimatable   m_AnimController;

    void Awake()
    {
        m_Rigidbody      = GetComponent<Rigidbody>();
        // PlayerAniController는 자식 모델 오브젝트에 붙어있으므로 InChildren으로 탐색
        m_AnimController = GetComponentInChildren<PlayerAniController>();
    }

    void Update()
    {
        if (m_CooldownTimer > 0)
            m_CooldownTimer -= Time.deltaTime;
    }

    public bool CanDash => !IsDashing && m_CooldownTimer <= 0;

    // PlayerController가 조건 확인 후 호출
    public void TryDash()
    {
        if (!CanDash) return;
        StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        IsDashing       = true;
        m_CooldownTimer = m_Cooldown;

        m_AnimController?.TriggerDash();
        m_AnimController?.SetDashing(true);

        // 수평 속도만 초기화, y는 유지 (점프 중 대시 대응)
        m_Rigidbody.linearVelocity = new Vector3(0, m_Rigidbody.linearVelocity.y, 0);

        Vector3 dashDir   = transform.forward;
        float   dashSpeed = m_DashDistance * 10f;

        m_Rigidbody.linearVelocity = new Vector3(
            dashDir.x * dashSpeed, m_Rigidbody.linearVelocity.y, dashDir.z * dashSpeed);

        float originalDamping      = m_Rigidbody.linearDamping;
        m_Rigidbody.linearDamping  = 10f;

        float elapsed = 0f;
        while (elapsed < m_DashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / m_DashDuration;

            Vector3 curr = new Vector3(dashDir.x * dashSpeed, m_Rigidbody.linearVelocity.y, dashDir.z * dashSpeed);
            Vector3 end  = new Vector3(0f, m_Rigidbody.linearVelocity.y, 0f);

            // t² — 초반 빠름, 후반 급감속
            m_Rigidbody.linearVelocity = Vector3.Lerp(curr, end, t * t);
            yield return null;
        }

        // 잔여 속도 제거 — 다음 이동 입력에 간섭 방지
        m_Rigidbody.linearVelocity = new Vector3(
            m_Rigidbody.linearVelocity.x * 0.2f,
            m_Rigidbody.linearVelocity.y,
            m_Rigidbody.linearVelocity.z * 0.2f);

        m_Rigidbody.linearDamping = originalDamping;
        IsDashing = false;

        m_AnimController?.SetDashing(false);

        // 대시 종료 이벤트 발행
        OnDashEnd?.Invoke();
    }
}
