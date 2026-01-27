using UnityEngine;

#if UNITY_EDITOR
[ExecuteAlways]
public sealed class ShockerRadiusGizmo : MonoBehaviour
{
    [SerializeField] private float radius = 2.2f;
    [SerializeField] private Color color = new Color(0f, 1f, 1f, 0.35f);

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = color;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
#endif
