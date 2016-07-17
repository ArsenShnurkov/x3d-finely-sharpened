using System;
using OpenTK;

namespace X3D.Engine
{
	public class SceneEntity 
	{
		public SceneEntity()
		{
			Up = Vector3.UnitZ;  
			Right = Vector3.UnitX;
			Look = Vector3.Zero;
			Direction = Vector3.Zero;
			Position = Vector3.Zero;
			Matrix = Matrix4.Identity;
			Orientation = Quaternion.Identity;
			yaw = Quaternion.Identity;
			pitch = Quaternion.Identity;
			roll = Quaternion.Identity;
		}
		//TODO: implement SceneEntity taking into account what is needed in Distributed Interactive Simulation spec
		// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/dis.html
		public Vector3 Up;
		public Vector3 Look;
		public Vector3 Right;
		public Vector3 Direction;
		public Vector3 Position;
		public Quaternion Orientation;
		public Quaternion yaw;
		public Quaternion pitch;
		public Quaternion roll;
		public Matrix4 Matrix;
		public Matrix4 ModelView;

		// scene entity velocity, orientation, location

		public void ApplyTransformations()
		{
			Matrix4 outm = Matrix4.Identity;
			Matrix4 lookat = Matrix4.Identity;
			Matrix4 location = Matrix4.Identity;

            

            outm = outm * Matrix4.CreateTranslation(Position);

			ModelView *= Matrix4.CreateScale(new Vector3(1.0f, 1.0f, -1.0f)); // ModelView = ModelView.Scale(1.0, 1.0, -1.0);

			this.Matrix = outm;
		}

		public void ApplyOrientation(float pitch, float yaw)
		{
            Vector3 direction = (this.Look - this.Position);
            direction.Normalize();

			Vector3 pitch_axis = Vector3.Cross(direction, this.Up);

			this.pitch = Quaternion.FromAxisAngle (pitch_axis, pitch);
			this.yaw = Quaternion.FromAxisAngle (this.Up, yaw);

			this.Orientation = this.yaw * this.pitch  /* this.roll */ ;
			this.Orientation.Normalize();

			this.Direction = QuaternionExtensions.Rotate(Orientation, Right);
		}

		//public Matrix4 lookAt(Vector3 eye, Vector3 center, Vector3 up) 
		//{
		//	return MatrixExtensions.LookAt(eye, center, up, this.Matrix);
		//}
	}
}

