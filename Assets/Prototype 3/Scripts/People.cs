using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class People : MonoBehaviour
{
    //This code was created based on many tutorials, so experimentation is more trial and error.
    public enum PersonState { Normal, IntermittentCough, Coughing, Infected, Chasing }

    [Header("Movement Settings")]
    public float wanderTimer = 3f;
    public float movementSpeed = 2f;
    public float infectedSpeed = 3.5f;

    [Header("Infection Settings")]
    public PersonState currentState = PersonState.Normal;
    public float minInfectionTime = 10f;
    public float maxInfectionTime = 30f;
    public float infectionRadius = 2f;
    public float infectionChance = 0.3f;
    public float coughDuration = 2f;
    public float chaseDuration = 8f;

    [Header("Intermittent Cough Settings")]
    public float minCoughInterval = 3f;
    public float maxCoughInterval = 8f;
    public float intermittentCoughDuration = 1f;
    public float proximityInfectionDelay = 2f;
    public float infectionProgressSpeed = 5f;
    public float recoveryChance = 0.4f;

    [Header("Visual Settings")]
    public SpriteRenderer Head;
    public Color infectedColor = Color.green;
    public float shakeIntensity = 0.1f;

    private NavMeshAgent agent;
    private float timer;
    private SpriteRenderer spriteRenderer;
    private Color originalHeadColor;
    private Transform currentTarget;
    private float nextCoughTime;
    private bool isRecovering = false;
    public float infectionProgress = 0f;
    [SerializeField] private bool willGetInfected = true;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (Head != null)
            originalHeadColor = Head.color;

        if (agent == null)
            agent = gameObject.AddComponent<NavMeshAgent>();

        agent.speed = movementSpeed;
        agent.angularSpeed = 0;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.1f;
        agent.updateRotation = false;
        agent.updateUpAxis = false;

        timer = wanderTimer;
        SetRandomDestination();

        if (willGetInfected)
            StartCoroutine(InfectionCountdown());
        else if (currentState == PersonState.Infected)
            OnBecomeInfected();
    }

    void Update()
    {
        timer += Time.deltaTime;

        switch (currentState)
        {
            case PersonState.Normal: HandleNormalState(); break;
            case PersonState.IntermittentCough: HandleIntermittentCoughState(); break;
            case PersonState.Coughing: agent.isStopped = true; break;
            case PersonState.Infected: HandleInfectedState(); break;
            case PersonState.Chasing: HandleChasingState(); break;
        }

        UpdateHeadColor();
        ForceZPosition();
    }

    void HandleNormalState()
    {
        if (timer >= wanderTimer)
        {
            SetRandomDestination();
            timer = 0;
        }
    }

    void HandleIntermittentCoughState()
    {
        if (timer >= wanderTimer)
        {
            SetRandomDestination();
            timer = 0;
        }

        if (Time.time >= nextCoughTime)
            StartIntermittentCough();

        UpdateInfectionProgress();
        CheckProximityDuringCough();
    }

    void HandleInfectedState()
    {
        if (timer >= wanderTimer)
        {
            SetRandomDestination();
            timer = 0;
        }
        CheckForNearbyPeople();
    }

    void HandleChasingState()
    {
        if (currentTarget != null)
            agent.SetDestination(currentTarget.position);
        else
        {
            SetRandomDestination();
            currentState = PersonState.Infected;
        }
    }

    void UpdateInfectionProgress() //Pensar si ajustaremos esta linea para determinar
                                   //si el aldeano seguira con los tag mas adelante de infectado o aldeano
    {
        if (currentState != PersonState.IntermittentCough) return;

        float progressChange = infectionProgressSpeed * Time.deltaTime;

        if (isRecovering)
        {
            infectionProgress -= progressChange;
            if (infectionProgress <= 0f)
                RecoverToNormal();
        }
        else
        {
            infectionProgress += progressChange;
            if (infectionProgress >= 100f)
                OnBecomeInfected();
        }

        infectionProgress = Mathf.Clamp(infectionProgress, 0f, 100f);
    }

    void UpdateHeadColor()
    {
        if (Head == null) return;

        if (currentState == PersonState.Infected)
        {
            Head.color = infectedColor;
            return;
        }

        float normalizedProgress = infectionProgress / 100f;

        if (isRecovering)
        {
            Head.color = Color.Lerp(originalHeadColor, Color.yellow, normalizedProgress);
        }
        else
        {
            if (normalizedProgress <= 0.33f)
                Head.color = Color.Lerp(originalHeadColor, Color.yellow, normalizedProgress * 3f);
            else if (normalizedProgress <= 0.66f)
                Head.color = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), (normalizedProgress - 0.33f) * 3f);
            else
                Head.color = Color.Lerp(new Color(1f, 0.5f, 0f), Color.red, (normalizedProgress - 0.66f) * 3f);
        }
    }

    IEnumerator InfectionCountdown()
    {
        if (!willGetInfected) yield break;

        yield return new WaitForSeconds(Random.Range(minInfectionTime, maxInfectionTime));
        StartIntermittentPhase();
    }

    void StartIntermittentPhase() 
    {
        if (currentState != PersonState.Normal) return;

        currentState = PersonState.IntermittentCough;
        infectionProgress = 10f;
        isRecovering = Random.value <= recoveryChance;
        SetNextCoughTime();
    }

    void SetNextCoughTime()
    {
        nextCoughTime = Time.time + Random.Range(minCoughInterval, maxCoughInterval);
    }

    void StartIntermittentCough() //Etapa de analisis
    {
        if (currentState == PersonState.IntermittentCough)
            StartCoroutine(IntermittentCoughRoutine());
    }

    IEnumerator IntermittentCoughRoutine()
    {
        PersonState previousState = currentState;
        currentState = PersonState.Coughing;

        StartCoroutine(ShakeSprite(intermittentCoughDuration));
        yield return new WaitForSeconds(intermittentCoughDuration);

        if (previousState == PersonState.IntermittentCough)
        {
            currentState = PersonState.IntermittentCough;
            agent.isStopped = false;
            SetNextCoughTime();
        }
    }

    void CheckProximityDuringCough()
    {
        if (infectionProgress < 75f) return;

        Collider2D[] nearbyPeople = Physics2D.OverlapCircleAll(transform.position, infectionRadius);

        foreach (Collider2D person in nearbyPeople)
        {
            if (person.CompareTag("Aldeano") && person.gameObject != this.gameObject)
            {
                People otherPerson = person.GetComponent<People>();
                if (otherPerson != null && otherPerson.currentState == PersonState.Normal)
                    otherPerson.StartProximityInfection();
            }
        }
    }

    public void StartProximityInfection()
    {
        if (currentState == PersonState.Normal)
            StartCoroutine(ProximityInfectionDelay());
    }

    IEnumerator ProximityInfectionDelay() //Establecer quizas otro metodo, juega mucho en contra
    {
        yield return new WaitForSeconds(proximityInfectionDelay);
        if (currentState == PersonState.Normal)
            StartIntermittentPhase();
    }

    void RecoverToNormal()
    {
        currentState = PersonState.Normal;
        isRecovering = false;
        agent.isStopped = false;

        if (willGetInfected)
            StartCoroutine(InfectionCountdown());
    }

    IEnumerator ShakeSprite(float duration)
    {
        Vector3 originalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = originalPosition.x + Random.Range(-shakeIntensity, shakeIntensity);
            float y = originalPosition.y + Random.Range(-shakeIntensity, shakeIntensity);
            transform.position = new Vector3(x, y, originalPosition.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
    }

    void OnBecomeInfected() 
    {
        currentState = PersonState.Infected;
        agent.speed = infectedSpeed;
        agent.isStopped = false;
        isRecovering = false;
        infectionProgress = 100f;
        gameObject.tag = "Infected";
    }

    void CheckForNearbyPeople() //util pero pero abra que mejorar la deteccion pa que no sea
        //muy repetitiva
    {
        Collider2D[] nearbyPeople = Physics2D.OverlapCircleAll(transform.position, infectionRadius);

        foreach (Collider2D person in nearbyPeople)
        {
            if (person.CompareTag("Aldeano") && person.gameObject != this.gameObject)
            {
                People otherPerson = person.GetComponent<People>();
                if (otherPerson != null && (otherPerson.currentState == PersonState.Normal || otherPerson.currentState == PersonState.IntermittentCough))
                {
                    otherPerson.StartDirectInfection();
                    currentState = PersonState.Chasing;
                    currentTarget = person.transform;
                    StartCoroutine(StopChasingAfterTime());
                }
            }
        }
    }

    public void StartDirectInfection()
    {
        if (currentState == PersonState.Normal)
            StartIntermittentPhase();
        else if (currentState == PersonState.IntermittentCough && isRecovering)
            isRecovering = false;
    }

    IEnumerator StopChasingAfterTime()
    {
        yield return new WaitForSeconds(chaseDuration);
        if (currentState == PersonState.Chasing)
        {
            currentState = PersonState.Infected;
            currentTarget = null;
            SetRandomDestination();
        }
    }

    public void SetImmunity(bool immune)
    {
        willGetInfected = !immune;
        if (immune) infectionProgress = 0f;
    }

    private void SetRandomDestination()
    {
        if (MVA.Instance != null)
        {
            Vector2 randomPos = MVA.Instance.GetRandomPositionInArea();
            agent.SetDestination(new Vector3(randomPos.x, randomPos.y, 0));
            agent.isStopped = false;
        }
    }

    private void ForceZPosition()
    {
        Vector3 currentPos = transform.position;
        if (currentPos.z != 0)
            transform.position = new Vector3(currentPos.x, currentPos.y, 0);
    }
}