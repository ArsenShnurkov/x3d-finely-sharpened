using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using X3D.Parser;

namespace X3D
{
    public partial class IndexedFaceSet
    {
        private Coordinate coordinate;
        private TextureCoordinate texCoordinate;
        private int[] _indices;
        private Vector3[] _coords;
        private int[] _colorIndicies;
        private Color color;
        private ColorRGBA colorRGBA;
        private int[] _texIndices;
        private Vector2[] _texCoords;
        private const int RESTART_INDEX = -1;
         
        public override void Load()
        {
            int? restartIndex = null;
            
            base.Load();
            
            texCoordinate = (TextureCoordinate)this.Items.FirstOrDefault(n => n.GetType() == typeof(TextureCoordinate));
            coordinate = (Coordinate)this.Items.FirstOrDefault(n => n.GetType() == typeof(Coordinate));
            color = (Color)this.Items.FirstOrDefault(n => n.GetType() == typeof(Color));
            colorRGBA = (ColorRGBA)this.Items.FirstOrDefault(n => n.GetType() == typeof(ColorRGBA));

            if (this.texCoordinate != null && !string.IsNullOrEmpty(this.texCoordIndex))
            {
                _texIndices = Helpers.ParseIndicies(this.texCoordIndex);
                _texCoords = Helpers.MFVec2f(this.texCoordinate.point);
            }

            if (this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
            {
                _indices = Helpers.ParseIndicies(this.coordIndex);
                _coords = Helpers.MFVec3f(this.coordinate.point);

                if (!string.IsNullOrEmpty(this.colorIndex))
                {
                    _colorIndicies = Helpers.ParseIndicies(this.colorIndex);
                }

                
                if (this.coordIndex.Contains(RESTART_INDEX.ToString()))
                {
                    restartIndex = RESTART_INDEX;
                }

                Helpers.Interleave(out _vbo_interleaved, out NumVerticies, _indices,_texIndices, _coords, 
                    _texCoords, null, restartIndex);
            }

            GL.UseProgram(Shape.shaderProgramHandle);

            int uniformSize = GL.GetUniformLocation(Shape.shaderProgramHandle, "size");
            int uniformScale = GL.GetUniformLocation(Shape.shaderProgramHandle, "scale");

            var size = new Vector3(1,1,1);
            //var scale = new Vector3(1, 1, 1);
            var scale = new Vector3(0.05f, 0.05f, 0.05f);

            GL.Uniform3(uniformSize, size);
            GL.Uniform3(uniformScale, scale);

            Console.WriteLine("IndexedFaceSet [loaded]");
        }
        int _vbo_interleaved;
        int NumVerticies;
        int vao;

        public override void Render(RenderingContext rc)
        {
            base.Render(rc);

            if(this.coordinate != null && !string.IsNullOrEmpty(this.coordIndex))
            {


                GL.UseProgram(Shape.shaderProgramHandle);
                //GL.BindVertexArray(vao);
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vbo_interleaved); // InterleavedArrayFormat.T2fC4fN3fV3f
                GL.DrawArrays(BeginMode.Triangles, 0, NumVerticies); // Triangles Points  Lines
            }
        }
    }
}
