using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.VFX;


//e스킬 투사체 클래스 
public class SlashProjectile : MonoBehaviour
{
    private float m_Speed = 7f;
    private float m_Damage = 10f;
    private float m_Lifetime = 3f;
    //삭제 딜레이
    private float m_DestroyDelay = 0.4f;
    //삭제 중인 상태 여부
    private bool m_IsDestroying = false;

    private Vector3 m_dir;
    private HashSet<Collider> m_HitTargets = new HashSet<Collider>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, m_Lifetime);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += m_dir * m_Speed * Time.deltaTime;
    }

    //skillmanger에서 방향 설정용으로 호출
    public void SetDirection(Vector3 dir)
    {
        m_dir = dir.normalized;
        if(m_dir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(m_dir);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"충돌 감지: {other.name}, Tag: {other.tag}");

        if (m_HitTargets.Contains(other)) return;

        if (other.CompareTag("Player")) return;

        if (other.CompareTag("Enemy"))
        {
            m_HitTargets.Add(other);
            Debug.Log($"검기가 {other.name}에게 {m_Damage}의 피해를 입힘.");
        }

        if (other.CompareTag("Rock"))
        {
            //코루틴을 통해 딜레이가 지난 후 오브젝트가 삭제되도록 함
            if (!m_IsDestroying)
            {
                StartCoroutine(DestroyAfterDelay());
            }
        }
    }

    IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(m_DestroyDelay);
        Destroy(gameObject);
    }
}
