using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ITMO.Learning.ADO.ControlTask.RentACar.RetroCarModel;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{
    static class AutoRepository
    {

        public static List<string> arrayAuto = new List<string>();
        public static bool changeFlag = false;
        //В зависимости от флага манипулируем массивом автомобилей загружаемых из БД
        public static void LoadArrayAuto(ComboBox cb, DateTime Start, DateTime End)
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
                            fullNameAuto.Append(auto.Brand).Append(" ").Append(auto.Model);
                            cb.Items.Add(fullNameAuto);
                            fullNameAuto.Append("|").Append(auto.CarNumber).Append("/").Append(auto.Price.ToString().Remove(auto.Price.ToString().IndexOf(',') + 3)).Append(" руб.");
                            arrayAuto.Add(fullNameAuto.ToString());
                        }
                    }
                    changeFlag = false;
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Возвращаем цену выбранного автомобиля
        public static string Price(int index)
        {
            return arrayAuto[index].Remove(0, arrayAuto[index].IndexOf("/") + 1);
        }
        //Возвращаем гос. номер выбранного автомобиля
        public static string CarNumber(int index)
        {
            string carNumber = arrayAuto[index].Substring(arrayAuto[index].IndexOf('|') + 1);
            return carNumber.Remove(carNumber.IndexOf('/'));
        }
        //Заполняем таблицу данными об автомобилях
        public static void FillAuto(DataGridView dgv)
        {
            try
            {
                dgv.Rows.Clear();
                List<v_InfoCar> arrayAutoInfo = new List<v_InfoCar>();

                using (RetroCarContext rcr = new RetroCarContext())
                {
                    arrayAutoInfo = (from auto in rcr.v_InfoCar
                                     select auto).ToList();

                }
                foreach (var i in arrayAutoInfo)
                {
                    dgv.Rows.Add(i.Brand, i.Model, i.CarNumber, i.Price, i.Status, i.Cause);
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
