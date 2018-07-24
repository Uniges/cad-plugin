using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;
using ApUtilitiesLib;

namespace ApplicationLib
{
    // при наслледовании интерфейса должны быть определены два метода инициализации и детерминации
    public class Application : IExtensionApplication
    {
        // инициализируемся, выводим сообщение
        public void Initialize()
        {
            MessageBox.Show("Плагин успешно загружен!" +
                "\nДля запуска введите в консоль: ПОИСКТОЧЕК" +
                "\nПрограмма выводит количество найденых точек/вершин и площадь");
        }

        public void Terminate() { }

        // команда, которую надо написать в консоль автокада
        [CommandMethod("ПоискТочек")]
        public void Main()
        {
            // вызываем утильный метод
            ApUtilities.Main();
        }
    }
}
