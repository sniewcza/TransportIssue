using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransportIssue.Utilities;
using System.Linq;
using System.Collections.Generic;

namespace UnitTests
{
    [TestClass]
    public class TransportIssueSolverTest
    {
        private TransportIssueSolver _solver;
        public TransportIssueSolverTest()
        {
            _solver = new TransportIssueSolver();
        }
        [TestMethod]
        public void Solver_Should_Return_Fixed_BaseSolution()
        {
            //Arrange           
            double[,] costs = new double[,]
            {
                {3,5,7 },
                {12,10,9},
                {13,3,9}
            };
            double[] delivers = new double[] { 50, 70, 30 };
            double[] recipents = new double[] { 20, 40, 90 };
            double[,] validResult = new double[,]
            {
                {20,10,20 },
                {0,0,70},
                {0,30,0}
            };

            //Act
            var solverResult = _solver.FindaBaseSolution(costs, delivers, recipents);

            //Assert

            Assert.IsTrue(CheckEquals(validResult, solverResult));
        }

        [TestMethod]
        public void Solver_Should_Return_Fixed_DualVariables()
        {
            //Arrange
            double[,] input = new double[,]
            {
                {3,5,0 },
                {0,10,9},
                {0,0,9}
            };

            double[] validAlfa = new double[] { 0, -5, -5 };
            double[] validBeta = new double[] { -3, -5, -4 };
            //Act
            var solutions = _solver.BuildDualVariables(input);

            //Assert
            Assert.IsTrue(CheckEquals(solutions.Item1, validAlfa));
            Assert.IsTrue(CheckEquals(solutions.Item2, validBeta));

        }

        private bool CheckEquals(double[,] m1, double[,] m2)
        {
            if (m1.Length != m2.Length)
            {
                throw new ArgumentException("Lengths are not equals!");
            }

            for (int i = 0; i < m1.GetLength(0); i++)
                for (int j = 0; j < m1.GetLength(1); j++)
                    if (m1[i, j] != m2[i, j])
                        return false;
            return true;
        }

        private bool CheckEquals(double[] v1, double[] v2)
        {
            return v1.SequenceEqual(v2);
        }

        private bool CheckEquals(int[] v1, int[] v2)
        {
            return v1.SequenceEqual(v2);
        }

        [TestMethod]
        public void Solver_Should_Return_Fixed_OptimalityIndexTable()
        {
            //Arrange
            double[,] input = new double[,]
            {
                {0,0,7 },
                {12,0,0 },
                {13,3,0 }
            };
            double[] validAlfa = new double[] { 0, -5, -5 };
            double[] validBeta = new double[] { -3, -5, -4 };

            double[,] validResult = new double[,]
            {
                {0,0,3 },
                {4,0,0 },
                {5,-7,0 }
            };

            //Act

            var solverResult = _solver.BuildOptimalityIndexTable(input, validAlfa, validBeta);

            //Assert
            Assert.IsTrue(CheckEquals(validResult, solverResult));
        }

        [TestMethod]
        public void Solver_Should_Return_Fixed_ReversedBaseSolutionTable()
        {
            //Arrange
            double[,] baseSolution = new double[,]
           {
                {20,10,20 },
                {0,0,70},
                {0,30,0}
           };
            double[,] costs = new double[,]
            {
                {3,5,7 },
                {12,10,9},
                {13,3,9}
            };
            double[,] validResult = new double[,]
            {
                {0,0,0 },
                {12,10,0 },
                {13,0,9 }
            };

            //Act
            var solverResult = _solver.InverseBaseSolutionMatrix(baseSolution, costs);

            //Assert
            Assert.IsTrue(CheckEquals(validResult, solverResult));
        }

        [TestMethod]
        public void Solver_Should_Return_Fixed_Cycle_Points()
        {
            //Assert
            double[,] optimalityIndexTable = new double[3, 3]
            {
                {0,0,3},
                {4,0,0},
                {5,-7,0}
            };

            var validResult = new Tuple<int, int>[]
            {
                new Tuple<int, int>(1,1),
                new Tuple<int, int>(1,2),
                new Tuple<int, int>(2,1),
                new Tuple<int, int>(2,2)
            };
            var solverResult = _solver.BuildCycle(optimalityIndexTable);

            Assert.IsTrue(CheckEquals(validResult.Select(i => i.Item1).ToArray(), solverResult.Select(i => i.Item1).ToArray()));
            Assert.IsTrue(CheckEquals(validResult.Select(i => i.Item2).ToArray(), solverResult.Select(i => i.Item2).ToArray()));
        }

        [TestMethod]
        public void Solver_Shoult_Return_Fixed_NextBaseSolution()
        {
            //Arrange
            double[,] optimalityIndexTable = new double[3, 3]
           {
                {0,0,3},
                {4,0,0},
                {5,-7,0}
           };

            var cyclePoints = _solver.BuildCycle(optimalityIndexTable);

            var baseSolution = new double[,]
            {
                {20,30,0 },
                {0,10,60 },
                {0,0,30 }
            };
            var validNextBaseSolution = new double[,]
            {
                {20,30,0 },
                {0,0,70 },
                {0,10,20 }
            };

            var solverResult = _solver.SolveTheCycle(cyclePoints, baseSolution);

            Assert.IsTrue(CheckEquals(validNextBaseSolution, solverResult));
        }
    }
}

