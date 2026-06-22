using UnityEngine;

// 이동·회전 물리 처리만 담당
// Rigidbody 조작은 이 컴포넌트에만 존재해야 한다 (점프·대시 제외)
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float m_MoveSpeed = 10f;
    [SerializeField] private float m_RotSpeed  = 30f;

    private Rigidbody m_Rigidbody;

    void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
    }

    // PlayerController가 FixedUpdate에서 호출
    // moveDir: 카메라 기준으로 변환된 월드 방향 벡터 (크기 1 or Zero)
    public void Move(Vector3 moveDir)
    {
        if (moveDir == Vector3.zero) return;

        // 부드러운 회전
        Quaternion targetRot = Quaternion.LookRotation(moveDir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, targetRot, Time.fixedDeltaTime * m_RotSpeed);

        // 위치 이동 — transform.position 직접 수정 대신 MovePosition 사용
        Vector3 targetPos = transform.position + moveDir * m_MoveSpeed * Time.fixedDeltaTime;
        m_Rigidbody.MovePosition(targetPos);
    }
}
