using UnityEngine;
using System.Text;

public class TransformTool
{
    /// <summary> 返回物件路徑 </summary>
    public static string GetPath(Transform obj)
    {
        return GetPathFrom(obj, null);
    }

    /// <summary> 返回父物件至子物件的路徑 </summary>
    public static string GetPathFrom(Transform child, Transform parent)
    {
        StringBuilder sb = new StringBuilder();
        bool isfirst = true;
        while (true)
        {
            if (child == parent || child == null) break;

            if (isfirst)
            {
                isfirst = false;
                sb.Append(child.name);
            }
            else
            {
                sb.Insert(0, "/").Insert(0, child.name);
            }
            child = child.parent;
        }
        return sb.ToString();
    }

    public static Transform GetRoot(Transform child, string path)
    {
        if (string.IsNullOrEmpty(path)) return child;

        Transform parent = child.parent;
        int index = -1;
        while (parent != null && (index = path.IndexOf("/", index + 1)) > 0)
        {
            parent = parent.parent;
        }
        return parent;
    }
}