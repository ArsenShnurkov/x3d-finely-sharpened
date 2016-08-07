using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class TNode
{
    public object Data { get; set; }

    public TNode Parent { get; set; }
    public List<TNode> Children { get; set; }
    public bool Visible { get; set; }
    public int Level { get; set; }
    public int ChildIndex { get; set; }

    public Vector3 Point;

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

    public float 
        angle, // angle from root coordinate in radians
        prev_gap,prev_angle,
        rightBisector, 
        leftBisector,
        leftTangent, // the left tangent angle
        rightTangent, // the right tangent angle
        sectorStartAngle, // the left angle of the sector specifying the start of the sector for the node's children
        sectorEndAngle; // the right angle of the sector specifying the end of the sector for the node's children

    private const double TWO_PI=2.0 * Math.PI;

    public bool IsLeaf { get { return this.Children.Count == 0; } }
    public bool HasChildren { get { return this.Children.Count > 0; } }

    public float LeftLimit()
    {
        return (float)Math.Min(this.normalize(this.leftBisector),(this.leftTangent));
    }

    public float RightLimit()
    {
        return (float)Math.Max(this.normalize(this.rightBisector),(this.rightTangent));
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

    public TNode()
    {
        Visible=true;
        Children=new List<TNode>();
        Point = Vector3.Zero;
    }
}
