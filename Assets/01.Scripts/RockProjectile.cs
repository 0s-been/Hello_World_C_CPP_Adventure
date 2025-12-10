using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    //q스킬 암석 생성 변수
    private float m_Damage = 20f;
    private float m_Lifetime = 30f;
    private float m_RiseHeight = 0.8f;
    //솟아오르는 시간
    private float m_RiseDuration = 0.2f;

    //파편에 대한 변수들
    //파편의 날아가는 힘
    private float m_FragmentForce = 20f;
    //파편 퍼짐 정도
    private float m_FragmentSpread = 0.3f;
    //파편 지속 시간
    private float m_FragmentLifetime = 5f;

    //이 부분 더 공부 필요
    private HashSet<Collider> m_HitTargets = new HashSet<Collider>();
    private bool m_IsRising = false;
    private bool m_IsDestroyed = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, m_Lifetime);
        StartCoroutine(Rise());
    }

    // Update is called once per frame
    void Update()
    {

    }

    IEnumerator Rise()
    {
        m_IsRising = true;

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + Vector3.up * m_RiseHeight;
        float elapsed = 0f;

        while (elapsed < m_RiseDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / m_RiseDuration;
            // ease-out 효과 적용
            t = 1f - (1f - t) * (1f - t);

            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }
        transform.position = endPos;
        m_IsRising = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) return;
        if (m_HitTargets.Contains(other)) return;

        if (other.CompareTag("Enemy"))
        {      
            m_HitTargets.Add(other);
            Debug.Log($"암석이 {other.name}에게 {m_Damage}의 피해");
        }

        if (other.CompareTag("SlashProjectile"))
        {
            if(!m_IsDestroyed)
            {
                m_IsDestroyed = true;
                Vector3 slashDirection = other.transform.forward;
                Explode(slashDirection);
            }
        }
    }

    void Explode(Vector3 hitDirection)
    {
        //부모의 Collider 비활성화 -> 추가 충돌 방지
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        //모든 자식 파편 처리
        foreach (Transform child in transform)
        {
            //자식을 독립 오브젝트로 분리
            child.SetParent(null);

            //자식(파편)오브젝트들에 리지드바디와 메쉬콜라이더 추가 및 설정
            Rigidbody rb = child.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = child.gameObject.AddComponent<Rigidbody>();
            }
            rb.isKinematic = false;
            rb.useGravity = true;

            if (child.GetComponent<Collider>() == null)
            {
                MeshCollider mc = child.gameObject.AddComponent<MeshCollider>();
                mc.convex = true;
            }

            //검기의 방향과 특정 범위 내에 랜덤하게 퍼지도록 설정
            Vector3 spread = new Vector3(
                Random.Range(-m_FragmentSpread, m_FragmentSpread),
                Random.Range(-m_FragmentSpread, m_FragmentSpread),
                Random.Range(-m_FragmentSpread, m_FragmentSpread));
            //방향만 필요하니 정규화시킴
            Vector3 forceDirection = (hitDirection + spread).normalized;
            rb.AddForce(forceDirection * m_FragmentForce, ForceMode.Impulse);

            //파편 일정 시간 후 삭제
            Destroy(child.gameObject, m_FragmentLifetime);
        }

        // 부모 오브젝트 삭제
        Destroy(gameObject);
    }
}
