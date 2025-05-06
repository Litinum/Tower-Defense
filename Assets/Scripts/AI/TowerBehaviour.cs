using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{

    [SerializeField] GameObject projectilePrefab;
    [Range(20,80)]
    [SerializeField] double damage = 2;
    [Range(0.1f, 0.8f)]
    [SerializeField] float attackSpeed = 0.5f;
    [SerializeField] AttackPriority priority = AttackPriority.Nearest;
    [SerializeField] StatusEffect appliesStatusEffect = StatusEffect.None;
    [SerializeField] bool isMultiAttack = false;
    [Range(2,5)]
    [SerializeField] int maxMultiAttackTargets = 1;
    [SerializeField] private int startingBuildCost = 30;
    [SerializeField] private double postBuildCostIncreasePerc = 0.1;

    private bool hasTarget;
    private float elapsedTime;
    private GameObject target;
    private List<GameObject> targetsInRange;
    private int buildCost;

    void Start()
    {
        targetsInRange = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (hasTarget && elapsedTime > attackSpeed)
        {
            if (isMultiAttack)
                MultiAttack();
            else
                AttackTarget();

            elapsedTime = 0;
        }

        elapsedTime += Time.deltaTime;
    }

    void MultiAttack()
    {
        List<GameObject> localTargetsInRange = targetsInRange.ToList();

        if (targetsInRange.Count == 0)
        {
            hasTarget = false;
            return;
        }

        for(int i = 0; i < maxMultiAttackTargets; i++)
        {
            target = GetNextTarget(localTargetsInRange);

            if (target == null)
                return;
            else
                hasTarget = true;

            localTargetsInRange.Remove(target);

            ShootProjectile();

        }
    }

    void AttackTarget()
    {
        if (targetsInRange.Count == 0)
        {
            hasTarget = false;
            return;
        }

        target = GetNextTarget(targetsInRange);

        if (target == null)
            return;
        else
            hasTarget = true;

        ShootProjectile();
    }

    void ShootProjectile()
    {
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Projectile projectileComp = projectile.GetComponent<Projectile>();
        projectileComp.damage = damage;
        projectileComp.targetGameObject = target;
        projectileComp.projectileStatusEffect = appliesStatusEffect;
    }

    GameObject GetNextTarget(List<GameObject> localTargetsInRange)
    {
        if (targetsInRange.Count == 0)
            return null;

        switch(priority)
        { 
            case AttackPriority.Nearest:
                return FindNearestTarget(localTargetsInRange);
            case AttackPriority.Farthest:
                return FindFarthestTarget(localTargetsInRange);
            case AttackPriority.HighestHealth:
                return FindHighestHealthTarget(localTargetsInRange);
            case AttackPriority.LowestHealth:
                return FindLowestHealthTarget(localTargetsInRange);
        }

        return null;
    }

    GameObject FindNearestTarget(List<GameObject> localTargetsInRange)
    {
        double minDistance = double.MaxValue;
        double distance;
        GameObject nearestTarget = null;

        foreach(GameObject localTarget in localTargetsInRange)
        {
            distance = GetDistanceToTarget(localTarget);

            if (distance < minDistance)
            {
                minDistance = distance;
                nearestTarget = localTarget;
            }
        }

        return nearestTarget;
    }

    GameObject FindFarthestTarget(List<GameObject> localTargetsInRange)
    {
        double maxDistance = 0;
        double distance;
        GameObject farthestTarget = null;

        foreach (GameObject localTarget in localTargetsInRange)
        {
            distance = GetDistanceToTarget(localTarget);

            if (distance > maxDistance)
            {
                maxDistance = distance;
                farthestTarget = localTarget;
            }
        }

        return farthestTarget;
    }

    double GetDistanceToTarget(GameObject target)
    {
        return Vector3.Distance(transform.position, target.transform.position);
    }

    GameObject FindLowestHealthTarget(List<GameObject> localTargetsInRange)
    {
        double minHealth = double.MaxValue;
        Stats stats;
        GameObject lowestHealthTarget = null;

        foreach(GameObject localTarget in localTargetsInRange)
        {
            stats = localTarget.GetComponent<Stats>();

            if (stats.currHealth < minHealth)
            {
                minHealth = stats.currHealth;
                lowestHealthTarget = localTarget;
            }
        }

        return lowestHealthTarget;
    }

    GameObject FindHighestHealthTarget(List<GameObject> localTargetsInRange)
    {
        double maxHealth = 0;
        Stats stats;
        GameObject HighestHealthTarget = null;

        foreach (GameObject localTarget in localTargetsInRange)
        {
            stats = localTarget.GetComponent<Stats>();

            if (stats.currHealth > maxHealth)
            {
                maxHealth = stats.currHealth;
                HighestHealthTarget = localTarget;
            }
        }

        return HighestHealthTarget;
    }

    public int GetBuildCost()
    {
        return buildCost;
    }

    public void IncreaseCost()
    {
        buildCost += (int)(buildCost * postBuildCostIncreasePerc);
    }

    public void ResetTowerCost()
    {
        buildCost = startingBuildCost;
    }

    void SubscribeToTargetStats(GameObject localTarget)
    {
        Stats targetStats = localTarget.GetComponent<Stats>();
        targetStats.onDestroy += OnTargetDestroyed;
    }

    void UnsubscribeToTargetStats(GameObject localTarget)
    {
        Stats targetStats = localTarget.GetComponent<Stats>();
        targetStats.onDestroy -= OnTargetDestroyed;
    }

    void OnTriggerEnter(Collider collider)
    {
        targetsInRange.Add(collider.gameObject);
        SubscribeToTargetStats(collider.gameObject);
        hasTarget = true;
    }

    void OnTriggerExit(Collider collider)
    {
        targetsInRange.Remove(collider.gameObject);
        UnsubscribeToTargetStats(collider.gameObject);

        if (targetsInRange.Count == 0)
            hasTarget = false;
    }

    void OnTargetDestroyed(GameObject localTarget)
    {
        targetsInRange.Remove(localTarget);
    }
}
