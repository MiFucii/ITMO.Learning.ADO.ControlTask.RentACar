using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{
    static class AutoRepository
    {

        public static List<string> arrayAuto = new List<string>();
        public static bool changeFlag = false;
        //В зависимости от флага манипулируем массивом автомобилей загружаемых из БД
        public static void LoadArrayAuto(DateTime Start, DateTime End)
        {
            try
            {
                if (changeFlag)
                {
                    arrayAuto.Clear();

                    StringBuilder fullNameAuto = new StringBuilder("", 130);
                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        var dbauto = rcr.usp_FreeCarOfDate(Start, End).ToList();
                        foreach (var auto in dbauto)
                        {
                            fullNameAuto.Clear();
                            fullNameAuto.Append(auto.NomerAuto).Append("|").Append(auto.Marka).Append(" ").Append(auto.Model).Append("/").Append(auto.Price.ToString().Remove(auto.Price.ToString().IndexOf(',') + 3)).Append(" руб.");
                            arrayAuto.Add(fullNameAuto.ToString());
                        }
                    }
                    changeFlag = false;
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Возвращаем цену выбранного автомобиля
        public static string Price(int index)
        {
            return arrayAuto[index].Substring(arrayAuto[index].IndexOf("/") + 1);
        }
        //Возвращаем гос. номер выбранного автомобиля
        public static string NomerAuto(int index)
        {
            return arrayAuto[index].Remove(arrayAuto[index].IndexOf('|'));
        }
    }
}
