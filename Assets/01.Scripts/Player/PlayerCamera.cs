using UnityEngine;
using System;
//https://www.youtube.com/watch?v=4611qmBWTC0&t=142s
//위 영상을 통해 공부한 후 작성한 코드입니다.
public class PlayerCamera : MonoBehaviour
{
    //추적할 대상
    public Transform m_Target;
    //카메라 설정값들 추적속도,마우스 감도, 각도 제한
    public float m_Followspeed = 10;
    public float m_Msensitivity = 100f;
    public float m_ClampAngle = 70f;
    //카메라가 부드럽게 움직일 수 있도록 하는 보정값
    public float m_MoveSmooth = 10f;

    //카메라 회전 각도
    private float m_rotX;
    private float m_rotY;

    //연결할 카메라
    public Transform m_RealCamera;

    //기본 방향 벡터와 연산을 거친 최종결과방향벡터
    public Vector3 m_Normaldir;
    public Vector3 m_Finaldir;

    //카메라와 오브젝트 간의 최소최대거리와 연산을 거친 후 최종 적용할 거리
    public float m_minDistance;
    public float m_maxDistance;
    public float m_FinalDistance;

    //레이캐스트 사용 시 플레이어 오브젝트와의 충돌은 무시하기 위한 레이어마스크
    public LayerMask m_LayerMask;

    private InputReader m_InputReader;
    //private bool m_IsMiniGameActive = false;

    void Awake()
    {
        // InputReader 찾기
        if (m_Target != null)
        {
            m_InputReader = m_Target.GetComponent<InputReader>();
        }

        if (m_InputReader == null)
        {
            m_InputReader = FindAnyObjectByType<InputReader>();
        }

        InputBlocker.OnBlockChanged += OnInputBlockChanged;

        //else
        //Debug.LogError("씬에서 InputReader를 찾지 못함!");
    }

    void Start()
    {
        m_rotX = transform.localRotation.eulerAngles.x;
        m_rotY = transform.localRotation.eulerAngles.y;
        m_Normaldir = m_RealCamera.localPosition.normalized;
        m_FinalDistance = m_RealCamera.localPosition.magnitude;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDestroy()
    {
        InputBlocker.OnBlockChanged -= OnInputBlockChanged;
    }

    private void OnInputBlockChanged(bool blocked)
    {
        if (blocked)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            //현재 마우스 입력값 초기화
            //토글 순간 카메라 튀는 현상 방지
            m_rotX = transform.localRotation.eulerAngles.x;
            m_rotY = transform.localRotation.eulerAngles.y;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    // Update is called once per frame
    void Update()
    {

        // 미니게임 중 카메라 회전 차단
        if (InputBlocker.IsBlocked)
        {
            //Debug.Log("카메라 회전 차단 중");
            return;
        }


        //마우스를 상하로 움직일 때 카메라는 x축 기준 회전
        m_rotX -= Input.GetAxis("Mouse Y") * m_Msensitivity * Time.deltaTime;
        //마우스를 좌우로 움직일 때 카메라는 y축 기준 회전
        m_rotY += Input.GetAxis("Mouse X") * m_Msensitivity * Time.deltaTime;
    
        //회전 각도의 최소값과 최대값을 제한(clamp). 범위 밖의 값을 최소값과 최대값으로 제한. 
        m_rotX = Mathf.Clamp(m_rotX, -m_ClampAngle, m_ClampAngle);

        //회전 적용
        Quaternion rot = Quaternion.Euler(m_rotX, m_rotY, 0);
        transform.rotation = rot;
    }

    //Update가 끝난 후 호출되는 함수
    void LateUpdate()
    {
        //카메라와 타겟의 위치, 추적 속도를 통해 위치 변경
        transform.position = Vector3.MoveTowards(transform.position
            , m_Target.position, m_Followspeed * Time.fixedDeltaTime);

        //TransformPoint : 로컬좌표를 월드좌표로 바꿔줌
        m_Finaldir = transform.TransformPoint(m_Normaldir * m_maxDistance);

        //카메라와 타겟 사이에 방해물을 감지하기 위한 변수
        RaycastHit hit;
        //Vector3 RayStart = m_Target.position + m_Finaldir * 0.5f;
        //사이에 방해물이 있을 경우
        //[버그] 버그3 : 플레이어의 정면이 카메라를 향할 때 jump나 dash 사용 시 카메라가 줌인 됨
        //원인 -> raycast가 플레이어의 오브젝트와 충돌하여 발생
        //해결 -> Linecast를 Raycast로 변경하고 m_LayerMask추가하여 ground일 때만 감지하도록 변경
        if (Physics.Raycast(transform.position, m_Finaldir, out hit, m_FinalDistance, m_LayerMask))
        {
            //방해물과의 거리를 최소거리와 최대거리 사이에서 제한한 값으로 변경 후 최종거리에 할당
            m_FinalDistance = hit.distance;
        }
        else
        {
            //없을 경우엔 최대거리를 할당
            m_FinalDistance = m_maxDistance;
        }

        m_RealCamera.localPosition = Vector3.Lerp(m_RealCamera.localPosition, m_Normaldir * m_FinalDistance, Time.fixedDeltaTime * m_MoveSmooth);
    }
}
