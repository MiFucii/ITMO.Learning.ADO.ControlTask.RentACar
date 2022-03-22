using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ITMO.Learning.ADO.ControlTask.RentACar
{
    static class Validation
    {
        //Проверяем на заполненость(Валидность) поля передоваемой формы
        public static bool ValidationNewClient(TableLayoutPanel tlp)
        {
            bool flag = true;
            foreach (var tb in tlp.Controls.OfType<TextBox>())
            {
                if (tb.Text == "")
                {
                    SetColorTextBox(tb, false);
                    flag = false;
                }
            }
            foreach (var mtb in tlp.Controls.OfType<MaskedTextBox>())
            {
                if (!mtb.MaskCompleted)
                {
                    SetColorTextBox(mtb, false);
                    flag = false;
                }
            }
            if (tlp.Controls["cbGender"].Text == "")
            {
                SetColorComboBox((ComboBox)tlp.Controls["cbGender"], false);
                flag = false;
            }

            return flag;
        }
        public static bool ValidationNewContract(TableLayoutPanel tlp)
        {
            bool flag = true;
            if (tlp.Controls["cbAuto"].Text == "")
            {
                SetColorComboBox((ComboBox)tlp.Controls["cbAuto"], false);
                flag = false;
            }
            else if (!ValidationNewClient(tlp)) {flag = false; }
            return flag;
        }
        //В зависимости от флага меняем цвет TextBox-ов
        public static void SetColorTextBox(TextBoxBase obj, bool flag)
        {
            if (flag) obj.BackColor = Color.White;
            else obj.BackColor = Color.FromArgb(255, 153, 153);
        }
        //В зависимости от флага меняем цвет ComboBox-ов
        public static void SetColorComboBox(ComboBox obj, bool flag)
        {
            if (flag) obj.BackColor = Color.White;
            else obj.BackColor = Color.FromArgb(255, 153, 153);
        }
    }
}
