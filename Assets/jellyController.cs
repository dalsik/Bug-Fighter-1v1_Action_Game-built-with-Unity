using UnityEngine;

public class jellyController : MonoBehaviour
{
    public float minX = -9.5f;
    public float maxX = 9.5f;
    public float spawnY = 5.5f;
    public float fallSpeed = 5f;
    public float lifetime = 10f;
    public float healAmount = 0.1f;
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
        // 캐릭터와 닿으면 그 캐릭터의 체력을 회복 — 1P인지 2P인지 몰라도 된다
        else
        {
            Health health = collision.GetComponentInParent<Health>();
            if (health != null)
            {
                health.Heal(healAmount);
                NotifyGenerator();
                Destroy(gameObject); // 젤리 제거
            }
        }
    }

    private void NotifyGenerator()
    {
        // JellyGenerator에 젤리가 파괴되었음을 알림
        jellyGenerator generator = FindObjectOfType<jellyGenerator>();
        if (generator != null)
        {
            generator.JellyDestroyed();
        }
    }
}
