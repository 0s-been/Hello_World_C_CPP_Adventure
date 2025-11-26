using UnityEngine;


//MonoBehaviour가 없어도 유니티 inspector창에 뜨게 해줌
[System.Serializable]
public class ComboStep 
{
    public string m_StateName;
    //콤보가 종료될 시간
    public float m_MaxStepTime = 0.8f;
    //다음 콤보로 넘어갈 수 있는 시간 범위
    public float m_ComboStart = 0.3f;
    public float m_ComboEnd = 0.7f;
}

public class PlayerComboAtk : MonoBehaviour
{
    [SerializeField]
    ComboStep[] m_Steps;

    [SerializeField]
    //다음 콤보공격을 쉽게 이어주기 위해 선 입력값을 감지해주는 시간
    float m_InputBufferTime = 0.2f;
    Animator m_Anim;

    //현재 콤보 인덱스로 -1은 콤보공격 시작 x
    int currCombo = -1;
    //현재 진행 중인 콤보공격이 언제 시작했는 지 저장할 변수
    //->지금 시간이 다음 콤보로 넘어갈 수 있는 지 판단
    float comboStartTime;
    //다음 콤보로 넘어갈 수 있는 조건이 충족되었는지 확인하는 변수
    bool queuedNextCombo;
    //마지막 입력 시간을 저장할 변수
    //마지막 입력이 다음 콤보로 넘어갈 수 있는 지를 판단하는데 쓰임
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
        //공격을 하지 않은 상태 콤보 시작
        if(currCombo < 0)
        {
            StartCombo(0);
            return;
        }
         
        //현재 실행된 콤보공격의 정보를 저장할 변수
        var step = m_Steps[currCombo];
        //콤보가 시작되고 얼마나 지났는지 저장할 변수
        float elapsed = Time.time - comboStartTime;

        //elapsed가 콤보타이밍 안에 있다면 true
        bool isInTiming = elapsed >= step.m_ComboStart &&
            elapsed <= step.m_ComboEnd;
        //lastInputTime이 버퍼타임 내에 있다면 true
        bool withinBuffer = Time.time - lastInputTime <= m_InputBufferTime;

        //위 두 조건이 충족된다면 다음 콤보로 넘어가기 위한 조건 충족
        if(isInTiming && withinBuffer)
            queuedNextCombo = true;
    }

    void StartCombo(int index)
    {
        //Clamp로 index 범위를 제한
        currCombo = Mathf.Clamp(index, 0, m_Steps.Length - 1);
        //콤보가 시작된 시간을 저장
        comboStartTime = Time.time;
        //다음 콤보로 자동으로 넘어가지 않도록 false로 차단
        queuedNextCombo = false;

        m_Anim.CrossFade(m_Steps[currCombo].m_StateName, 0.05f);
    }


    void UpdateCombo()
    {
        //현재 콤보 공격의 정보를 담을 변수
        var step = m_Steps[currCombo];
        //현재 콤보가 시작되고 얼마가 지났는지를 저장할 변수
        float elapsed = Time.time - comboStartTime;

        //다음 콤보가 예약이 되어있고 다음 콤보로 넘어갈 수 있는 시간대로
        //진입했다면 다음 콤보로 진입
        if(queuedNextCombo && elapsed >= step.m_ComboStart)
        {
            int next = currCombo + 1;
            if (next < m_Steps.Length)
                StartCombo(next);
            //next가 m_Steps의 크기를 초과하면 콤보를 초기화
            else
                ResetCombo();

            return;
        }
        //경과 시간이 콤보의 마지막 시간보다 커졌다면 콤보 초기화
        if(elapsed >= step.m_MaxStepTime)
            ResetCombo();
    }

    //콤보를 초기화하는 함수
    void ResetCombo()
    {
        currCombo = -1;
        queuedNextCombo = false;
    }
}
