using UnityEngine;
using System;

// 입력 감지만 담당하는 컴포넌트
// 다른 클래스는 Input API를 직접 호출하지 않고 이 클래스의 이벤트를 구독한다
public class InputReader : MonoBehaviour
{
    // === 이벤트 정의 ===
    // Vector2: 수평(x), 수직(y) 입력값
    public event Action<Vector2> OnMoveInput;

    public event Action OnJumpInput;
    public event Action OnDashInput;
    public event Action OnAttackInput;

    // Q, E 스킬 입력
    public event Action OnSkillQInput;
    public event Action OnSkillEInput;

    //K키 스킬트리UI창 
    public event Action OnMiniGameInput;

    //I키 인벤토리
    public event Action OnInventoryInput;

    void Update()
    {
        // 이동 입력 — 매 프레임 발행 (0,0도 포함, 이동 중단 감지를 위해)
        float horz = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");
        OnMoveInput?.Invoke(new Vector2(horz, vert));

        // 단발성 입력 — GetButtonDown / GetKeyDown으로 이벤트 발행
        if (Input.GetButtonDown("Jump"))
            OnJumpInput?.Invoke();

        if (Input.GetButtonDown("Dash"))
            OnDashInput?.Invoke();

        if (Input.GetMouseButtonDown(0))
            OnAttackInput?.Invoke();

        if (Input.GetKeyDown(KeyCode.Q))
            OnSkillQInput?.Invoke();

        if (Input.GetKeyDown(KeyCode.E))
            OnSkillEInput?.Invoke();
        if (Input.GetKeyDown(KeyCode.K))
        {
            //Debug.Log("K키 입력 감지");
            OnMiniGameInput?.Invoke();
        }

        if(Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("I키 입력 감지");
            OnInventoryInput?.Invoke();
        }
    }
}
