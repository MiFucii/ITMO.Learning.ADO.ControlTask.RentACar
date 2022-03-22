using ITMO.Learning.ADO.ControlTask.RentACar.ClassLibrary;
using ITMO.Learning.ADO.ControlTask.RentACar.RetroCarModel;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{
    public partial class WorkForm : Form
    {
        public WorkForm() { InitializeComponent(); }
        //Определение местоположения окна авторизации
        private void WorkForm_Resize(object sender, EventArgs e)
        {
            AuthorizationBox.Location = new Point(((this.ClientSize.Width - AuthorizationBox.Width) - 110),
               ((this.ClientSize.Height - AuthorizationBox.Height) / 2));
        }
        //Проверяем данные о пользователе(менеджере) и если они валидны начинаем работу приложения
        private void btnEntry_Click(object sender, EventArgs e)
        {
            try
            {
                lbError.Visible = false;
                StringBuilder fullName = new StringBuilder("", 101);
                int idMan = 0;
                //Проверяем логин и пароль пользователя и если он верный получаем инфомацию о пользователе
                using (RetroCarContext rcr = new RetroCarContext())
                {
                    var query = (from lp in rcr.t_Manager
                                 where lp.Login == tbLogin.Text && lp.Password == tbPassword.Text
                                 select new { lp.IDManager, lp.Name, lp.Surname, lp.Patronymic }).ToList();                    
                    if (query.Count == 1)
                    {
                        fullName.Append("Менеджер: ").Append(query[0].Surname).Append(" ").Append(query[0].Name).Append(" ").Append(query[0].Patronymic);
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

                    //Отображаем интерфейс и загружаем данные в таблицы
                    StartWork();
                    ContractRepository.LoadArrayContract(dgContract, idMan);
                    AutoRepository.FillAuto(dgAuto);
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
                        dgInfoClientContract.Rows.Clear();
                        ContractRepository.LoadArrayContract(dgInfoClientContract, idClient:idClient);
                        ContractRepository.LoadArrayArhiveContract(dgInfoClientContract, idClient: idClient);

                        tbClientName.Text = Client[0].Name;
                        tbClientSurname.Text = Client[0].Surname;
                        tbClientPatronymic.Text = Client[0].Patronymic;
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
                    Validation.SetColorTextBox(tbPhone, false);
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
                    if (SQLQuery.SearchClient(tbPhone) > 0) MessageBox.Show("Клиент с таким номером телефона уже есть в базе!", "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        string genderChar = "M";
                        if (cbGender.Text == "Женский") genderChar = "Ж";

                        using (RetroCarContext rcr = new RetroCarContext())
                        {
                            t_Client client = new t_Client
                            {
                                Name = tbClientName.Text,
                                Surname = tbClientSurname.Text,
                                Patronymic = tbClientPatronymic.Text,
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
        private void tbClientPatronymic_Enter(object sender, EventArgs e) { Validation.SetColorTextBox(tbClientPatronymic, true); }
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
            lbStatus.Text = "Готов";
        }
        //Показываем окно авторизации
        private void EndWork()
        {
            foreach (var panel in this.Controls.OfType<Panel>()) { panel.Visible = false; }
            SecondMenuMainPanel.Visible = false;
            SecondMenuNewContractPanel.Visible = false;
            SecondMenuArchiveContract.Visible = false;
            NewContractPanelClear();
            dgContract.Rows.Clear();
            dgClient.Rows.Clear();
            dgAuto.Rows.Clear();
            StatusPanel.Visible = false;
            
            this.Text = "RRCar ";            
            tbPassword.Text = "";
            tbPassword.Focus();

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
        //Осуществляем поиск договоров по номеру автомобиля
        private void SearchContractByCar()
        {
            string nomerAuto = dgAuto.CurrentRow.Cells[2].Value.ToString();
            ContractRepository.LoadArrayContract(dgContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))), nomerAuto);
        }
        //Загрузка доступных для аренды автомобилей в comboBox
        private void cbAuto_Enter(object sender, EventArgs e)
        {
            Validation.SetColorComboBox(cbAuto, true);
            if (AutoRepository.changeFlag)
            {
                cbAuto.Items.Clear();
                AutoRepository.LoadArrayAuto(cbAuto, dtStart.Value, dtEnd.Value);
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
                        Client.Surname = tbClientSurname.Text;
                        Client.Patronymic = tbClientPatronymic.Text;
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
            catch (System.Data.Entity.Infrastructure.DbUpdateException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Добовляем новый договор в БД
        private void btnArrange_Click(object sender, EventArgs e)
        {
            try
            {
                if (Validation.ValidationNewContract(NewContractForm))
                {
                    if (dtEnd.Value <= dtStart.Value) MessageBox.Show("Введенные даты не корректны", "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        using (RetroCarContext rcr = new RetroCarContext())
                        {
                            var query = (from clnt in rcr.t_Client
                                         where clnt.Phone == tbPhone.Text.Replace(" ", "")
                                         select clnt).ToList();

                            if (query.Count > 0)
                            {
                                StringBuilder sbClientFio = new StringBuilder();
                                StringBuilder sbClinetPasport = new StringBuilder();
                                sbClientFio.Append(query[0].Surname).Append(" ").Append(query[0].Name).Append(" ").Append(query[0].Patronymic);
                                sbClinetPasport.Append(query[0].PassportSeries).Append(" ").Append(query[0].PassportNumber);

                                string gender = "ий";
                                if (query[0].Gender == "Ж") gender = "ая";

                                var wordFile = new WorkWithWord("Resource\\ContractTemplate\\TemplateContract.docx");

                                t_Сontract contract = new t_Сontract
                                {
                                    CarNumber = AutoRepository.CarNumber(cbAuto.SelectedIndex),
                                    IDManager = int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.LastIndexOf('|'))),
                                    IDClient = query[0].IDClient,
                                    DateStart = DateTime.Parse(dtStart.Value.ToString("dd.MM.yyyy HH:00")),
                                    DateEnd = DateTime.Parse(dtEnd.Value.ToString("dd.MM.yyyy HH:00")),
                                    DateOfConclusion = DateTime.Now.Date,
                                    Status = "Действует"
                                };
                                rcr.t_Сontract.Add(contract);
                                rcr.SaveChanges();
                                var items = new Dictionary<string, string>
                            {
                                { "{cManager}", WorkForm.ActiveForm.Text.Remove(0, 19) },
                                { "{cNum}", contract.IDContract.ToString() },
                                { "{cDate}", DateTime.Now.ToString("d") },
                                { "{cClient}", sbClientFio.ToString() },
                                { "{gender}", gender },
                                { "{cAuto}", cbAuto.Text },
                                { "{cAutoNumber}", AutoRepository.CarNumber(cbAuto.SelectedIndex)},
                                { "{cDateStart}", dtStart.Value.ToString("dd.MM.yyyy HH:00") },
                                { "{cDateEnd}", dtEnd.Value.ToString("dd.MM.yyyy HH:00") },
                                { "{cHours}", (dtEnd.Value - dtStart.Value).TotalHours.ToString() },
                                { "{cCost}", lbPrice.Text.Remove(lbPrice.Text.IndexOf(" ")) },
                                { "{clientPasport}", sbClinetPasport.ToString() },
                            };
                                wordFile.FillTemplate(items);

                                PrintNotification("Договор успешно оформлен");
                                NewContractPanelClear();
                            }
                            else MessageBox.Show("Клиент не найден в базе данных.", "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Переходим на панель нового договора
        private void btnMenuNewContract_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = false;
            ArchiveContractPanel.Visible = false;
            dgArchiveClient.Rows.Clear();
            dgArchiveContract.Rows.Clear();
            NewContractPanel.Visible = true;
            SecondMenuArchiveContract.Visible = false;
            SecondMenuMainPanel.Visible = false;
            SecondMenuNewContractPanel.Visible = true;            
        }
        //Переходим на главную панель
        private void btnMenuHome_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = true;
            NewContractPanel.Visible = false;
            ArchiveContractPanel.Visible = false;
            dgArchiveClient.Rows.Clear();
            dgArchiveContract.Rows.Clear();
            SecondMenuNewContractPanel.Visible = false;
            SecondMenuArchiveContract.Visible = false;
            SecondMenuMainPanel.Visible = true;

            ContractRepository.LoadArrayContract(dgContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));

            AutoRepository.FillAuto(dgAuto);
        }
        //Обновляем отображаемые в dataGrid договоры
        private void btnDataUpdate_Click(object sender, EventArgs e)
        {
            ContractRepository.LoadArrayContract(dgContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));
            AutoRepository.FillAuto(dgAuto);

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
                string cause;
                if (dgContract.CurrentRow.Cells[6].Value == null) cause = null;
                else cause = dgContract.CurrentRow.Cells[6].Value.ToString();

                int idContract = int.Parse(dgContract.CurrentRow.Cells[0].Value.ToString());

                using (RetroCarContext rtc = new RetroCarContext())
                {
                    var Contract = rtc.t_Сontract.First(ct => ct.IDContract == idContract);

                    
                    Contract.CarNumber = dgContract.CurrentRow.Cells[1].Value.ToString();
                    Contract.DateStart = Convert.ToDateTime(dgContract.CurrentRow.Cells[2].Value);
                    Contract.DateEnd = Convert.ToDateTime(dgContract.CurrentRow.Cells[3].Value);
                    Contract.Summa = null;
                    Contract.Cause = cause;

                    rtc.SaveChanges();
                }
                PrintNotification("Договор успешно обновлен");
                ContractRepository.LoadArrayContract(dgContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));
            }
            catch (System.Data.Entity.Infrastructure.DbUpdateException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (EntityException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Удаляем выбранный договор в БД
        private void btnDeleteContract_Click(object sender, EventArgs e)
        {
            int idContract = int.Parse(dgContract.CurrentRow.Cells[0].Value.ToString());
            if (SQLQuery.DeleteContract(idContract))
            {
                PrintNotification("Договор успешно удален");
                ContractRepository.LoadArrayContract(dgContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));
            }
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
                        .Where(r => r.Cells[0].Value.ToString().Equals(scf.tbValue.Text))
                        .First();
                    rowIndex = row.Index;
                    dgContract.Rows[rowIndex].Selected = true;
                    SQLQuery.FillClientInTable(dgClient, int.Parse(dgContract.CurrentRow.Cells[7].Value.ToString()));
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
            try
            {
                SaveManager sv = new SaveManager();
                if (sv.GetFileData()) File.Delete(Path.Combine(Application.StartupPath, "Data.json"));
                PrintNotification("Пользователь удален");
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Открываем сформированный из шаблона файл договора в MS Word
        private void btnOpenContractWithWord_Click(object sender, EventArgs e)
        {
            WorkWithWord wordFile = new WorkWithWord();
            wordFile.OpenWordFile();
        }
        //Печатаем сформированный из шаблона файл договора
        private void btnContractPrint_Click(object sender, EventArgs e)
        {
            WorkWithWord wordFile = new WorkWithWord();
            wordFile.PrintWordFile();
        }
        //Меняем статус автомобиля
        private void btnChangeStatus_Click(object sender, EventArgs e)
        {
            try
            {
                string carNumber = dgAuto.CurrentRow.Cells[2].Value.ToString();

                if (dgAuto.CurrentRow.Cells[4].Value.Equals("Недоступен"))
                {
                    if(SQLQuery.ChangeStatusCar(carNumber, true)) PrintNotification("Статус успешно изменен");
                }
                else
                {
                    if (SQLQuery.ChangeStatusCar(carNumber, false))
                    {                        
                        SearchContractByCar();
                        PrintNotification("Статус успешно изменен");
                    }
                    else PrintNotification("Изменение статуса отменено");
                }
                AutoRepository.FillAuto(dgAuto);
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Переходим на панель с таблицей архивных договоров
        private void btnMenuArchive_Click(object sender, EventArgs e)
        {
            MainPanel.Visible = false;
            NewContractPanel.Visible = false;
            SecondMenuMainPanel.Visible = false;
            SecondMenuNewContractPanel.Visible = false;
            SecondMenuArchiveContract.Visible = true;
            ArchiveContractPanel.Visible = true;
            dgArchiveClient.Rows.Clear();
            dgArchiveContract.Rows.Clear();

            ContractRepository.LoadArrayArhiveContract(dgArchiveContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));
        }
        //Поиск договоров в архивной таблице 
        private void btnArchiveContractSearch_Click(object sender, EventArgs e)
        {
            try
            {
                SearchContractForm scf = new SearchContractForm();
                scf.ShowDialog();
                if (scf.DialogResult == DialogResult.OK)
                {
                    int rowIndex = -1;

                    DataGridViewRow row = dgArchiveContract.Rows
                        .Cast<DataGridViewRow>()
                        .Where(r => r.Cells[0].Value.ToString().Equals(scf.tbValue.Text))
                        .First();
                    rowIndex = row.Index;
                    dgArchiveContract.Rows[rowIndex].Selected = true;
                    SQLQuery.FillClientInTable(dgArchiveClient, int.Parse(dgArchiveContract.CurrentRow.Cells[7].Value.ToString()));
                }
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Обновляем данные в таблице архивных договоров
        private void btnArchiveContractUpdate_Click(object sender, EventArgs e)
        {
            ContractRepository.LoadArrayArhiveContract(dgArchiveContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));
        }
        //Выводим все договора оформленные на выбранный автомобиль
        private void btnSearchByCar_Click(object sender, EventArgs e)
        {
            SearchContractByCar();
        }
        //Подтверждаем закрытие договора
        private void btnContractConfirm_Click(object sender, EventArgs e)
        {
            try
            {
                DateTime dt = new DateTime();
                dt = DateTime.Parse(dgContract.CurrentRow.Cells[3].Value.ToString());
                if (dt.Date == DateTime.Now.Date && dt.Hour < DateTime.Now.Hour)
                {
                    int idContract = int.Parse(dgContract.CurrentRow.Cells[0].Value.ToString());
                    if (SQLQuery.ConfirmContract(idContract))
                    {
                        ContractRepository.LoadArrayContract(dgContract, int.Parse(lbManagerID.Text.Remove(lbManagerID.Text.IndexOf("|"))));
                        PrintNotification("Договор успешно закрыт");
                    }
                }
                else PrintNotification("Условия для закрытия договора не выполнены");
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Автоматически загружаем данные о клиенте при выборе договора
        private void dgContract_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                int idClient = int.Parse(dgContract.CurrentRow.Cells[7].Value.ToString());
                SQLQuery.FillClientInTable(dgClient, idClient);
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
        //Автоматически загружаем данные о клиенте при выборе архивного договора
        private void dgArchiveContract_SelectionChanged(object sender, EventArgs e)
        {
            try
            {
                int idClient = int.Parse(dgArchiveContract.CurrentRow.Cells[7].Value.ToString());
                SQLQuery.FillClientInTable(dgArchiveClient, idClient);
            }
            catch (InvalidOperationException error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }
    }
}
