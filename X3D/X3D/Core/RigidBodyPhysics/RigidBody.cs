using System;
using System.Collections.Generic;
using OpenTK;
using X3D.Core.RigidBodyPhysics;

namespace X3D
{
    /// <summary>
    ///     == Rigid Body Physics == for solid non-deformable objects in the Scene Graph.
    ///     Precondition: Evaluate the Physics Model after any keybindings, and scripts have executed,
    ///     right before the Scene Graph is rendered.
    /// </summary>
    public partial class RigidBody
    {
        /// <summary>
        ///     Gets the translation component of the current non-deformable solid object from the current Transform Hierarchy
        ///     context.
        /// </summary>
        /// <returns>
        ///     A Vector3 composing the translation component.
        /// </returns>
        public Vector3 GetTranslation()
        {
            List<Transform> transformHierarchy;

            transformHierarchy = AscendantByType<Transform>();


            throw new NotImplementedException();
        }

        public void AddForce(Vector3 force, ForceType type)
        {
            switch (type)
            {
                case ForceType.Force:
                    ApplyForce(force);
                    break;
                case ForceType.Acceleration:
                    ApplyAcceleration(force);
                    break;
                case ForceType.Impulse:
                    ApplyForceImpulse(force);
                    break;
                case ForceType.Velocity:
                    ApplyVelocity(force);
                    break;
            }
        }

        public void ApplyForce(Vector3 force)
        {
            Vector3 translation;

            translation = GetTranslation();

            Physics.ApplyForce(this, translation, force);
        }

        public void ApplyForceImpulse(Vector3 force)
        {
            Vector3 translation;

            translation = GetTranslation();

            Physics.ApplyForceImpulse(this, translation, force);
        }

        public void ApplyVelocity(Vector3 velocity)
        {
            Vector3 translation;

            translation = GetTranslation();

            Physics.ApplyVelocity(this, translation, velocity);
        }

        public void ApplyAcceleration(Vector3 acceleration)
        {
            Vector3 translation;

            translation = GetTranslation();

            Physics.ApplyAcceleration(this, translation, acceleration);
        }

        public void ApplyGravity()
        {
            Vector3 translation;

            translation = GetTranslation();

            Physics.ApplyGravityAcceler(this, translation, Physics.Gravity);
        }

        public void ApplyGravity(Vector3 gravity)
        {
            Vector3 translation;

            translation = GetTranslation();

            Physics.ApplyGravityAcceler(this, translation, gravity);
        }
    }
}