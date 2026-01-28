using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using _MessageType;

public class EnemiesIA : MonoBehaviour
{
    public enum State { Patrol, Chasing, Attack, Dead }
    public State currentState = State.Patrol;
    [SerializeField] float MESSAGE_SEND_DELAY = 0.01f;

    [SerializeField] float attackRange = 10f;
    [SerializeField] float attackNexoRange = 30f;
    [SerializeField] float visionRange = 15f;
    [SerializeField] float attackCoolDown = 2f;
    [SerializeField] GameObject projectilePrefab;
    [SerializeField] private GameObject muzzleFlashPrefab; // Prefab del efecto visual

    [SerializeField] Transform barrel_Left;
    [SerializeField] Transform barrel_Right;

    [SerializeField] int HP = 3;

    private Animator animator; // Referencia al Animator
    private Transform nexo;
    private GameObject[] players;
    private NavMeshAgent agent;
    private float lastAttackTime = 0f;
    private Transform target = null;
    private bool isDead = false;
    private bool isInNexoCollider = false;

    private int enemyID;


    private void Start()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
        nexo = GameObject.FindGameObjectWithTag("Nexo").transform;
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>(); // Inicializar el Animator
        currentState = State.Patrol;

       // StartCoroutine(SendMyState());
    }

    private void Update()
    {
        if (isDead) return; // No hacer nada si est・muerto

        switch (currentState)
        {
            case State.Patrol:
                Patrol();
                break;
            case State.Chasing:
                Chase();
                break;
            case State.Attack:
                Attack();
                break;
        }
    }

    void Patrol()
    {
        target = FindNearestPlayerInRange(visionRange);
        agent.destination = target.position;

        animator.SetBool("isWalking", true); // Activar animaci de caminar
        animator.SetBool("isShooting", false);

        if (target != null)
        {
            currentState = State.Chasing;
        }
    }

    void Chase()
    {
        agent.destination = target.position;

        if (Vector3.Distance(target.position, transform.position) < attackRange)
        {
            currentState = State.Attack;
            return;
        }
        if (Vector3.Distance(nexo.position, transform.position) < attackNexoRange)
        {
            target = nexo;
            currentState = State.Attack;
        }


        else if (Vector3.Distance(target.position, transform.position) > visionRange)
        {
            animator.SetBool("isWalking", true); // Activar animaci de caminar
            animator.SetBool("isShooting", false);
            currentState = State.Patrol;
        }
    }


    private void OnDrawGizmosSelected()
    {

        // Cambia el color para el rango de visión
        Gizmos.color = Color.green;
        // Dibuja un círculo para el rango de visión
        Gizmos.DrawWireSphere(transform.position, attackNexoRange);
        // Cambia el color para el rango de visión
        Gizmos.color = Color.blue;
        // Dibuja un círculo para el rango de visión
        Gizmos.DrawWireSphere(transform.position, visionRange);

        // Cambia el color para el rango de ataque
        Gizmos.color = Color.red;
        // Dibuja un círculo para el rango de ataque
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    void Attack()
    {
        agent.isStopped = true;
        animator.SetBool("isWalking", false);
        animator.SetBool("isShooting", true); // Activar animacion de disparo

        if (Time.time > lastAttackTime + attackCoolDown)
        {
            Shoot();

            lastAttackTime = Time.time;
        }
        if (Vector3.Distance(target.position, transform.position) < attackNexoRange)
        {
            return;
        }

        if (Vector3.Distance(target.position, transform.position) > attackRange)
        {
            animator.SetBool("isWalking", true); // Activar animaci de caminar
            animator.SetBool("isShooting", false);
            currentState = State.Patrol;
            agent.isStopped = false;
        }
    }

    void Shoot()
    {
        if (target == null) return;

        // Forzar la reproducción de la animación de disparo desde el inicio

        // Activar el booleano isShooting
        animator.SetBool("isShooting", true);

        // Calcula la dirección hacia el objetivo para ambos barrels
        Vector3 directionL = (target.position - barrel_Left.position).normalized;
        Vector3 directionR = (target.position - barrel_Right.position).normalized;

        // Efecto de muzzle flash para el barrel izquierdo
        if (muzzleFlashPrefab != null)
        {
            GameObject muzzleFlashL = Instantiate(muzzleFlashPrefab, barrel_Left.position, barrel_Left.rotation * Quaternion.Euler(0, 180, 0), barrel_Left);
            Destroy(muzzleFlashL, 0.5f); // Duración del efecto

            GameObject muzzleFlashR = Instantiate(muzzleFlashPrefab, barrel_Right.position, barrel_Right.rotation * Quaternion.Euler(0, 180, 0), barrel_Right);
            Destroy(muzzleFlashR, 0.5f); // Duración del efecto
        }

        // Crear las balas desde ambos barrels
        GameObject bulletL = Instantiate(projectilePrefab, barrel_Left.position, Quaternion.LookRotation(directionL));
        GameObject bulletR = Instantiate(projectilePrefab, barrel_Right.position, Quaternion.LookRotation(directionR));

        // Configurar propiedades de las balas
        bulletController bulletControllerL = bulletL.GetComponent<bulletController>();
        bulletControllerL.target = target.position;
        bulletControllerL.hit = true;
        bulletControllerL.original = true;
        bulletControllerL.isEnemy = true;

        bulletController bulletControllerR = bulletR.GetComponent<bulletController>();
        bulletControllerR.target = target.position;
        bulletControllerR.hit = true;
        bulletControllerR.original = true;
        bulletControllerR.isEnemy = true;
    }



    public void Die()
    {
        isDead = true;
        currentState = State.Dead;
        animator.SetBool("isDead", true ); // Activar animaci de muerte
        agent.isStopped = true;
        //MessageManager.SendMessage(new KillEnemyMessage(enemyID));
        // Opcional: Destruir el objeto despu駸 de un tiempo

        EnemyManager.instance.RemoveEnemy(gameObject);
        Destroy(gameObject, 3f);
    }

    public void TakeDMG()
    {
        
        HP--;
        if(HP <= 0) { Die(); }
    }

    Transform FindNearestPlayerInRange(float range)
    {
        Transform nearestPlayer = nexo;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
            if (distanceToPlayer < range && distanceToPlayer < nearestDistance)
            {
                nearestDistance = distanceToPlayer;
                nearestPlayer = player.transform;
            }
        }

        return nearestPlayer;
    }

    public int GetEnemyID()
    {
        return enemyID;
    }

    // Setter para enemyID
    public void SetEnemyID(int newEnemyID)
    {
        enemyID = newEnemyID;
    }

    IEnumerator SendMyState()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(MESSAGE_SEND_DELAY);
            MessageManager.SendMessage(new Position(transform.position,
                transform.rotation.eulerAngles.y));
        }
    }

    public void ResetIA()
    {
        target = null;
        if (players != null)
        {
            System.Array.Clear(players, 0, players.Length);
        }
        agent.isStopped = false;
        players = GameObject.FindGameObjectsWithTag("Player");
        currentState = State.Patrol;
    }
}