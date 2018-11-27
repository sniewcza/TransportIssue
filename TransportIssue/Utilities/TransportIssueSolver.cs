using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra.Double;
using AForge.Math;
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

        public double[,] BuildOptimalityIndexTable(double[,] input, double[] alfa, double[] beta)
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

        public double getZ(double[,] costs, double[,] solution)
        {
            DenseMatrix c = DenseMatrix.OfArray(costs);
            DenseMatrix s = DenseMatrix.OfArray(solution);

            double summ = 0;
            for (int i = 0; i < c.RowCount; i++)
                for (int j = 0; j < c.ColumnCount; j++)
                {
                    summ += c[i, j] * s[i, j];
                }
            return summ;
        }
        public bool isOptimal(double[,] input)
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
                        if (betaVector[index] != int.MaxValue && values[i] != 0)
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

        public double[,] InverseBaseSolutionMatrix(double[,] baseSolution, double[,] transportCosts)
        {
            DenseMatrix inversed = DenseMatrix.Create(3, 3, 0);

            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (baseSolution[i, j] == 0)
                    {
                        inversed[i, j] = transportCosts[i, j];
                    }
            return inversed.ToArray();
        }

        public double[,] InverseCostsMatrix(double[,] costs, double[,] baseSolution)
        {
            DenseMatrix inversed = DenseMatrix.Create(3, 3, 0);
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    if (baseSolution[i, j] != 0)
                    {
                        inversed[i, j] = costs[i, j];
                    }
            return inversed.ToArray();
        }

        public double[,] BuildNextBaseSolution(double[,] baseSolution, List<Tuple<int, int>> cycle)
        {

            DenseMatrix curentSolution = DenseMatrix.OfArray(baseSolution);

            var negative1 = cycle[1];
            var negative2 = cycle[3];
            var val1 = curentSolution[negative1.Item1, negative1.Item2];
            var val2 = curentSolution[negative2.Item1, negative2.Item2];
            double min = val1 < val2 ? val1 : val2;

            for (int i = 0; i < cycle.Count; i++)
            {
                if (i == 0 || i % 2 == 0)
                {
                    curentSolution[cycle[i].Item1, cycle[i].Item2] += min;
                }
                else
                {
                    curentSolution[cycle[i].Item1, cycle[i].Item2] -= min;
                }
            }

            return curentSolution.ToArray();
        }

        public List<Tuple<int, int, double>> BuildCycle(double[,] input)
        {
            DenseMatrix matrix = DenseMatrix.OfArray(input);
            List<Tuple<int, int, double>> list = new List<Tuple<int, int, double>>();
            Tuple<int, int, double> _start = null;



            double temp1 = 0;
            int minX = 0;
            int minY = 0;
            // Start element of cycle
            for (int i = 0; i < matrix.ColumnCount; i++)
                for (int j = 0; j < matrix.RowCount; j++)
                    if (matrix[i, j] < 0)
                    {

                        if (temp1 > matrix[i, j])
                        {
                            temp1 = matrix[i, j];
                            minX = i;
                            minY = j;
                        }
                    }
            Tuple<int, int> min = new Tuple<int, int>(minX, minY);
            _start = new Tuple<int, int, double>(min.Item1, min.Item2, min.Item1 + min.Item2);

            list.Add(_start);
            FirstPath(list, matrix);
            if (list.Count < 4)
            {
                list.Clear();
                list.Add(_start);
                SecondPath(list, matrix);
            }
            //list.Sort((x,y) => x.Item3.CompareTo(y.Item3));

            //swap
            if (list.ElementAt(1).Item1 > list.ElementAt(2).Item1)
            {
                Tuple<int, int, double> temp = list.ElementAt(1);
                list[1] = list[2];
                list[2] = temp;
            }

            return list;

        }

        public List<Tuple<int, int, double>> FirstPath(List<Tuple<int, int, double>> list, DenseMatrix matrix)
        {


            //#2
            for (int i = 0; i < matrix.ColumnCount; i++)
                if (matrix[i, list.ElementAt(0).Item2] == 0 && i != list.ElementAt(0).Item1)
                {
                    list.Add(new Tuple<int, int, double>(i, list.ElementAt(0).Item2, i + list.ElementAt(0).Item2));
                    break;
                }

            //#3
            for (int j = 0; j < matrix.RowCount; j++)
                if (matrix[list.ElementAt(1).Item1, j] == 0 && j != list.ElementAt(1).Item2)
                {
                    list.Add(new Tuple<int, int, double>(list.ElementAt(1).Item1, j, list.ElementAt(1).Item1 + j));
                    break;
                }
            //#4
            for (int i = 0; i < matrix.ColumnCount; i++)
                if (matrix[i, list.ElementAt(2).Item2] == 0 && i != list.ElementAt(2).Item1)
                {
                    list.Add(new Tuple<int, int, double>(i, list.ElementAt(2).Item2, i + list.ElementAt(2).Item2));
                    break;
                }

            return list;
        }

        public List<Tuple<int, int, double>> SecondPath(List<Tuple<int, int, double>> list, DenseMatrix matrix)
        {
            //#2
            for (int j = 0; j < matrix.RowCount; j++)
                if (matrix[list.ElementAt(0).Item1, j] == 0 && j != list.ElementAt(0).Item2)
                {
                    list.Add(new Tuple<int, int, double>(list.ElementAt(0).Item1, j, list.ElementAt(0).Item1 + j));
                    break;
                }
            //#3
            for (int i = 0; i < matrix.ColumnCount; i++)
                if (matrix[i, list.ElementAt(1).Item2] == 0 && i != list.ElementAt(1).Item1)
                {
                    list.Add(new Tuple<int, int, double>(i, list.ElementAt(1).Item2, i + list.ElementAt(1).Item2));
                    break;
                }

            //#4
            for (int j = 0; j < matrix.RowCount; j++)
                if (matrix[list.ElementAt(2).Item1, j] == 0 && j != list.ElementAt(2).Item2)
                {
                    list.Add(new Tuple<int, int, double>(list.ElementAt(2).Item1, j, list.ElementAt(2).Item1 + j));
                    break;
                }

            return list;
        }

        public double[,] SolveTheCycle(List<Tuple<int, int, double>> list, double[,] baseSolution)
        {
            DenseMatrix curentSolution = DenseMatrix.OfArray(baseSolution);

            double ToSubtract = 0;

            var points = list.Select(t => new AForge.IntPoint(t.Item1, t.Item2)).ToArray();

            List<float> distances = new List<float>();
            for (int i = 1; i < points.Count(); i++)
            {
                distances.Add(points[0].DistanceTo(points[i]));
            }

            var max = distances.Max();
          
            var ptmax = points.First(pt => points[0].DistanceTo(pt) == max);

            var pointsToSubstract = points.Where(pt => pt != points[0] && pt != ptmax).ToArray();

            var pointsToAdd = points.Where(pt => pt == points[0] || pt == ptmax).ToArray();



            ToSubtract = curentSolution[pointsToSubstract[0].X,pointsToSubstract[0].Y] < curentSolution[pointsToSubstract[1].X,pointsToSubstract[1].Y] ?
             curentSolution[pointsToSubstract[0].X, pointsToSubstract[0].Y] : curentSolution[pointsToSubstract[1].X, pointsToSubstract[1].Y];
            
            foreach(var pt in pointsToSubstract)
            {
                curentSolution[pt.X, pt.Y] -= ToSubtract;
            }

            foreach(var pt in pointsToAdd)
            {
                curentSolution[pt.X, pt.Y] += ToSubtract;
            }

            return curentSolution.ToArray();
        }
    }

}
