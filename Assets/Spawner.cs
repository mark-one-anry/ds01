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

    // Start is called before the first frame update
    void Start()
    {
        timer = TimeToSpawn;
        CanSpawn = false;

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

            Collider2D coll = Physics2D.OverlapCircle(transform.position, RadiusSpawn, EnemyLayer);
            if (coll is null) {CanSpawn = true;}
            else {CanSpawn = false;}
               

 

        }
    }
}