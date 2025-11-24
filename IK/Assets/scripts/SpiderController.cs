using System.Collections.Generic;
using UnityEngine;

// Garante que não hajam muitos passos ao mesmo tempo e ajusta o corpo da aranha
public class SpiderController : MonoBehaviour
{
    [Header("Legs (arraste as emissores que têm LegController)")]
    public List<LegController> legs = new List<LegController>();

    [Header("Body balancing")]
    public Transform body;
    public float bodyHeightOffset = 0.6f;
    public float bodyAdjustSpeed = 5f;
    public float rotationAdjustSpeed = 5f;

    [Header("Gait Settings")]
    public float minTimeBetweenStepRequests = 0.06f; // cooldown global entre pedidos de step

    private float lastStepRequestTime = 0f;

    void Start()
    {
        if (body == null) body = transform;
    }

    void Update()
    {
        // Simples gerência de gait: se uma perna tentar dar passo, verifica cooldown e se outras estão em step
        for (int i = 0; i < legs.Count; i++)
        {
            var leg = legs[i];
            if (leg == null) continue;

            // Se a perna não está em stepping e quer dar passo (baseado na distância calculada internamente),
            // vamos prevenir múltiplos passos simultâneos verificando se já ocorreu um pedido recente.
            if (!leg.isStepping)
            {
                if (Time.time - lastStepRequestTime < minTimeBetweenStepRequests) continue;
                // Não forçamos aqui o StartCoroutine (LegController decide). Só atualizamos o tempo para dar espaço.
                lastStepRequestTime = Time.time;
            }
        }

        AdjustBody();
    }

    void AdjustBody()
    {
        if (legs.Count == 0) return;

        Vector3 sum = Vector3.zero;
        int count = 0;
        Vector3 leftSum = Vector3.zero;
        Vector3 rightSum = Vector3.zero;
        int leftCount = 0, rightCount = 0;

        foreach (var leg in legs)
        {
            if (leg == null || leg.footTarget == null) continue;
            Vector3 p = leg.footTarget.position;
            sum += p;
            count++;

            // separa esquerda/direita por posição local do emissor
            if (leg.transform.localPosition.x < 0)
            {
                leftSum += p; leftCount++;
            }
            else
            {
                rightSum += p; rightCount++;
            }
        }

        if (count == 0) return;
        Vector3 avg = sum / count;
        Vector3 desired = new Vector3(body.position.x, avg.y + bodyHeightOffset, body.position.z);
        body.position = Vector3.Lerp(body.position, desired, Time.deltaTime * bodyAdjustSpeed);

        float leftAvgY = leftCount > 0 ? leftSum.y / leftCount : avg.y;
        float rightAvgY = rightCount > 0 ? rightSum.y / rightCount : avg.y;

        float pitch = Mathf.Clamp((leftAvgY - rightAvgY) * 2f, -20f, 20f);
        Quaternion desiredRot = Quaternion.Euler(pitch, body.rotation.eulerAngles.y, 0);
        body.rotation = Quaternion.Slerp(body.rotation, desiredRot, Time.deltaTime * rotationAdjustSpeed);
    }
}
