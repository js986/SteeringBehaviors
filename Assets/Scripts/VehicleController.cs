using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VehicleController : MonoBehaviour
{
    public Vector2 velocity;
    public Vector2 acceleration;
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
                Seek(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            case Behavior.FLEE:
                Flee(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            case Behavior.ARRIVE:
                Arrive(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                break;
            case Behavior.PURSUE:
                break;
            case Behavior.EVADE:
                break;
            case Behavior.WANDER:
                Wander();
                break;
            case Behavior.WANDERNOISE:
                WanderNoise();
                break;
            case Behavior.OBSTACLEAVOID:
                break;
        }

    }

    void Seek(Vector2 target)
    {
        Vector2 desired = target - new Vector2(agent.transform.position.x, agent.transform.position.y);
        desired.Normalize();
        desired *= maxSpeed;
        Vector2 steer = desired - velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);

        velocity += steer;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        agent.transform.position += new Vector3(velocity.x,velocity.y, 0f) * Time.fixedDeltaTime;
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        angle -= 90f;
    
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        agent.transform.rotation = q;
        //agent.transform.Translate(velocity * Time.fixedDeltaTime);

        Debug.DrawLine(agent.transform.position,velocity.normalized, Color.green);
        Debug.DrawLine(agent.transform.position, steer.normalized, Color.red);
        Debug.DrawLine(agent.transform.position, desired, Color.blue);
    }

    void Flee(Vector2 target)
    {
        Vector2 desired = new Vector2(agent.transform.position.x, agent.transform.position.y) - target;
        desired.Normalize();
        desired *= maxSpeed;
        Vector2 steer = desired - velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);

        velocity += steer;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        agent.transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.fixedDeltaTime;
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        angle -= 90;

        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        agent.transform.rotation = q;

        Debug.DrawLine(agent.transform.position, velocity.normalized, Color.green);
        Debug.DrawLine(agent.transform.position, desired, Color.blue);
    }

    void Arrive(Vector2 target)
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

        velocity += steer;
        agent.transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.fixedDeltaTime;
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        angle -= 90f;

        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        agent.transform.rotation = q;
    }

    void Pursue(GameObject target)
    {
        Vector3 targetPos = target.transform.position;
        Vector3 prediction = target.GetComponent<VehicleController>().velocity;
        prediction *= 3; //Where the target will be in three frames
        targetPos += prediction;
    }

    void Wander()
    {
        Vector2 desired = GetWanderForce();
        desired = desired.normalized * maxSpeed;

        Vector2 steer = desired - velocity;
        steer = Vector2.ClampMagnitude(steer, maxForce);

        velocity += steer;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);

        agent.transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.fixedDeltaTime;
        float angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        angle -= 90f;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        agent.transform.rotation = q;

        Debug.DrawLine(agent.transform.position, velocity, Color.green);
        Debug.DrawLine(agent.transform.position, desired, Color.blue);
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

    void WanderNoise()
    {
        float angle = Mathf.PerlinNoise(perlinOffset, 0) * (Mathf.PI*2) * 2 * Mathf.Rad2Deg;
        //Debug.Log(angle);
        Vector2 steer = Quaternion.Euler(0,0,angle) * Vector2.right;
        steer = Vector2.ClampMagnitude(steer, maxForce);
        perlinOffset += 0.01f;
        velocity += steer;
        velocity = Vector2.ClampMagnitude(velocity, maxSpeed);
        agent.transform.position += new Vector3(velocity.x, velocity.y, 0f) * Time.fixedDeltaTime;
        float rotAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        rotAngle -= 90f;
        Quaternion q = Quaternion.AngleAxis(rotAngle, Vector3.forward);
        agent.transform.rotation = q;
        Debug.DrawLine(agent.transform.position, velocity.normalized*-1, Color.green);
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

