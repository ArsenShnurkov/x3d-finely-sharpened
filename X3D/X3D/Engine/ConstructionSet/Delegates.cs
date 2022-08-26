using OpenTK;

namespace X3D.ConstructionSet
{
    /// <summary>
    ///     Computes a color given vertex and or (x, z) coordinates of a plane.
    /// </summary>
    public delegate Vector3 ElevationColorSequencerDelegate(int face, int vertex, int x, int z);

    /// <summary>
    ///     Computes a height value given (x,z) coordinates of a plane.
    /// </summary>
    public delegate float HeightComputationDelegate(int x, int z);
}