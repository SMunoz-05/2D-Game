using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float speed = 8f;
    public float lifetime = 4f;
    private Vector2 direction;
    private int damage;

    public void Setup(Vector2 targetDirection, int dmg)
    {
        direction = targetDirection;
        damage = dmg;
        Destroy(gameObject, lifetime); // Evitar basura en memoria si falla el tiro
    }

    private void Update()
    {
        // Se desplaza linealmente en la dirección calculada
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }
            Destroy(gameObject); // Se destruye al impactar al jugador
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Suelo"))
        {
            Destroy(gameObject); // Se destruye al chocar contra paredes o pisos
        }
    }
}