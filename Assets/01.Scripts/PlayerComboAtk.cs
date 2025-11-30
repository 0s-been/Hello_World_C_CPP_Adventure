using UnityEngine;


//MonoBehaviour�� ��� ����Ƽ inspectorâ�� �߰� ����
[System.Serializable]
public class ComboStep 
{
    public string m_StateName;
    //�޺��� ����� �ð�
    public float m_MaxStepTime = 0.8f;
    //���� �޺��� �Ѿ �� �ִ� �ð� ����
    public float m_ComboStart = 0.3f;
    public float m_ComboEnd = 0.7f;
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
        if(currCombo < 0)
        {
            StartCombo(0);
            return;
        }
        var step = m_Steps[currCombo];
        float elapsed = Time.time - comboStartTime;
        bool isInTiming = elapsed >= step.m_ComboStart &&
            elapsed <= step.m_ComboEnd;
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
        //�޺��� ���۵� �ð��� ����
        comboStartTime = Time.time;
        //���� �޺��� �ڵ����� �Ѿ�� �ʵ��� false�� ����
        queuedNextCombo = false;
        lastInputTime = Time.time;

        m_Anim.CrossFade(m_Steps[currCombo].m_StateName, 0.05f);
    }


    void UpdateCombo()
    {
        //���� �޺� ������ ������ ���� ����
        var step = m_Steps[currCombo];
        //���� �޺��� ���۵ǰ� �󸶰� ���������� ������ ����
        float elapsed = Time.time - comboStartTime;

        //���� �޺��� ������ �Ǿ��ְ� ���� �޺��� �Ѿ �� �ִ� �ð����
        //�����ߴٸ� ���� �޺��� ����
        if(queuedNextCombo && elapsed >= step.m_ComboStart)
        {
            int next = currCombo + 1;
            if (next < m_Steps.Length)
                StartCombo(next);
            //next�� m_Steps�� ũ�⸦ �ʰ��ϸ� �޺��� �ʱ�ȭ
            else
                ResetCombo();

            return;
        }
        //��� �ð��� �޺��� ������ �ð����� Ŀ���ٸ� �޺� �ʱ�ȭ
        if(elapsed >= step.m_MaxStepTime)
            ResetCombo();
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
