using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndTile : MonoBehaviour
{
    private PlayerStats playerStats;

    void Start()
    {
        playerStats = PlayerStats.Instance;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Enemy")
        {
            playerStats.HealthLoss();
        }
    }
}
