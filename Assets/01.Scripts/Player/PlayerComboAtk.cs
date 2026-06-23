using UnityEngine;


//https://www.youtube.com/watch?v=egnQTod1Vyk
//위 영상으로 공부한 후 작성한 코드입니다.
//MonoBehaviour를 상속해 유니티 Inspector창에 추가 가능
[System.Serializable]
//한 콤보공격에 대한 클래스
public class ComboStep 
{
    //애니메이션 이름
    public string m_StateName;
    //콤보가 유지될 시간
    public float m_MaxStepTime = 0.8f;
    //정규화된 시간으로 변경 (0~1, 애니메이션의 몇 % 지점)
    //애니메이션 60% 지점부터
    [Range(0f, 1f)]
    public float m_ComboStartNormalized = 0.6f;
    //애니메이션 90% 지점까지
    [Range(0f, 1f)]
    public float m_ComboEndNormalized = 0.9f;  
}

public class PlayerComboAtk : MonoBehaviour, IAttackState
{
    [SerializeField]
    //콤보를 배열로 만들어서 콤보공격을 구현
    ComboStep[] m_Steps;

    [SerializeField]
    //다음 콤보공격을 이어주기 위한 입력값을 저장해주는 시간
    float m_InputBufferTime = 0.2f;
    Animator m_Anim;

    //###### InputReader 컴포넌트를 통해 공격 입력을 받아와서 콤보 기능을 수행하도록 기능을 나누었습니다.
    //######SRP 원칙을 준수하도록 하였습니다.
    private InputReader m_InputReader;

    //현재 콤보 인덱스가 -1면 콤보공격 중이 아님
    public int currCombo = -1;
    //현재 실행 중인 콤보공격이 언제 시작했는지 알 저장할 변수
    //->경과 시간이 다음 콤보로 넘어갈 수 있는지 판단
    float comboStartTime;
    //다음 콤보로 넘어갈 수 있는 입력이 예약되었는지 확인하는 변수
    bool queuedNextCombo;
    //마지막 입력 시간을 저장할 변수
    //->마지막 입력이 다음 콤보로 넘어갈 수 있는 범위 판단하는데 사용
    float lastInputTime = -999f;
    //콤보 간격 시간
    float ComboInterval = 0.15f;

    //외부에서 공격 중인지 확인할 수 있는 프로퍼티 추가
    public bool IsAttacking => currCombo >= 0;

    void Start()
    {
        m_Anim = GetComponent<Animator>();
        m_InputReader = GetComponent<InputReader>();

        if (m_InputReader != null)
        {
            m_InputReader.OnAttackInput += TryAttack;
            InputBlocker.OnBlockChanged += OnInputBlockChanged;
        }
    }

    void OnDestroy()
    {
        if (m_InputReader != null)
        {
            m_InputReader.OnAttackInput -= TryAttack;
            InputBlocker.OnBlockChanged -= OnInputBlockChanged;
        }
    }
    
    private void OnInputBlockChanged(bool blocked)
    {
        if(blocked) CancelCombo();
    }

    void Update()
    {
        if(currCombo >= 0)
        {
            UpdateCombo();
        }
    }
    void TryAttack()
    {

        //미니게임 중 공격 차단
        if (InputBlocker.IsBlocked) return;

        //m_Steps배열이 비어있을 때 널참조를 막기 위한 예외처리문
        if (m_Steps == null || m_Steps.Length == 0)
        {
            Debug.Log("ComboStep 배열이 설정되지 않았습니다!");
            return;
        }

        if (currCombo < 0)
        {
            StartCombo(0);
            return;
        }

        //콤보 시작 직후에는 입력 무시 콤보 간격을 위헤
        float timeSinceComboStart = Time.time - comboStartTime;
        if(timeSinceComboStart < ComboInterval)
            return;

        var step = m_Steps[currCombo];

        //현재 애니메이션의 정규화된 시간 가져오기
        float normalizedTime = GetAniNomalizedTime();
    
        bool isInTiming = normalizedTime >= step.m_ComboStartNormalized &&
            normalizedTime <= step.m_ComboEndNormalized;
        if(isInTiming)
        {
            //Debug.Log("Next Combo Queued");
            queuedNextCombo = true;
            lastInputTime = Time.time;
        }
        else if(Time.time - lastInputTime <= m_InputBufferTime)
        {
            queuedNextCombo = true;
        }
    }

    void StartCombo(int index)
    {
        //Clamp로 index 범위를 제한
        currCombo = Mathf.Clamp(index, 0, m_Steps.Length - 1);
        comboStartTime = Time.time;
        queuedNextCombo = false;
        lastInputTime = Time.time;

        m_Anim.CrossFade(m_Steps[currCombo].m_StateName, 0.1f);
    }


    void UpdateCombo()
    {
        //현재 콤보 스텝의 정보를 가져와 저장
        var step = m_Steps[currCombo];

        //콤보 시작 직후에는 입력 무시 콤보 간격을 위헤
        float timeSinceComboStart = Time.time - comboStartTime;
        if (timeSinceComboStart < ComboInterval)
            return;

        //현재 애니메이션의 정규화된 시간 가져오기
        float normalizedTime = GetAniNomalizedTime();

        //다음 콤보가 예약되어 있고, 전환 가능 시점을 지났으면 다음 콤보 실행
        if (queuedNextCombo && normalizedTime >= step.m_ComboStartNormalized)
        {
            int next = currCombo + 1;
            if (next < m_Steps.Length)
                StartCombo(next);
            else
                ResetCombo();
            return;
        }

        //애니메이션이 거의 끝났으면 (95% 이상) 콤보 리셋
        if (normalizedTime >= 0.95f && !queuedNextCombo)
            ResetCombo();
    }

    //현재 재생 중인 애니메이션의 정규화된 시간 (0~1) 반환
    float GetAniNomalizedTime()
    {
        AnimatorStateInfo stateInfo = m_Anim.GetCurrentAnimatorStateInfo(0);
        //normalizedTime이 1을 넘으면 루프된 것이므로 소수점만 사용
        return stateInfo.normalizedTime % 1;
    }

    //콤보를 초기화하는 함수
    void ResetCombo()
    {
        currCombo = -1;
        queuedNextCombo = false;
    }

    //외부에서 강제로 콤보 취소 시 사용할 리셋 함수
    public void CancelCombo()
    {
        ResetCombo();
    }
}
