using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using ApUtilitiesLib;
using Dot = ApUtilitiesLib.ApUtilities.Dot;
using System;

namespace UnitTestLib
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestPrivateMethod()
        {
            // создаем объект PrivateType, чтобы протестировать инкапсулированные методы
            PrivateType pr = new PrivateType(typeof(ApUtilities));
            
            // тестируем треугольник. должны вернуться три его вершины!
            List<Dot> dots = new List<Dot>() { new Dot(1, 2), new Dot(2, 3), new Dot(1, 4) };
            var result = pr.InvokeStatic("FindVertices", dots);
            CollectionAssert.AreEquivalent(dots, (List<Dot>)result);

            // тестируем выпуклый четырехугольник. должны вернуться четыре вершины!
            List<Dot> dots2 = new List<Dot>() { new Dot(0, 0), new Dot(2, 0), new Dot(0, 2), new Dot(2,2) };
            var result2 = pr.InvokeStatic("FindVertices", dots2);
            CollectionAssert.AreEquivalent(dots2, (List<Dot>)result2);

            // тестируем вогнутый четырехугольник. должны вернуться три вершины!
            List<Dot> dots3 = new List<Dot>() { new Dot(0, 0), new Dot(2, 0), new Dot(0, 8), new Dot(1, 1) };
            var result3 = (List<Dot>) pr.InvokeStatic("FindVertices", dots3);
            List<Dot> actual = new List<Dot>() { new Dot(0, 0), new Dot(0, 8), new Dot(2, 0) };
            for (int i = 0; i < actual.Count; i++)
            {
                Assert.AreEqual(actual[i].X, result3[i].X);
                Assert.AreEqual(actual[i].Y, result3[i].Y);
            }

            // пробуем ввести количество точек, меньше, чем три, тем самым вызвав исключение
            try
            {
                List<Dot> dots4 = new List<Dot>() { new Dot(5, 5), new Dot(8, 3)};
                pr.InvokeStatic("FindVertices", dots4);
            } catch (Exception e)
            {
                Assert.AreEqual("Для расчета площади необходимо, минимум, 3 точки", e.InnerException.Message);
            }
        }
    }
}
