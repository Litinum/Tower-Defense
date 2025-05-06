using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class BuildingSystem : MonoBehaviour
{
    [SerializeField] private Material transparentMaterial;

    public static BuildingSystem Instance { get; private set; }

    private GameObject toBuild;
    private Cell cell = new Cell();
    private GameObject toBuildPreviewObject;
    private TowerBehaviour toBuildStats;
    private GameObject structureParentObject;

    void Awake()
    {
        Instance = this;

        structureParentObject = Helper.GetStructureParentObject();
    }

    void Update()
    {
        if (toBuild != null)
        {
            cell.GetXZ(Mouse3D.GetMouseWorldPosition(), out int x, out int z);

            if(toBuildPreviewObject == null)
            {
                toBuildPreviewObject = Instantiate(toBuild, cell.GetWorldPosition(x, z), Quaternion.identity);
                toBuildPreviewObject.GetComponent<TowerBehaviour>().enabled = false;
                toBuildPreviewObject.GetComponent<SphereCollider>().enabled = false;
                SetTransparentMaterialToObject();
            }
            else
                toBuildPreviewObject.transform.position = new Vector3(x, 0, z);

            SetPreviewColour();
        }

        if (Input.GetMouseButtonDown(0) && toBuild != null)
        {
            cell.GetXZ(Mouse3D.GetMouseWorldPosition(), out int x, out int z);
            //Debug.Log($"{Mouse3D.GetMouseWorldPosition()}, GRID: {x}, {z}");
            cell = Grid.Instance.GetCell(x, z);

            if (!cell.isPath && !cell.hasBuilding)
                BuildStructure(x, z);
        }

        if(Input.GetMouseButtonDown(1) && toBuild != null)
        {
            if(toBuildPreviewObject != null)
                Destroy(toBuildPreviewObject);

            toBuild = null;
        }
    }

    void SetTransparentMaterialToObject()
    {
        if (toBuildPreviewObject == null)
            return;

        foreach(Transform child in toBuildPreviewObject.transform)
        {
            child.GetComponent<MeshRenderer>().material = transparentMaterial;
        }
    }

    void SetPreviewColour()
    {
        if (toBuildPreviewObject == null)
            return;

        if (PlayerStats.Instance.gold >= toBuildStats.GetBuildCost())
        {
            foreach (Transform child in toBuildPreviewObject.transform)
            {
                child.GetComponent<MeshRenderer>().material.color = Color.white;
            }
        }
        else
        {
            foreach (Transform child in toBuildPreviewObject.transform)
            {
                child.GetComponent<MeshRenderer>().material.color = Color.red;
            }
        }
    }

    public void SetBuildStructure(GameObject structureToBuild)
    {
        toBuild = structureToBuild;
        toBuildStats = structureToBuild.GetComponent<TowerBehaviour>();
    }

    public bool BuildStructure(int x, int z)
    {
        if (PlayerStats.Instance.gold >= toBuildStats.GetBuildCost())
        {
            GameObject structure; 

            if (toBuildPreviewObject != null)
                Destroy(toBuildPreviewObject);

            PlayerStats.Instance.SpendGold(toBuildStats.GetBuildCost());

            structure = Instantiate(toBuild, cell.GetWorldPosition(x, z), Quaternion.identity);
            structure.transform.parent = structureParentObject.transform;

            toBuildStats.IncreaseCost();
        
            cell.hasBuilding = true;
            toBuild = null;
            toBuildStats = null;

            return true;
        }

        return false;
    }
}
