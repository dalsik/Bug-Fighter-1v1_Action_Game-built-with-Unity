using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 발사체 사정거리 클래스(MonBehaviour)
public class ProjectileScript : MonoBehaviour
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

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigid2D;
    Animator animator;
    public GameObject slashPrefab; // 검기 효과 프리팹
    public GameObject swordPrefab; // 강화 원거리 검 프리팹

    bool isExecutingSkill = false; // 필살기 중복 사용 방지

    float dashDistance = 5.0f; // 필살기 순간이동 거리
    float skillDelay = 0.5f; // 필살기 준비 시간
    float slashDuration = 1.0f; // 검기가 유지되는 시간
    float shieldDuration = 1.0f;

    float jumpForce = 1650.0f;          // 점프 가중치
    float walkForce = 30.0f;            // 걷기 가중치
    float maxWalkSpeed = 4.0f;          // 최대 걷기 속도

    float dashForce = 15.0f; // 대시할 때의 힘
    float dashDuration = 0.1f; // 대시 지속 시간
    bool isDashing = false; // 대시 중인지 확인

    float doubleTapTimeLeft = 0.0f; // 왼쪽 화살표 키 두 번 누르는 시간 감지
    float doubleTapTimeRight = 0.0f; // 오른쪽 화살표 키 두 번 누르는 시간 감지
    float doubleTapDelay = 0.3f; // 두 번 누르기 감지 시간

    public GameObject projectilePrefab;     // 원거리 발사체로 사용할 프리팹 변수
    public GameObject shieldPrefab; //쉴드 프리팹 변수
    bool isShieldActive = false; // 쉴드 활성화 여부를 나타내는 변수

    // 스킬 쿨타임 관련 변수
    private bool canUseEnhancedMelee = true; // 강화 근거리 공격 쿨타임 상태
    private bool canUseEnhancedRanged = true; // 강화 원거리 공격 쿨타임 상태
    private bool canUseShield = true; // 쉴드 쿨타임 상태
    private bool canUseSpecialSkill = true; // 살충기 단 1회 사용 여부



    void Start()
    {
        Application.targetFrameRate = 60;       // 프레임 속도 60으로 고정
        this.rigid2D = GetComponent<Rigidbody2D>();     // rigid2D 변수에 Rigidbody2D Physics를 불러옴
        this.animator = GetComponent<Animator>();       // 애니메이터
    }

    void Update()
    {
        // 대시 입력 감지
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - doubleTapTimeLeft < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.left)); // 왼쪽으로 대시
            }
            doubleTapTimeLeft = Time.time;
        }

        // 대시 입력 감지 (오른쪽 방향)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - doubleTapTimeRight < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.right)); // 오른쪽으로 대시
            }
            doubleTapTimeRight = Time.time;
        }

        // 점프한다(개미,벌 유동적으로)
        if (Input.GetKeyDown(KeyCode.UpArrow) && this.rigid2D.velocity.y == 0)
        {
            this.rigid2D.AddForce(transform.up * this.jumpForce);
        }

        //쉴드
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CreateShieldEffects();
        }

        // 근접 공격(개미, 벌 유동적으로)
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            this.animator.SetTrigger("AttackTrigger");
            float direction = transform.localScale.x > 0 ? -1 : 1;

            PerformMeleeAttack(direction, 0.05f);
        }

        // 원거리 공격(플레이어2)
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootProjectile();
        }


        // 강화 근거리 공격 (포물선 공격)
        if (Input.GetKeyDown(KeyCode.Keypad4) && canUseEnhancedMelee)
        {
            StartCoroutine(PerformEnhancedMeleeAttack());
        }

        // 강화 원거리 공격
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            this.animator.SetTrigger("SSpitTrigger");
            ShootSword();
        }

        // 살충기 공격
        if (Input.GetKeyDown(KeyCode.Keypad3) && canUseSpecialSkill)
        {
            StartCoroutine(ExecuteSpecialSkill());
            canUseSpecialSkill = false; // 단 1회만 사용 가능
        }
        float key = 0f;

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            this.animator.SetTrigger("WalkTrigger");
            key = 0.9f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            this.animator.SetTrigger("WalkTrigger");
            key = -0.9f;
        }

        // 플레이어 속도
        float speedx = Mathf.Abs(this.rigid2D.velocity.x);

        // 스피드 제한
        if (speedx < this.maxWalkSpeed)
        {
            this.rigid2D.AddForce(transform.right * key * this.walkForce);
        }

        // 움직이는 방향에 맞춰 이미지 반전
        if (key != 0)
        {
            transform.localScale = new Vector3(key, 0.9f, 0.9f);
        }

        // 플레이어 속도에 맞춰 애니메이션 속도를 바꾼다.
        if (speedx != 0)
        {
            this.animator.speed = 2.0f;
        }


    }
    // 원거리 공격 발사체
    void ShootProjectile()
    {
        // 원거리 발사체 생성 위치
        Vector3 AntY = new Vector3(this.transform.position.x - 0.5f, this.transform.position.y, 0);

        // 발사체 생성
        GameObject projectile = Instantiate(projectilePrefab, AntY, Quaternion.identity);

        // 발사체에 ProjectileScript 추가 및 속성 설정
        ProjectileScript projectileScript = projectile.AddComponent<ProjectileScript>();
        projectileScript.maxDistance = 5.0f; // 사정거리 설정 (필요에 따라 값 조정 가능)

        // 발사 방향 설정
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // 플레이어의 현재 보는 방향(왼쪽 또는 오른쪽)에 따라 발사 방향을 결정
        float direction = transform.localScale.x > 0 ? -1f : 1f; // localScale.x가 0보다 크면 왼쪽(-1), 작으면 오른쪽(1)
        rb.velocity = new Vector2(direction * 5f, 0f); // X축 방향으로 속도 

        projectile.transform.localScale = new Vector3(-direction * 0.3f, 0.3f, 0.3f); // 발사체 이미지 반전 

        // **플레이어와 발사체의 충돌 무시**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, projectileCollider);
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        rigid2D.velocity = Vector2.zero; // 현재 속도 초기화
        rigid2D.AddForce(direction * dashForce, ForceMode2D.Impulse); // 대시 힘 적용

        yield return new WaitForSeconds(dashDuration); // 대시 지속 시간

        isDashing = false; // 대시 상태 종료
    }

    // 살충기 구현 코드
    IEnumerator ExecuteSpecialSkill()
    {
        isExecutingSkill = true;

        // 준비 자세 애니메이션 실행
        animator.SetTrigger("UltiTrigger");

        // 0.5초 대기
        yield return new WaitForSeconds(skillDelay);

        // 현재 위치 저장
        float startX = transform.position.x;

        // 방향 계산 (왼쪽: -1, 오른쪽: 1)
        float direction = transform.localScale.x > 0 ? -1 : 1;

        // 순간이동: x 좌표를 dashDistance만큼 이동
        transform.position = new Vector3(transform.position.x + dashDistance * direction, transform.position.y, transform.position.z);

        // 이동 후 위치 저장
        float endX = transform.position.x;

        // 검기 생성
        CreateSlashEffects(startX, endX, direction);

        // 필살기 후 1초 대기
        yield return new WaitForSeconds(slashDuration);

        // 필살기 종료
        isExecutingSkill = false;
    }

    // 살충기 검기 이펙트 생성 코드
    void CreateSlashEffects(float startX, float endX, float direction)
    {
        // 검기 생성 위치는 시작과 끝 중간 지점
        float slashX = (startX + endX) / 2;

        // 검기 생성
        GameObject slash = Instantiate(slashPrefab, new Vector3(slashX, transform.position.y, 0), Quaternion.identity);

        // 검기의 방향 설정
        slash.transform.localScale = new Vector3(direction * 2.5f, 2, 1);

        // 검기 파괴(1초 후)
        Destroy(slash, slashDuration);
        ApplySlashDamage(startX, endX);

    }

    void ApplySlashDamage(float startX, float endX)
    {
        // 시작과 끝 사이의 범위 계산
        float minX = Mathf.Min(startX, endX);
        float maxX = Mathf.Max(startX, endX);

        // 해당 범위 내의 모든 충돌체 탐색
        Collider2D[] hitTargets = Physics2D.OverlapAreaAll(
            new Vector2(minX, transform.position.y - 0.5f), // 아래쪽 범위
            new Vector2(maxX, transform.position.y + 0.5f)  // 위쪽 범위
        );

        foreach (Collider2D target in hitTargets)
        {
            // 자신 제외 확인 (같은 태그가 아닌 경우만 처리)
            if (target.CompareTag("Player1") || target.CompareTag("Player2"))
            {
                if (target.tag != gameObject.tag) // 자신의 태그와 다른 태그일 경우
                {
                    // GameDirector를 통해 데미지 처리
                    GameObject director = GameObject.Find("GameDirector");
                    if (director != null)
                    {
                        GameDirector gameDirector = director.GetComponent<GameDirector>();
                        if (gameDirector != null)
                        {
                            // 상대방 태그에 따라 HP 감소 처리
                            if (target.CompareTag("Player1"))
                            {
                                gameDirector.DecreaseHP1(0.3f); // 예시 데미지: 0.1
                            }
                            else if (target.CompareTag("Player2"))
                            {
                                gameDirector.DecreaseHP2(0.3f); // 예시 데미지: 0.1
                            }
                        }
                    }

                    // 넉백 효과 추가
                    Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();
                    if (targetRigidBody != null)
                    {
                        float direction = (target.transform.position.x - transform.position.x) > 0 ? 1 : -1; // 밀리는 방향
                        ApplyKnockback(targetRigidBody, direction);
                    }
                }
            }
        }
    }

    // 강화 근접 공격
    IEnumerator PerformEnhancedMeleeAttack()
    {
        // 애니메이션 실행
        animator.SetTrigger("AttackTrigger");

        // 강화 근거리 공격 쿨타임 설정
        canUseEnhancedMelee = false;
        StartCoroutine(SetCooldown(() => canUseEnhancedMelee = true, 2.0f)); // 쿨타임 2초

        // 포물선 이동 초기 속도 설정
        float direction = transform.localScale.x > 0 ? -1 : 1;
        Vector2 jumpVelocity = new Vector2(direction * 20.0f, 8.0f); // X축 이동 및 Y축 점프 초기 속도
        rigid2D.velocity = Vector2.zero; // 기존 속도 초기화
        rigid2D.AddForce(jumpVelocity, ForceMode2D.Impulse); // 포물선 이동 시작

        // 일정 시간 대기 (포물선 이동 중)
        float attackDuration = 0.1f; // 공격 지속 시간
        yield return new WaitForSeconds(attackDuration);

        // 한 번만 충돌 감지
        PerformMeleeAttack(direction, 0.15f);
    }

    // 쿨타임 관리 코루틴
    private IEnumerator SetCooldown(System.Action onComplete, float cooldownDuration)
    {
        yield return new WaitForSeconds(cooldownDuration);
        onComplete?.Invoke();
    }

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

    // 점프 공격 중의 영역에서 충돌 감지 (한 번만 실행)


    void PerformMeleeAttack(float direction, float damage)
    {
        // 현재 캐릭터 위치 기준으로 영역 설정
        float attackWidth = 1.0f; // 공격 범위 너비
        float attackHeight = 2.0f; // 공격 범위 높이

        // 공격 중심은 캐릭터 앞쪽 방향으로 설정
        Vector2 attackCenter = new Vector2(transform.position.x + direction * (attackWidth / 2), transform.position.y + 0.5f);

        // 공격 범위 크기
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
                    // 데미지 처리
                    GameObject director = GameObject.Find("GameDirector");
                    if (director != null)
                    {
                        GameDirector gameDirector = director.GetComponent<GameDirector>();
                        if (gameDirector != null)
                        {
                            if (target.CompareTag("Player1"))
                            {
                                gameDirector.DecreaseHP1(damage);
                            }
                            else if (target.CompareTag("Player2"))
                            {
                                gameDirector.DecreaseHP2(damage);
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
            Color.red,
            0.1f
        );
    }


    void CreateShieldEffects()
    {
        if (!canUseShield) return; // 쿨타임 중이면 실행하지 않음

        // 쉴드 생성 중으로 설정
        canUseShield = false;

        // 쉴드 생성
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

        // 쉴드 지속 시간 후 비활성화 및 쿨타임 해제
        StartCoroutine(DisableShieldAfterDuration(shield));
        StartCoroutine(SetCooldown(() => canUseShield = true, 5.0f)); // 5초 쿨타임 적용
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

    // 강화 원거리 공격
    void ShootSword()
    {
        if (!canUseEnhancedRanged) return; // 쿨타임 중이면 실행하지 않음

        // 스킬 사용 중으로 설정
        canUseEnhancedRanged = false;

        // 원거리 발사체 생성 위치
        Vector3 AntY = new Vector3(this.transform.position.x - 0.5f, this.transform.position.y - 0.5f, 0);

        // 발사체 생성
        GameObject sword = Instantiate(swordPrefab, AntY, Quaternion.identity);

        // 발사체에 ProjectileScript 추가 및 속성 설정
        ProjectileScript projectileScript = sword.AddComponent<ProjectileScript>();
        projectileScript.maxDistance = 10.0f; // 사정거리 설정

        // 발사 방향 설정
        Rigidbody2D rb = sword.GetComponent<Rigidbody2D>();

        // 플레이어의 현재 보는 방향(왼쪽 또는 오른쪽)에 따라 발사 방향을 결정
        float direction = transform.localScale.x > 0 ? -1f : 1f; // localScale.x가 0보다 크면 왼쪽(-1), 작으면 오른쪽(1)
        rb.velocity = new Vector2(direction * 5f, 0f); // X축 방향으로 속도 

        sword.transform.localScale = new Vector3(direction, 1, 1); // 발사체 이미지 반전 

        // **플레이어와 발사체의 충돌 무시**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D projectileCollider = sword.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, projectileCollider);

        // 2초 후 쿨타임 해제
        StartCoroutine(SetCooldown(() => canUseEnhancedRanged = true, 2.0f));
    }
}