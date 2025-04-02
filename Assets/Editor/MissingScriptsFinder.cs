using UnityEngine;
using UnityEditor;

public class MissingScriptsFinder : MonoBehaviour
{
    [MenuItem("Tools/Find Missing Scripts In Scene")]
    public static void FindMissingScripts()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject go in allObjects)
        {
            Component[] components = go.GetComponents<Component>();

            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == null)
                {
                    Debug.LogWarning($"Missing script found on GameObject: {GetHierarchyPath(go)}", go);
                    count++;
                }
            }
        }

        Debug.Log($"Finished scanning. Found {count} GameObject(s) with missing scripts.");
    }

    private static string GetHierarchyPath(GameObject obj)
    {
        string path = obj.name;
        while (obj.transform.parent != null)
        {
            obj = obj.transform.parent.gameObject;
            path = obj.name + "/" + path;
        }
        return path;
    }
}
