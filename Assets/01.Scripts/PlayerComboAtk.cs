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
    float lastInputTime;

    void Start()
    {
        m_Anim = GetComponent<Animator>();
    }

    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            lastInputTime = Time.time;
            StartComboAtk();
        }

        if(currCombo >= 0)
        {
            UpdateCombo();
        }
    }
    void StartComboAtk()
    {
        //������ ���� ���� ���� �޺� ����
        if(currCombo < 0)
        {
            StartCombo(0);
            return;
        }
         
        //���� ����� �޺������� ������ ������ ����
        var step = m_Steps[currCombo];
        //�޺��� ���۵ǰ� �󸶳� �������� ������ ����
        float elapsed = Time.time - comboStartTime;

        //elapsed�� �޺�Ÿ�̹� �ȿ� �ִٸ� true
        bool isInTiming = elapsed >= step.m_ComboStart &&
            elapsed <= step.m_ComboEnd;
        //lastInputTime�� ����Ÿ�� ���� �ִٸ� true
        bool withinBuffer = Time.time - lastInputTime <= m_InputBufferTime;

        //�� �� ������ �����ȴٸ� ���� �޺��� �Ѿ�� ���� ���� ����
        if(isInTiming && withinBuffer)
            queuedNextCombo = true;
    }

    void StartCombo(int index)
    {
        //Clamp�� index ������ ����
        currCombo = Mathf.Clamp(index, 0, m_Steps.Length - 1);
        //�޺��� ���۵� �ð��� ����
        comboStartTime = Time.time;
        //���� �޺��� �ڵ����� �Ѿ�� �ʵ��� false�� ����
        queuedNextCombo = false;

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
}
