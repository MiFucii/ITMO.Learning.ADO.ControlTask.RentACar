using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using Word = Microsoft.Office.Interop.Word;

namespace ITMO.Learning.ADO.ControlTask.RentACar.ClassLibrary
{
    class WorkWithWord
    {
        private FileInfo fileInfo;
        public WorkWithWord() { }
        public WorkWithWord(string fileName)
        {
            if (File.Exists(fileName)) fileInfo = new FileInfo(fileName);
            else throw new FileNotFoundException("Фаил шаблон договора не найден!");
        }
        //Открываем и заполняем шаблон договора данными из формы
        internal void FillTemplate(Dictionary<string, string> items)
        {
            Word.Application app = null;
            try
            {
                app = new Word.Application();
                Object file = fileInfo.FullName;
                Object missing = Type.Missing;

                app.Documents.Open(file);
                foreach (var i in items)
                {
                    Word.Find find = app.Selection.Find;
                    find.Text = i.Key;
                    find.Replacement.Text = i.Value;

                    Object wrap = Word.WdFindWrap.wdFindContinue;
                    Object replace = Word.WdReplace.wdReplaceAll;

                    find.Execute(
                        FindText: Type.Missing,
                        MatchCase: false,
                        MatchWholeWord: false,
                        MatchWildcards: false,
                        MatchSoundsLike: missing,
                        MatchAllWordForms: false,
                        Forward: true,
                        Wrap: wrap,
                        Format: false,
                        ReplaceWith: missing,
                        Replace: replace);
                }

                Object newFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), fileInfo.Name);
                app.ActiveDocument.SaveAs2(newFileName);

            }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { if (app != null) { app.ActiveDocument.Close(); app.Quit(); } }
        }
        //Метод открытия договора сформированного из шаблона в MS Word
        internal void OpenWordFile()
        {
            var app = new Word.Application();
            try
            {
                app.Documents.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TemplateContract.docx"));
                app.Visible = true;
            }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }           
        }
        //Метод выполнения печати договора сформированного из шаблона
        internal void PrintWordFile()
        {
            var app = new Word.Application();
            try
            {
                app.Documents.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TemplateContract.docx"));
                app.Dialogs[Word.WdWordDialog.wdDialogFilePrint].Show();
            }
            catch (Exception error) { MessageBox.Show(error.Message, "Ошибка:", MessageBoxButtons.OK, MessageBoxIcon.Error); }
            finally { app.Quit(); }            
        }
    }
}
