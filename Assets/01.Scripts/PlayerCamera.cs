using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public Transform m_Target;
    public float m_Followspeed = 10;
    public float m_Msesensitivity = 100f;
    public float m_ClampAngle = 70f;

    private float m_rotX;
    private float m_rotY;

    public Transform m_RealCamera;
    public Vector3 m_Nomaldir;
    public Vector3 m_Finaldir;
    public float m_minDistance;
    public float m_maxDistance;
    public float m_FinalDistance;
    //카메라가 부드럽게 움직일 수 있도록 하는 보정값
    public float m_MoveSmooth = 10f;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_rotX = transform.localRotation.eulerAngles.x;
        m_rotY = transform.localRotation.eulerAngles.y;

        m_Nomaldir = m_RealCamera.localPosition.normalized;
        m_FinalDistance = m_RealCamera.localPosition.magnitude;

        //커서 가리기
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        //마우스를 상하로 움직일 때 카메라는 x축 기준 회전
        m_rotX -= Input.GetAxis("Mouse Y") * m_Msesensitivity * Time.deltaTime;
        //마우스를 좌우로 움직일 때 카메라는 y축 기준 회전
        m_rotY += Input.GetAxis("Mouse X") * m_Msesensitivity * Time.deltaTime;

        //회전 각도의 최소값과 최대값을 제한(clamp). 범위 밖의 값을 최소값과 최대값으로 제한. 
        m_rotX = Mathf.Clamp(m_rotX, -m_ClampAngle, m_ClampAngle);

        Quaternion rot = Quaternion.Euler(m_rotX, m_rotY, 0);
        transform.rotation = rot;
    }

    //Update가 끝난 후 호출되는 함수
    void LateUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position
            , m_Target.position, m_Followspeed * Time.deltaTime);

        //TransformPoint : 로컬좌표를 월드좌표로 바꿔줌
        m_Finaldir = transform.TransformPoint(m_Nomaldir * m_maxDistance);

        //카메라와 타겟 사이에 방해물을 감지하기 위한 변수
        RaycastHit hit;
        //사이에 방해물이 있을 경우
        if(Physics.Linecast(transform.position, m_Finaldir, out hit))
        {
            //방해물과의 거리를 최소거리와 최대거리 사이에서 제한한 값으로 변경 후 최종거리에 할당
            m_FinalDistance = Mathf.Clamp(hit.distance, m_minDistance, m_maxDistance);
        }
        else
        {
            //없을 경우엔 최대거리를 할당
            m_FinalDistance = m_maxDistance;
        }

        m_RealCamera.localPosition = Vector3.Lerp(m_RealCamera.localPosition, m_Nomaldir * m_FinalDistance, Time.deltaTime * m_MoveSmooth);
    }
}
