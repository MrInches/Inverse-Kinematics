using UnityEngine;

// Opcional: usa 3 targets para calcular normal do terreno e alinhar o corpo de forma mais precisa.
public class BodyBalancer : MonoBehaviour
{
    public Transform body;
    public Transform[] footTargets;
    public float heightOffset = 0.6f;
    public float smoothSpeed = 6f;

    void Update()
    {
        if (footTargets == null || footTargets.Length == 0 || body == null) return;

        Vector3 avg = Vector3.zero;
        int c = 0;
        foreach (var ft in footTargets)
            if (ft != null) { avg += ft.position; c++; }

        if (c == 0) return;
        avg /= c;

        Vector3 targetPos = new Vector3(body.position.x, avg.y + heightOffset, body.position.z);
        body.position = Vector3.Lerp(body.position, targetPos, Time.deltaTime * smoothSpeed);

        // Se tiver pelo menos 3 footTargets, calcular normal aproximada
        if (c >= 3)
        {
            Vector3 a = footTargets[0].position;
            Vector3 b = footTargets[1].position;
            Vector3 cpos = footTargets[2].position;
            Vector3 normal = Vector3.Cross(b - a, cpos - a).normalized;
            Quaternion rot = Quaternion.LookRotation(Vector3.ProjectOnPlane(body.forward, normal), normal);
            body.rotation = Quaternion.Slerp(body.rotation, rot, Time.deltaTime * smoothSpeed);
        }
    }
}
