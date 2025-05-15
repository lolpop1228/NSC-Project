using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float followSpeed = 4f;
    public float rotationSpeed = 10f;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float attackDuration = 0.5f;
    public float damage = 10f;

    [Header("Separation Settings")]
    public float separationRadius = 1.5f;
    public float separationForce = 1f;

    [Header("Roaming Around Player")]
    public float verticalThreshold = 2f; // How high above the enemy the player must be to trigger roaming
    public float roamRadius = 3f;         // Radius around player to roam
    public float roamInterval = 2f;       // How often to pick a new roam point

    private Transform player;
    private NavMeshAgent agent;
    private float lastAttackTime;
    private Vector3 roamOffset;
    private float lastRoamTime;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        agent = GetComponent<NavMeshAgent>();
        agent.speed = followSpeed;
        agent.acceleration = 999f;
        agent.angularSpeed = 999f;
        agent.updateRotation = false;

        agent.avoidancePriority = Random.Range(30, 60);
    }

    void Update()
    {
        if (player == null || agent == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        float verticalDifference = player.position.y - transform.position.y;

        if (verticalDifference > verticalThreshold)
        {
            // Player is above — roam around the base
            RoamAroundPlayer();
        }
        else if (distanceToPlayer > attackRange)
        {
            if (agent.isStopped) agent.isStopped = false;

            agent.SetDestination(player.position);

            if (agent.pathStatus != NavMeshPathStatus.PathComplete)
            {
                agent.ResetPath();
                return;
            }
        }
        else
        {
            if (!agent.isStopped) agent.isStopped = true;

            if (Time.time - lastAttackTime >= attackDuration)
            {
                Attack();
                lastAttackTime = Time.time;
            }
        }

        RotateTowardsPlayer();
        ApplySeparation();
    }

    void RotateTowardsPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0f;

        if (direction.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void ApplySeparation()
    {
        Collider[] neighbors = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 separationDir = Vector3.zero;
        int count = 0;

        foreach (Collider neighbor in neighbors)
        {
            if (neighbor.gameObject != gameObject && neighbor.CompareTag("Enemy"))
            {
                Vector3 away = transform.position - neighbor.transform.position;
                if (away.magnitude > 0f)
                {
                    separationDir += away.normalized / away.magnitude;
                    count++;
                }
            }
        }

        if (count > 0)
        {
            separationDir /= count;
            Vector3 velocity = agent.velocity + separationDir * separationForce;

            // Clamp to max speed
            if (velocity.magnitude > agent.speed)
            {
                velocity = velocity.normalized * agent.speed;
            }

            agent.velocity = velocity;
        }
    }

    void Attack()
    {
        Debug.Log("Kuy");
    }

    void RoamAroundPlayer()
    {
        if (Time.time - lastRoamTime >= roamInterval)
        {
            // Pick a random direction around the player horizontally
            Vector2 randomCircle = Random.insideUnitCircle.normalized * roamRadius;
            roamOffset = new Vector3(randomCircle.x, 0f, randomCircle.y);
            lastRoamTime = Time.time;
        }

        Vector3 roamTarget = new Vector3(player.position.x, transform.position.y, player.position.z) + roamOffset;

        if (agent.isStopped) agent.isStopped = false;
        agent.SetDestination(roamTarget);
    }
}
