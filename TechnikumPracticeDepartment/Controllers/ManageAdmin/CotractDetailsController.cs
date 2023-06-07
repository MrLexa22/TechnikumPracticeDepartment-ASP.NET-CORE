using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Syncfusion.DocIO.DLS;
using Syncfusion.DocIO;
using System.Security.Claims;
using TechnikumPracticeDepartment.Controllers.ManagePractice;
using TechnikumPracticeDepartment.Controllers.StudentPage;
using TechnikumPracticeDepartment.Models.ModelsContractDetail;
using TechnikumPracticeDepartment.ModelsDB;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Bibliography;
using System.IO;

namespace TechnikumPracticeDepartment.Controllers.ManageAdmin
{
    [ViewLayout("_LayoutAuthenticatedUser")]
    public class CotractDetailsController : Controller
    {
        TechnikumPracticeDepartmentContext db;
        public CotractDetailsController(TechnikumPracticeDepartmentContext context)
        {
            db = context;
        }
        public bool UpdateIn(int role)
        {
            if (User.IsInRole("Не подтверждён ФЗ"))
                return false;
            if (!User.Identity.IsAuthenticated)
                return false;

            User userDB = new User();
            try
            {
                userDB = db.Users.Where(p => p.Email == HttpContext.User.FindFirst(ClaimTypes.Email).Value).First();
                if (userDB == null)
                {
                    HttpContext.SignOutAsync("Application");
                    return false;
                }
            }
            catch
            {
                return false;
            }

            if (role == 0)
                return true;
            var rolesUser = db.UsersRoles.Where(p => p.UserId == userDB.IdUser).ToList();
            if (role > 0 && rolesUser.Where(p => p.RoleId == role).Count() > 0)
                return true;
            else
                return false;
        }
        public IActionResult Index()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");
            ContractDetail model = new();
            model.Error = false;
            FileStream fileStream = new FileStream(@"exmplesWord/InformationForDogovor.txt", FileMode.Open);
            using (StreamReader reader = new StreamReader(fileStream))
            {
                model.SurnameDirector_IM = reader.ReadLine();
                model.NamameDirector_IM = reader.ReadLine();
                model.PatronymicNameDirector_IM = reader.ReadLine();

                model.SurnameDirector_ROD = reader.ReadLine();
                model.NamameDirector_ROD = reader.ReadLine();
                model.PatronymicNameDirector_ROD = reader.ReadLine();

                var date = reader.ReadLine().Split(".");
                model.DovernDate = new DateTime(Convert.ToInt16(date[2]), Convert.ToInt16(date[1]), Convert.ToInt16(date[0]));
                model.NumberDovern = reader.ReadLine();
                model.AddressUniversity = reader.ReadLine();
                model.INN_University = reader.ReadLine();
                model.KPP_University = reader.ReadLine();
                model.OKTMO_University = reader.ReadLine();
                model.Bank_University = reader.ReadLine();
            }
            return View("~/Views/ManageAdmin/ContractDetail.cshtml", model);
        }
        public IActionResult downloadExample()
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            MemoryStream stream = new MemoryStream();
            try
            {
                FileStream fileStreamPath = new FileStream(@"exmplesWord/Договор.docx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using (WordDocument document = new WordDocument(fileStreamPath, FormatType.Automatic))
                {
                    FileStream fileStream = new FileStream(@"exmplesWord/InformationForDogovor.txt", FileMode.Open);
                    string DIRECTOR_IM = "";
                    string DOVETN_MONTH = "";
                    string DOVERN_YEAR = "";
                    string DOVERN_DATE = "";
                    string DOVERN_NUMBER = "";
                    string INITIALSD = "";
                    string ADDRESS_UN = "";
                    string INN_UN = "";
                    string KPP_UN = "";
                    string OKTMO_UN = "";
                    string BANK_UN = "";
                    using (StreamReader reader = new StreamReader(fileStream))
                    {
                        INITIALSD = reader.ReadLine();
                        INITIALSD += " " + reader.ReadLine()[0] + ".";
                        string otch = reader.ReadLine();
                        INITIALSD += otch == "" ? "" : otch[0] + ".";

                        DIRECTOR_IM = reader.ReadLine();
                        DIRECTOR_IM += " " + reader.ReadLine();
                        otch = reader.ReadLine();
                        DIRECTOR_IM += otch == "" ? "" : " " + otch;

                        string[] date = reader.ReadLine().Split(".");
                        DateTime dateTime = new DateTime(Convert.ToInt16(date[2]), Convert.ToInt16(date[1]), Convert.ToInt16(date[0]));
                        DOVETN_MONTH = dateTime.ToString("MMMM");
                        if (DOVETN_MONTH != "март" && DOVETN_MONTH != "август" && DOVETN_MONTH != "май")
                            DOVETN_MONTH = DOVETN_MONTH.Replace("ь", "я");
                        else if (DOVETN_MONTH != "март" || DOVETN_MONTH != "август")
                            DOVETN_MONTH = DOVETN_MONTH + "а";
                        else if (DOVETN_MONTH != "май")
                            DOVETN_MONTH = DOVETN_MONTH.Replace("й", "я");
                        DOVERN_YEAR = dateTime.Year.ToString();
                        DOVERN_DATE = dateTime.Day.ToString();

                        DOVERN_NUMBER = reader.ReadLine();
                        ADDRESS_UN = reader.ReadLine();
                        INN_UN = reader.ReadLine();
                        KPP_UN = reader.ReadLine();
                        OKTMO_UN = reader.ReadLine();
                        BANK_UN = reader.ReadLine();
                    }
                    document.Replace("<<YEAR_NOW>>", DateTime.Now.Year.ToString(), true, true);
                    document.Replace("<<INITIALSD>>", INITIALSD, true, true);
                    document.Replace("<<DIRECTOR_IM>>", DIRECTOR_IM, true, true);
                    document.Replace("<<DOVETN_MONTH>>", DOVETN_MONTH, true, true);
                    document.Replace("<<DOVERN_YEAR>>", DOVERN_YEAR, true, true);
                    document.Replace("<<DOVERN_DATE>>", DOVERN_DATE, true, true);
                    document.Replace("<<DOVERN_NUMBER>>", DOVERN_NUMBER, true, true);
                    document.Replace("<<ADDRESS_UN>>", ADDRESS_UN, true, true);
                    document.Replace("<<INN_UN>>", INN_UN, true, true);
                    document.Replace("<<KPP_UN>>", KPP_UN, true, true);
                    document.Replace("<<OKTMO_UN>>", OKTMO_UN, true, true);
                    document.Replace("<<BANK_UN>>", BANK_UN, true, true);

                    document.Replace("<<FULLNAME_ORGANIZATION>>", "Полное наименование организации", true, true);
                    document.Replace("<<ADDRESS_ORGANIZATION>>", "Адрес организации", true, true);
                    document.Replace("<<INN_ORGANIZATION>>", "ИНН организации", true, true);

                    document.Replace("<<CODE_SPEC>>", "Код специальности", true, true);
                    document.Replace("<<NAME_SPEC>>", "Наименование специальности", true, true);
                    document.Replace("<<QUAL_NAME>>", "Квалификация специальности", true, true);
                    document.Replace("<<STUDENT_FIO_GROUP>>", "ФИО студента Группа", true, true);

                    WTable table = new WTable(document);

                    table.ResetCells(2, 3);
                    table[0, 0].AddParagraph().AppendText("Профессиональный модуль (ПМ), в рамках которого проводится производственная практика");
                    table[0, 1].AddParagraph().AppendText("Наименование производственной практики");
                    table[0, 2].AddParagraph().AppendText("Периоды проведения практики.\nДни практики");

                    table[1, 0].AddParagraph().AppendText("Наименование проф модуля");
                    table[1, 1].AddParagraph().AppendText("Наименование практики");

                    string days = "";
                    days = "ПН, ЧТ";
                    var practicePeriod = "09.01.2023-15.01.2023\n08.02.2023-15.02.2023";
                    table[1, 2].AddParagraph().AppendText(practicePeriod + "\n" + days);

                    WTableStyle tableStyle = document.AddTableStyle("CustomStyle") as WTableStyle;
                    tableStyle.TableProperties.Borders.Color = Syncfusion.Drawing.Color.Black;
                    tableStyle.CellProperties.VerticalAlignment = VerticalAlignment.Middle;

                    ConditionalFormattingStyle firstRowStyle = tableStyle.ConditionalFormattingStyles.Add(ConditionalFormattingType.FirstRow);
                    firstRowStyle.CharacterFormat.Italic = true;
                    firstRowStyle.CharacterFormat.Bold = true;
                    firstRowStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;

                    tableStyle.ParagraphFormat.HorizontalAlignment = HorizontalAlignment.Center;
                    tableStyle.ParagraphFormat.AfterSpacing = 0;

                    table.TableFormat.Borders.LineWidth = 0.5f;
                    table.TableFormat.Borders.Horizontal.LineWidth = 0.5f;
                    table.TableFormat.Borders.Vertical.LineWidth = 0.5f;
                    WTableRow row = table.Rows[0];
                    table.ApplyStyle("CustomStyle");

                    TextBodyPart bodyPart = new TextBodyPart(document);
                    bodyPart.BodyItems.Add(table);
                    document.Replace("<<TABLE>>", bodyPart, true, true, true);

                    stream = new MemoryStream();
                    document.Save(stream, FormatType.Docx);
                    document.Close();
                    stream.Position = 0;
                }

                stream = RemoveWatermarks.removeWatermarks(stream);
            }
            catch { }
            return File(stream, "application/msword", "Dogovor.docx");
        }

        [HttpPost]
        public IActionResult UpdateContractDetail(ContractDetail model)
        {
            if (UpdateIn(1) == false)
                return RedirectToAction("Index", "Home");

            if(!ModelState.IsValid)
                return RedirectToAction("Index", "CotractDetails");
            try
            {
                using (FileStream fs = new FileStream(@"exmplesWord/InformationForDogovor.txt", FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None))
                {
                    StreamReader sr = new StreamReader(fs);
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        fs.SetLength(0);
                        sw.WriteLine(ToUpperFirstLetter.UpperFirstLetter(model.SurnameDirector_IM.Trim()));
                        sw.WriteLine(ToUpperFirstLetter.UpperFirstLetter(model.NamameDirector_IM.Trim()));
                        sw.WriteLine(ToUpperFirstLetter.UpperFirstLetter(model.PatronymicNameDirector_IM?.Trim()));
                        sw.WriteLine(ToUpperFirstLetter.UpperFirstLetter(model.SurnameDirector_ROD.Trim()));
                        sw.WriteLine(ToUpperFirstLetter.UpperFirstLetter(model.NamameDirector_ROD.Trim()));
                        sw.WriteLine(ToUpperFirstLetter.UpperFirstLetter(model.PatronymicNameDirector_ROD?.Trim()));
                        sw.WriteLine(model.DovernDate.ToString("dd.MM.yyyy"));
                        sw.WriteLine(model.NumberDovern.Trim());
                        sw.WriteLine(model.AddressUniversity.Trim());
                        sw.WriteLine(model.INN_University.Trim());
                        sw.WriteLine(model.KPP_University.Trim());
                        sw.WriteLine(model.OKTMO_University.Trim());
                        sw.WriteLine(model.Bank_University.Trim());
                    }
                }
            }
            catch
            {
                model.Error = true;
                return View("~/Views/ManageAdmin/ContractDetail.cshtml", model);
            }

            return RedirectToAction("Index", "CotractDetails");
        }
        public bool CheckDovernDate(DateTime DovernDate)
        {
            if (DovernDate > DateTime.Now)
                return false;
            //2021-15 = 2006   2023-15 = 2008
            if(DovernDate.Year < DateTime.Now.Year-15)
                return false;
            return true;
        }
        public bool CheckPatronymicNameDirector_IM(string? PatronymicNameDirector_IM)
        {
            string? PatronymicNameUser = PatronymicNameDirector_IM;
            if (PatronymicNameUser == null)
            {
                return true;
            }
            else if (PatronymicNameUser.Trim() == "")
            {
                return true;
            }
            if (PatronymicNameUser.StartsWith(" "))
            {
                return false;
            }
            else if (PatronymicNameUser.EndsWith(" "))
            {
                return false;
            }
            else if (PatronymicNameUser.Length < 3 || PatronymicNameUser.Length > 100)
            {
                return false;
            }
            else if (!Regex.IsMatch(PatronymicNameUser, "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$"))
            {
                return false;
            }
            return true;
        }
        public bool CheckPatronymicNameDirector_ROD(string? PatronymicNameDirector_ROD)
        {
            string? PatronymicNameUser = PatronymicNameDirector_ROD;
            if (PatronymicNameUser == null)
            {
                return true;
            }
            else if (PatronymicNameUser.Trim() == "")
            {
                return true;
            }
            if (PatronymicNameUser.StartsWith(" "))
            {
                return false;
            }
            else if (PatronymicNameUser.EndsWith(" "))
            {
                return false;
            }
            else if (PatronymicNameUser.Length < 3 || PatronymicNameUser.Length > 100)
            {
                return false;
            }
            else if (!Regex.IsMatch(PatronymicNameUser, "^[а-яёА-ЯЁ]+(?:[\\s.-][а-яёА-ЯЁ]+)*$"))
            {
                return false;
            }
            return true;
        }
    }
}
