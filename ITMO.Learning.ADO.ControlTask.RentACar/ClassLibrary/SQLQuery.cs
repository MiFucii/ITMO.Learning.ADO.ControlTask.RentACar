using ITMO.Learning.ADO.ControlTask.RentACar.RetroCarModel;
using System;
using System.Data.Entity.Core;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ITMO.Learning.ADO.ControlTask.RentACar.ClassLibrary
{
    static class SQLQuery
    {
        public static int SearchClient(MaskedTextBox tb)
        {
            int validFlag = -1;
            try
            {
                using (RetroCarContext rcr = new RetroCarContext())
                {
                    validFlag = (from clnt in rcr.t_Client
                                 where clnt.Phone == tb.Text.Replace(" ", "")
                                 select clnt).Count();
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            return validFlag;
        }

        public static void FillClientInTable(DataGridView dgw, int idClient)
        {
            try
            {
                dgw.Rows.Clear();
                using (RetroCarContext rcr = new RetroCarContext())
                {
                    var Client = rcr.t_Client.First(cl => cl.IDClient == idClient);
                    StringBuilder passport = new StringBuilder(Client.PassportSeries).Append(" ").Append(Client.PassportNumber);
                    dgw.Rows.Add(Client.Name, Client.Surname, Client.Patronymic, Client.Age, Client.Gender, Client.Phone, passport);
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        public static bool ChangeStatusCar(string carNumber, bool switchFlag)
        {
            bool flag = true;
            try
            {
                if (switchFlag)
                {
                    using (RetroCarContext rtc = new RetroCarContext())
                    {
                        var Auto = rtc.t_Car.First(ct => ct.CarNumber == carNumber);

                        Auto.Status = "Доступен";
                        Auto.Cause = null;

                        rtc.SaveChanges();
                    }
                }
                else
                {
                    ChangeStatusForm csf = new ChangeStatusForm();
                    csf.ShowDialog();
                    if (csf.DialogResult == DialogResult.OK)
                    {
                        using (RetroCarContext rtc = new RetroCarContext())
                        {
                            var Auto = rtc.t_Car.First(ct => ct.CarNumber == carNumber);

                            Auto.Status = "Недоступен";
                            Auto.Cause = csf.tbCause.Text;

                            rtc.SaveChanges();
                        }
                    }
                    else flag = false;
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            return flag;
        }

        public static bool ConfirmContract(int idContract)
        {
            bool flag = true;
            try
            {
                using (RetroCarContext rtc = new RetroCarContext())
                {
                    var Contract = rtc.t_Сontract.First(ct => ct.IDContract == idContract);
                    Contract.Status = "Закрыт";
                    rtc.SaveChanges();
                }
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            return flag;
        }

        public static bool DeleteContract(int idContract)
        {
            bool flag = true;
            try
            {               
                using (RetroCarContext rtc = new RetroCarContext())
                {
                    var Contract = rtc.t_Сontract.First(ct => ct.IDContract == idContract);
                    rtc.t_Сontract.Remove(Contract);
                    rtc.SaveChanges();
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); flag = false; }
            return flag;
        }
    }
}
