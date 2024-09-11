using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class FPSBot : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float detectionRange = 20f;
    public float shootingRange = 15f;
    public float guardAreaRadius = 10f;
    public float agentStoppingDistance = 15;
    public float shootingInterval = 0.5f;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public int poolSize = 25;

    private NavMeshAgent agent;
    private Transform player;
    private bool isPlayerInRange = false;
    private float lastShootTime;
    private List<GameObject> projectilePool;
    public int projectileForce=1500;

    public int magazineSize = 6;
    public float reloadTime = 2f;
    private int currentAmmo;
    private bool isReloading = false;


    public List<Transform> patrolPoints;
    private int currentPatrolIndex = 0;

    public enum BotBehavior
    {
        Wander,
        GuardArea,
        Patrol,
    }

    public BotBehavior currentBehavior = BotBehavior.Wander;

    private Vector3 guardPosition;


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        agent.speed = moveSpeed;
        guardPosition = transform.position;
        currentAmmo = magazineSize;
        InitializeProjectilePool();
        StartCoroutine(BotBehaviorCoroutine());
    }

    void InitializeProjectilePool()
    {
        projectilePool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            GameObject projectile = Instantiate(projectilePrefab);
            projectile.SetActive(false);
            projectilePool.Add(projectile);
        }
    }

    GameObject GetProjectileFromPool()
    {
        foreach (GameObject projectile in projectilePool)
        {
            if (!projectile.activeInHierarchy)
            {
                projectile.SetActive(true);
                return projectile;
            }
        }
        // If all projectiles are in use, create a new one and add it to the pool
        GameObject newProjectile = Instantiate(projectilePrefab);
        projectilePool.Add(newProjectile);
        return newProjectile;
    }

    IEnumerator BotBehaviorCoroutine()
    {
        while (true)
        {
            if (isPlayerInRange)
            {
                Chase();
            }
            else
            {
                // Choose behavior when player is not in range
                switch (currentBehavior)
                {
                    case BotBehavior.Wander:
                        Wander();
                        break;
                    case BotBehavior.GuardArea:
                        GuardArea();
                        break;
                    case BotBehavior.Patrol:
                        Patrol();
                        break;
                }
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    void Update()
    {
        // Check if player is in detection range
        isPlayerInRange = Vector3.Distance(transform.position, player.position) <= detectionRange;
    }

    void Shoot()
    {
        if (isReloading) return;

        if (currentAmmo > 0 && Time.time - lastShootTime >= shootingInterval)
        {
            GameObject projectile = GetProjectileFromPool();
            projectile.transform.position = firePoint.position;
            projectile.transform.rotation = firePoint.rotation * Quaternion.Euler(90, 0, 0);

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero; // Reset velocity
            rb.angularVelocity = Vector3.zero; // Reset angular velocity
            rb.AddForce(firePoint.forward * projectileForce);

            lastShootTime = Time.time;
            currentAmmo--;

            StartCoroutine(DisableProjectileAfterTime(projectile, 5f)); // Disable after 5 seconds

            if (currentAmmo == 0)
            {
                StartCoroutine(Reload());
            }
        }
        else if (currentAmmo == 0 && !isReloading)
        {
            StartCoroutine(Reload());
        }
    }
    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        isReloading = false;
        Debug.Log("Reload complete!");
    }

    IEnumerator DisableProjectileAfterTime(GameObject projectile, float time)
    {
        yield return new WaitForSeconds(time);
        projectile.SetActive(false);
    }

    private void Chase()
    {
        // Face the player
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        // Calculate distance to the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // If distance is greater than 5 units, move towards the player
        if (distanceToPlayer > agentStoppingDistance)
        {
            Vector3 targetPosition = player.position - direction * agentStoppingDistance;
            agent.SetDestination(targetPosition);
        }
        else
        {
            // Stop moving if within 5 units
            agent.ResetPath();
        }
        // Shoot if in range
        if (Vector3.Distance(transform.position, player.position) <= shootingRange)
        {
            Shoot();
        }
    }

    void Wander()
    {
        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomDirection = Random.insideUnitSphere * 10f;
            randomDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomDirection, out hit, 10f, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
        }
    }

    void GuardArea()
    {
        if (Vector3.Distance(transform.position, guardPosition) > guardAreaRadius)
        {
            agent.SetDestination(guardPosition);
        }
        else if (agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 randomPoint = Random.insideUnitSphere * guardAreaRadius;
            randomPoint += guardPosition;
            NavMeshHit hit;
            NavMesh.SamplePosition(randomPoint, out hit, guardAreaRadius, NavMesh.AllAreas);
            agent.SetDestination(hit.position);
        }
    }


    void Patrol()
    {
        if (patrolPoints.Count == 0)
        {
            Debug.LogWarning("No patrol points set for FPSBot.");
            return;
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Count;
            agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        }
    }
}