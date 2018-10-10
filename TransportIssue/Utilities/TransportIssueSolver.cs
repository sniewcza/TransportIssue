using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
namespace TransportIssue.Utilities
{
    public class TransportIssueSolver : ITransportIssueSolver
    {
        public double[,] FindaBaseSolution(double[,] transportCosts, double[] delivers, double[] recipents)
        {
            if (transportCosts.Length != 9 || delivers.Length != 3 || recipents.Length != 9)
            {
                throw new ArgumentException("It only works for 3x3 matrix");
            }
            if (delivers.Length * recipents.Length != transportCosts.Length)
            {
                throw new ArgumentException("Transport Costs matrix has to be delivers*recipents lenght");
            }

            var result = DenseMatrix.Create(3, 3, -1);
            var costs = DenseMatrix.OfArray(transportCosts);

            do
            {
                var minimalValue = costs.Values.Where(val => val > 0).Min();
                var minEl = costs.Find(val => val == minimalValue);

                var Iindex = minEl.Item1;
                var Jindex = minEl.Item2;
                double deliver = delivers[minEl.Item1];
                double recipent = recipents[minEl.Item2];
                result[Iindex, Jindex] = deliver <= recipent ? deliver : recipent;

                delivers[Iindex] -= costs[Iindex, Jindex];
                recipents[Jindex] -= costs[Iindex, Jindex];

                if (delivers[Iindex] == 0)
                {
                    var row = result.Row(Iindex).ToArray().Select(val => val == result[Iindex, Jindex] ? val : 0);
                    result.SetRow(Iindex, row.ToArray());
                    costs.SetRow(Iindex, new double[] { 0, 0, 0 });
                }

                if (recipents[Jindex] == 0)
                {
                    var column = result.Column(Jindex).ToArray().Select(val => val == result[Iindex, Jindex] ? val : 0);
                    result.SetColumn(Jindex, column.ToArray());
                    costs.SetColumn(Jindex, new double[] { 0, 0, 0 });
                }
            } while (result.Values.Contains(-1));

            return result.ToArray();
        }
    }
}
