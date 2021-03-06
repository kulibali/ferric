﻿using System;
using System.Linq;
using Ferric.Math.Common;
using Ferric.Math.MachineLearning.Classifiers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Ferric.Math.MachineLearning.Test.Classifiers
{
    [TestClass]
    public class LeastSquaresTests
    {
        static class COutput
        {
            public const int Orange = 0;
            public const int Blue = 1;
        }

        [TestMethod]
        public void Math_MachineLearning_Classifiers_LeastSquares()
        {
            const int num = 1000;

            var r = new Random(314);
            var outputs = Enumerable.Range(0, num).Select(i => new[] { r.NextDouble() < 0.5 ? COutput.Orange : COutput.Blue }).ToArray();
            var inputs = outputs.Select(output =>
                {
                    double x = r.NextDouble();
                    double y = r.NextDouble();

                    if (output[0] == COutput.Blue)
                    {
                        x *= 10.0;
                        y *= 10.0;
                    }

                    return new[] { x, y };
                }).ToArray();

            var trainingInputs = inputs.Take(num * 2 / 3).ToArray();
            var trainingOutputs = outputs.Take(num * 2 / 3).ToArray();

            var testingInputs = inputs.Skip(num * 2 / 3).ToArray();
            var testingOutputs = outputs.Skip(num * 2 / 3).ToArray();

            var cl = new LeastSquares<double, int>(2, 2, o => (double)(int)o, i => i < 0.5 ? COutput.Orange : COutput.Blue);
            cl.TrainModel(trainingInputs, trainingOutputs);

            var pct = cl.TestModel(testingInputs, testingOutputs);
            Assert.IsTrue(pct > 0.93);
        }
    }
}
