using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RockProjectile : MonoBehaviour
{
    //q스킬 암석 생성 변수
    private float m_Damage = 20f;
    private float m_Lifetime = 30f;
    private float m_RiseHeight = 2f;
    //솟아오르는 시간
    private float m_RiseDuration = 0.2f;

    //이 부분 더 공부 필요
    private HashSet<Collider> m_HitTargets = new HashSet<Collider>();
    private bool m_IsRising = false;

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
            Destroy(gameObject);
        }
    }
}
