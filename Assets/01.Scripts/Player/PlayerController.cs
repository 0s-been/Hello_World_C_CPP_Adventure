using UnityEngine;

// ##추가된 주석은 ##으로 표시했습니다.
//########################################################
// 기존 master클래스였던 playercontroller를 플레이어의 상태를 관리하는 클래스이자
// 여러 컴포넌트를 저장하는 일종의 컨테이너 클래스로 리팩토링했습니다만
// 더 깊이 들어가면 playerstate라는 클래스로 상태 관리 부분을 분리할 수 있을 것 같습니다.
// 실제 로직은 PlayerMover·PlayerJump·PlayerDash·PlayerAnimationBridge에 위임하여 기능 분리 뿐만 아니라 유지보수에도 용이하도록 변경하였습니다
// SRP, OCP, ISP 원칙을 적용하여 각 컴포넌트가 자신의 책임에 집중하도록 했습니다.
//########################################################
public class PlayerController : MonoBehaviour
{
    // 지면 판별 레이어마스크
    [SerializeField] private LayerMask m_GroundLayer;

    //######상태에 대한 변수들을 프로퍼티로 변경하여 변수를 보호하였습니다.
    public bool    IsGrounded { get; private set; }
    public bool    IsMoving   { get; private set; }
    public Vector3 MoveDir    { get; private set; }

    //######분리된 기능들을 담당하는 컴포넌트들을 참조하는 변수들
    //######ex) 입력 처리, 이동, 점프, 대시, 애니메이션, 콤보 공격등
    ///######playercontroller는 이 컴포넌트들의 인터페이스를 통해 기능을 사용하도록 하였습니다.
    //######이 부분들을 전부 나누도록 설계는 했지만 구현까지 하려면 너무 많은 작업량이 생길 것 같아서
    //######설계만 하였습니다..(PlayerMover, PlayerJump,  PlayerDash)
    private InputReader           m_InputReader;
    private PlayerMover           m_Mover;
    private PlayerJump            m_Jump;
    private PlayerDash            m_Dash;
    private PlayerAnimationBridge m_AnimBridge;
    private IAttackState          m_ComboAtk;
    private Transform             m_CameraTrans;

    //######언리얼의 이벤트디스패처는 써봤지만 유니티의 이벤트 시스템은 처음이라 이 부분은 ai의 도움을 받았습니다..
    //#####이벤트 처리에 필요한 변수들입니다.
    private Vector2 m_RawMoveInput;
    private bool    m_JumpRequested;
    private bool    m_DashRequested;

    private bool m_IsMiniGameActive = false;
    private void OnMoveInput(Vector2 input) => m_RawMoveInput = input;
    private void OnJumpInput() => m_JumpRequested = true;
    private void OnDashInput() => m_DashRequested = true;

    private void OnMiniGameInput()
    {
        m_IsMiniGameActive = !m_IsMiniGameActive;

        // 미니게임 진입 시 입력 초기화
        if (m_IsMiniGameActive)
        {
            m_RawMoveInput = Vector2.zero;
            m_JumpRequested = false;
            m_DashRequested = false;
        }
    }
    void Awake()
    {
        m_InputReader = GetComponent<InputReader>();
        m_Mover       = GetComponent<PlayerMover>();
        m_Jump        = GetComponent<PlayerJump>();
        m_Dash        = GetComponent<PlayerDash>();
        m_AnimBridge  = GetComponent<PlayerAnimationBridge>();
        m_ComboAtk = GetComponent<PlayerComboAtk>();
    }

    void Start()
    {
        // 카메라 참조
        Camera mainCam = Camera.main;
        if (mainCam != null)
            m_CameraTrans = mainCam.transform.parent;

        // InputReader를 통해 여기선 입력 감지만 하고 실제 로직 처리는 각 컴포넌트에서 하도록 변경하여 srp 원칙을 적용하였습니다.
        if (m_InputReader != null)
        {
            m_InputReader.OnMoveInput += OnMoveInput;
            m_InputReader.OnJumpInput += OnJumpInput;
            m_InputReader.OnDashInput += OnDashInput;
            m_InputReader.OnMiniGameInput += OnMiniGameInput;
        }
    }

    void OnDestroy()
    {
        if (m_InputReader != null)
        {
            m_InputReader.OnMoveInput -= OnMoveInput;
            m_InputReader.OnJumpInput -= OnJumpInput;
            m_InputReader.OnDashInput -= OnDashInput;
            m_InputReader.OnMiniGameInput -= OnMiniGameInput;
        }
    }


    //######기존 update에서 모든 처리를 담당했지만 기능들을 분리하였기에
    //######리팩토링한 update에서는 상태 체크와 애니메이션 브릿지의 Tick만 담당하도록 변경하였습니다. -> srp 원칙 적용
    void Update()
    {
        //if (m_IsMiniGameActive) return;
        if (InputBlocker.IsBlocked) return;

        CheckGrounded();
        m_AnimBridge?.Tick();
    }

    //######코루틴으로 처리하던 점프와 대시 요청을 플래그로 처리하여 FixedUpdate에서 소비하도록 변경하였습니다.
    //이벤트와 플래그 부분은 ai의 도움을 받아 구현하였습니다...
    void FixedUpdate()
    {
        //if (m_IsMiniGameActive) return;
        if (InputBlocker.IsBlocked) return;

        if (!m_Dash.IsDashing)
        {
            // 이동 방향 계산 후 Mover에 전달
            UpdateMoveDir();
            m_Mover?.Move(MoveDir);
            m_Jump?.ApplyJumpGravity();
        }

        // 점프 요청 소비
        if (m_JumpRequested)
        {
            m_JumpRequested = false;
            if (IsGrounded && !m_Dash.IsDashing)
                m_Jump?.Jump();
        }

        // 대시 요청 소비
        if (m_DashRequested)
        {
            m_DashRequested = false;
            m_Dash?.TryDash();
        }
    }

    private void UpdateMoveDir()
    {
        // 대시 중이거나 공격 중이면 이동 방향 갱신 중단
        if (m_Dash.IsDashing || (m_ComboAtk != null && m_ComboAtk.IsAttacking))
        {
            MoveDir  = Vector3.zero;
            IsMoving = false;
            return;
        }

        float horz = m_RawMoveInput.x;
        float vert = m_RawMoveInput.y;

        if (m_CameraTrans == null)
        {
            MoveDir  = new Vector3(horz, 0f, vert).normalized;
            IsMoving = MoveDir != Vector3.zero;
            return;
        }

        Vector3 camForward = m_CameraTrans.forward; camForward.y = 0f; camForward.Normalize();
        Vector3 camRight   = m_CameraTrans.right;   camRight.y   = 0f; camRight.Normalize();

        MoveDir  = (camForward * vert + camRight * horz).normalized;
        IsMoving = MoveDir != Vector3.zero;
    }

    private void CheckGrounded()
    {
        Vector3 rayStart = transform.position + Vector3.up * 0.2f;
        IsGrounded = Physics.Raycast(rayStart, Vector3.down, 1.0f, m_GroundLayer);
    }
}
