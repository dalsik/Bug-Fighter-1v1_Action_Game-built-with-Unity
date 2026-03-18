using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    protected Rigidbody2D rigid2D;
    protected Animator animator;

    // 공통 이동 및 점프 변수
    protected float jumpForce = 700.0f;
    protected float walkForce = 30.0f;
    protected float maxWalkSpeed = 4.0f;

    // 대시 변수
    protected float dashForce = 15.0f;
    protected float dashDuration = 0.1f;
    protected bool isDashing = false;

    // 더블 탭 대시 변수 (자식 클래스에서 설정)
    protected float doubleTapTimeLeft = 0.0f;
    protected float doubleTapTimeRight = 0.0f;
    protected float doubleTapDelay = 0.3f;

    // 쉴드 변수
    protected float shieldDuration = 1.0f;
    public GameObject shieldPrefab;
    protected bool canUseShield = true;

    // 공격 변수
    protected float attackWidth = 0.7f;
    protected float attackHeight = 1.0f;
    protected float damage = 0.1f;

    // 넉백 변수
    protected float knockbackForce = 10.0f;
    protected float knockbackDuration = 0.2f;

    // 특수 공격 쿨다운 (자식 클래스에서 설정)
    protected bool canUseSpecialSkill = true;
    protected bool canUseEnhancedMelee = true;
    protected bool canUseEnhancedRanged = true;

    protected virtual void Start()
    {
        Application.targetFrameRate = 60;
        this.rigid2D = GetComponent<Rigidbody2D>();
        this.animator = GetComponent<Animator>();
    }

    protected virtual void Update()
    {
        HandleInput();
        HandleMovement();
    }

    // 자식 클래스에서 오버라이드하여 입력 처리
    protected virtual void HandleInput()
    {
        
    }

    // 공통 이동 처리
    protected void HandleMovement()
    {
        float key = GetMovementKey();
        float speedx = Mathf.Abs(this.rigid2D.velocity.x);

        if (speedx < this.maxWalkSpeed)
        {
            this.rigid2D.AddForce(transform.right * key * this.walkForce);
        }

        if (key != 0)
        {
            transform.localScale = new Vector3(key, transform.localScale.y, transform.localScale.z);
        }

        if (speedx != 0)
        {
            this.animator.speed = 2.0f;
        }
    }

    // 자식 클래스에서 오버라이드하여 이동 키 반환
    protected virtual float GetMovementKey()
    {
        return 0f;
    }

    // 공통 대시 코루틴
    protected IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        rigid2D.velocity = Vector2.zero;
        rigid2D.AddForce(direction * dashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashDuration);
        isDashing = false;
    }

    // 공통 근접 공격
    protected void PerformMeleeAttack(float direction, float damageAmount = 0.1f)
    {
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
                            {
                                gameDirector.DecreaseHP1(damageAmount);
                            }
                            else if (target.CompareTag("Player2"))
                            {
                                gameDirector.DecreaseHP2(damageAmount);
                            }
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

    // 공통 넉백
    protected void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;
        targetRigidBody.velocity = Vector2.zero;
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);
        StartCoroutine(DisableControlForDuration(targetRigidBody.gameObject, knockbackDuration));
    }

    // 공통 제어 비활성화
    protected IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        Player controller = target.GetComponent<Player>();
        if (controller != null)
        {
            controller.enabled = false;
            yield return new WaitForSeconds(duration);
            controller.enabled = true;
        }
    }

    // 공통 쉴드 생성
    protected void CreateShieldEffects()
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
                {
                    gameDirector.isPlayer1ShieldActive = true;
                }
                else if (gameObject.CompareTag("Player2"))
                {
                    gameDirector.isPlayer2ShieldActive = true;
                }
            }
        }

        StartCoroutine(DisableShieldAfterDuration(shield));
        StartCoroutine(SetCooldown(() => canUseShield = true, 5.0f));
    }

    // 쉴드 비활성화 코루틴
    protected IEnumerator DisableShieldAfterDuration(GameObject shield)
    {
        yield return new WaitForSeconds(shieldDuration);

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

        Destroy(shield);
    }

    // 쿨다운 설정 코루틴
    protected IEnumerator SetCooldown(System.Action onComplete, float cooldownDuration)
    {
        yield return new WaitForSeconds(cooldownDuration);
        onComplete?.Invoke();
    }
}