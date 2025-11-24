using System.Collections;
using UnityEngine;

// Controla um "pé" procedurally: faz raycast para detectar o chão, decide quando dar passo e anima o footTarget.
// Anexe este script em um Empty que represente o "emissor" da perna (perto da articulação do ombro).
public class LegController : MonoBehaviour
{
    [Header("Referências")]
    public Transform body;            // referência ao corpo (usado para calcular origem do raio)
    public Transform footTarget;      // target que a IK (ou o rig) seguirá
    public Transform defaultRest;     // posição de repouso (opcional)

    [Header("Step Settings")]
    public float stepDistance = 0.8f;   // distância mínima para disparar novo passo
    public float stepHeight = 0.25f;    // altura do arco do passo
    public float stepDuration = 0.18f;  // duração da animação do passo
    public LayerMask groundMask = ~0;   // camadas consideradas chão
    public float raycastDistance = 3f;  // distância máxima do raycast
    public Vector3 rayOriginOffset = new Vector3(0f, 0.5f, 0f); // offset local na origem do raio

    [Header("Debug")]
    public bool debugDraw = false;

    [HideInInspector]
    public bool isStepping = false;

    private Vector3 lastStepPosition;
    private Quaternion lastStepRotation;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    void Start()
    {
        if (footTarget == null)
        {
            Debug.LogError($"{name}: footTarget não atribuído.");
            enabled = false;
            return;
        }

        lastStepPosition = footTarget.position;
        lastStepRotation = footTarget.rotation;
        targetPosition = lastStepPosition;
        targetRotation = lastStepRotation;
    }

    void Update()
    {
        // calcula origem do raycast (em world space)
        Vector3 origin = (body ? body.position : transform.position) + transform.TransformDirection(rayOriginOffset);

        if (debugDraw) Debug.DrawRay(origin, Vector3.down * raycastDistance, Color.cyan);

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, raycastDistance, groundMask))
        {
            Vector3 predictedPoint = hit.point;
            // alinhar rotação do target com a normal do hit
            Quaternion predictedRotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * Quaternion.LookRotation(body ? body.forward : transform.forward);

            float dist = Vector3.Distance(lastStepPosition, predictedPoint);

            if (!isStepping && dist > stepDistance)
            {
                // iniciar passo
                StartCoroutine(PerformStep(predictedPoint, predictedRotation));
            }
            else
            {
                // seguir suavemente posição final conhecida
                targetPosition = lastStepPosition;
                targetRotation = lastStepRotation;
            }
        }
        else
        {
            // sem chão detectado - voltar para posição de descanso se houver
            if (defaultRest != null)
            {
                targetPosition = defaultRest.position;
                targetRotation = defaultRest.rotation;
            }
        }

        // quando não estiver em step, segue target suavemente
        if (!isStepping)
        {
            footTarget.position = Vector3.Lerp(footTarget.position, targetPosition, Time.deltaTime * 10f);
            footTarget.rotation = Quaternion.Slerp(footTarget.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    IEnumerator PerformStep(Vector3 newPos, Quaternion newRot)
    {
        isStepping = true;

        Vector3 startPos = footTarget.position;
        Quaternion startRot = footTarget.rotation;

        float elapsed = 0f;
        while (elapsed < stepDuration)
        {
            float t = elapsed / stepDuration;
            float ease = Mathf.SmoothStep(0f, 1f, t);
            Vector3 horiz = Vector3.Lerp(startPos, newPos, ease);
            float arc = Mathf.Sin(t * Mathf.PI) * stepHeight;
            Vector3 pos = new Vector3(horiz.x, horiz.y + arc, horiz.z);

            footTarget.position = pos;
            footTarget.rotation = Quaternion.Slerp(startRot, newRot, ease);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // finalizar passo
        footTarget.position = newPos;
        footTarget.rotation = newRot;
        lastStepPosition = newPos;
        lastStepRotation = newRot;
        targetPosition = newPos;
        targetRotation = newRot;

        isStepping = false;
    }

    // opcional: desenha gizmos para debugging no editor
    void OnDrawGizmosSelected()
    {
        if (!debugDraw) return;
        Vector3 origin = (body ? body.position : transform.position) + transform.TransformDirection(rayOriginOffset);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(origin, origin + Vector3.down * raycastDistance);
        Gizmos.DrawSphere(lastStepPosition, 0.05f);
    }
}
