using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

// �߻�ü �����Ÿ� Ŭ����(MonBehaviour)
public class ShootStingerc : MonoBehaviour
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

public class BeeController : MonoBehaviour
{
    Rigidbody2D rigid2D;
    Animator animator;
    float jumpForce = 700.0f;
    float walkForce = 30.0f;
    float maxWalkSpeed = 4.0f;

    float dashForce = 15.0f; // ����� ���� ��
    float dashDuration = 0.1f; // ��� ���� �ð�
    bool isDashing = false; // ��� ������ Ȯ��

    float doubleTapTimeD = 0.0f; // DŰ�� �� �� ������ �ð� ����
    float doubleTapTimeA = 0.0f; // AŰ�� �� �� ������ �ð� ����
    float doubleTapDelay = 0.3f; // �� �� ������ ���� �ð�

    int maxjumpcnt = 3;
    [SerializeField] int jumpcnt = 0;

    public GameObject stingerPrefab;

    float shieldDuration = 1.0f;
    public GameObject shieldPrefab;

    // ������ ���� ����
    public GameObject laserPrefab; // ������ ������
    GameObject currentLaser; // ���� Ȱ��ȭ�� �������� ����
    bool isFrozen = false; // ������ ���� �÷���
    Vector2 frozenPosition; // ������ ���� �� ���� ��ġ ����
    bool laserCanDamage = false; // �������� �������� �� �� �ִ� ��������


    // ��Ÿ�� ���� ����
    [SerializeField] private float attackUCooldown = 2f;
    [SerializeField] private float attackICooldown = 2f;
    [SerializeField] private float ShieldCooldown = 5f;

    private float lastattackUTime;
    private float lastattackITime;
    private float lastShieldTime;

    private bool isFirstAttackU = true;
    private bool isFirstAttackI = true;
    private bool isFirstShield = true;

    private bool laserAttackUsed = false; // ������ ���� ��� ���θ� �����ϴ� ����


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;   // 60������ ����
        this.rigid2D = GetComponent<Rigidbody2D>(); // rigid2D ��ü
        this.animator = GetComponent<Animator>();       // �ִϸ�����
                                                        // ��Ÿ�� ���� �ʱ�ȭ
        lastattackUTime = 0;
        lastattackITime = 0;
        lastShieldTime = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // D Ű ��� �Է� ����
        if (Input.GetKeyDown(KeyCode.D))
        {
            if (Time.time - doubleTapTimeD < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.right)); // ������ ���
            }
            doubleTapTimeD = Time.time;
        }

        // A Ű ��� �Է� ����
        if (Input.GetKeyDown(KeyCode.A))
        {
            if (Time.time - doubleTapTimeA < doubleTapDelay && !isDashing)
            {
                StartCoroutine(Dash(Vector2.left)); // ���� ���
            }
            doubleTapTimeA = Time.time;
        }

        // ����
        if (Input.GetKeyDown(KeyCode.W) && jumpcnt < maxjumpcnt)
        {
            jumpcnt++;
            this.rigid2D.AddForce(Vector2.up * jumpForce);
            this.rigid2D.velocity = new Vector2(rigid2D.velocity.x, 0.1f); // ���� y�ӵ��� �ʱ�ȭ�ؼ� �������� �� �� ������
        }

        if (jumpcnt == maxjumpcnt && rigid2D.velocity.y == 0) jumpcnt = 0; // ���� �����ϸ� ���� ī��Ʈ�� �ٽ� 0���� �ʱ�ȭ

        // ���Ÿ� ����(����, �� ����������)
        if (Input.GetKeyDown(KeyCode.K))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootStinger(0f, 0f, 10f);
        }

        // ��ȭ ���Ÿ� ����(����, �� ����������)
        if (Input.GetKeyDown(KeyCode.I) && (isFirstAttackI || Time.time - lastattackITime > attackICooldown))
        {
            this.animator.SetTrigger("SpitTrigger");
            ShootStinger(-0.2f, -0.1f, 7.5f);
            ShootStinger(0f, 0f, 7.5f);
            ShootStinger(-0.1f, 0.1f, 7.5f);
            lastattackITime = Time.time;
            isFirstAttackI = false;
        }

        // ���� ����
        if (Input.GetKeyDown(KeyCode.J))
        {
            this.animator.SetTrigger("AttackTrigger");
            PerformMeleeAttack(); // ���� ���� ����
        }

        // ��ȭ ���� ����
        if (Input.GetKeyDown(KeyCode.U) && (isFirstAttackU || Time.time - lastattackUTime > attackUCooldown))
        {
            StartCoroutine(DashAndAttack());
            lastattackUTime = Time.time;
            isFirstAttackU = false;
        }

        // LŰ �Է� ���� �� ������ ���� ��� ���� Ȯ��
        if (Input.GetKeyDown(KeyCode.L) && !isFrozen && !laserAttackUsed)
        {
            StartCoroutine(PerformLaserAttack());
            laserAttackUsed = true; // ������ ���� ��� ǥ��
        }

        if (isFrozen)
        {
            // ������ ���� �� ��ġ ����
            rigid2D.velocity = Vector2.zero; // �ӵ� ����
            rigid2D.position = frozenPosition; // ������ ��ġ�� ����
            return; // �̵� ó�� ����
        }

        //����
        if (Input.GetKeyDown(KeyCode.S) && (isFirstShield || Time.time - lastShieldTime > ShieldCooldown))
        {
            CreateShieldEffects();
            lastShieldTime = Time.time;
            isFirstShield = false;
        }

        // �¿� �̵� (��� ���� �ƴ� ����)
        if (!isDashing)
        {
            float key = 0f;
            if (Input.GetKey(KeyCode.D))
            {
                this.animator.SetTrigger("WalkTrigger");
                key = 1.0f; // ������ �̵�
            }
            if (Input.GetKey(KeyCode.A))
            {
                this.animator.SetTrigger("WalkTrigger");
                key = -1.0f; // ���� �̵�
            }
            // �÷��̾� �ӵ�
            float speedx = Mathf.Abs(this.rigid2D.velocity.x);

            // ���ǵ� ����
            if (speedx < this.maxWalkSpeed)
            {
                this.rigid2D.AddForce(transform.right * key * this.walkForce);
            }

            if (key != 0)
            {
                float direction = key > 0 ? 1 : -1;
                transform.localScale = new Vector3(0.7f * direction, 0.7f, 0.7f);
            }

            // �÷��̾� �ӵ��� ���� �ִϸ��̼� �ӵ��� �ٲ۴�.
            if (speedx != 0)
            {
                this.animator.speed = 2.0f;
            }
        }
    }

    void ShootStinger(float positionx, float positiony, float distance)
    {
        // ���Ÿ� �߻�ü ���� Y��ǥ
        Vector3 AntY = new Vector3(this.transform.position.x + positionx, this.transform.position.y - 0.6f + positiony, 0);

        // �߻�ü ����
        GameObject stinger = Instantiate(stingerPrefab, AntY, Quaternion.identity);

        // �߻�ü�� ProjectileScript �߰� �� �Ӽ� ����
        ShootStingerc projectileScript = stinger.AddComponent<ShootStingerc>();
        projectileScript.maxDistance = distance; // �����Ÿ� ���� (�ʿ信 ���� �� ���� ����)

        // �߻� ���� ����
        Rigidbody2D rb = stinger.GetComponent<Rigidbody2D>();

        // �÷��̾��� ���� ���� ����(���� �Ǵ� ������)�� ���� �߻� ������ ����
        float direction = transform.localScale.x > 0 ? 1f : -1f; // localScale.x�� 0���� ũ�� ����(-1), ������ ������(1)
        rb.velocity = new Vector2(direction * 5f, 0f); // X�� �������� �ӵ� 

        //�߻�ü ũ��� ���� ����
        stinger.transform.localScale = new Vector3(direction * 0.3f, 0.3f, 0.3f);

        // **�÷��̾�� �߻�ü�� �浹 ����**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D stingerCollider = stinger.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, stingerCollider);

    }

    IEnumerator Dash(Vector2 direction)
    {
        isDashing = true;
        rigid2D.velocity = Vector2.zero; // ���� �ӵ� �ʱ�ȭ
        rigid2D.AddForce(direction * dashForce, ForceMode2D.Impulse); // ��� �� ����

        yield return new WaitForSeconds(dashDuration); // ��� ���� �ð�

        isDashing = false; // ��� ���� ����
    }

    IEnumerator PerformLaserAttack()
    {
        // �ִϸ��̼� Ʈ���� ����
        this.animator.SetTrigger("UltiTrigger");

        // ������ ���� ����
        isFrozen = true;
        frozenPosition = rigid2D.position; // ���� ��ġ ����

        // ������ ����
        CreateLaser();

        // 1�� �Ŀ� ������ Ȱ��ȭ
        yield return new WaitForSeconds(1.0f);
        laserCanDamage = true;

        // 3�� �� ������ ����
        yield return new WaitForSeconds(1.0f); // �� 2�� (1�� + 1��)
        laserCanDamage = false;

        // ������ ���� ���� ��
        laserCanDamage = false;
        isFrozen = false; // ������ ���� ����
        if (currentLaser != null)
        {
            Destroy(currentLaser);
        }

        // ������ ���� ��� �Ϸ� ǥ�� (�̹� Update���� ����������, Ȯ���� �ϱ� ���� ���⼭�� ����)
        laserAttackUsed = true;
    }

    void CreateLaser()
    {

        // ���� �������� �ִٸ� ����
        if (currentLaser != null)
        {
            Destroy(currentLaser);
        }

        // ĳ���� ���⿡ ���� ������ ��ġ ����
        Vector3 laserPosition;
        float direction = transform.localScale.x > 0 ? 1f : -1f; // ���� Ȯ�� (������: 1, ����: -1)

        if (direction > 0)
        {
            // �������� �� ��
            laserPosition = new Vector3(this.transform.position.x + 10f, this.transform.position.y - 0.5f, 0);
        }
        else
        {
            // ������ �� ��
            laserPosition = new Vector3(this.transform.position.x - 10f, this.transform.position.y - 0.5f, 0);
        }

        // ������ ����
        currentLaser = Instantiate(laserPrefab, laserPosition, Quaternion.identity);

        // ������ ũ�⸦ ȭ�� ������ Ȯ��
        currentLaser.transform.localScale = new Vector3(2.0f * direction, 1.0f, 1.0f); // ���⿡ ���� X�� ũ�� ����
        // ������ Collider2D ����
        Collider2D laserCollider = currentLaser.GetComponent<Collider2D>();
        if (laserCollider != null)
        {
            laserCollider.isTrigger = true; // Ʈ���� Ȱ��ȭ
        }
        // **�÷��̾�� �߻�ü�� �浹 ����**
        Collider2D playerCollider = GetComponent<Collider2D>();
        Collider2D currentLaserCollider = currentLaser.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(playerCollider, currentLaserCollider);
    }

    IEnumerator DashAndAttack()
    {
        // ���� �÷��̾ �ٶ󺸴� �������� ���
        Vector2 dashDirection = transform.localScale.x > 0 ? Vector2.right : Vector2.left;
        yield return StartCoroutine(Dash(dashDirection));

        // ��ȭ ���� ���� ����
        this.animator.SetTrigger("AAttackTrigger");
        PerformMeleeAttack(true); // ��ȭ ���� ���� ����
    }

    // ���� ���� ����
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

    void PerformMeleeAttack(bool isEnhanced = false)
    {
        // ���� ������ �߽� ����
        float attackWidth = isEnhanced ? 0.7f : 0.7f; // ��ȭ ���� �� �� ���� ����
        float attackHeight = 1.0f;
        float damage = isEnhanced ? 0.2f : 0.1f; // ��ȭ ���� �� �� ū ������

        // ĳ���� ���⿡ ���� ���� �߽� ���
        float direction = transform.localScale.x > 0 ? 1 : -1;
        Vector2 attackCenter = new Vector2(transform.position.x + direction * (attackWidth / 2), transform.position.y);

        // ���� ���� ����
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
                    GameObject director = GameObject.Find("GameDirector");
                    if (director != null)
                    {
                        GameDirector gameDirector = director.GetComponent<GameDirector>();
                        if (gameDirector != null)
                        {
                            if (target.CompareTag("Player1"))
                            {
                                gameDirector.DecreaseHP1(damage); // Player1 ������ ����
                            }
                            else if (target.CompareTag("Player2"))
                            {
                                gameDirector.DecreaseHP2(damage); // Player2 ������ ����
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
            isEnhanced ? Color.yellow : Color.red, // ��ȭ ������ �����, �Ϲ� ������ ������
            0.1f
        );
    }

    void CreateShieldEffects()
    {
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

        // ���� ���� �ð� �� ��Ȱ��ȭ
        StartCoroutine(DisableShieldAfterDuration(shield));
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
}