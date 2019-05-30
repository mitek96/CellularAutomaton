namespace Automat1D
{
   public enum NeighborDirection
    {
        topLeft,
        top,
        topRight,
        left,
        right,
        bottomLeft,
        bottom,
        bottomRight
    }

    public enum BoundaryCondition
    {
        periodic,
        sorption
    }

    public enum Neighborhood
    {
        VonNeumann,
        Moore,
        Radial,
        PentLeft,
        PentRight,
        PentTop,
        PentBottom,
        PentRand,
        HexLeft,
        HexRight,
        HexRand
    }

}