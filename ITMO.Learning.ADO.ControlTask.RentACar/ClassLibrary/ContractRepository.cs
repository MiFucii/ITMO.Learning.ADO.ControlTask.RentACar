using ITMO.Learning.ADO.ControlTask.RentACar.RetroCarModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{
    static class ContractRepository
    {
        private static List<t_Сontract> arrayContract = new List<t_Сontract>();
        private static List<t_ArchiveСontract> arrayArchiveContract = new List<t_ArchiveСontract>();
        //Отображаем загруженные данные в таблицах
        private static void FillDGW(DataGridView dgw, byte flagForSwitch, bool clear)
        {
            try
            {
                if (clear) dgw.Rows.Clear();
                DateTime dt = new DateTime();
                byte count = 0;
                switch (flagForSwitch)
                {
                    case 1:
                        foreach (var i in arrayArchiveContract)
                        {
                            dgw.Rows.Add(i.IDContract, i.CarNumber, i.DateStart.ToString("dd.MM.yyyy HH:00"), i.DateEnd.ToString("dd.MM.yyyy HH:00"), i.Summa, i.DateOfConclusion.ToShortDateString(), i.Cause, i.IDClient);
                        }
                        break;
                    case 2:
                        foreach (var i in arrayContract)
                        {
                            if (i.Status == "Действует")
                            {
                                dgw.Rows.Add(i.IDContract, i.CarNumber, i.DateStart.ToString("dd.MM.yyyy HH:00"), i.DateEnd.ToString("dd.MM.yyyy HH:00"), i.Summa, i.DateOfConclusion.ToShortDateString(), i.Cause, i.IDClient);

                                dt = (DateTime)i.DateEnd;
                                if (dt.Hour < DateTime.Now.Hour && dt.Date == DateTime.Now.Date) dgw.Rows[count].DefaultCellStyle.BackColor = Color.Yellow;
                                else if (dt.Date == DateTime.Now.Date) dgw.Rows[count].DefaultCellStyle.BackColor = Color.LightGreen;
                                else if (dt.Date < DateTime.Now.Date) dgw.Rows[count].DefaultCellStyle.BackColor = Color.MediumVioletRed;

                                count++;
                            }
                        }
                        break;
                    case 3:
                        foreach (var i in arrayContract)
                        {
                            dgw.Rows.Add(i.IDContract, i.CarNumber, i.DateStart.ToString("dd.MM.yyyy HH:00"), i.DateEnd.ToString("dd.MM.yyyy HH:00"), i.Summa, i.DateOfConclusion.ToShortDateString(), i.Cause);

                            if (!String.IsNullOrEmpty(i.Cause))
                            {
                                dgw.Rows[count].DefaultCellStyle.BackColor = Color.MediumVioletRed;
                            }

                            count++;
                        }
                        break;
                    case 4:
                        foreach (var i in arrayArchiveContract)
                        {
                            dgw.Rows.Add(i.IDContract, i.CarNumber, i.DateStart.ToString("dd.MM.yyyy HH:00"), i.DateEnd.ToString("dd.MM.yyyy HH:00"), i.Summa, i.DateOfConclusion.ToShortDateString(), i.Cause);

                            if (!String.IsNullOrEmpty(i.Cause))
                            {
                                dgw.Rows[count].DefaultCellStyle.BackColor = Color.MediumVioletRed;
                            }
                        }
                        break;
                }
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Загружаем массив договоров из БД в зависимости от параметров
        public static void LoadArrayContract(DataGridView dgw, int idManager = -1, string nomerAuto = null, int idClient = -1)
        {
            try
            {
                byte flagToSwitch = 2;
                arrayContract.Clear();
                if (String.IsNullOrEmpty(nomerAuto) && idClient == -1)
                {
                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        arrayContract = (from ct in rcr.t_Сontract
                                         where ct.IDManager == idManager
                                         select ct).ToList();
                    }
                }
                else if (idClient == -1)
                {
                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        arrayContract = (from ct in rcr.t_Сontract
                                         where ct.IDManager == idManager && ct.CarNumber == nomerAuto
                                         select ct).ToList();
                    }
                }
                else if (String.IsNullOrEmpty(nomerAuto) && idManager == -1)
                {
                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        arrayContract = (from ct in rcr.t_Сontract
                                         where ct.IDClient == idClient
                                         select ct).ToList();
                    }
                    flagToSwitch = 3;
                }
                FillDGW(dgw, flagToSwitch, true);
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Загружаем массив архивых договоров из БД
        public static void LoadArrayArhiveContract(DataGridView dgw, int idManager = -1, int idClient = -1)
        {
            try
            {
                arrayArchiveContract.Clear();
                if (idManager >= 0)
                {
                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        arrayArchiveContract = (from ct in rcr.t_ArchiveСontract
                                                where ct.IDManager == idManager
                                                select ct).ToList();
                    }
                    FillDGW(dgw, 1, true);
                }
                else if (idClient >= 0)
                {
                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        arrayArchiveContract = (from ct in rcr.t_ArchiveСontract
                                                where ct.IDClient == idClient
                                                select ct).ToList();
                    }
                    FillDGW(dgw, 4, false);
                }
                
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
