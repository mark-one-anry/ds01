using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{

    //public EnemyBase EnemyPrefab;
    public GameObject EnemyPrefab;
    public float TimeToSpawn, RadiusSpawn;
    public LayerMask EnemyLayer;

    private float timer;
    private bool CanSpawn;
    private Transform enemy;
    private LayerMask PlayerLayer;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0;
        CanSpawn = false;
        PlayerLayer = LayerMask.GetMask("Player");

    }

    // Update is called once per frame
    void Update()
    {
        timer -= Time.deltaTime;



        if (timer <= 0)
        {
            if (CanSpawn) {
                //EnemyBase Enemy = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);
                GameObject Enemy = Instantiate(EnemyPrefab, transform.position, Quaternion.identity);
                timer = TimeToSpawn;
                CanSpawn = false;
            }

            Collider2D collEnemy = Physics2D.OverlapCircle(transform.position, RadiusSpawn + 3, EnemyLayer);
            Collider2D collPlayer = Physics2D.OverlapCircle(transform.position, RadiusSpawn - 1, PlayerLayer);
            if (collEnemy is null && collPlayer is null) { CanSpawn = true;}
            else {CanSpawn = false;} 
               

 

        }
    }
}