using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{
    public partial class WorkForm : Form
    {
        public WorkForm()
        {
            InitializeComponent();
        }

        //Определение местоположения окна авторизации
        private void WorkForm_Resize(object sender, EventArgs e)
        {
            AuthorizationBox.Location = new Point(((this.ClientSize.Width - AuthorizationBox.Width) - 110),
               ((this.ClientSize.Height - AuthorizationBox.Height) / 2));
        }
        //Проверяем данные о пользователе и если они валидны начинаем работу приложения
        private void btnEntry_Click(object sender, EventArgs e)
        {
            try
            {
                lbError.Visible = false;
                StringBuilder fullName = new StringBuilder("", 101);
                int idMan = 0;

                using (RetroCarContext rcr = new RetroCarContext())
                {
                    var query = (from lp in rcr.t_Manager
                                 where lp.Login == tbLogin.Text && lp.Password == tbPassword.Text
                                 select new { lp.IDManager, lp.Name, lp.Surname }).ToList();
                    if (query.Count == 1)
                    {
                        fullName.Append("Менеджер: ").Append(query[0].Name).Append(" ").Append(query[0].Surname);
                        idMan = query[0].IDManager;
                    }
                }
                if (fullName.Length > 0)
                {
                    if (cbSave.Checked == true)
                    {
                        SaveManager sv = new SaveManager();
                        sv.SaveTextBoxData(tbLogin.Text, tbPassword.Text);
                        cbSave.Visible = false;
                    }
                    this.Text += fullName.ToString();
                    lbManagerID.Text = string.Format("{0}|:", idMan);
                    lbStatus.Text = "Готов";
                    StartWork();
                    ContractRepository.LoadArrayContract(idMan);
                    foreach (var i in ContractRepository.arrayContract)
                    {
                        dgContract.Rows.Add(i.IDContract, i.NomerAuto, i.DateStart, i.DateEnd, i.Summa, i.DateOfConclusion, i.IDClient);
                    }
                }
                else
                {
                    tbLogin.Text = ""; tbPassword.Text = ""; tbLogin.Focus();
                    lbError.Visible = true;
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Заканчиваем работу с приложением и переходим на экран авторизации
        private void btnMenuExit_Click(object sender, EventArgs e) { EndWork(); }
        //Осуществляем поиск клиента в БД и если он найден загружаем полученую информацию
        private void btnClientSearch_Click(object sender, EventArgs e)
        {
            try
            {
                if (tbPhone.Text.Length == 16)
                {
                    string phoneNumer = tbPhone.Text.Replace(" ", "");
                    List<t_Client> Client = new List<t_Client>();

                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        Client = (from clnt in rcr.t_Client
                                  where clnt.Phone == phoneNumer
                                  select clnt).ToList();
                    }

                    if (Client.Count == 1)
                    {
                        int idClient = Client[0].IDClient;

                        var infClientContract = ContractRepository.arrayContract.Where(ct => ct.IDClient == idClient);
                        foreach (var i in infClientContract)
                        {
                            dgInfoClientContract.Rows.Add(i.IDContract, i.NomerAuto, i.DateStart, i.DateEnd, i.Summa, i.DateOfConclusion);
                        }

                        tbClientName.Text = Client[0].Name;
                        tbClientSurname.Text = Client[0].SurName;
                        nAge.Value = Client[0].Age;
                        if (Client[0].Gender == "М") cbGender.SelectedIndex = 0;
                        else cbGender.SelectedIndex = 1;
                        tbPassportNumber.Text = Client[0].PassportNumber.ToString();
                        tbPassportSeries.Text = Client[0].PassportSeries.ToString();
                    }
                    else MessageBox.Show("Клиент с данным номером телефона не найден!", "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    tbPhone.BackColor = Color.Red;
                    tbPhone.Clear();
                    tbPhone.Focus();
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Очищаем форму оформления нового договора
        private void btnClearForm_Click(object sender, EventArgs e) { NewContractPanelClear(); }
        //Добовляем нового клиента в БД
        private void btnClientAdd_Click(object sender, EventArgs e)
        {
            try
            {
                //Проверяем все поля на заполненность
                if (Validation.ValidationNewClient(NewContractForm))
                {
                    int validFlag = 0;//Флаг для проверки существования данного клиента в базе
                    using (RetroCarContext rcr = new RetroCarContext())
                    {
                        validFlag = (from clnt in rcr.t_Client
                                     where clnt.Phone == tbPhone.Text.Replace(" ", "")
                                     select clnt).Count();
                    }
                    if (validFlag > 0) MessageBox.Show("Клиент с таким номером телефона уже есть в базе!", "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        string genderChar = "M";
                        if (cbGender.Text == "Женский") genderChar = "Ж";
                        using (RetroCarContext rcr = new RetroCarContext())
                        {
                            t_Client client = new t_Client
                            {
                                Name = tbClientName.Text,
                                SurName = tbClientSurname.Text,
                                Age = (int)nAge.Value,
                                Gender = genderChar,
                                Phone = tbPhone.Text.Replace(" ", ""),
                                PassportNumber = tbPassportNumber.Text.Replace(" ", ""),
                                PassportSeries = tbPassportSeries.Text.Replace(" ", "")
                            };
                            rcr.t_Client.Add(client);
                            rcr.SaveChanges();
                        }
                        PrintNotification("Клиент успешно добавлен");
                    }
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Снимаем красное выделение с элемента где возникла ошибка валидации данных.
        #region True Validation
        private void tbClientName_Enter(object sender, EventArgs e) { Validation.SetColorTextBox(tbClientName, true); }
        private void tbClientSurname_Enter(object sender, EventArgs e) { Validation.SetColorTextBox(tbClientSurname, true); }
        private void cbGender_Enter(object sender, EventArgs e) { Validation.SetColorComboBox(cbGender, true); }
        private void tbPhone_Enter(object sender, EventArgs e) { Validation.SetColorTextBox(tbPhone, true); }
        private void tbPassportSeries_Enter(object sender, EventArgs e) { Validation.SetColorTextBox(tbPassportSeries, true); }
        private void tbPassportNumber_Enter(object sender, EventArgs e) { Validation.SetColorTextBox(tbPassportNumber, true); }
        #endregion

        //Показываем главное окно приложения
        private void StartWork()
        {
            AuthorizationBox.Visible = false;
            pbScreensaver.Visible = false;
            MainPanel.Visible = true;
            MenuPanel.Visible = true;
            StatusPanel.Visible = true;
            SecondMenuMainPanel.Visible = true;
        }
        //Показываем окно авторизации
        private void EndWork()
        {
            foreach (var panel in this.Controls.OfType<Panel>()) { panel.Visible = false; }
            SecondMenuMainPanel.Visible = false;
            SecondMenuNewContractPanel.Visible = false;
            NewContractPanelClear();
            dgContract.Rows.Clear();
            dgClient.Rows.Clear();
            StatusPanel.Visible = false;
            
            this.Text = "RetroCar ";            
            tbPassword.Text = "";

            AuthorizationBox.Visible = true;
            pbScreensaver.Visible = true;

        }
        //Очищаем все поля в окне добавления нового договора
        private void NewContractPanelClear()
        {
            dgInfoClientContract.Rows.Clear();
            nAge.Value = 18;
            foreach (var tbb in NewContractForm.Controls.OfType<TextBoxBase>())
            {
                tbb.Text = "";
                Validation.SetColorTextBox(tbb, true);
            }
            foreach (var cb in NewContractForm.Controls.OfType<ComboBox>())
            {
                cb.Text = "";
                Validation.SetColorComboBox(cb, true);
            }
            dtStart.Value = DateTime.Now;
            dtEnd.Value = DateTime.Now;
            AutoRepository.changeFlag = false;

            cbAuto.Items.Clear();
            lbPrice.Text = "0,00 руб.";

            tbClientName.Focus();            
        }
        //Выводим уведомление в статус бар
        private void PrintNotification(string message)
        {
            lbStatus.Text = message;
            TimerNotification.Start();
        }
        //Загрузка доступных для аренды автомобилей в comboBox
        private void cbAuto_Enter(object sender, EventArgs e)
        {
            Validation.SetColorComboBox(cbAuto, true);
            if (AutoRepository.changeFlag)
            {
                cbAuto.Items.Clear();
                AutoRepository.LoadArrayAuto(dtStart.Value, dtEnd.Value);

                foreach (string i in AutoRepository.arrayAuto)
                {
                    string autoName = i.Remove(i.IndexOf('/'));
                    cbAuto.Items.Add(autoName.Substring(autoName.IndexOf('|') + 1));
                }
            }
        }
        //Обновление стоимости аренды в Label
        private void cbAuto_SelectedValueChanged(object sender, EventArgs e) { lbPrice.Text = AutoRepository.Price(cbAuto.SelectedIndex); }
        //Устанавливаем флаг о изменении дат
        private void dtStart_ValueChanged(object sender, EventArgs e) { AutoRepository.changeFlag = true; }
        private void dtEnd_ValueChanged(object sender, EventArgs e) { AutoRepository.changeFlag = true; }
        //Обновляем выбранного клиента в базе данных
        private void btnClientUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (Validation.ValidationNewClient(NewContractForm))
                {
                    using (RetroCarContext rtc = new RetroCarContext())
                    {
                        var Client = rtc.t_Client.First(cl => cl.Phone == tbPhone.Text.Replace(" ", ""));
                        Client.Name = tbClientName.Text;
                        Client.SurName = tbClientSurname.Text;
                        Client.Age = (int)nAge.Value;
                        if (cbGender.Text == "Женский") Client.Gender = "Ж";
                        else Client.Gender = "М";
                        Client.PassportNumber = tbPassportNumber.Text.Replace(" ", "");
                        Client.PassportSeries = tbPassportSeries.Text.Replace(" ", "");

                        rtc.SaveChanges();
                    }
                    PrintNotification("Данные о клиенте успешно обновлены");
                }
            }
            catch (InvalidOperationException) { MessageBox.Show("Для обновления номера телефона обратитесь к администратору!", "Предупреждение:", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Добовляем новый договор в БД
        private void btnArrange_Click(object sender, EventArgs e)
        {
            //try
            //{
                if (Validation.ValidationNewContract(NewContractForm))
                {
                    if (dtEnd.Value <= dtStart.Value) MessageBox.Show("Введенные даты не корректны", "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        using (RetroCarContext rcr = new RetroCarContext())
                        {
                            var query = (from clnt in rcr.t_Client
                                         where clnt.Phone == tbPhone.Text.Replace(" ", "")
                                         select new { clnt.IDClient }).ToList();
                            if (query.Count > 0)
                            {
                                t_Сontract contract = new t_Сontract
                                {
                                    NomerAuto = AutoRepository.NomerAuto(cbAuto.SelectedIndex),
                                    IDManager = int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.LastIndexOf('|'))),
                                    IDClient = query[0].IDClient,
                                    DateStart = dtStart.Value,
                                    DateEnd = dtEnd.Value,
                                    DateOfConclusion = DateTime.Now
                                };
                                rcr.t_Сontract.Add(contract);
                                rcr.SaveChanges();
                                PrintNotification("Договор успешно оформлен");
                            }
                            else MessageBox.Show("Клиент не найден в базе данных.", "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        
                    }
                }
            //}
            //catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            //catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Переходим на панель нового договора
        private void btnMenuNewContract_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = false;
            NewContractPanel.Visible = true;
            SecondMenuNewContractPanel.Visible = true;
            SecondMenuMainPanel.Visible = false;
        }
        //Переходим на главную панель
        private void btnMenuHome_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = true;
            NewContractPanel.Visible = false;
            SecondMenuNewContractPanel.Visible = false;
            SecondMenuMainPanel.Visible = true;
        }

        private void dgContract_SelectionChanged(object sender, EventArgs e)
        {
            int idClient = int.Parse(dgContract.CurrentRow.Cells[6].Value.ToString());
            dgClient.Rows.Clear();
            using (RetroCarContext rcr = new RetroCarContext())
            {
                var Client = rcr.t_Client.First(cl => cl.IDClient == idClient);
                StringBuilder passport = new StringBuilder(Client.PassportSeries).Append(" ").Append(Client.PassportNumber);
                dgClient.Rows.Add(Client.Name, Client.SurName, Client.Age, Client.Gender, Client.Phone, passport);
            }
        }
        //Обновляем отображаемые в dataGrid договоры
        private void btnDataUpdate_Click(object sender, EventArgs e)
        {
            dgContract.Rows.Clear();
            ContractRepository.LoadArrayContract(int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));
            foreach (var i in ContractRepository.arrayContract)
            {
                dgContract.Rows.Add(i.IDContract, i.NomerAuto, i.DateStart, i.DateEnd, i.Summa, i.DateOfConclusion, i.IDClient);
            }
            PrintNotification("Данные обновлены");
        }
        //Таймер Уведомлений
        private void TimerNotification_Tick(object sender, EventArgs e)
        {
            lbStatus.Text = "Готов";
            TimerNotification.Stop();
        }
        //Обновляем выбранный договор в БД
        private void btnContractUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                int idContract = int.Parse(dgContract.CurrentRow.Cells[0].Value.ToString());
                using (RetroCarContext rtc = new RetroCarContext())
                {
                    var Contract = rtc.t_Сontract.First(ct => ct.IDContract == idContract);

                    
                    Contract.NomerAuto = dgContract.CurrentRow.Cells[1].Value.ToString();
                    Contract.DateStart = Convert.ToDateTime(dgContract.CurrentRow.Cells[2].Value);
                    Contract.DateEnd = Convert.ToDateTime(dgContract.CurrentRow.Cells[3].Value);
                    Contract.Summa = null;
                    Contract.DateOfConclusion = Convert.ToDateTime(dgContract.CurrentRow.Cells[5].Value);

                    rtc.SaveChanges();
                }
                PrintNotification("Договор успешно обновлен");
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Удаляем выбранный договор в БД
        private void btnDeleteContract_Click(object sender, EventArgs e)
        {
            try
            {
                int idContract = int.Parse(dgContract.CurrentRow.Cells[0].Value.ToString());

                using (RetroCarContext rtc = new RetroCarContext())
                {
                    var Contract = rtc.t_Сontract.First(ct => ct.IDContract == idContract);
                    rtc.t_Сontract.Remove(Contract);
                    rtc.SaveChanges();
                }
                PrintNotification("Договор успешно удален");
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Осуществляем поиск договора в dataGrid по его номеру
        private void btnSearchContract_Click(object sender, EventArgs e)
        {
            try
            {
                SearchContractForm scf = new SearchContractForm();
                scf.ShowDialog();
                if (scf.DialogResult == DialogResult.OK)
                {
                    int rowIndex = -1;

                    DataGridViewRow row = dgContract.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => r.Cells[0].Value.ToString().Equals(scf.textBox1.Text))
                        .First();
                    rowIndex = row.Index;
                    dgContract.Rows[rowIndex].Selected = true;
                }
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //При загрузке формы загружаем данные о пользователь из файла
        private void WorkForm_Load(object sender, EventArgs e)
        {
            SaveManager sv = new SaveManager();
            if (sv.GetFileData())
            {
                cbSave.Visible = false;
                string data = sv.LoadTextBoxData();
                tbLogin.Text = data.Substring(0, data.IndexOf('|'));
                tbPassword.Text = data.Substring(data.IndexOf('|') + 1);
            }
        }
        //Удаляем файл данных о пользователе
        private void btnMenuDeleteDataUser_Click(object sender, EventArgs e)
        {
            SaveManager sv = new SaveManager();
            if(sv.GetFileData()) File.Delete(Path.Combine(Application.StartupPath,"Data.json"));
            PrintNotification("Пользователь удален");
        }
    }
}
