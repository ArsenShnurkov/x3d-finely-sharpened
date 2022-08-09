using System.Collections.Generic;

public enum TraversalType
{
    DepthFirst,
    BreadthFirst
}

/// <summary>
///     An intermediate data structure useful for traversal applications
/// </summary>
public class TraversalListIDS<T>
{
    private readonly Stack<T> K;
    private readonly Queue<T> Q;

    public TraversalListIDS(TraversalType type)
    {
        Type = type;

        if (Type == TraversalType.DepthFirst)
            K = new Stack<T>();
        else if (Type == TraversalType.BreadthFirst) Q = new Queue<T>();
    }

    public TraversalType Type { get; set; }

    public int Count
    {
        get
        {
            if (Type == TraversalType.DepthFirst)
                return K.Count;
            if (Type == TraversalType.BreadthFirst) return Q.Count;
            return 0;
        }
    }

    /// <summary>
    ///     Returns the first item in the list
    /// </summary>
    public T Peek()
    {
        if (Type == TraversalType.DepthFirst)
            return K.Peek();
        if (Type == TraversalType.BreadthFirst) return Q.Peek();
        return default;
    }

    /// <summary>
    ///     Removes and returns the first item from the list
    /// </summary>
    public T Take()
    {
        if (Type == TraversalType.DepthFirst)
            return K.Pop();
        if (Type == TraversalType.BreadthFirst) return Q.Dequeue();
        return default;
    }

    /// <summary>
    ///     Adds the item to the list
    /// </summary>
    public void Add(T item)
    {
        if (Type == TraversalType.DepthFirst)
            K.Push(item);
        else if (Type == TraversalType.BreadthFirst) Q.Enqueue(item);
    }
}