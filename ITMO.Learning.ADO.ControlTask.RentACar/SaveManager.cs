using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{

    class SaveManager
    {
        public string Login { get; set; }
        public string Password { get; set; }

        private string LoadData()
        {
            string loadData = null;
            //Читаем файл даных
            loadData = File.ReadAllText(Path.Combine(Application.StartupPath, "Data.json"));
            //Считаем кол-во символов
            int charCount = loadData.Length;
            //Делим на 2
            byte[] bytes = new byte[charCount / 2];
            //Разделяем биты для расшифровки
            for (int i = 0; i < charCount; i += 2) bytes[i / 2] = Convert.ToByte(loadData.Substring(i, 2), 16);
            //Преобразовываем в латиницу
            loadData = Encoding.UTF8.GetString(bytes, 0, bytes.Length);
            //Возвращаем полученный результат
            return loadData;
        }

        public string LoadTextBoxData()
        {
            StringBuilder sb = new StringBuilder();
            if (GetFileData())
            {
                SaveManager sm = new SaveManager();
                sm = JsonSerializer.Deserialize<SaveManager>(LoadData());
                sb.Append(sm.Login).Append("|").Append(sm.Password);                
            }
            return sb.ToString();
        }

        public void SaveTextBoxData(string login, string password)
        {
            SaveManager sv = new SaveManager
            {
                Login = login,
                Password = password
            };
            //Защифровываем
            byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(sv));
            string hex = BitConverter.ToString(bytes);
            //Записываем в файл
            File.WriteAllText(Path.Combine(Application.StartupPath, "Data.json"), hex.Replace("-", ""));
        }

        public bool GetFileData() 
        {
            bool flag = false;
            if (File.Exists(Path.Combine(Application.StartupPath, "Data.json"))) flag = true;
            return flag;
        }
    }
}
