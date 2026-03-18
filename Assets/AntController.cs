using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AntController : Player
{
    // 개미 특화 변수
    public GameObject slashPrefab;      // slash 이펙트 프리팹
    public GameObject swordPrefab;      // 강화 원거리 칼 프리팹
    public GameObject projectilePrefab; // 원거리 발사체에 사용할 프리팹

    bool isExecutingSkill = false;      // 스킬 중복 실행 방지

    float dashDistance = 5.0f;         // 스킬의 슬라이드 거리
    float skillDelay = 0.5f;           // 스킬의 준비 시간
    float slashDuration = 1.0f;        // slash가 지속되는 시간

    protected override void Start()
    {
        base.Start();
        jumpForce = 1650.0f;                                      // 개미 점프력 설정
        transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);    // 개미 스케일
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void HandleInput()
    {
        // 왼쪽 입력 처리
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - doubleTapTimeLeft < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.left));
            }
            doubleTapTimeLeft = Time.time;
        }

        // 오른쪽 입력 처리
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - doubleTapTimeRight < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.right));
            }
            doubleTapTimeRight = Time.time;
        }

        // 점프
        if (Input.GetKeyDown(KeyCode.UpArrow) && this.rigid2D.velocity.y == 0)
        {
            this.rigid2D.AddForce(transform.up * this.jumpForce);
        }

        // 실드
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CreateShieldEffects();
        }

        // 근거리 공격
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

        // 강화 근거리 공격
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

        // 스페셜 스킬
        if (Input.GetKeyDown(KeyCode.Keypad3) && canUseSpecialSkill)
        {
            StartCoroutine(ExecuteSpecialSkill());
            canUseSpecialSkill = false;
        }
    }

    protected override float GetMovementKey()
    {
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

        return key;
    }

    // ── 발사체 ──────────────────────────────────────────────────────────────
    void ShootProjectile()
    {
        Vector3 spawnPos = new Vector3(transform.position.x - 0.5f, transform.position.y, 0);
        float direction = transform.localScale.x > 0 ? -1f : 1f;

        GameObject projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        projectile.tag = gameObject.tag; // 발사체에 플레이어 태그 적용

        // 통합 Projectile 컴포넌트 설정
        Projectile proj = projectile.GetComponent<Projectile>();
        if (proj == null) proj = projectile.AddComponent<Projectile>();
        proj.maxDistance = 5.0f;
        proj.speed = 5.0f;
        proj.damage = 0.1f;

        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(direction * proj.speed, 0f);

        projectile.transform.localScale = new Vector3(-direction * 0.3f, 0.3f, 0.3f);

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), projectile.GetComponent<Collider2D>());
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        rigid2D.velocity = Vector2.zero;
        rigid2D.AddForce(direction * dashForce, ForceMode2D.Impulse);

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }

    // ── 스페셜 스킬 ─────────────────────────────────────────────────────────
    IEnumerator ExecuteSpecialSkill()
    {
        isExecutingSkill = true;

        animator.SetTrigger("UltiTrigger");

        yield return new WaitForSeconds(skillDelay);

        float startX = transform.position.x;
        float direction = transform.localScale.x > 0 ? -1 : 1;

        transform.position = new Vector3(transform.position.x + dashDistance * direction, transform.position.y, transform.position.z);

        float endX = transform.position.x;

        CreateSlashEffects(startX, endX, direction);

        yield return new WaitForSeconds(slashDuration);

        isExecutingSkill = false;
    }

    void CreateSlashEffects(float startX, float endX, float direction)
    {
        float slashX = (startX + endX) / 2;

        GameObject slash = Instantiate(slashPrefab, new Vector3(slashX, transform.position.y, 0), Quaternion.identity);
        slash.transform.localScale = new Vector3(direction * 2.5f, 2, 1);

        Destroy(slash, slashDuration);
        ApplySlashDamage(startX, endX);
    }

    void ApplySlashDamage(float startX, float endX)
    {
        float minX = Mathf.Min(startX, endX);
        float maxX = Mathf.Max(startX, endX);

        Collider2D[] hitTargets = Physics2D.OverlapAreaAll(
            new Vector2(minX, transform.position.y - 0.5f),
            new Vector2(maxX, transform.position.y + 0.5f)
        );

        foreach (Collider2D target in hitTargets)
        {
            if (target.CompareTag("Player1") || target.CompareTag("Player2"))
            {
                if (target.tag != gameObject.tag)
                {
                    GameObject director = GameObject.Find("GameDirector");
                    if (director != null)
                    {
                        GameDirector gameDirector = director.GetComponent<GameDirector>();
                        if (gameDirector != null)
                        {
                            if (target.CompareTag("Player1"))
                                gameDirector.DecreaseHP1(0.3f);
                            else if (target.CompareTag("Player2"))
                                gameDirector.DecreaseHP2(0.3f);
                        }
                    }

                    Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();
                    if (targetRigidBody != null)
                    {
                        float dir = (target.transform.position.x - transform.position.x) > 0 ? 1 : -1;
                        ApplyKnockback(targetRigidBody, dir);
                    }
                }
            }
        }
    }

    // ── 강화 근거리 공격 ─────────────────────────────────────────────────────
    IEnumerator PerformEnhancedMeleeAttack()
    {
        animator.SetTrigger("AttackTrigger");

        canUseEnhancedMelee = false;
        StartCoroutine(SetCooldown(() => canUseEnhancedMelee = true, 2.0f));

        float direction = transform.localScale.x > 0 ? -1 : 1;
        Vector2 jumpVelocity = new Vector2(direction * 20.0f, 8.0f);
        rigid2D.velocity = Vector2.zero;
        rigid2D.AddForce(jumpVelocity, ForceMode2D.Impulse);

        yield return new WaitForSeconds(0.1f);

        PerformMeleeAttack(direction, 0.15f);
    }

    IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        Player controller = target.GetComponent<Player>();
        if (controller != null)
        {
            controller.enabled = false;
            yield return new WaitForSeconds(duration);
            controller.enabled = true;
        }
    }

    void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        float knockbackForce = 10.0f;
        float knockbackDuration = 0.2f;

        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

        targetRigidBody.velocity = Vector2.zero;
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(DisableControlForDuration(targetRigidBody.gameObject, knockbackDuration));
    }

    void PerformMeleeAttack(float direction, float damage)
    {
        float attackWidth = 1.0f;
        float attackHeight = 2.0f;

        Vector2 attackCenter = new Vector2(transform.position.x + direction * (attackWidth / 2), transform.position.y + 0.5f);
        Vector2 attackSize = new Vector2(attackWidth, attackHeight);

        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(attackCenter, attackSize, 0);

        foreach (Collider2D target in hitTargets)
        {
            if (target.CompareTag("Player1") || target.CompareTag("Player2"))
            {
                if (target.tag != gameObject.tag)
                {
                    GameObject director = GameObject.Find("GameDirector");
                    if (director != null)
                    {
                        GameDirector gameDirector = director.GetComponent<GameDirector>();
                        if (gameDirector != null)
                        {
                            if (target.CompareTag("Player1"))
                                gameDirector.DecreaseHP1(damage);
                            else if (target.CompareTag("Player2"))
                                gameDirector.DecreaseHP2(damage);
                        }
                    }

                    Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();
                    if (targetRigidBody != null)
                    {
                        ApplyKnockback(targetRigidBody, direction);
                    }
                }
            }
        }

        Debug.DrawLine(
            attackCenter - new Vector2(attackWidth / 2, attackHeight / 2),
            attackCenter + new Vector2(attackWidth / 2, attackHeight / 2),
            Color.red,
            0.1f
        );
    }

    void CreateShieldEffects()
    {
        if (!canUseShield) return;

        canUseShield = false;

        GameObject shield = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
        shield.transform.SetParent(transform);

        GameObject director = GameObject.Find("GameDirector");
        if (director != null)
        {
            GameDirector gameDirector = director.GetComponent<GameDirector>();
            if (gameDirector != null)
            {
                if (gameObject.CompareTag("Player1"))
                    gameDirector.isPlayer1ShieldActive = true;
                else if (gameObject.CompareTag("Player2"))
                    gameDirector.isPlayer2ShieldActive = true;
            }
        }

        StartCoroutine(DisableShieldAfterDuration(shield));
        StartCoroutine(SetCooldown(() => canUseShield = true, 5.0f));
    }

    IEnumerator DisableShieldAfterDuration(GameObject shield)
    {
        yield return new WaitForSeconds(shieldDuration);

        GameObject director = GameObject.Find("GameDirector");
        if (director != null)
        {
            GameDirector gameDirector = director.GetComponent<GameDirector>();
            if (gameDirector != null)
            {
                if (gameObject.CompareTag("Player1"))
                    gameDirector.isPlayer1ShieldActive = false;
                else if (gameObject.CompareTag("Player2"))
                    gameDirector.isPlayer2ShieldActive = false;
            }
        }

        Destroy(shield);
    }

    // ── 강화 원거리 공격 ─────────────────────────────────────────────────────
    void ShootSword()
    {
        if (!canUseEnhancedRanged) return;

        canUseEnhancedRanged = false;

        Vector3 spawnPos = new Vector3(transform.position.x - 0.5f, transform.position.y - 0.5f, 0);
        float direction = transform.localScale.x > 0 ? -1f : 1f;

        GameObject sword = Instantiate(swordPrefab, spawnPos, Quaternion.identity);
        sword.tag = gameObject.tag; // 발사체에 플레이어 태그 적용

        // 통합 Projectile 컴포넌트 설정
        Projectile proj = sword.GetComponent<Projectile>();
        if (proj == null) proj = sword.AddComponent<Projectile>();
        proj.maxDistance = 10.0f;
        proj.speed = 5.0f;
        proj.damage = 0.15f;

        Rigidbody2D rb = sword.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(direction * proj.speed, 0f);

        sword.transform.localScale = new Vector3(direction, 1, 1);

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), sword.GetComponent<Collider2D>());

        StartCoroutine(SetCooldown(() => canUseEnhancedRanged = true, 2.0f));
    }
}