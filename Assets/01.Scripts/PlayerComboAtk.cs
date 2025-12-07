using UnityEngine;


//MonoBehaviour�� ��� ����Ƽ inspectorâ�� �߰� ����
[System.Serializable]
public class ComboStep 
{
    public string m_StateName;
    //�޺��� ����� �ð�
    public float m_MaxStepTime = 0.8f;
    //정규화된 시간으로 변경 (0~1, 애니메이션의 몇 % 지점)
    [Range(0f, 1f)]
    public float m_ComboStartNormalized = 0.6f;//애니메이션 60% 지점부터
    [Range(0f, 1f)]
    public float m_ComboEndNormalized = 0.9f;  //애니메이션 90% 지점까지
}

public class PlayerComboAtk : MonoBehaviour
{
    [SerializeField]
    ComboStep[] m_Steps;

    [SerializeField]
    //���� �޺������� ���� �̾��ֱ� ���� �� �Է°��� �������ִ� �ð�
    float m_InputBufferTime = 0.2f;
    Animator m_Anim;

    //���� �޺� �ε����� -1�� �޺����� ���� x
    public int currCombo = -1;
    //���� ���� ���� �޺������� ���� �����ߴ� �� ������ ����
    //->���� �ð��� ���� �޺��� �Ѿ �� �ִ� �� �Ǵ�
    float comboStartTime;
    //���� �޺��� �Ѿ �� �ִ� ������ �����Ǿ����� Ȯ���ϴ� ����
    bool queuedNextCombo;
    //������ �Է� �ð��� ������ ����
    //������ �Է��� ���� �޺��� �Ѿ �� �ִ� ���� �Ǵ��ϴµ� ����
    float lastInputTime = -999f;
    //콤보 간격 시간
    float ComboInterval = 0.15f;

    //외부에서 공격 중인지 확인할 수 있는 프로퍼티 추가
    public bool IsAttacking => currCombo >= 0;

    void Start()
    {
        m_Anim = GetComponent<Animator>();
    }

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            TryAttack();       
        }

        if(currCombo >= 0)
        {
            UpdateCombo();
        }
    }
    void TryAttack()
    {
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
        //Clamp�� index ������ ����
        currCombo = Mathf.Clamp(index, 0, m_Steps.Length - 1);
        comboStartTime = Time.time;
        queuedNextCombo = false;
        lastInputTime = Time.time;

        m_Anim.CrossFade(m_Steps[currCombo].m_StateName, 0.1f);
    }


    void UpdateCombo()
    {
        //���� �޺� ������ ������ ���� ����
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

    //�޺��� �ʱ�ȭ�ϴ� �Լ�
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
