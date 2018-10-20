using System;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
namespace TransportIssue.Utilities
{
    public class TransportIssueSolver : ITransportIssueSolver
    {
        public double[,] FindaBaseSolution(double[,] transportCosts, double[] delivers, double[] recipents)
        {           
            if (transportCosts.Length != 9 || delivers.Length != 3 || recipents.Length != 3)
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

                delivers[Iindex] -= result[Iindex, Jindex];
                recipents[Jindex] -= result[Iindex, Jindex];

                if (delivers[Iindex] == 0)
                {
                    var row = result.Row(Iindex).ToArray().Select(val => val == -1 ? 0 : val);
                    result.SetRow(Iindex, row.ToArray());
                    costs.SetRow(Iindex, new double[] { 0, 0, 0 });
                }

                if (recipents[Jindex] == 0)
                {
                    var column = result.Column(Jindex).ToArray().Select(val => val == -1 ? 0 : val);
                    result.SetColumn(Jindex, column.ToArray());
                    costs.SetColumn(Jindex, new double[] { 0, 0, 0 });
                }
            } while (result.Values.Contains(-1));

            return result.ToArray();
        }

        public double[,] BuildOptimalityIndexTable(double[,] input , double[] alfa, double[] beta)
        {
           
            DenseMatrix matrix = DenseMatrix.OfArray(input);

            for (int i = 0; i < matrix.RowCount; i++)
                for (int j = 0; j < matrix.ColumnCount; j++)
                    if (matrix[i, j] != 0) 
                    {
                        matrix[i, j] = matrix[i, j] + alfa[i] + beta[j];
                    }

            return matrix.ToArray();
        }

        private bool isOptimal(double[,] input)
        {
            return DenseMatrix.OfArray(input).Find(val => val < 0) == null ? true : false;
        }

        public Tuple<double[], double[]> BuildDualVariables(double[,] solution)
        {
            DenseVector alfaVector = DenseVector.Create(3, int.MaxValue);
            DenseVector betaVector = DenseVector.Create(3, int.MaxValue);
            var matrix = DenseMatrix.OfArray(solution);

            alfaVector[0] = 0;

            do
            {
                for (int index = 0; index < 3; index++)
                {
                    var values = matrix.Row(index);

                    for (int i = 0; i < values.Count; i++)
                    {
                        if (alfaVector[index] != int.MaxValue && values[i] != 0)
                        {
                            betaVector[i] = Solve(alfaVector[index], values[i]);
                        }
                    }

                    values = matrix.Column(index);

                    for (int i = 0; i < values.Count; i++)
                    {
                        if (betaVector[index] != int.MaxValue && values[i] !=0)
                        {
                            alfaVector[i] = Solve(betaVector[index], values[i]);
                        }
                    }
                }
            } while (alfaVector.Contains(int.MaxValue) && betaVector.Contains(int.MaxValue));

            return new Tuple<double[], double[]>(alfaVector.Values, betaVector.Values);
        }

        private double Solve(double a, double b)
        {
            return -a - b;
        }
    }
}
