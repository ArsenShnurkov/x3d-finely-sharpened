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
        public static Vector3 Gravity; 

        public static void ApplyForce(Vector3 translation, Vector3 force)
        {
            throw new NotImplementedException();
        }

        public static void ApplyForceImpulse(Vector3 translation, Vector3 force)
        {
            throw new NotImplementedException();
        }

        public static void ApplyVelocity(Vector3 translation, Vector3 velocity)
        {
            throw new NotImplementedException();
        }

        public static void ApplyAcceleration(Vector3 translation, Vector3 acceleration)
        {
            throw new NotImplementedException();
        }

        public static void ApplyGravityAcceler(Vector3 translation, Vector3 gravity)
        {
            throw new NotImplementedException();
        }
    }
}
