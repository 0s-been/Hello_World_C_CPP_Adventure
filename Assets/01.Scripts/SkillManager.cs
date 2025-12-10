using MaykerStudio.Demo;
using System.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    //e스킬 관련 정보
    [SerializeField] private GameObject m_Slashprefab;
    [SerializeField] private Transform m_Spawnpoint;
    private float m_SpawnOffset = 1f;
    [SerializeField]
    private float m_SpawnHeight = 1.5f;
    [SerializeField] private float m_SkillECooltime = 5f;
    [SerializeField] private float m_Skill_E_timer = 0f;
    [SerializeField] private bool m_IsUsingSkill_E = false;
    private string m_Skill_E_Name = "E_skill_sword_wave";

    //q스킬 관련 정보
    [SerializeField] private GameObject m_RockPrefab;
    [SerializeField] private float m_SkillQcooltime = 10f;
    private int m_RockCount = 4;
    //바위 간격
    private float m_RockSpaicing = 2.0f;
    private float m_RockSpawnDelay = 0.2f;
    //첫 생성 바위와 플레이어 사이의 간격
    private float m_RockStartDistance = 1f;
    private float m_Skill_Q_timer = 0f;
    private bool m_IsUsingSkill_Q = false;
    private string m_Skill_Q_Name = "Q_skill_strike_wave";
    //첫 바위의 기본 크기
    private float m_RockStartScale = 2f;
    //다음 바위의 크기 증가량
    private float m_RockScaleIncrease = 0.6f;
    //바위 생성 시 랜덤 회전 범위
    //연달아 생성 되는 바위가 더 자연스럽도록 하기 위해 랜덤 회전을 추가함
    private float m_RockRandomRotationY = 90f;
    private float m_RockRandomRotationXZ = 30f;

    //이 getter 더 공부 필요
    public bool IsUsingSkill => m_IsUsingSkill_E || m_IsUsingSkill_Q;

    private PlayerAniController m_PlayerAniController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_PlayerAniController = GetComponent<PlayerAniController>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateCooldowns();

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryUseSkillE();
        }
        if (Input.GetKeyDown(KeyCode.Q))
        {
            TryUseSkillQ();
        }
        if (m_IsUsingSkill_E)
        {
            UpdateSkillE();
        }
        if (m_IsUsingSkill_Q)
        {
            UpdateSkillQ();
        }
    }

    void UpdateCooldowns()
    {
        if (m_Skill_E_timer > 0)
        {
            m_Skill_E_timer -= Time.deltaTime;
        }
        if (m_Skill_Q_timer > 0)
        {
            m_Skill_Q_timer -= Time.deltaTime;
        }
    }

    //--------------------E스킬 함수들-------------------//
    void TryUseSkillE()
    {
        //스킬이 쿨이거나 사용 중이면 사용 불가
        if (m_Skill_E_timer > 0 || m_IsUsingSkill_E) return;

        m_IsUsingSkill_E = true;
        m_Skill_E_timer = m_SkillECooltime;

        //nullable로 안정성 검사
        m_PlayerAniController?.TriggerSkillE();
    }

    void UpdateSkillE()
    {
        if (m_PlayerAniController == null) return;

        

        //IsPlayingState,IsAnimationFinished 함수 좀 더 공부해보기
        if (!m_PlayerAniController.IsPlayingState(m_Skill_E_Name))
        {
            return;
        }
        if (m_PlayerAniController.IsAnimationFinished())
        {
            m_IsUsingSkill_E = false;
        }
    }

    public void OnFireSkillE()
    {
        if (m_Slashprefab == null)
        {
            Debug.Log("m_Slashprefab 미설정");
            return;
        }
        Vector3 spawnpos;
        if (m_Spawnpoint != null)
        {
            spawnpos = m_Spawnpoint.position + Vector3.up * m_SpawnHeight;
        }
        else
        {
            spawnpos = transform.position + transform.forward * m_SpawnOffset + Vector3.up * m_SpawnHeight;
        }

        GameObject slash = Instantiate(m_Slashprefab, spawnpos, Quaternion.identity);
        SlashProjectile projectile = slash.GetComponent<SlashProjectile>();
        projectile?.SetDirection(transform.forward);
    }


    //--------------------Q스킬 함수들-------------------//
    void TryUseSkillQ()
    {
        //스킬이 쿨이거나 사용 중이면 사용 불가
        if (m_Skill_Q_timer > 0 || m_IsUsingSkill_Q) return;

        m_IsUsingSkill_Q = true;
        m_Skill_Q_timer = m_SkillQcooltime;
        //nullable로 안정성 검사
        m_PlayerAniController?.TriggerSkillQ();
    }

    void UpdateSkillQ()
    {
        if (m_PlayerAniController == null) return;
        //IsPlayingState,IsAnimationFinished 함수 좀 더 공부해보기
        if (!m_PlayerAniController.IsPlayingState(m_Skill_Q_Name))
        {
            return;
        }
        if (m_PlayerAniController.IsAnimationFinished())
        {
            m_IsUsingSkill_Q = false;
        }
    }

    public void OnFireSkillQ()
    {
        if (m_RockPrefab == null)
        {
            Debug.Log("m_RockPrefab 미설정");
            return;
        }
        //바위 생성 코루틴 시작
        StartCoroutine(SpawnRocks());
    }

    IEnumerator SpawnRocks()
    {
        Vector3 forward = transform.forward;
        Vector3 startpos = transform.position;

        for(int i = 0; i < m_RockCount; i++)
        {
            //바위 위치는 플레이어 정면으로 일렬
            float distance = m_RockStartDistance + (i * m_RockSpaicing);
            Vector3 spawnPos =startpos + forward * distance;
            //땅 밑에서 시작하도록 초기 y값 설정
            spawnPos.y = startpos.y - 1.5f;

            //랜덤 회전
            float randX = Random.Range(-m_RockRandomRotationXZ, m_RockRandomRotationXZ);
            float randZ = Random.Range(-m_RockRandomRotationXZ, m_RockRandomRotationXZ);
            float randY = Random.Range(-m_RockRandomRotationY, m_RockRandomRotationY);
            Quaternion randomRot = Quaternion.Euler(randX, randY, randZ);

            //바위 생성
            GameObject rock =Instantiate(m_RockPrefab, spawnPos, randomRot);

            //크기 증가
            float scale = m_RockStartScale + (i * m_RockScaleIncrease);
            rock.transform.localScale = Vector3.one * scale;

            //다음 바위 생성 전 대기
            yield return new WaitForSeconds(m_RockSpawnDelay);
        }
    }
}


