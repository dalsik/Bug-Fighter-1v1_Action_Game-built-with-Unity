using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// �߻�ü �����Ÿ� Ŭ����(MonBehaviour)
public class ProjectileScript : MonoBehaviour
{
    private Vector3 startPosition; // �߻�ü�� ���� ��ġ
    public float maxDistance = 10.0f; // �߻�ü�� ������� �ִ� �Ÿ�

    void Start()
    {
        // �߻�ü ���� �� ���� ��ġ�� ���� ��ġ�� ����
        startPosition = transform.position;
    }

    void Update()
    {
        // �߻�ü�� ���� ��ġ�� ���� ��ġ ������ �Ÿ��� ���
        float distanceTraveled = Vector3.Distance(startPosition, transform.position);

        // �Ÿ��� �ִ� �����Ÿ��� �ʰ��ϸ� �߻�ü�� �ı�
        if (distanceTraveled > maxDistance)
        {
            Destroy(gameObject);
        }
    }
    // ������Ʈ �����ǰ� �ϴ� ��ũ��Ʈ
    void OnTriggerEnter2D(Collider2D collision)
    {
        // ���� �±װ� "Player1" �Ǵ� "Player2"�� ���� ó��
        if (collision.CompareTag("Player1") || collision.CompareTag("Player2"))
        {
            if (collision.tag != gameObject.tag) // �ڽŰ� �ٸ� �±׸� ó��
            {
                // �浹 �� �߻�ü �ı�
                Destroy(gameObject);
            }
        }
    }
}

public class PlayerController : MonoBehaviour
{
    Rigidbody2D rigid2D;
    Animator animator;
    public GameObject slashPrefab; // �˱� ȿ�� ������
    public GameObject swordPrefab; // ��ȭ ���Ÿ� �� ������

    bool isExecutingSkill = false; // �ʻ�� �ߺ� ��� ����

    float dashDistance = 5.0f; // �ʻ�� �����̵� �Ÿ�
    float skillDelay = 0.5f; // �ʻ�� �غ� �ð�
    float slashDuration = 1.0f; // �˱Ⱑ �����Ǵ� �ð�
    float shieldDuration = 1.0f;

    float jumpForce = 1650.0f;          // ���� ����ġ
    float walkForce = 30.0f;            // �ȱ� ����ġ
    float maxWalkSpeed = 4.0f;          // �ִ� �ȱ� �ӵ�

    float dashForce = 15.0f; // ����� ���� ��
    float dashDuration = 0.1f; // ��� ���� �ð�
    bool isDashing = false; // ��� ������ Ȯ��

    float doubleTapTimeLeft = 0.0f; // ���� ȭ��ǥ Ű �� �� ������ �ð� ����
    float doubleTapTimeRight = 0.0f; // ������ ȭ��ǥ Ű �� �� ������ �ð� ����
    float doubleTapDelay = 0.3f; // �� �� ������ ���� �ð�

    public GameObject projectilePrefab;     // ���Ÿ� �߻�ü�� ����� ������ ����
    public GameObject shieldPrefab; //���� ������ ����
    bool isShieldActive = false; // ���� Ȱ��ȭ ���θ� ��Ÿ���� ����

    // ��ų ��Ÿ�� ���� ����
    private bool canUseEnhancedMelee = true; // ��ȭ �ٰŸ� ���� ��Ÿ�� ����
    private bool canUseEnhancedRanged = true; // ��ȭ ���Ÿ� ���� ��Ÿ�� ����
    private bool canUseShield = true; // ���� ��Ÿ�� ����
    private bool canUseSpecialSkill = true; // ����� �� 1ȸ ��� ����



    void Start()
    {
        Application.targetFrameRate = 60;       // ������ �ӵ� 60���� ����
        this.rigid2D = GetComponent<Rigidbody2D>();     // rigid2D ������ Rigidbody2D Physics�� �ҷ���
        this.animator = GetComponent<Animator>();       // �ִϸ�����
    }

    void Update()
    {
        // ��� �Է� ����
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (Time.time - doubleTapTimeLeft < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.left)); // �������� ���
            }
            doubleTapTimeLeft = Time.time;
        }

        // ��� �Է� ���� (������ ����)
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (Time.time - doubleTapTimeRight < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.right)); // ���������� ���
            }
            doubleTapTimeRight = Time.time;
        }

        // �����Ѵ�(����,�� ����������)
        if (Input.GetKeyDown(KeyCode.UpArrow) && this.rigid2D.velocity.y == 0)
        {
            this.rigid2D.AddForce(transform.up * this.jumpForce);
        }

        //����
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CreateShieldEffects();
        }

        // ���� ����(����, �� ����������)
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            this.animator.SetTrigger("AttackTrigger");
            float direction = transform.localScale.x > 0 ? -1 : 1;

            PerformMeleeAttack(direction, 0.05f);
        }

        // ���Ÿ� ����(�÷��̾�2)
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootProjectile();
        }


        // ��ȭ �ٰŸ� ���� (������ ����)
        if (Input.GetKeyDown(KeyCode.Keypad4) && canUseEnhancedMelee)
        {
            StartCoroutine(PerformEnhancedMeleeAttack());
        }

        // ��ȭ ���Ÿ� ����
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            this.animator.SetTrigger("SSpitTrigger");
            ShootSword();
        }

        // ����� ����
        if (Input.GetKeyDown(KeyCode.Keypad3) && canUseSpecialSkill)
        {
            StartCoroutine(ExecuteSpecialSkill());
            canUseSpecialSkill = false; // �� 1ȸ�� ��� ����
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

        // �÷��̾� �ӵ�
        float speedx = Mathf.Abs(this.rigid2D.velocity.x);

        // ���ǵ� ����
        if (speedx < this.maxWalkSpeed)
        {
            this.rigid2D.AddForce(transform.right * key * this.walkForce);
        }

        // �����̴� ���⿡ ���� �̹��� ����
        if (key != 0)
        {
            transform.localScale = new Vector3(key, 0.9f, 0.9f);
        }

        // �÷��̾� �ӵ��� ���� �ִϸ��̼� �ӵ��� �ٲ۴�.
        if (speedx != 0)
        {
            this.animator.speed = 2.0f;
        }


    }
    // ���Ÿ� ���� �߻�ü
    void ShootProjectile()
    {
        // ���Ÿ� �߻�ü ���� ��ġ
        Vector3 AntY = new Vector3(this.transform.position.x - 0.5f, this.transform.position.y, 0);

        // �߻�ü ����
        GameObject projectile = Instantiate(projectilePrefab, AntY, Quaternion.identity);

        // �߻�ü�� ProjectileScript �߰� �� �Ӽ� ����
        ProjectileScript projectileScript = projectile.AddComponent<ProjectileScript>();
        projectileScript.maxDistance = 5.0f; // �����Ÿ� ���� (�ʿ信 ���� �� ���� ����)

        // �߻� ���� ����
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();

        // �÷��̾��� ���� ���� ����(���� �Ǵ� ������)�� ���� �߻� ������ ����
        float direction = transform.localScale.x > 0 ? -1f : 1f; // localScale.x�� 0���� ũ�� ����(-1), ������ ������(1)
        rb.velocity = new Vector2(direction * 5f, 0f); // X�� �������� �ӵ� 

        projectile.transform.localScale = new Vector3(-direction * 0.3f, 0.3f, 0.3f); // �߻�ü �̹��� ���� 

        // **�÷��̾�� �߻�ü�� �浹 ����**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, projectileCollider);
    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        rigid2D.velocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
        rigid2D.AddForce(direction * dashForce, ForceMode2D.Impulse); // ��� �� ����

        yield return new WaitForSeconds(dashDuration); // ��� ���� �ð�

        isDashing = false; // ��� ���� ����
    }

    // ����� ���� �ڵ�
    IEnumerator ExecuteSpecialSkill()
    {
        isExecutingSkill = true;

        // �غ� �ڼ� �ִϸ��̼� ����
        animator.SetTrigger("UltiTrigger");

        // 0.5�� ���
        yield return new WaitForSeconds(skillDelay);

        // ���� ��ġ ����
        float startX = transform.position.x;

        // ���� ��� (����: -1, ������: 1)
        float direction = transform.localScale.x > 0 ? -1 : 1;

        // �����̵�: x ��ǥ�� dashDistance��ŭ �̵�
        transform.position = new Vector3(transform.position.x + dashDistance * direction, transform.position.y, transform.position.z);

        // �̵� �� ��ġ ����
        float endX = transform.position.x;

        // �˱� ����
        CreateSlashEffects(startX, endX, direction);

        // �ʻ�� �� 1�� ���
        yield return new WaitForSeconds(slashDuration);

        // �ʻ�� ����
        isExecutingSkill = false;
    }

    // ����� �˱� ����Ʈ ���� �ڵ�
    void CreateSlashEffects(float startX, float endX, float direction)
    {
        // �˱� ���� ��ġ�� ���۰� �� �߰� ����
        float slashX = (startX + endX) / 2;

        // �˱� ����
        GameObject slash = Instantiate(slashPrefab, new Vector3(slashX, transform.position.y, 0), Quaternion.identity);

        // �˱��� ���� ����
        slash.transform.localScale = new Vector3(direction * 2.5f, 2, 1);

        // �˱� �ı�(1�� ��)
        Destroy(slash, slashDuration);
        ApplySlashDamage(startX, endX);

    }

    void ApplySlashDamage(float startX, float endX)
    {
        // ���۰� �� ������ ���� ���
        float minX = Mathf.Min(startX, endX);
        float maxX = Mathf.Max(startX, endX);

        // �ش� ���� ���� ��� �浹ü Ž��
        Collider2D[] hitTargets = Physics2D.OverlapAreaAll(
            new Vector2(minX, transform.position.y - 0.5f), // �Ʒ��� ����
            new Vector2(maxX, transform.position.y + 0.5f)  // ���� ����
        );

        foreach (Collider2D target in hitTargets)
        {
            // �ڽ� ���� Ȯ�� (���� �±װ� �ƴ� ��츸 ó��)
            if (target.CompareTag("Player1") || target.CompareTag("Player2"))
            {
                if (target.tag != gameObject.tag) // �ڽ��� �±׿� �ٸ� �±��� ���
                {
                    // GameDirector�� ���� ������ ó��
                    GameObject director = GameObject.Find("GameDirector");
                    if (director != null)
                    {
                        GameDirector gameDirector = director.GetComponent<GameDirector>();
                        if (gameDirector != null)
                        {
                            // ���� �±׿� ���� HP ���� ó��
                            if (target.CompareTag("Player1"))
                            {
                                gameDirector.DecreaseHP1(0.3f); // ���� ������: 0.1
                            }
                            else if (target.CompareTag("Player2"))
                            {
                                gameDirector.DecreaseHP2(0.3f); // ���� ������: 0.1
                            }
                        }
                    }

                    // �˹� ȿ�� �߰�
                    Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();
                    if (targetRigidBody != null)
                    {
                        float direction = (target.transform.position.x - transform.position.x) > 0 ? 1 : -1; // �и��� ����
                        ApplyKnockback(targetRigidBody, direction);
                    }
                }
            }
        }
    }

    // ��ȭ ���� ����
    IEnumerator PerformEnhancedMeleeAttack()
    {
        // �ִϸ��̼� ����
        animator.SetTrigger("AttackTrigger");

        // ��ȭ �ٰŸ� ���� ��Ÿ�� ����
        canUseEnhancedMelee = false;
        StartCoroutine(SetCooldown(() => canUseEnhancedMelee = true, 2.0f)); // ��Ÿ�� 2��

        // ������ �̵� �ʱ� �ӵ� ����
        float direction = transform.localScale.x > 0 ? -1 : 1;
        Vector2 jumpVelocity = new Vector2(direction * 20.0f, 8.0f); // X�� �̵� �� Y�� ���� �ʱ� �ӵ�
        rigid2D.velocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
        rigid2D.AddForce(jumpVelocity, ForceMode2D.Impulse); // ������ �̵� ����

        // ���� �ð� ��� (������ �̵� ��)
        float attackDuration = 0.1f; // ���� ���� �ð�
        yield return new WaitForSeconds(attackDuration);

        // �� ���� �浹 ����
        PerformMeleeAttack(direction, 0.15f);
    }

    // ��Ÿ�� ���� �ڷ�ƾ
    private IEnumerator SetCooldown(System.Action onComplete, float cooldownDuration)
    {
        yield return new WaitForSeconds(cooldownDuration);
        onComplete?.Invoke();
    }

    // ���� ���� �Լ�
    IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        PlayerController controller = target.GetComponent<PlayerController>();
        if (controller != null)
        {
            controller.enabled = false; // ��Ʈ�� ��Ȱ��ȭ
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // ��Ʈ�� ��Ȱ��ȭ
        }
    }

    // �˹� �Լ� 
    void ApplyKnockback(Rigidbody2D targetRigidBody, float direction)
    {
        float knockbackForce = 10.0f; // �˹� ����
        float knockbackDuration = 0.2f; // �˹� ���� �ð�

        // �˹� ���� ����
        Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized; // x��� �ణ�� y������ �и�

        // �� �߰�
        targetRigidBody.velocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
        targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

        // �˹� �� ��� ��� ����
        StartCoroutine(DisableControlForDuration(targetRigidBody.gameObject, knockbackDuration));
    }

    // ���� ���� ���� �������� �浹 ���� (�� ���� ����)


    void PerformMeleeAttack(float direction, float damage)
    {
        // ���� ĳ���� ��ġ �������� ���� ����
        float attackWidth = 1.0f; // ���� ���� �ʺ�
        float attackHeight = 2.0f; // ���� ���� ����

        // ���� �߽��� ĳ���� ���� �������� ����
        Vector2 attackCenter = new Vector2(transform.position.x + direction * (attackWidth / 2), transform.position.y + 0.5f);

        // ���� ���� ũ��
        Vector2 attackSize = new Vector2(attackWidth, attackHeight);

        // �ش� ���� ���� �浹ü Ž��
        Collider2D[] hitTargets = Physics2D.OverlapBoxAll(attackCenter, attackSize, 0);

        foreach (Collider2D target in hitTargets)
        {
            // �������� Ȯ��
            if (target.CompareTag("Player1") || target.CompareTag("Player2"))
            {
                if (target.tag != gameObject.tag) // �ڽŰ� �ٸ� �±׸� ����
                {
                    // ������ ó��
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
                    // �˹� ȿ�� �߰�
                    Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();
                    if (targetRigidBody != null)
                    {
                        ApplyKnockback(targetRigidBody, direction);
                    }
                }
            }
        }

        // �ð��� ����׿� (Unity Scene���� ���� ������ Ȯ�� ����)
        Debug.DrawLine(
            attackCenter - new Vector2(attackWidth / 2, attackHeight / 2),
            attackCenter + new Vector2(attackWidth / 2, attackHeight / 2),
            Color.red,
            0.1f
        );
    }


    void CreateShieldEffects()
    {
        if (!canUseShield) return; // ��Ÿ�� ���̸� �������� ����

        // ���� ���� ������ ����
        canUseShield = false;

        // ���� ����
        GameObject shield = Instantiate(shieldPrefab, new Vector3(transform.position.x, transform.position.y, transform.position.z), Quaternion.identity);

        // ���带 ���� ������Ʈ(�÷��̾�)�� �ڽ����� ����
        shield.transform.SetParent(transform);

        // ���� Ȱ��ȭ
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

        // ���� ���� �ð� �� ��Ȱ��ȭ �� ��Ÿ�� ����
        StartCoroutine(DisableShieldAfterDuration(shield));
        StartCoroutine(SetCooldown(() => canUseShield = true, 5.0f)); // 5�� ��Ÿ�� ����
    }

    IEnumerator DisableShieldAfterDuration(GameObject shield)
    {
        yield return new WaitForSeconds(shieldDuration);

        // ���� ��Ȱ��ȭ
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

        // ���� ��ü ����
        Destroy(shield);
    }

    // ��ȭ ���Ÿ� ����
    void ShootSword()
    {
        if (!canUseEnhancedRanged) return; // ��Ÿ�� ���̸� �������� ����

        // ��ų ��� ������ ����
        canUseEnhancedRanged = false;

        // ���Ÿ� �߻�ü ���� ��ġ
        Vector3 AntY = new Vector3(this.transform.position.x - 0.5f, this.transform.position.y - 0.5f, 0);

        // �߻�ü ����
        GameObject sword = Instantiate(swordPrefab, AntY, Quaternion.identity);

        // �߻�ü�� ProjectileScript �߰� �� �Ӽ� ����
        ProjectileScript projectileScript = sword.AddComponent<ProjectileScript>();
        projectileScript.maxDistance = 10.0f; // �����Ÿ� ����

        // �߻� ���� ����
        Rigidbody2D rb = sword.GetComponent<Rigidbody2D>();

        // �÷��̾��� ���� ���� ����(���� �Ǵ� ������)�� ���� �߻� ������ ����
        float direction = transform.localScale.x > 0 ? -1f : 1f; // localScale.x�� 0���� ũ�� ����(-1), ������ ������(1)
        rb.velocity = new Vector2(direction * 5f, 0f); // X�� �������� �ӵ� 

        sword.transform.localScale = new Vector3(direction, 1, 1); // �߻�ü �̹��� ���� 

        // **�÷��̾�� �߻�ü�� �浹 ����**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D projectileCollider = sword.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, projectileCollider);

        // 2�� �� ��Ÿ�� ����
        StartCoroutine(SetCooldown(() => canUseEnhancedRanged = true, 2.0f));
    }
}