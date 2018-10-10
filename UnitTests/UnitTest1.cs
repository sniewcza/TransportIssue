using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TransportIssue.Utilities;
namespace UnitTests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //Arrange
            TransportIssueSolver solver = new TransportIssueSolver();
            double[,] costs = new double[,]
            {
                {3,5,7 },
                {12,10,9},
                {13,3,9}
            };
            double[] delivers = new double[] { 50, 70, 30 };
            double[] recipents = new double[] { 20, 40, 90 };
            double[,] result = new double[,]
            {
                {20,10,20 },
                {0,0,70},
                {0,30,0}
            };

            //Act
            var solverResult = solver.FindaBaseSolution(costs, delivers, recipents);

            //Assert

            Assert.IsTrue(CheckEquals(result, solverResult));
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

    }
}
