using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Linq;
using System.Windows.Forms;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{
    static class ContractRepository
    {
        public static List<t_Сontract> arrayContract = new List<t_Сontract>();
        //Загружаем массив договоров из БД
        public static void LoadArrayContract(int idManager)
        {
            try
            {
                arrayContract.Clear();
                using (RetroCarContext rcr = new RetroCarContext())
                {
                    arrayContract = (from ct in rcr.t_Сontract
                                     where ct.IDManager == idManager
                                     select ct).ToList();
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
