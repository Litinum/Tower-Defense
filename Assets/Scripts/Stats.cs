using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Stats: MonoBehaviour
{
    [Range(80, 400)]
    [SerializeField] private double maxHealth;
    [Range(30,100)]
    [SerializeField] private double maxShield;
    [Range(0,30)]
    [SerializeField] private double shieldRegenRate;
    [Range(1, 3)]
    [SerializeField] private double maxSpeed;
    [Range(1,7)]
    [SerializeField] private int goldDrop = 3;
    [SerializeField] private StatusEffect appliedStatusEffect;


    public delegate void OnDestroyObject(GameObject obj);
    public event OnDestroyObject onDestroy;
    public double currHealth { get; private set; }
    public double currSpeed { get; private set; }
    public double currShield { get; private set; }


    private Movement movementComp;
    private double statusEffectDuration, statusEffectValue;
    private double elapsedTime;
    private double timeSenseLastAttack;


    void Start()
    {
        currHealth = maxHealth;
        currShield = maxShield;
        currSpeed = maxSpeed;
        appliedStatusEffect = StatusEffect.None;

        movementComp = gameObject.GetComponent<Movement>();

        if (movementComp != null)
        {
            movementComp.speed = currSpeed;
        }
    }

    void Update()
    {

        if (appliedStatusEffect != StatusEffect.None)
        {
            if(statusEffectValue == 0)
            {
                (statusEffectValue, statusEffectDuration) = Damage.GetStatusEffectValues(appliedStatusEffect);
                elapsedTime = Damage.DamageOvertimeInterval;
            }

            ApplyStatusEffectValue();

            elapsedTime += Time.deltaTime;
            statusEffectDuration -= Time.deltaTime;

            if(statusEffectDuration <= 0)
            {
                appliedStatusEffect = StatusEffect.None;
                statusEffectValue = 0;
                ApplyStatusEffectValue();
            }
        }

        RechargeShield();
    }

    public void TakeDamage(double damageAmount)
    {
        if(currShield > 0)
        {
            if(currShield - damageAmount < 0)
            {
                damageAmount -= currShield;
                currShield = 0;
            }
            else
            {
                currShield -= damageAmount;
                damageAmount = 0;
            }
        }
        

        if(damageAmount > 0)
        {
            currHealth -= damageAmount;

            if (currHealth <= 0)
                Destroy(gameObject);
        }

        timeSenseLastAttack = 0;
    }

    public void ApplyStatusEffect(StatusEffect statusEffect)
    {
        if (appliedStatusEffect != StatusEffect.None)
            return;

        appliedStatusEffect = statusEffect;
    }

    private void ApplyStatusEffectValue()
    {
        switch (appliedStatusEffect)
        {
            case StatusEffect.None:         // Reset values
                ChangeMovementSpeed(maxSpeed);
                break;
            case StatusEffect.Fire:
                if(elapsedTime >= Damage.DamageOvertimeInterval)
                {
                    TakeDamage(statusEffectValue);
                    elapsedTime = 0;
                }
                break;
            case StatusEffect.Frost:
                if (currSpeed == maxSpeed)
                {
                    double newMovementSpeed = currSpeed - (currSpeed * statusEffectValue);

                    ChangeMovementSpeed(newMovementSpeed);
                }
                break;
        }
    }

    private void ChangeMovementSpeed(double amount)
    {
        currSpeed = amount;
        movementComp.speed = currSpeed;
    }

    private void RechargeShield()
    {
        if (maxShield == 0)
            return;

        if (currShield == maxShield)
            return;

        if(timeSenseLastAttack > 1.5f)
            currShield = Mathf.Clamp((float)(currShield + shieldRegenRate * Time.deltaTime), 0, (float)maxShield);

        timeSenseLastAttack += Time.deltaTime;
    }

    void OnDisable()
    {
        onDestroy?.Invoke(gameObject);
        PlayerStats.Instance.ReceiveGold(goldDrop);
    }
}

