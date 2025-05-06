using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public static class Helper
{
    private static string enemyObjectParentName = "Enemies";
    private static string structureObjectParentName = "Structures";

    public static GameObject GetEnemyParentObject()
    {
        GameObject gameObject;

        gameObject = GameObject.Find("/" + enemyObjectParentName);

        if (gameObject == null)
            gameObject = new GameObject(enemyObjectParentName);

        return gameObject;
    }

    public static GameObject GetStructureParentObject()
    {
        GameObject gameObject;

        gameObject = GameObject.Find("/" + structureObjectParentName);

        if (gameObject == null)
            gameObject = new GameObject(structureObjectParentName);

        return gameObject;
    }
}
