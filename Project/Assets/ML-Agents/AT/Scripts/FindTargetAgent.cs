using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using static UnityEngine.GraphicsBuffer;

public class FindTargetAgent : Agent
{
    #region Exposed Instance Variables

    [SerializeField]
    private float speed = 10.0f;

    [SerializeField]
    private GameObject target = null;

    [SerializeField]
    private float distanceRequired = 1.5f;

    #endregion

    #region Private Instance Variables

    private Rigidbody playerRigidbody;

    private Vector3 originalPosition;

    private Vector3 originalTargetPosition;

    #endregion

    #region Material Editor
    [SerializeField] private MeshRenderer groundMeshRenderer;
    [SerializeField] private Material successMat;
    [SerializeField] private Material FailureMat;
    [SerializeField] private Material defaultMat;

    #endregion

    public override void Initialize()
    {
        playerRigidbody = GetComponent<Rigidbody>();
        originalPosition = transform.localPosition;
        originalTargetPosition = target.transform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        if (transform.localPosition.y < 0)
        {
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.velocity = Vector3.zero;
        }

        transform.LookAt(target.transform);
        target.transform.localPosition = originalTargetPosition;
        transform.localPosition = originalPosition;
        transform.localPosition = new Vector3(Random.Range(-4, 4), originalPosition.y, originalPosition.z);

        target.transform.localPosition = new Vector3(Random.value * 8 - 4,
                                           0.5f,
                                           Random.value * 8 - 4);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 observations - x, y, z
        sensor.AddObservation(transform.localPosition);

        // 3 observations - x, y, z
        sensor.AddObservation(target.transform.localPosition);

        // 1 observation
        sensor.AddObservation(playerRigidbody.velocity.x);

        // 1 observation
        sensor.AddObservation(playerRigidbody.velocity.z);
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Actions, size = 2
        Vector3 controlSignal = Vector3.zero;
        controlSignal.x = actionBuffers.ContinuousActions[0];
        controlSignal.z = actionBuffers.ContinuousActions[1];
        playerRigidbody.AddForce(controlSignal * speed);

        // Rewards
        float distanceToTarget = Vector3.Distance(transform.localPosition, target.transform.localPosition);

        // Reached target
        if (distanceToTarget < distanceRequired)
        {
            SetReward(1f);
            EndEpisode();

            StartCoroutine(SwapGroundMaterial(successMat, 0.5f));
        }

/*        if (distanceToTarget > distanceRequired)
        {
            SetReward(0.05f);
            
        }*/

        // Fell off platform
        else if (transform.localPosition.y < 0)
        {
            SetReward(-1f);
            EndEpisode();

            StartCoroutine(SwapGroundMaterial(FailureMat, 0.5f));
        }

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActionsOut = actionsOut.ContinuousActions;
        continuousActionsOut[0] = Input.GetAxis("Horizontal");
        continuousActionsOut[1] = Input.GetAxis("Vertical");
    }

    private IEnumerator SwapGroundMaterial(Material mat, float time)
    {
        groundMeshRenderer.material = mat;
        yield return new WaitForSeconds(time);
        groundMeshRenderer.material = defaultMat;
    }
}
