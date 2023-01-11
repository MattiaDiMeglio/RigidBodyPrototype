using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public static class GameObjectExtension
{
    /// <summary>
    /// Metodo che restituisce la mesh di un gameobject.
    /// In caso sia un solo elemento con un meshfilter, restituirá la sharedMesh di questi
    /// In caso ci siano figli, ottiene tutte le sharedMesh e le unisce in una
    /// </summary>
    /// <param name="prefab">L'oggetto di cui si vuole ottenere la mesh</param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Mesh GetMeshFromPrefab(this GameObject prefab)
    {
        if (prefab.GetComponent<MeshFilter>() != null && prefab.GetComponent<MeshFilter>().sharedMesh != null)
        {
            return prefab.GetComponent<MeshFilter>().sharedMesh;
        }
        if (prefab.transform.childCount > 0)
        {
            List<MeshFilter> meshFilters = new List<MeshFilter>(prefab.GetComponentsInChildren<MeshFilter>());
            meshFilters.Remove(prefab.GetComponent<MeshFilter>());
            Matrix4x4 transform = prefab.transform.worldToLocalMatrix;
            CombineInstance[] combine = new CombineInstance[meshFilters.Count];
            for (int i = 0; i < meshFilters.Count; i++)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = transform * meshFilters[i].transform.localToWorldMatrix;
                //meshFilters[i].gameObject.SetActive(false);
            }
            Mesh newMesh = new Mesh();
            newMesh.CombineMeshes(combine);
            return newMesh;
        }
        return null;
    }

    /// <summary>
    /// Metodo che setta Active il GameObject parent e tutti i suoi figli
    /// </summary>
    /// <param name="parent">Il gameobject che si vuole attivare</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ActivateAllChildren(this GameObject parent)
    {
        parent.SetActive(true);
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            parent.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Metodo che setta Inactive il GameObject parent e tutti i suoi figli
    /// </summary>
    /// <param name="parent">Il gameobject che si vuole disattivare</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void DeactivateAllChildren(this GameObject parent)
    {
        parent.SetActive(false);
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            parent.transform.GetChild(i).gameObject.SetActive(false);
        }
    }
}
