using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class MatrixCollection
{
    public MatrixCollection Parent { get; set; }

    internal Matrix4 projection,
                    modelview;

    internal static Matrix4 DefaultModelview = Matrix4.Identity;

    private int uniform_mview,
                uniform_pview;

    public void GetMatrixUniforms()
    {
        //uniform_pview = shader.GetUniformLocation("projection");
        //uniform_mview = shader.GetUniformLocation("modelview");
    }

    public void SetMatrixUniforms()
    {
        if (Parent == null)
        {
            // root transform
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref projection);
        }

        //modelview *= projection;

        //GL.UniformMatrix4(uniform_pview, false, ref projection);
        //GL.UniformMatrix4(uniform_mview, false, ref modelview);

        GL.MatrixMode(MatrixMode.Modelview);
        GL.LoadMatrix(ref modelview);
    }

    public void ApplyLocalTransformations(Vector3 translation, Vector3 scale)
    {
        modelview = Matrix4.Identity
            * Matrix4.CreateTranslation(translation)
            * Matrix4.Scale(scale);

        if (Parent == null)
        {

        }
        else
        {
            modelview *= Parent.modelview;
        }



        //modelview *= Matrix4.CreateTranslation(translation); // Matrix4.Mult(ref translation, ref modelview, out modelview);
        //modelview *= Matrix4.Scale(scale); // Matrix4.Mult(ref scalematrix, ref modelview, out modelview);
    }

    public void UpdateProjection(INativeWindow window)
    {
        this.projection = CreatePerspectiveFieldOfView(window);
    }

    public static MatrixCollection CreateInitialMatricies()
    {
        MatrixCollection mc = new MatrixCollection();

        mc.modelview = Matrix4.Identity;
        //mc.modelview = Matrix4.LookAt(Vector3.UnitX, Vector3.UnitZ, Vector3.UnitY);
        mc.projection = Matrix4.Identity;

        return mc;
    }

    public static MatrixCollection CreateInitialMatricies(INativeWindow window)
    {
        MatrixCollection mc = new MatrixCollection();

        mc.modelview = Matrix4.Identity;
        //mc.modelview = Matrix4.LookAt(Vector3.UnitX, Vector3.UnitZ, Vector3.UnitY);
        mc.projection = CreatePerspectiveFieldOfView(window);

        return mc;
    }

    public static Matrix4 CreatePerspectiveFieldOfView(INativeWindow window)
    {
        float aspect = (float)window.Width / (float)window.Height;

        //return Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1.0f, 500.0f);
        return Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 0.00001f, 500.0f);
    }



    public void BeginModel()
    {
        //Matrices.PushMatrix(this);
    }

    public void EndModel()
    {
        //Matrices.PopMatrix();
    }
}
