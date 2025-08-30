using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// 발사체 사정거리 클래스(MonBehaviour)
public class ShootStingerc : MonoBehaviour
{
    private Vector3 startPosition; // 발사체의 시작 위치
    public float maxDistance = 10.0f; // 발사체가 사라지는 최대 거리

    void Start()
    {
        // 발사체 생성 시 현재 위치를 시작 위치로 저장
        startPosition = transform.position;
    }

    void Update()
    {
        // 발사체의 현재 위치와 시작 위치 사이의 거리를 계산
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);

        // 거리가 최대 사정거리를 초과하면 발사체를 파괴
        if (distanceTraveled > maxDistance)
        {
            Destroy(gameObject);
        }
    }
    // 오브젝트 삭제되게 하는 스크립트
    void OnTriggerEnter2D(Collider2D collision)
    {
        // 상대방 태그가 "Player1" 또는 "Player2"일 때만 처리
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            if (collision.tag != gameObject.tag) // 자신과 다른 태그만 처리
            {
                // 충돌 후 발사체 파괴
                Destroy(gameObject);
            }
        }
    }
}

public class BeeController : MonoBehaviour
{
    Rigidbody2D rigid2D;
    Animator animator;
    float jumpForce = 700.0f;
    float walkForce = 30.0f;
    float maxWalkSpeed = 4.0f;

    float dashForce = 15.0f; // 대시할 때의 힘
    float dashDuration = 0.1f; // 대시 지속 시간
    bool isDashing = false; // 대시 중인지 확인

    float doubleTapTimeD = 0.0f; // D키를 두 번 누르는 시간 감지
    float doubleTapTimeA = 0.0f; // A키를 두 번 누르는 시간 감지
    float doubleTapDelay = 0.3f; // 두 번 누르기 감지 시간

    int maxjumpcnt = 3;
    [SerializeField] int jumpcnt = 0;

    public GameObject stingerPrefab;

    float shieldDuration = 1.0f;
    public GameObject shieldPrefab;

    // 레이저 관련 변수
    public GameObject laserPrefab; // 레이저 프리팹
    GameObject currentLaser; // 현재 활성화된 레이저를 저장
    bool isFrozen = false; // 움직임 제한 플래그
    Vector2 frozenPosition; // 움직임 제한 시 고정 위치 저장
    bool laserCanDamage = false; // 레이저로 데미지를 줄 수 있는 상태인지


    // 쿨타임 관련 변수
    [SerializeField] private float attackUCooldown = 2f;
    [SerializeField] private float attackICooldown = 2f;
    [SerializeField] private float ShieldCooldown = 5f;

    private float lastattackUTime;
    private float lastattackITime;
    private float lastShieldTime;

    private bool isFirstAttackU = true;
    private bool isFirstAttackI = true;
    private bool isFirstShield = true;

    private bool laserAttackUsed = false; // 레이저 공격 사용 여부를 추적하는 변수


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;   // 60프레임 고정
        this.rigid2D = GetComponent<Rigidbody2D>(); // rigid2D 객체
        this.animator = GetComponent<Animator>();       // 애니메이터
                                                        // 쿨타임 변수 초기화
        lastattackUTime = 0;
        lastattackITime = 0;
        lastShieldTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // D 키 대시 입력 감지
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - doubleTapTimeD < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.right)); // 오른쪽 대시
            }
            doubleTapTimeD = Time.time;
        }

        // A 키 대시 입력 감지
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time - doubleTapTimeA < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.left)); // 왼쪽 대시
            }
            doubleTapTimeA = Time.time;
        }

        // 점프
        if (Input.GetKeyDown(KeyCode.W) && jumpcnt < maxjumpcnt)
        {
            jumpcnt++;
            this.rigid2D.AddForce(Vector2.up * jumpForce);
            this.rigid2D.velocity = new Vector2(rigid2D.velocity.x, 0.1f); // 기존 y속도를 초기화해서 점프감을 더 잘 느끼게
        }

        if (jumpcnt == maxjumpcnt && rigid2D.velocity.y == 0) jumpcnt = 0; // 땅에 착지하면 점프 카운트를 다시 0으로 초기화

        // 원거리 공격(개미, 벌 유동적으로)
        if (Input.GetKeyDown(KeyCode.K))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootStinger(0f, 0f, 10f);
        }

        // 강화 원거리 공격(개미, 벌 유동적으로)
        if (Input.GetKeyDown(KeyCode.I) && (isFirstAttackI || Time.time - lastattackITime > attackICooldown))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootStinger(-0.2f, -0.1f, 7.5f);
            ShootStinger(0f, 0f, 7.5f);
            ShootStinger(-0.1f, 0.1f, 7.5f);
            lastattackITime = Time.time;
            isFirstAttackI = false;
        }

        // 근접 공격
        if (Input.GetKeyDown(KeyCode.J))
        {
            this.animator.SetTrigger("AttackTrigger");
            PerformMeleeAttack(); // 근접 공격 실행
        }

        // 강화 근접 공격
        if (Input.GetKeyDown(KeyCode.U) && (isFirstAttackU || Time.time - lastattackUTime > attackUCooldown))
        {
            StartCoroutine(DashAndAttack());
            lastattackUTime = Time.time;
            isFirstAttackU = false;
        }

        // L키 입력 감지 및 레이저 공격 사용 여부 확인
        if (Input.GetKeyDown(KeyCode.L) && !isFrozen && !laserAttackUsed)
        {
            StartCoroutine(PerformLaserAttack());
            laserAttackUsed = true; // 레이저 공격 사용 표시
        }

        if (isFrozen)
        {
            // 움직임 제한 시 위치 고정
            rigid2D.velocity = Vector2.zero; // 속도 정지
            rigid2D.position = frozenPosition; // 고정된 위치로 설정
            return; // 이동 처리 종료
        }

        //쉴드
        if (Input.GetKeyDown(KeyCode.S) && (isFirstShield || Time.time - lastShieldTime > ShieldCooldown))
        {
            CreateShieldEffects();
            lastShieldTime = Time.time;
            isFirstShield = false;
        }

        // 좌우 이동 (대시 중이 아닐 때만)
        if (!isDashing)
        {
            float key = 0f;
            if (Input.GetKey(KeyCode.D))
            {
                this.animator.SetTrigger("WalkTrigger");
                key = 1.0f; // 오른쪽 이동
            }
            if (Input.GetKey(KeyCode.A))
            {
                this.animator.SetTrigger("WalkTrigger");
                key = -1.0f; // 왼쪽 이동
            }
            // 플레이어 속도
            float speedx = Mathf.Abs(this.rigid2D.velocity.x);

            // 스피드 제한
            if (speedx < this.maxWalkSpeed)
            {
                this.rigid2D.AddForce(transform.right * key * this.walkForce);
            }

            if (key != 0)
            {
                float direction = key > 0 ? 1 : -1;
                transform.localScale = new Vector3(0.7f * direction, 0.7f, 0.7f);
            }

            // 플레이어 속도에 맞춰 애니메이션 속도를 바꾼다.
            if (speedx != 0)
            {
                this.animator.speed = 2.0f;
            }
        }
    }

    void ShootStinger(float positionx, float positiony, float distance)
    {
        // 원거리 발사체 생성 Y좌표
        Vector3 AntY = new Vector3(this.transform.position.x + positionx, this.transform.position.y - 0.6f + positiony, 0);

        // 발사체 생성
        GameObject stinger = Instantiate(stingerPrefab, AntY, Quaternion.identity);

        // 발사체에 ProjectileScript 추가 및 속성 설정
        ShootStingerc projectileScript = stinger.AddComponent<ShootStingerc>();
        projectileScript.maxDistance = distance; // 사정거리 설정 (필요에 따라 값 조정 가능)

        // 발사 방향 설정
        Rigidbody2D rb = stinger.GetComponent<Rigidbody2D>();

        // 플레이어의 현재 보는 방향(왼쪽 또는 오른쪽)에 따라 발사 방향을 결정
        float direction = transform.localScale.x > 0 ? 1f : -1f; // localScale.x가 0보다 크면 왼쪽(-1), 작으면 오른쪽(1)
        rb.velocity = new Vector2(direction * 5f, 0f); // X축 방향으로 속도 

        //발사체 크기와 방향 조정
        stinger.transform.localScale = new Vector3(direction * 0.3f, 0.3f, 0.3f);

        // **플레이어와 발사체의 충돌 무시**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D stingerCollider = stinger.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, stingerCollider);

    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        rigid2D.velocity = Vector2.zero; // 현재 속도 초기화
        rigid2D.AddForce(direction * dashForce, ForceMode2D.Impulse); // 대시 힘 적용

        yield return new WaitForSeconds(dashDuration); // 대시 지속 시간

        isDashing = false; // 대시 상태 종료
    }

    IEnumerator PerformLaserAttack()
    {
        // 애니메이션 트리거 실행
        this.animator.SetTrigger("UltiTrigger");

        // 움직임 제한 설정
        isFrozen = true;
        frozenPosition = rigid2D.position; // 현재 위치 저장

        // 레이저 생성
        CreateLaser();

        // 1초 후에 데미지 활성화
        yield return new WaitForSeconds(1.0f);
        laserCanDamage = true;

        // 3초 후 레이저 삭제
        yield return new WaitForSeconds(1.0f); // 총 2초 (1초 + 1초)
        laserCanDamage = false;

        // 레이저 공격 종료 후
        laserCanDamage = false;
        isFrozen = false; // 움직임 제한 해제
        if (currentLaser != null)
        {
            Destroy(currentLaser);
        }

        // 레이저 공격 사용 완료 표시 (이미 Update에서 설정했지만, 확실히 하기 위해 여기서도 설정)
        laserAttackUsed = true;
    }

    void CreateLaser()
    {

        // 기존 레이저가 있다면 삭제
        if (currentLaser != null)
        {
            Destroy(currentLaser);
        }

        // 캐릭터 방향에 따라 레이저 위치 설정
        Vector3 laserPosition;
        float direction = transform.localScale.x > 0 ? 1f : -1f; // 방향 확인 (오른쪽: 1, 왼쪽: -1)

        if (direction > 0)
        {
            // 오른쪽을 볼 때
            laserPosition = new Vector3(this.transform.position.x + 10f, this.transform.position.y - 0.5f, 0);
        }
        else
        {
            // 왼쪽을 볼 때
            laserPosition = new Vector3(this.transform.position.x - 10f, this.transform.position.y - 0.5f, 0);
        }

        // 레이저 생성
        currentLaser = Instantiate(laserPrefab, laserPosition, Quaternion.identity);

        // 레이저 크기를 화면 끝까지 확장
        currentLaser.transform.localScale = new Vector3(2.0f * direction, 1.0f, 1.0f); // 방향에 따라 X축 크기 설정
        // 레이저 Collider2D 설정
        Collider2D laserCollider = currentLaser.GetComponent<Collider2D>();
        if (laserCollider != null)
        {
            laserCollider.isTrigger = true; // 트리거 활성화
        }
        // **플레이어와 발사체의 충돌 무시**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D currentLaserCollider = currentLaser.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, currentLaserCollider);
    }

    IEnumerator DashAndAttack()
    {
        // 현재 플레이어가 바라보는 방향으로 대시
        Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        yield return StartCoroutine(Dash(dashDirection));

        // 강화 근접 공격 실행
        this.animator.SetTrigger("AAttackTrigger");
        PerformMeleeAttack(true); // 강화 근접 공격 실행
    }

    // 근접 공격 실행
    // 제어 제한 함수
    IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        PlayerController controller = target.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false; // 컨트롤 비활성화
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // 컨트롤 재활성화
        }

    }
    // 넉백 함수 
    void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        float knockbackForce = 10.0f; // 넉백 강도
        float knockbackDuration = 0.2f; // 넉백 지속 시간

        // 넉백 방향 결정
        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized; // x축과 약간의 y축으로 밀림

        // 힘 추가
        targetRigidBody.velocity = Vector2.zero; // 기존 속도 초기화
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // 넉백 중 제어를 잠시 제한
        StartCoroutine(DisableControlForDuration(targetRigidBody.gameObject, knockbackDuration));
    }

    void PerformMeleeAttack(bool isEnhanced = false)
    {
        // 공격 범위와 중심 설정
        float attackWidth = isEnhanced ? 0.7f : 0.7f; // 강화 공격 시 더 넓은 범위
        float attackHeight = 1.0f;
        float damage = isEnhanced ? 0.2f : 0.1f; // 강화 공격 시 더 큰 데미지

        // 캐릭터 방향에 따라 공격 중심 계산
        float direction = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackCenter = new Vector2(transform.position.x + direction * (attackWidth / 2), transform.position.y);

        // 공격 범위 설정
        Vector2 attackSize = new Vector2(attackWidth, attackHeight);

        // 해당 범위 내의 충돌체 탐색
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(attackCenter, attackSize, 0);

        foreach (Collider2D target in hitTargets)
        {
            // 상대방인지 확인
            if (target.CompareTag("Player1") || target.CompareTag("Player2"))
            {
                if (target.tag != gameObject.tag) // 자신과 다른 태그만 공격
                {
                    GameObject director = GameObject.Find("GameDirector");
                    if (director != null)
                    {
                        GameDirector gameDirector = director.GetComponent<GameDirector>();
                        if (gameDirector != null)
                        {
                            if (target.CompareTag("Player1"))
                            {
                                gameDirector.DecreaseHP1(damage); // Player1 데미지 적용
                            }
                            else if (target.CompareTag("Player2"))
                            {
                                gameDirector.DecreaseHP2(damage); // Player2 데미지 적용
                            }
                        }
                    }

                    // 넉백 효과 추가
                    Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();
                    if (targetRigidBody != null)
                    {
                        ApplyKnockback(targetRigidBody, direction);
                    }
                }
            }
        }

        // 시각적 디버그용 (Unity Scene에서 공격 범위를 확인 가능)
        Debug.DrawLine(
            attackCenter - new Vector2(attackWidth / 2, attackHeight / 2),
            attackCenter + new Vector2(attackWidth / 2, attackHeight / 2),
            isEnhanced ? Color.yellow : Color.red, // 강화 공격은 노란색, 일반 공격은 빨간색
            0.1f
        );
    }

    void CreateShieldEffects()
    {
        GameObject shield = Instantiate(shieldPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);
        // 쉴드를 현재 오브젝트(플레이어)의 자식으로 설정
        shield.transform.SetParent(transform);
        // 쉴드 활성화
        GameObject director = GameObject.Find("GameDirector");
        if (director != null)
        {
            GameDirector gameDirector = director.GetComponent<GameDirector>();
            if (gameDirector != null)
            {
                if (gameObject.CompareTag("Player1"))
                {
                    gameDirector.isPlayer1ShieldActive = true;
                }
                else if (gameObject.CompareTag("Player2"))
                {
                    gameDirector.isPlayer2ShieldActive = true;
                }
            }
        }

        // 쉴드 지속 시간 후 비활성화
        StartCoroutine(DisableShieldAfterDuration(shield));
    }

    IEnumerator DisableShieldAfterDuration(GameObject shield)
    {
        yield return new WaitForSeconds(shieldDuration);

        // 쉴드 비활성화
        GameObject director = GameObject.Find("GameDirector");
        if (director != null)
        {
            GameDirector gameDirector = director.GetComponent<GameDirector>();
            if (gameDirector != null)
            {
                if (gameObject.CompareTag("Player1"))
                {
                    gameDirector.isPlayer1ShieldActive = false;
                }
                else if (gameObject.CompareTag("Player2"))
                {
                    gameDirector.isPlayer2ShieldActive = false;
                }
            }
        }

        // 쉴드 객체 삭제
        Destroy(shield);
    }
}