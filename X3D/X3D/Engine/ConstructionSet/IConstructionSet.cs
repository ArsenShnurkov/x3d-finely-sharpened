namespace X3D.Engine
{
    public interface IConstructionSet
    {
        IElevationBuilder ElevationBuilder { get; set; }

        IPerlinNoiseGenerator GetPerlinNoiseProvider();
    }
}