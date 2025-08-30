using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour
{
    private bool hasCollided = false; // �浹 ���θ� Ȯ���ϴ� ����
    private float destroyDelay = 3.0f; // �������� �����Ǵ� �ð� (��)
    public float knockbackForce = 10.0f; // �˹��� ����
    public float knockbackDuration = 0.2f; // �˹� ���� �ð�
    void Start()
    {
        // �ʿ� �� �ʱ� ���� ����
    }

    void Update()
    {
        // �ʿ� �� �� ������ ó�� ����
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // �̹� �浹 ó���� ��� �� �̻� ó������ ����
        if (hasCollided)
            return;
        float direction = transform.localScale.x > 0 ? 1f : -1f;
        // �浹�� ����� ��� �÷��̾����� �±׷� Ȯ��
        if (collision.CompareTag("Player2")) // ��� ĳ���Ϳ� "Player2" �±� ����
        {
            GameObject director = GameObject.Find("GameDirector");
            director.GetComponent<GameDirector>().DecreaseHP2(0.2f); // ��� ĳ������ HP ���� ó��
            ApplyKnockback(collision.gameObject, direction); // �˹� ȿ�� �߰� (-1�� ���� ����)
            HandleCollisionEffect(); // �浹 ȿ�� ó��
        }
        else if (collision.CompareTag("Player1")) // ��� ĳ���Ϳ� "Player1" �±� ����
        {
            GameObject director = GameObject.Find("GameDirector");
            director.GetComponent<GameDirector>().DecreaseHP1(0.2f); // ��� ĳ������ HP ���� ó��
            ApplyKnockback(collision.gameObject, direction); // �˹� ȿ�� �߰� (-1�� ���� ����)
            HandleCollisionEffect(); // �浹 ȿ�� ó��
        }
    }

    private void HandleCollisionEffect()
    {
        // �浹 ó�� �� ���� �ð� ���� ����
        hasCollided = true; // �浹 ���� ����
        StartCoroutine(DestroyAfterDelay()); // ���� �ð� �� ������ ����
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay); // 3�� ���
        Destroy(gameObject); // ������ ����
    }

    void ApplyKnockback(GameObject target, float direction)
    {
        Rigidbody2D targetRigidBody = target.GetComponent<Rigidbody2D>();

        if (targetRigidBody != null)
        {
            // �˹� ���� ���� (�ణ ���� �и��� ȿ�� ����)
            Vector2 knockbackDirection = new Vector2(direction, 0.5f).normalized;

            // ���� �ӵ� �ʱ�ȭ �� �˹� ����
            targetRigidBody.velocity = Vector2.zero;
            targetRigidBody.AddForce(knockbackDirection * knockbackForce, ForceMode2D.Impulse);

            // �˹� �� �÷��̾� ��Ʈ�� ����
            StartCoroutine(DisableControlForDuration(target, knockbackDuration));
        }
    }

    IEnumerator DisableControlForDuration(GameObject target, float duration)
    {
        PlayerController controller = target.GetComponent<PlayerController>();

        if (controller != null)
        {
            controller.enabled = false; // �÷��̾� ��Ʈ�� ��Ȱ��ȭ
            yield return new WaitForSeconds(duration);
            controller.enabled = true; // �÷��̾� ��Ʈ�� ��Ȱ��ȭ
        }
    }

}
