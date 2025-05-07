using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{
    public bool isPath;
    public Grid.MovementDirection direction;
    public Transform transform;
    public Renderer renderer;
    public bool hasBuilding;

    public override string ToString()
    {
        return this.transform.position.x + ", " + this.transform.position.z; 
    }

    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x, 0, z);
    }

    public void GetXZ(Vector3 worldPosition, out int x, out int z)
    {
        x = Mathf.FloorToInt(worldPosition.x + 0.5f);
        z = Mathf.FloorToInt(worldPosition.z + 0.5f);
    }
}
