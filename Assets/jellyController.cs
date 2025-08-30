using UnityEngine;

public class jellyController : MonoBehaviour
{
    public float minX = -9.5f;
    public float maxX = 9.5f;
    public float spawnY = 5.5f;
    public float fallSpeed = 5f;
    public float lifetime = 10f;
    private bool isGrounded = false;
    private float groundY = -2.3f;

    void Start()
    {
        float randomX = Random.Range(minX, maxX);
        transform.position = new Vector3(randomX, spawnY, 0);
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        if (!isGrounded)
        {
            transform.Translate(Vector3.down * fallSpeed * Time.deltaTime);
        }

        if (transform.position.y < -5.5f)
        {
            NotifyGenerator();
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "background1_step_1" ||
            collision.gameObject.name == "background1_step_2")
        {
            isGrounded = true;
            transform.position = new Vector3(
                transform.position.x,
                collision.transform.position.y + 0.6f,
                transform.position.z
            );
        }
        else if (collision.gameObject.name == "background1_step_under")
        {
            isGrounded = true;
            transform.position = new Vector3(transform.position.x, groundY, transform.position.z);
        }
        else if (collision.gameObject.name == "flower_step")
        {
            isGrounded = true;
            transform.position = new Vector3(transform.position.x, groundY + 3.0f, transform.position.z);
        }
        // PlayerTag1 �Ǵ� PlayerTag2�� �浹���� ��
        else if (collision.gameObject.CompareTag("Player1"))
        {
            GameDirector gameDirector = FindObjectOfType<GameDirector>();
            if (gameDirector != null)
            {
                gameDirector.IncreaseHP1(0.1f); // Player1 ü�� ����
            }
            NotifyGenerator();
            Destroy(gameObject); // ���� ����
        }
        else if (collision.gameObject.CompareTag("Player2"))
        {
            GameDirector gameDirector = FindObjectOfType<GameDirector>();
            if (gameDirector != null)
            {
                gameDirector.IncreaseHP2(0.1f); // Player2 ü�� ����
            }
            NotifyGenerator();
            Destroy(gameObject); // ���� ����
        }
    }

    private void NotifyGenerator()
    {
        // JellyGenerator�� ������ �ı��Ǿ����� �˸�
        jellyGenerator generator = FindObjectOfType<jellyGenerator>();
        if (generator != null)
        {
            generator.JellyDestroyed();
        }
    }
}
