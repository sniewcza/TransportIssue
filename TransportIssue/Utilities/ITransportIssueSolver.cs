namespace TransportIssue.Utilities
{
    public interface ITransportIssueSolver
    {
        double[,] FindaBaseSolution(double[,] transportCosts, double[] delivers, double[] recipents);
    }
}
