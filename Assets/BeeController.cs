using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeeController : Player
{
    // 벌 특화 변수
    public GameObject stingerPrefab;
    public GameObject laserPrefab;
    GameObject currentLaser;
    bool isFrozen = false;
    Vector2 frozenPosition;
    bool laserCanDamage = false;
    bool laserAttackUsed = false;

    int maxjumpcnt = 3;
    [SerializeField] int jumpcnt = 0;

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

    protected override void Start()
    {
        base.Start();
        transform.localScale = new Vector3(0.7f, 0.7f, 0.7f); // 벌 스케일
        lastattackUTime = 0;
        lastattackITime = 0;
        lastShieldTime = 0;
    }

    protected override void Update()
    {
        if (isFrozen)
        {
            rigid2D.velocity = Vector2.zero;
            rigid2D.position = frozenPosition;
            return;
        }
        base.Update();
    }

    protected override void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - doubleTapTimeRight < doubleTapDelay && !isDashing)
                StartCoroutine(Dash(Vector2.right));
            doubleTapTimeRight = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time - doubleTapTimeLeft < doubleTapDelay && !isDashing)
                StartCoroutine(Dash(Vector2.left));
            doubleTapTimeLeft = Time.time;
        }

        // 점프 (최대 3단 점프)
        if (Input.GetKeyDown(KeyCode.W) && jumpcnt < maxjumpcnt)
        {
            jumpcnt++;
            this.rigid2D.AddForce(Vector2.up * jumpForce);
            this.rigid2D.velocity = new Vector2(rigid2D.velocity.x, 0.1f);
        }

        if (jumpcnt == maxjumpcnt && rigid2D.velocity.y == 0) jumpcnt = 0;

        // 기본 원거리 공격
        if (Input.GetKeyDown(KeyCode.K))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootStinger(0f, 0f, 10f, 0.1f);
        }

        // 강화 원거리 공격 (3방향)
        if (Input.GetKeyDown(KeyCode.I) && (isFirstAttackI || Time.time - lastattackITime > attackICooldown))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootStinger(-0.2f, -0.1f, 7.5f, 0.1f);
            ShootStinger(0f,    0f,    7.5f, 0.1f);
            ShootStinger(-0.1f, 0.1f,  7.5f, 0.1f);
            lastattackITime = Time.time;
            isFirstAttackI = false;
        }

        // 근거리 공격
        if (Input.GetKeyDown(KeyCode.J))
        {
            this.animator.SetTrigger("AttackTrigger");
            PerformMeleeAttack();
        }

        // 강화 근거리 공격 (대시 후 공격)
        if (Input.GetKeyDown(KeyCode.U) && (isFirstAttackU || Time.time - lastattackUTime > attackUCooldown))
        {
            StartCoroutine(DashAndAttack());
            lastattackUTime = Time.time;
            isFirstAttackU = false;
        }

        // 레이저 공격
        if (Input.GetKeyDown(KeyCode.L) && !isFrozen && !laserAttackUsed)
        {
            StartCoroutine(PerformLaserAttack());
            laserAttackUsed = true;
        }

        // 실드
        if (Input.GetKeyDown(KeyCode.S) && (isFirstShield || Time.time - lastShieldTime > ShieldCooldown))
        {
            CreateShieldEffects();
            lastShieldTime = Time.time;
            isFirstShield = false;
        }
    }

    protected override float GetMovementKey()
    {
        float key = 0f;
        if (Input.GetKey(KeyCode.D)) { this.animator.SetTrigger("WalkTrigger"); key = 0.7f; }
        if (Input.GetKey(KeyCode.A)) { this.animator.SetTrigger("WalkTrigger"); key = -0.7f; }
        return key;
    }

    // ── 발사체 ──────────────────────────────────────────────────────────────
    void ShootStinger(float positionx, float positiony, float distance, float damage)
    {
        Vector3 spawnPos = new Vector3(
            transform.position.x + positionx,
            transform.position.y - 0.6f + positiony,
            0
        );

        GameObject stinger = Instantiate(stingerPrefab, spawnPos, Quaternion.identity);
        stinger.tag = gameObject.tag; // 발사체에 플레이어 태그 적용

        // 통합 Projectile 컴포넌트 설정
        Projectile proj = stinger.GetComponent<Projectile>();
        if (proj == null) proj = stinger.AddComponent<Projectile>();
        proj.maxDistance = distance;
        proj.speed = 5.0f;
        proj.damage = damage;

        float dir = transform.localScale.x > 0 ? 1f : -1f;

        Rigidbody2D rb = stinger.GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(dir * proj.speed, 0f);

        stinger.transform.localScale = new Vector3(dir * 0.3f, 0.3f, 0.3f);

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), stinger.GetComponent<Collider2D>());
    }

    // ── 레이저 ──────────────────────────────────────────────────────────────
    IEnumerator PerformLaserAttack()
    {
        this.animator.SetTrigger("UltiTrigger");

        isFrozen = true;
        frozenPosition = rigid2D.position;

        CreateLaser();

        yield return new WaitForSeconds(1.0f);
        laserCanDamage = true;

        yield return new WaitForSeconds(1.0f);
        laserCanDamage = false;

        isFrozen = false;
        if (currentLaser != null)
            Destroy(currentLaser);

        laserAttackUsed = true;
    }

    void CreateLaser()
    {
        if (currentLaser != null)
            Destroy(currentLaser);

        float direction = transform.localScale.x > 0 ? 1f : -1f;
        Vector3 laserPosition = new Vector3(
            transform.position.x + direction * 10f,
            transform.position.y - 0.5f,
            0
        );

        currentLaser = Instantiate(laserPrefab, laserPosition, Quaternion.identity);
        currentLaser.transform.localScale = new Vector3(2.0f * direction, 1.0f, 1.0f);

        Collider2D laserCollider = currentLaser.GetComponent<Collider2D>();
        if (laserCollider != null)
            laserCollider.isTrigger = true;

        Physics2D.IgnoreCollision(GetComponent<Collider2D>(), currentLaser.GetComponent<Collider2D>());
    }

    // ── 대시 후 강화 공격 ────────────────────────────────────────────────────
    IEnumerator DashAndAttack()
    {
        Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        yield return StartCoroutine(Dash(dashDirection));

        this.animator.SetTrigger("AAttackTrigger");
        PerformMeleeAttack(true);
    }

    // ── 근거리 공격 ──────────────────────────────────────────────────────────
    void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        float knockbackForce = 10.0f;
        float knockbackDuration = 0.2f;

        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

        targetRigidBody.velocity = Vector2.zero;
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        StartCoroutine(DisableControlForDuration(targetRigidBody.gameObject, knockbackDuration));
    }

    void PerformMeleeAttack(bool isEnhanced = false)
    {
        float attackWidth = 0.7f;
        float attackHeight = 1.0f;
        float damage = isEnhanced ? 0.2f : 0.1f;

        float direction = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackCenter = new Vector2(transform.position.x + direction * (attackWidth / 2), transform.position.y);
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
                        ApplyKnockback(targetRigidBody, direction);
                }
            }
        }

        Debug.DrawLine(
            attackCenter - new Vector2(attackWidth / 2, attackHeight / 2),
            attackCenter + new Vector2(attackWidth / 2, attackHeight / 2),
            isEnhanced ? Color.yellow : Color.red,
            0.1f
        );
    }

    // ── 실드 ─────────────────────────────────────────────────────────────────
    void CreateShieldEffects()
    {
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
}