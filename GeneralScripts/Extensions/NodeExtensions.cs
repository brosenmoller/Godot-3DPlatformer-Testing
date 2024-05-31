using Godot;
using System.Collections.Generic;

public static class NodeExtensions
{
    public static void SetForward(this Node3D node, Vector3 forward)
    {
        node.GlobalBasis = node.GlobalBasis with { Z = -forward };
    }

    /// <summary>
    /// Acts like Unity's GetComponent<T> and GetComponentInChildren<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="node"></param>
    /// <param name="recursive"></param>
    /// <returns>The child if it found one otherwise it will return null</returns>
    public static T GetChildByType<T>(this Node node, bool recursive = true) where T : Node
    {
        int childCount = node.GetChildCount();

        for (int i = 0; i < childCount; i++)
        {
            Node child = node.GetChild(i);
            if (child is T childT)
            {
                return childT;
            }

            if (recursive && child.GetChildCount() > 0)
            {
                T recursiveResult = child.GetChildByType<T>(true);
                if (recursiveResult != null)
                {
                    return recursiveResult;
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Acts like Unity's GetComponents<T> and GetComponentsInChildren<T>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="node"></param>
    /// <param name="recursive"></param>
    /// <returns>Returns all children of this node</returns>
    public static T[] GetAllChildrenByType<T>(this Node node, bool recursive = true) where T : Node
    {
        List<T> values = new();

        int childCount = node.GetChildCount();

        for (int i = 0; i < childCount; i++)
        {
            Node child = node.GetChild(i);
            if (child is T childT)
            {
                values.Add(childT);
            }

            if (recursive && child.GetChildCount() > 0)
            {
                values.AddRange(child.GetAllChildrenByType<T>());
            }
        }

        return values.ToArray();
    }

    // Acts like Unity's GetComponentInParent<T>
    public static T GetParentByType<T>(this Node node) where T : Node
    {
        Node parent = node.GetParent();
        if (parent != null)
        {
            if (parent is T parentT)
            {
                return parentT;
            }
            else
            {
                return parent.GetParentByType<T>();
            }
        }

        return null;
    }

    // Acts like Unity's FindObjectOfType<T>
    public static T GetNodeByType<T>(this Node node) where T : Node
    {
        Node rootNode = node.GetTree().Root;
        return rootNode.GetChildByType<T>();
    }

    // Acts like Unity's FindObjectsOfType<T>
    public static T[] GetAllNodesByType<T>(this Node node) where T : Node
    {
        Node rootNode = node.GetTree().Root;
        return rootNode.GetAllChildrenByType<T>();
    }
}