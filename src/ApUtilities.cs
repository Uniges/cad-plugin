using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
// убираем неоднозначность, т.к. Application содержится еще в System.Windows.Forms (иначе нужно использовать полное имя)
using acadApp = Autodesk.AutoCAD.ApplicationServices.Application;

namespace ApUtilitiesLib
{
    internal class ApUtilities
    {
        // метод класса, доступный для вызова внутри сборки. все другие приватные методы инкапсулированы
        internal static void Main()
        {
            // получаем все точки с экрана
            List<Dot> dots = ApUtilities.FindDots();

            // получаем все вершины, анализируя точки
            List<Dot> vertices = ApUtilities.FindVertices(dots);

            // отрисовываем контур, используя вершины, и получаем его площадь
            double area = ApUtilities.DrawPLineAndCountArea(vertices);

            // выводим результаты
            ApUtilities.PrintResult(dots.Count, vertices.Count, area);
        }

        // инкапсулированный метод для работы в пределах класса
        private static Database TakeDB()
        {
            return HostApplicationServices.WorkingDatabase;
        }

        // парсим рабочее пространство, возвращаем массив точек с координатами
        private static List<Dot> FindDots()
        {
            // получаем текущую БД
            Database db = ApUtilities.TakeDB();

            // создаем массив точек
            List<Dot> dots = new List<Dot>();

            // начинаем транзакцию
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // получаем ссылку на пространство модели (ModelSpace)
                BlockTableRecord ms = (BlockTableRecord)tr.GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), OpenMode.ForRead);

                // "пробегаем" по всем объектам в пространстве модели
                foreach (ObjectId id in ms)
                {
                    // приводим каждый из них к типу Entity
                    Entity entity = (Entity)tr.GetObject(id, OpenMode.ForRead);

                    // если это точка - создаем объект Dot
                    if (entity.GetType() == typeof(DBPoint))
                    {
                        dots.Add(new Dot(((DBPoint)entity).Position.X, ((DBPoint)entity).Position.Y));
                    }
                }
            }
            return dots;
        }

        // 2 смежных метода, которые позволяют вычислить вершины. 
        // второй метод инкапсулирован, т.к. доступ к нему нужен только внутри класса
        private static List<Dot> FindVertices(List<Dot> dots)
        {
            // проверяем полученные данные, кидаем ошибку, если точек меньше 3х
            if (dots.Count < 3)
            {
                throw new ArgumentException("Для расчета площади необходимо, минимум, 3 точки");
            }

            // результирующий массив
            List<Dot> vertices = new List<Dot>();

            // находим крайнюю левую точку
            Dot vPointOnHull = dots.Where(p => p.X == dots.Min(min => min.X)).First();

            // рассчитываем остальные вершины
            Dot vEndpoint;
            do
            {
                vertices.Add(vPointOnHull);
                vEndpoint = dots[0];
                for (int i = 1; i < dots.Count; i++)
                {
                    if ((vPointOnHull == vEndpoint)
                        || (Orientation(vPointOnHull, vEndpoint, dots[i]) == -1))
                    {
                        vEndpoint = dots[i];
                    }
                }
                vPointOnHull = vEndpoint;
            }
            while (vEndpoint != vertices[0]);
            return vertices;
        }

        private static int Orientation(Dot d1, Dot d2, Dot d)
        {
            double orin = (d2.X - d1.X) * (d.Y - d1.Y) - (d.X - d1.X) * (d2.Y - d1.Y);
            if (orin > 0)
                return -1;
            if (orin < 0)
                return 1;
            return 0;
        }

        // метод рисует полилинию по вершинам и возвращает площадь
        // возврат площади нужен для того, чтобы не парсить пространство заново (экономия ресурсов)
        private static double DrawPLineAndCountArea(List<Dot> vertices)
        {
            Database db = ApUtilities.TakeDB();

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                // открытие таблицы Блоков для чтения
                BlockTable blkTbl;
                blkTbl = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;

                // открытие записи таблицы Блоков пространства Модели для записи
                BlockTableRecord blkTblRec;
                blkTblRec = tr.GetObject(blkTbl[BlockTableRecord.ModelSpace], OpenMode.ForWrite) as BlockTableRecord;

                // создание полилинии по найденным вершинам
                Polyline acPoly = new Polyline();
                acPoly.SetDatabaseDefaults();

                for (int i = 0; i < vertices.Count; i++)
                {
                    // строим полилинию по вершинам
                    acPoly.AddVertexAt(i, new Point2d(vertices[i].X, vertices[i].Y), 0, 0, 0);
                    // замыкаем полилинию
                    if (i == vertices.Count - 1)
                    {
                        acPoly.AddVertexAt(i + 1, new Point2d(vertices[0].X, vertices[0].Y), 0, 0, 0);
                    }
                }

                // добавление нового объекта в запись таблицы блоков и в транзакцию
                blkTblRec.AppendEntity(acPoly);
                tr.AddNewlyCreatedDBObject(acPoly, true);

                // сохранение нового объекта в базе данных
                tr.Commit();

                // возврат площади фигуры
                return Math.Round(acPoly.Area, 4);
            }
        }

        // выводим данные в консоль
        private static void PrintResult(int dot, int vertex, double area)
        {
            // количество найденных точек
            acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Найдено точек - {0}\n", dot);

            // количество найденных вершин
            acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Найдено вершин - {0}\n", vertex);

            // площадь
            acadApp.DocumentManager.MdiActiveDocument.Editor.WriteMessage("Общая площадь области - {0}\n", area);
        }

        // класс точки
        internal class Dot
        {
            internal Dot(double x, double y)
            {
                X = x;
                Y = y;
            }
            internal double X { get; }
            internal double Y { get; }
        }
    }
}
