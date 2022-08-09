using System;
using System.Collections.Generic;
using OpenTK;

public class TNode
{
    private const double TWO_PI = 2.0 * Math.PI;

    public float
        angle, // angle from root coordinate in radians
        prev_gap,
        prev_angle,
        rightBisector,
        leftBisector,
        leftTangent, // the left tangent angle
        rightTangent, // the right tangent angle
        sectorStartAngle, // the left angle of the sector specifying the start of the sector for the node's children
        sectorEndAngle; // the right angle of the sector specifying the end of the sector for the node's children

    public Vector3 Point;

    public TNode()
    {
        Visible = true;
        Children = new List<TNode>();
        Point = Vector3.Zero;
    }

    public object Data { get; set; }

    public TNode Parent { get; set; }
    public List<TNode> Children { get; set; }
    public bool Visible { get; set; }
    public int Level { get; set; }
    public int ChildIndex { get; set; }

    public TNode Left
    {
        get
        {
            if (Children.Count == 0)
                return null;
            return Children[0];
        }
        set
        {
            if (Children.Count == 0)
                Children.Add(value);
            else Children[0] = value;
        }
    }

    public TNode Right
    {
        get
        {
            if (Children.Count < 2)
                return null;
            return Children[1];
        }
        set
        {
            if (Children.Count < 2)
                Children.Add(value);
            else Children[1] = value;
        }
    }

    public bool IsLeaf => Children.Count == 0;
    public bool HasChildren => Children.Count > 0;

    public float LeftLimit()
    {
        return (float)Math.Min(normalize(leftBisector), leftTangent);
    }

    public float RightLimit()
    {
        return (float)Math.Max(normalize(rightBisector), rightTangent);
    }

    private double normalize(double angle)
    {
        //while (angle > TWO_PI) {
        //    angle -= TWO_PI;
        //}
        //while (angle < -TWO_PI) {
        //    angle += TWO_PI;
        //}
        return angle;
    }
}