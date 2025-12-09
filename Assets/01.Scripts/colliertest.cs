using UnityEngine;

public class colliertest : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Rock이 충돌 감지: {other.name}");
    }

    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"Rock이 Collision 감지: {collision.gameObject.name}");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
