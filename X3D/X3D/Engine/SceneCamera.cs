/* Description: Scene Camera Library
 * Author Copyright © 2013 - 2016 Gerallt G. Franke
 * Licence: BSD
 * */


// TODO: steroscopic mode

using System;
using OpenTK;
using OpenTK.Graphics.OpenGL4;

namespace X3D.Engine
{
    public enum NavigationType
    {
        Walk,
        Fly,
        Examine
    }

    public enum CameraPerspectiveType
    {
        Projection, // Standard 3D perspective frustum  projection.
        Orthographic, // 'CAD editor' like perspective.
        Isometric, // 'Diablo like' isometric perspective.
        Stereographic // 'VR like' Stereo perspective with or without a distortion filter.
    }

    public class SceneCamera
    {
        public Vector3 calibOrient = Vector3.Zero;
        public Vector3 calibSpeed = new Vector3(0.01f, 0.01f, 0.01f);

        /// <summary>
        ///     Value used for debugging
        /// </summary>
        public Vector3 calibTrans = Vector3.Zero;

        public float camera_pitch;

        public float camera_roll;
        public float camera_yaw;
        public bool crouched;

        public Vector3 Direction;

        public Vector3 DollyDirection = Vector3.UnitZ;

        public Vector3 Forward, Up, Right, Look;

        //public Q3Movement playerMover;
        public bool HasChanges;
        public int Height;
        public bool inverted = false;
        public float max_pitch = 5.0f;
        public float max_yaw = 5.0f;
        public Vector3 movement;
        public bool noclip = false; // TODO: implement no clipping (turn off object collision)
        public bool onGround;
        public Vector2 OrbitLocalOrientation = Vector2.Zero;
        public Quaternion Orientation, PrevOrientation;

        public Quaternion orientation_quat;
        public Quaternion pitch;

        public float playerHeight = 0.0f;
        public Vector3 Position, Origin, PrevPosition;
        public Matrix4 Projection;
        public Quaternion roll;
        public Vector3 Rotation, OriginRotation;
        public Vector3 Scale = Vector3.One;
        public Vector3 velocity;
        public Matrix4 ViewMatrix;
        public Matrix4 ViewMatrixNoRot;
        public int Width;

        public Quaternion yaw;

        public SceneCamera(int viewportWidth, int viewportHeight)
        {
            // Keyboard navigation parameters

            velocity = Vector3.Zero;
            onGround = false;

            ViewMatrix = Matrix4.Identity;
            Orientation = Quaternion.Identity;

            Rotation = Vector3.Zero;
            yaw = Quaternion.Identity;
            pitch = Quaternion.Identity;
            roll = Quaternion.Identity;
            Look = Vector3.Zero;
            Up = Vector3.Zero;
            Right = Vector3.Zero;

            PrevOrientation = Quaternion.Identity;
            PrevPosition = Vector3.Zero;
            HasChanges = false;


            Right = Vector3.UnitX;

            Forward = Vector3.UnitZ;

            Direction = Forward;

            //Mouse Navigation parameters
            Up = Vector3.UnitY;

            //Position = Origin = movement = new Vector3(0, 0, -2); /*
            Position = Origin = movement = Vector3.Zero; // Q3 // */

            Width = viewportWidth;
            Height = viewportHeight;

            ApplyViewport(viewportWidth, viewportHeight);
        }

        /// <summary>
        ///     Reset camera to point at horizon
        /// </summary>
        public void Horizon()
        {
            //this.Forward = Vector3.UnitY;
            //this.Up = Vector3.UnitZ; // UnitZ UnitY
            //this.Right = Vector3.UnitX;
        }

        /// <summary>
        ///     Get the current orientation and return it in a Matrix with no translations applied.
        /// </summary>
        public Matrix4 GetWorldOrientation()
        {
            Matrix4 worldView;
            Vector3 worldPosition;

            // Set translation to world origin
            worldPosition = Vector3.Zero;
            Look = worldPosition + Direction * 1.0f;
            worldView = Matrix4.LookAt(worldPosition, Look, Up);

            // Apply Orientation
            var q = Orientation; //.Inverted();

            worldView *= MathHelpers.CreateRotation(ref q);

            return worldView;
        }

        public Matrix4 GetModelTranslation()
        {
            Matrix4 modelView;
            Vector3 playerPosition;

            playerPosition = new Vector3(Position.X + NavigationInfo.AvatarSize.X,
                Position.Y + NavigationInfo.AvatarSize.Y,
                Position.Z + playerHeight + NavigationInfo.AvatarSize.Z
            );

            Look = playerPosition + Direction * 1.0f;
            modelView = Matrix4.LookAt(playerPosition, Look, Up);

            return modelView;
        }

        /// <summary>
        ///     Applies transformations using camera configuration and camera vectors and assigns a new View Matrix.
        /// </summary>
        public void ApplyTransformations()
        {
            GL.Viewport(0, 0, Width, Height);


            var PlayerPosition = new Vector3(Position.X + NavigationInfo.AvatarSize.X,
                Position.Y + NavigationInfo.AvatarSize.Y,
                Position.Z + playerHeight + NavigationInfo.AvatarSize.Z
            );

            var outm = Matrix4.Identity;

            if (NavigationInfo.NavigationType == NavigationType.Examine)
            {
                outm = Matrix4.LookAt(Position, Position + DollyDirection, Up);
                // Test code put in quickly just for Mouse Navigation merged here
            }
            else if (NavigationInfo.NavigationType == NavigationType.Walk ||
                     NavigationInfo.NavigationType == NavigationType.Fly)
            {
                Look = PlayerPosition + Direction * 1.0f;

                //Look = QuaternionExtensions.Rotate(Orientation, Direction);

                outm = Matrix4.LookAt(PlayerPosition, Look, Up);
            }

            var q = Orientation;

            //ViewMatrix = MathHelpers.CreateRotation(ref q) * outm; // orientation applies in local space
            ViewMatrix = outm * MathHelpers.CreateRotation(ref q); // orientation applies in world space
            ViewMatrixNoRot = outm; // always stick in front of current player (dont move with world)  
            //ViewMatrix = outm;

            //Vector3 left = Up.Cross(Forward);
            //Matrix = MatrixExtensions.CreateTranslationMatrix(Right, Up, left, PlayerPosition);

            PrevPosition = Position;
        }


        public void ApplyRotation()
        {
            var direction = Look - Position;
            direction.Normalize();

            //MakeOrthogonal();


            var lookat = QuaternionExtensions.Rotate(Orientation, Vector3.UnitZ);
            var forward = new Vector3(lookat.X, 0, lookat.Z).Normalized();
            var up = Vector3.UnitY;
            var left = up.Cross(forward);


            //Vector3 pitch_axis = -1 * Vector3.Cross(direction, Up);
            var roll_axis = forward + Up;
            //Vector3 roll_axis = Forward + Up;

            //pitch_axis = roll_axis;


            //pitch = Quaternion.FromAxisAngle(pitch_axis, camera_pitch  );
            //yaw = Quaternion.FromAxisAngle(Up, camera_yaw);
            //roll = Quaternion.FromAxisAngle(roll_axis, -camera_roll);

            //Orientation = pitch * roll * yaw ;
            //Orientation = pitch * yaw;


            //Orientation = QuaternionExtensions.QuaternionFromEulerAnglesRad(camera_yaw, camera_pitch, 0f );


            //Vector3 Amount = new Vector3(camera_pitch, camera_yaw, 0f);

            //// create orientation vectors
            //Vector3 up = Vector3.UnitY;
            //Quaternion anotherRotation = Quaternion.Identity;

            //Vector3 lookat = QuaternionExtensions.Rotate(anotherRotation, Vector3.UnitZ); //Vector3 lookat = quatRotate(anotherRotation, Vector3.UnitZ);
            //Vector3 forward = new Vector3(lookat.X, 0, lookat.Z).Normalized();
            //Vector3 left = up.Cross(forward);

            //// rotate camera with quaternions created from axis and angle
            //Orientation = (new Quaternion(up, Amount.Y)) * Orientation;
            //Orientation = (new Quaternion(left, Amount.X)) * Orientation;
            //Orientation = (new Quaternion(forward, Amount.Z)) * Orientation;

            Orientation = QuaternionExtensions.EulerToQuat(-camera_pitch, -camera_yaw, 0);

            //roll = QuaternionExtensions.EulerToQuat(0, 0, -camera_roll);
            //roll.Conjugate();
            //roll.Normalize();

            //Orientation *= roll;

            //Orientation.Normalize();


            // Update Direction
            //this.Direction = QuaternionExtensions.Rotate(Orientation, Vector3.UnitZ);


            //Look = QuaternionExtensions.Rotate(Orientation, Vector3.UnitZ);
            //Vector3 forward = new Vector3(lookat.X, 0, lookat.Z).Normalized();
            //Vector3 up = Vector3.UnitY;
            //Vector3 left = up.Cross(forward);

            //this.Direction = lookat;


            Orientation.Normalize();
        }


        //Matrix4 lookAt(Vector3 eye, Vector3 center, Vector3 up) 
        //{
        //	return MatrixExtensions.LookAt(eye,center,up, this.Matrix);
        //}

        public void invert()
        {
            InvNeg();
        }

        public bool InvNeg()
        {
            if (inverted) return InvPos();

            var cam = Position;
            var moveTo = cam;
            var pos = Vector3.UnitZ * -1.0f;

            moveTo = moveTo + pos;
            Position = moveTo;

            crouched = true;

            return true;
        }

        public bool InvPos()
        {
            if (!inverted) return InvNeg();

            var cam = Position; // clone
            var moveTo = cam;
            var pos = Vector3.UnitZ;

            moveTo = moveTo + pos;
            Position = moveTo;

            crouched = false;

            return true;
        }

        public void update(int frame_time)
        {
            // todo: apply any current movement animations here
        }

        public void MakeOrthogonal()
        {
            Look.Normalize();
            Up = Vector3.Cross(Look, Right);
            Right = Vector3.Cross(Up, Look);
            Up.Normalize();
            Right.Normalize();
        }

        public void Reset()
        {
            // Could be used for respawning
            Position = Origin;
            Rotation = OriginRotation;
            Orientation = Quaternion.Identity;
            //xAngle = 0.0;

            camera_pitch = 0;
            camera_roll = 0;
            camera_yaw = 0;
        }

        public void SetOrigin(Vector3 origin, Vector3 rotation)
        {
            Position = Origin = origin;
            Rotation = OriginRotation = rotation;
            Orientation = Quaternion.Identity;
            //xAngle = 0.0;

            camera_pitch = OriginRotation.X * MathHelpers.PiOver180;
            camera_yaw = OriginRotation.Z * MathHelpers.PiOver180;
        }

        #region Viewport

        public void ApplyViewportProjection(Viewpoint viewpoint, View viewport)
        {
            if (!(viewpoint.fieldOfView > 0.0f && viewpoint.fieldOfView < MathHelpers.PI))
            {
                Console.WriteLine("Viewpoint {1} fov '{0}' is out of range. Must be between 0 and PI",
                    viewpoint.fieldOfView,
                    viewpoint.description);
                return;
            }

            float FOVhorizontal, FOVvertical = FOVhorizontal = viewpoint.fieldOfView;

            var dispWidth = (float)Math.Tan(FOVhorizontal / 2.0f);
            var dispHeight = (float)Math.Tan(FOVvertical / 2.0f);

            /* According to spec:
               display width    tan(FOVhorizontal/2)
               -------------- = -------------------
               display height   tan(FOVvertical/2)
             */

            dispWidth = viewport.Width;
            dispHeight = viewport.Height;

            ApplyViewportProjection((int)dispWidth, (int)dispHeight, viewpoint.fieldOfView);
        }

        public void ApplyViewportProjection(int width, int height, float fovy = MathHelper.PiOver4)
        {
            if (!(fovy > 0.0f && fovy < MathHelpers.PI))
            {
                Console.WriteLine("Viewpoint fov '{0}' is out of range. Must be between 0 and PI",
                    fovy);
                return;
            }

            // TODO: define new field-of-view and correct aspect ratio as specified in the Viewpoint specification

            // make use of the camera in the context to define the new viewpoint

            Width = width;
            Height = height;
            var aspectRatio = Width / (float)Height;

            GL.Viewport(0, 0, Width, Height);

            Projection = Matrix4.CreatePerspectiveFieldOfView(fovy, aspectRatio, 0.01f, 1000.0f);
        }

        public void ApplyViewport(int viewportWidth, int viewportHeight)
        {
            Width = viewportWidth;
            Height = viewportHeight;
            var aspectRatio = Width / (float)Height;

            GL.Viewport(0, 0, Width, Height);

            Projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspectRatio, 0.01f, 1000.0f);


            //Matrix4 projection;


            //projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, window.Width / (float)window.Height, 1.0f, 500.0f);
            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadMatrix(ref projection);


            //GL.MatrixMode(MatrixMode.Projection);
            //GL.LoadIdentity();
            //GL.Ortho(-10.0 - zoom - panX, 10.0 + zoom - panX, -10.0 - zoom + panY, 10.0 + zoom + panY, -50.0, 50.0);
        }

        #endregion

        #region Flying Naviagion

        public void Yaw(float radians)
        {
            //angle = MathHelpers.ClampCircular(angle, 0f, MathHelpers.PI2);

            // Up
            var m = Matrix4.CreateFromAxisAngle(Up, radians);

            // Transform vector by matrix, project result back into w = 1.0f
            Right = MatrixExtensions.Transform(m, Right); // TransformVectorCoord
            Up = MatrixExtensions.Transform(m, Look);
        }

        //private float pitchAngle = 0f, yawAngle = 0f;
        public void Pitch(float radians)
        {
            //angle = MathHelpers.ClampCircular(angle, 0f, MathHelpers.PI2);

            // Right
            var m = Matrix4.CreateFromAxisAngle(Right, radians);

            // Transform vector by matrix, project result back into w = 1.0f
            Right = MatrixExtensions.Transform(m, Up); // TransformVectorCoord
            Up = MatrixExtensions.Transform(m, Look);
        }

        public void Roll(float radians)
        {
            // Look, Right and Up
            var m = Matrix4.CreateFromAxisAngle(Look, radians);

            // Transform vector by matrix, project result back into w = 1.0f
            Right = MatrixExtensions.Transform(m, Right); // TransformVectorCoord
            Up = MatrixExtensions.Transform(m, Up);
        }

        public void ForwardOne(float magnitude)
        {
        }

        public void Walk(float magnitude)
        {
            var lookat = QuaternionExtensions.Rotate(Orientation, Vector3.UnitZ);

            Position += lookat * magnitude;
        }

        public void Strafe(float magnitude)
        {
            var lookat = QuaternionExtensions.Rotate(Orientation, Vector3.UnitZ);
            var forward = new Vector3(lookat.X, 0, lookat.Z).Normalized();
            var up = Vector3.UnitY;
            var left = up.Cross(forward);

            Position += left * magnitude;
        }

        public void Fly(float units)
        {
            var up = Vector3.UnitY;

            Position += up * units;
        }

        public void ApplyPitch(float radians)
        {
            //Check bounds with the max pitch rate so that we aren't moving too fast
            //if (radians < -max_pitch)
            //{
            //    radians = -max_pitch;
            //}
            //else if (radians > max_pitch)
            //{
            //    radians = max_pitch;
            //}
            //camera_pitch += radians;

            ////Check bounds for the camera pitch
            //if (camera_pitch > MathHelpers.TwoPi)
            //{
            //    camera_pitch -= MathHelpers.TwoPi;
            //}
            //else if (camera_pitch < -MathHelpers.TwoPi)
            //{
            //    camera_pitch += MathHelpers.TwoPi;
            //}


            //degrees = MathHelpers.ClampCircular(degrees, 0.0f, MathHelpers.PI2);

            camera_pitch += radians;

            Pitch(radians);
        }

        public void ApplyYaw(float radians)
        {
            //Check bounds with the max heading rate so that we aren't moving too fast
            //if (radians < -max_yaw)
            //{
            //    radians = -max_yaw;
            //}
            //else if (radians > max_yaw)
            //{
            //    radians = max_yaw;
            //}
            ////This controls how the heading is changed if the camera is pointed straight up or down
            ////The heading delta direction changes
            //if (camera_pitch > MathHelpers.PIOver2 && camera_pitch < MathHelpers.ThreePIOver2 
            //    || (camera_pitch < -MathHelpers.PIOver2 && camera_pitch > -MathHelpers.ThreePIOver2))
            //{
            //    camera_yaw -= radians;
            //}
            //else
            //{
            //    camera_yaw += radians;
            //}
            ////Check bounds for the camera heading
            //if (camera_yaw > MathHelpers.TwoPi)
            //{
            //    camera_yaw -= MathHelpers.TwoPi;
            //}
            //else if (camera_yaw < -MathHelpers.TwoPi)
            //{
            //    camera_yaw += MathHelpers.TwoPi;
            //}


            //degrees = MathHelpers.ClampCircular(degrees, 0.0f, MathHelpers.PI2);

            camera_yaw += radians;


            Yaw(radians);
        }

        public void ApplyRoll(float radians)
        {
            camera_roll += radians;

            Roll(radians);
        }

        #endregion

        #region Mouse Navigation

        public void Dolly(float distance)
        {
            Position += distance * DollyDirection;
        }

        public void PanXY(float x, float y)
        {
            Position += new Vector3(x, y, 0);
        }

        public void ScaleXY(float x, float y)
        {
            Scale.X = Scale.X + x * .02f;
            Scale.Y = Scale.Y + y * .02f;
        }

        public void OrbitObjectsXY(float x, float y)
        {
            OrbitLocalOrientation.X += x;
            OrbitLocalOrientation.Y += y;

            //OrbitLocalOrientation *= 0.005f;
        }

        #endregion
    }
}