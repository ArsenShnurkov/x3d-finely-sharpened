using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// http://www.web3d.org/documents/specifications/19775-1/V3.3/Part01/components/rigid_physics.html#ConceptsOverview
    /// </summary>
    public class Physics
    {
        public static Vector3 Up = Vector3.UnitY;
        public static Vector3 Gravity = Up * 0.5f; 

        public static void ApplyForce(float mass, Vector3 translation, Vector3 force)
        {
            throw new NotImplementedException();
        }

        public static void ApplyForceImpulse(float mass, Vector3 translation, Vector3 force)
        {
            throw new NotImplementedException();
        }

        public static void ApplyVelocity(float mass, Vector3 translation, Vector3 velocity)
        {
            throw new NotImplementedException();
        }

        public static void ApplyAcceleration(float mass, Vector3 translation, Vector3 acceleration)
        {
            throw new NotImplementedException();
        }

        public static void ApplyGravityAcceler(float mass, Vector3 translation, Vector3 gravity)
        {
            throw new NotImplementedException();
        }


        public void AddExplosionForce(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier, ForceType mode)
        {
            throw new NotImplementedException();
        }

        public void AddForceAtPosition(float mass, Vector3 force, Vector3 position)
        {
            throw new NotImplementedException();
        }

        public void AddForceAtPosition(float mass, Vector3 force, Vector3 position, ForceType mode)
        {
            throw new NotImplementedException();
        }

        public void AddRelativeForce(float mass, Vector3 force, ForceType mode)
        {
            throw new NotImplementedException();
        }

        public void AddTorque(float mass, Vector3 torque, ForceType mode)
        {
            throw new NotImplementedException();
        }
    }
}
