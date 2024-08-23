using UnityEngine;

public class CardPath : MonoBehaviour
{
    public Collider pathCollider; // Asignar el colider del trayecto en el inspector

    private void OnDrawGizmos()
    {
        if (pathCollider != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(pathCollider.bounds.center, pathCollider.bounds.size);
        }
    }
}
