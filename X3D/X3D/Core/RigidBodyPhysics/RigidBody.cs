using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X3D.Core.RigidBodyPhysics;

namespace X3D
{
    public partial class RigidBody
    {
        public Vector3 GetTranslation()
        {
            List<Transform> transformHierarchy;

            transformHierarchy = this.AscendantByType<Transform>();



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
