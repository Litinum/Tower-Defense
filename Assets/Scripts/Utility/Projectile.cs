using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public double damage = 2;
    public float projectileSpeed = 1000;
    public GameObject targetGameObject;
    public StatusEffect projectileStatusEffect = StatusEffect.None;

    private void Update()
    {
        if (targetGameObject == null)
        {
            Destroy(gameObject);
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, targetGameObject.transform.position, projectileSpeed * Time.deltaTime * 10);
    }


    private void OnCollisionEnter(Collision collision)
    {
        if((collision.gameObject.tag == "Enemy") && (collision.gameObject.GetInstanceID() == targetGameObject.GetInstanceID()))
        {
            Stats targetStats = collision.gameObject.GetComponent<Stats>();
            targetStats.TakeDamage(damage);
            targetStats.ApplyStatusEffect(projectileStatusEffect);

            Destroy(gameObject);
        }
    }
}
