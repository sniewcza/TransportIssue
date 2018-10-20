using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransportIssue.Utilities;
using System.Linq;
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
            Assert.IsTrue(CheckEquals(solutions.Item2,validBeta));

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
          return  v1.SequenceEqual(v2);
        }

    }
}
