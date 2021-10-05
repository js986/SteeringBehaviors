using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public Vector2 velocity;
    public Vector2 acceleration;
    public GameObject pursueTarget; //Target for pursue behavior
    public enum Behavior {
        SEEK,
        FLEE,
        ARRIVE,
        PURSUE,
        EVADE,
        WANDER,
        WANDERNOISE,
        OBSTACLEAVOID,
    };

    public Behavior agentBehavior; // Controls the behavior of the agent

    Vector2 wanderTarget;
    Vector2 wanderForce;
    float perlinOffset;
    float r = 2f; // Radius around target
    float maxSpeed; // Maximum speed for desired velocity
    float maxForce; // Maximum force for steering velocity
    float maxRadius = 4f;
    float slowingDistance = 2f; // For arrive behavior; distance to target before velocity is slowed
    float turnChance = 0.05f; // Change to turn for wander behavior
    float nextDecision = 5f;
    float circleDistance = 3f;
    GameObject agent;
    // Start is called before the first frame update
    void Start()
    {
        agent = this.gameObject;
        maxSpeed = 4;
        maxForce = 0.1f;
        velocity = Random.insideUnitCircle;
        wanderForce = GetRandomWanderForce();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        switch (agentBehavior) {
            case Behavior.SEEK:
                velocity+=Seek(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            case Behavior.FLEE:
                velocity+=Flee(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            case Behavior.ARRIVE:
                velocity+=Arrive(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            case Behavior.PURSUE:
                velocity+=Pursue(pursueTarget);
                break;
            case Behavior.EVADE:
                velocity += Evade(pursueTarget);
                break;
            case Behavior.WANDER:
                velocity+=Wander();
                break;
            case Behavior.WANDERNOISE:
                velocity+=WanderNoise();
                break;
            case Behavior.OBSTACLEAVOID:
                break;
        }

        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.fixedDeltaTime;
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        angle -= 90f;

        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = q;

    }

    Vector2 Seek(Vector2 target)
    {
        Vector2 desired = target - new Vector2(agent.transform.position.x, agent.transform.position.y);
        desired.Normalize();
        desired *= maxSpeed;
        Vector2 steer = desired - velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);

        Debug.DrawLine(agent.transform.position, velocity.normalized, Color.green);
        Debug.DrawLine(agent.transform.position, steer.normalized, Color.red);
        Debug.DrawLine(agent.transform.position, desired, Color.blue);

        return steer;
        
    }

    Vector2 Flee(Vector2 target)
    {
        Vector2 desired = new Vector2(agent.transform.position.x, agent.transform.position.y) - target;
        desired.Normalize();
        desired *= maxSpeed;
        Vector2 steer = desired - velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);
   
        Debug.DrawLine(agent.transform.position, velocity.normalized, Color.green);
        Debug.DrawLine(agent.transform.position, desired, Color.blue);

        return steer;
    }

    Vector2 Arrive(Vector2 target)
    {
        Vector2 desired = target - new Vector2(agent.transform.position.x, agent.transform.position.y);
        float dist = desired.magnitude;
        if (dist < slowingDistance)
        {
            desired = desired.normalized * maxSpeed * (dist/slowingDistance);
        }
        else
        {
            desired = desired.normalized * maxSpeed;
        }

        Vector2 steer = desired - velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);

        Debug.DrawLine(agent.transform.position, velocity, Color.green);
        //Debug.DrawLine(velocity, steer, Color.red);
        Debug.DrawLine(agent.transform.position, desired, Color.blue);

        return steer;
    }

    Vector2 Pursue(GameObject target)
    {
        Vector3 targetPos = target.transform.position;
        Vector3 prediction = target.GetComponent<VehicleController>().velocity;
        prediction *= 3; //Where the target will be in three frames
        targetPos += prediction;

        return Seek(targetPos);
    }

    Vector2 Evade(GameObject target)
    {
        return Pursue(target) * -1;
    }

    Vector2 Wander()
    {
        Vector2 desired = GetWanderForce();
        desired = desired.normalized * maxSpeed;

        Vector2 steer = desired - velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);

        Debug.DrawLine(agent.transform.position, velocity, Color.green);
        Debug.DrawLine(agent.transform.position, desired, Color.blue);

        return steer;
    }

    Vector2 GetWanderForce()
    { 

        if (agent.transform.position.magnitude > maxRadius)
        {
            Vector2 directionToCenter = wanderTarget - new Vector2(agent.transform.position.normalized.x, agent.transform.position.normalized.y);
            wanderForce = velocity.normalized + directionToCenter;
        } 
        else if (Random.value < turnChance){
            wanderForce = GetRandomWanderForce();
        }
        return wanderForce;
    }

    Vector2 GetRandomWanderForce()
    {
        Vector2 circleCenter = velocity.normalized;
        Vector2 displacement = Random.insideUnitCircle * r;

        Vector2 wanderForce = circleCenter + displacement;
        return wanderForce;
    }

    Vector2 WanderNoise()
    {
        float angle = Mathf.PerlinNoise(perlinOffset, 0) * (Mathf.PI*2) * 2 * Mathf.Rad2Deg;
        Vector2 steer = Quaternion.Euler(0,0,angle) * Vector2.right;
        steer = Vector2.ClampMagnitude(steer, maxForce);
        perlinOffset += 0.01f;
       
        Debug.DrawLine(agent.transform.position, velocity.normalized*-1, Color.green);
        return steer;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (agentBehavior == Behavior.WANDER || agentBehavior == Behavior.WANDERNOISE)
        {
            Gizmos.DrawSphere(velocity.normalized, r);
        }

        //Gizmos.DrawSphere(Camera.main.ScreenToWorldPoint(Input.mousePosition), slowingDistance);
    }

}

