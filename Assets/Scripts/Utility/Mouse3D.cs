using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mouse3D : MonoBehaviour
{

    [SerializeField] private LayerMask mouseColliderLayerMask = new LayerMask();

    public static Mouse3D Instance { get; private set; }
    public static Vector3 GetMouseWorldPosition() => Instance.GetMouse3DWorldPositionInstance();

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask))
            transform.position = raycastHit.point;

        //Debug.Log(raycastHit.point);
    }

    Vector3 GetMouse3DWorldPositionInstance()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, mouseColliderLayerMask))
            return raycastHit.point;

        return Vector3.zero;
    }
}
