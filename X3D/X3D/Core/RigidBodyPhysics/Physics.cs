using System;
using OpenTK;

namespace X3D.Core.RigidBodyPhysics
{
    public enum ForceType
    {
        Force,
        Impulse,
        Velocity,
        Acceleration
    }

    /// <summary>
    ///     CLASSICAL MECHANICS
    ///     A model Abstracting Rigid Body Physics
    ///     with API based somewhat on Unity3D's RigidBodyPhysics component model.
    ///     See spec:
    ///     http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/rigid_physics.html#ConceptsOverview
    /// </summary>
    public class Physics
    {
        public static Vector3 Up = Vector3.UnitY;
        public static Vector3 Gravity = Up * 0.5f;

        public static void ApplyForce(RigidBody rb, Vector3 translation, Vector3 force)
        {
            throw new NotImplementedException();
        }

        public static void ApplyForceImpulse(RigidBody rb, Vector3 translation, Vector3 force)
        {
            throw new NotImplementedException();
        }

        public static void ApplyVelocity(RigidBody rb, Vector3 translation, Vector3 velocity)
        {
            throw new NotImplementedException();
        }

        public static void ApplyAcceleration(RigidBody rb, Vector3 translation, Vector3 acceleration)
        {
            throw new NotImplementedException();
        }

        public static void ApplyGravityAcceler(RigidBody rb, Vector3 translation, Vector3 gravity)
        {
            throw new NotImplementedException();
        }


        public void ApplyExplosionForce(RigidBody rb, float explosionForce, Vector3 explosionPosition,
            float explosionRadius, float upwardsModifier, ForceType mode)
        {
            throw new NotImplementedException();
        }

        public void ApplyForceAtPosition(RigidBody rb, Vector3 force, Vector3 position)
        {
            throw new NotImplementedException();
        }

        public void ApplyForceAtPosition(RigidBody rb, Vector3 force, Vector3 position, ForceType mode)
        {
            throw new NotImplementedException();
        }

        public void ApplyRelativeForce(RigidBody rb, Vector3 force, ForceType mode)
        {
            throw new NotImplementedException();
        }

        public void ApplyTorque(RigidBody rb, Vector3 torque, ForceType mode)
        {
            throw new NotImplementedException();
        }

        public void ApplyTwistForce(RigidBody rb, Vector3 twist, ForceType mode)
        {
        }
    }
}