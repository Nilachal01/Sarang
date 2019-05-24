using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using System.Speech.Synthesis;
using System.Speech.AudioFormat;
using Sarang.Models;
using System.Text;
using System.Web.UI;
using Sarang.App_Code;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.Data;
using System.Threading;
using System.Globalization;
using NAudio.Lame;
using NAudio.Wave;
using System.Net;
using Newtonsoft.Json.Linq;
using System.Net.Mail;
using PagedList;
using PagedList.Mvc;
using System.Web.Script.Serialization;
using System.Data.OleDb;
using Org.BouncyCastle.Utilities;
using Twilio;
using Twilio.Clients;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace Sarang.Controllers
{
    public class UserController : Controller
    {
        Student student = new Student();
        CollegeEntities cg = new CollegeEntities();
        IGITEntities dc = new IGITEntities();
        
        public ActionResult Index()
        {
           
            return View();
        }

        [HttpPost]

        public ActionResult Index(FormCollection FC)
        {

            var FirstName = FC["FName"]; //Collect data from index cshtml form(View)
            var LastName = FC["LName"];
            var EmailID = FC["EmailIDs"];
            var DateOfBirth = Convert.ToDateTime(FC["DOB"]);//Convert  to date and time format
            string Password = FC["Password"];

            var Exist = dc.Students.Where(x => x.EmailID == EmailID).FirstOrDefault();

            if (Exist != null)
            {
                ViewBag.Email = "This EmailID is already Registerd! ";
                return View();
            }
            //string pass = StringHelper.Encrypt(Password); //used for encrypt the string


            var result = dc.SPInsert(FirstName, LastName, EmailID, DateOfBirth, Password);//Insert data into table through store procedure
            return RedirectToAction("List");
        }

        public JsonResult Exist(string Email)
        {
            var check = dc.Students.Where(x => x.EmailID == Email).FirstOrDefault();
            if (check != null)
            {
                string data = "Email already Exist";
                return Json(data, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string data = null;
                return Json(data, JsonRequestBehavior.AllowGet);
            }



        }


        public ActionResult List(int? page)
        {
            if (Session["EmailID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {

                ViewBag.Data = dc.Students.ToList();


                return View();
            }
        }

        public ActionResult ForgotPass()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ForgotPass(FormCollection fc)
        {
            var mail = fc["EmailID"];
            var pass = dc.Students.Where(x => x.EmailID == mail).Select(x => x.Password).FirstOrDefault();
            var ID = dc.Students.Where(x => x.EmailID == mail).Select(x => x.ID).FirstOrDefault().ToString();

            var Activation_Code = Guid.NewGuid();
            var ActiveCode = Activation_Code.ToString();

            var verifyUrl = "/User/mailverify?EmailID=" + StringHelper.Encrypt(mail) + "&ID=" + ID + "&ActiveCode=" + StringHelper.Encrypt(ActiveCode);
            var link = Request.Url.AbsoluteUri.Replace(Request.Url.PathAndQuery, verifyUrl);


            #region send mail



            var fromEmail = new MailAddress("nilachal03@gmail.com", "Convergent Technologies ");
            var toEmail = new MailAddress(mail);
            var fromEmailPassword = "9937902043";
            string subject = "Reset password of Convergent Technologies";
            LinkedResource imagelink = new LinkedResource(System.Web.HttpContext.Current.Server.MapPath("~\\Image") + "/2.jpg", "image/jpg");
            string body = "<br></br>Dear Graduate,<br/><br/> Your  password is" + pass + ". Or your reset password link of Convergent Technologies  is below" + ".<br/> Please click the link to reset your password " + "<br><a href='" + link + "'>" + link + " </a>" + "<br/><a href='convergenttec.com'>Convergent Technologies</a>" + "<br/><br/>Thanks and Regards," + "<br/>Nilachal sethi<br/>" + "<img src='" + imagelink + "'/>";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromEmail.Address, fromEmailPassword)
            };




            //Add view to the Email Message





            using (var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = subject,
                Body = body,
                IsBodyHtml = true


            }

                )

                smtp.Send(message);


            #endregion


            TempData["mail"] = mail;
            return RedirectToAction("Mailed");
        }
        public ActionResult Mailed()
        {
            
            if(TempData["mail"].ToString() != null)
            {
                var Name = TempData["mail"].ToString();
                string Asterik_Name = Name.Substring(Name.Length - 16).PadLeft(Name.Length, '*');
                ViewBag.mail = Asterik_Name;
                return View();
            }
            else
            {
                ViewBag.Error = "Please Enter your EmailID";
                return RedirectToAction("ForgotPass");
            }
            
        }

        public ActionResult mailverify(string EmailID, string ActiveCode, long ID)
        {

            string EmailIDP = StringHelper.Decrypt(EmailID);
            string ActiveCodeP = StringHelper.Decrypt(ActiveCode);
            var UID = Convert.ToInt64(ID);
            //var IDU = Convert.ToInt64(UID);



            var v = dc.Students.Where(a => a.EmailID == EmailIDP && a.ID == UID).FirstOrDefault();
            if (v != null)
            {
                ViewBag.ID = UID;

                return View();
            }
            return View("ForgotPass");

        }
        [HttpPost]
        public ActionResult mailverify(FormCollection fc)
        {
            var pass = fc["pass"];
            var pass1 = fc["pass1"];

            if (pass == pass1)
            {
                var ID = Convert.ToInt64(fc["ID"]);
                var upadte = dc.SPchangePass(ID, pass);
                return RedirectToAction("Login");
            }



            return Content("Password did not matched");
        }



        public ActionResult EXPORT()
        {
            if (Session["EmailID"] == null)
            {
                return RedirectToAction("Login", "User");
            }
            else
            {
                string mail = Session["EmailID"].ToString();

                var id = dc.Students.Where(x => x.EmailID.Equals(mail)).Select(x => x.ID).FirstOrDefault();//get id by linq and lambda expression

                //var v = dc.Students.ToList();
                var v = dc.AllData(id).ToList();//merge two table data by store procedure
                int c = 0;
                StringBuilder Ex = new StringBuilder();


                if (v != null && v.Any())
                {

                    Ex.Append("<table  border='1' style='border:4px solid black; font-size:12px;'>");
                    Ex.Append("<tr>");
                    Ex.Append("<th align='center'  style='width:40px;background-color:#fc9003;'><b>Sr. No.</b></th>");
                    Ex.Append("<th align='center'  style='width:40px;background-color:#fc9003;'><b>ID</b></th>");
                    Ex.Append("<th align='center'  style='width:40px;background-color:#fc9003;'><b>FirstName</b></th>");
                    Ex.Append("<th align='center'  style='width:40px;background-color:#fc9003;'><b>LastName</b></th>");
                    Ex.Append("<th align='center'  style='width:100px;background-color:#fc9003;'><b>EmailID</b></th>");
                    Ex.Append("<th align='center'  style='width:100px;background-color:#fc9003;'><b>Date Of Birth</b></th>");
                    Ex.Append("<th align='center'  style='width:100px;background-color:#fc9003;'><b>Address</b></th>");
                    Ex.Append("<th align='center'  style='width:100px;background-color:#fc9003;'><b>PhoneNumber</b></th>");
                    Ex.Append("<th align='center'  style='width:100px;background-color:#fc9003;'><b>Certificate</b></th>");

                    Ex.Append("</tr>");


                    foreach (var item in v)
                    {
                        Ex.Append("<tr>");
                        Ex.Append("<td align='left'  style='width:40px;height:50px;'>" + (++c) + "</td>");
                        Ex.Append("<td align='left' style='width:40px;height:50px;'>" + item.ID + "</td>");
                        Ex.Append("<td align='left' style='width:40px;height:50px;'>" + item.FirstName + "</td>");
                        Ex.Append("<td align='left' style='width:40px;height:50px;'>" + item.LastName + "</td>");
                        Ex.Append("<td align='left' style='width:100px;height:50px;'>" + item.EmailID + "</td>");
                        Ex.Append("<td align='left' style='width:100px;height:50px;'>" + item.DateOfBirth + "</td>");
                        Ex.Append("<td align='left' style='width:100px;height:50px;'>" + item.Address + "</td>");
                        Ex.Append("<td align='left' style='width:100px;height:50px;'>" + item.PhoneNumber + "</td>");
                        Ex.Append("<td align='left' style='width:100px;height:50px;'>" + item.Certificate + "</td>");
                        Ex.Append("</tr>");

                    }
                    Ex.Append("</table>");
                }

                string fileName = "Data.xls";
                HttpContext.Response.AddHeader("content-disposition", "inline; filename=" + fileName);
                //this.Response.ContentType = "application/vnd.ms-excel";
                this.Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

                byte[] buffer = System.Text.Encoding.UTF8.GetBytes(Ex.ToString());

                return File(buffer, "application/vnd.ms-excel");
            }
        }


        public ActionResult pdfDownload()
        {
            var background = Server.MapPath("/");
            string fileName = "Data";
            using (StringWriter sw = new StringWriter())
            {
                using (HtmlTextWriter hw = new HtmlTextWriter(sw))
                {

                    string imageFilePath = background + "Image\\maxresdefault.jpg";
                    iTextSharp.text.Image jpg = iTextSharp.text.Image.GetInstance(imageFilePath);
                    iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 0, 0, 0, 0);
                    jpg.ScaleToFit(1000, 1013);
                    jpg.Alignment = iTextSharp.text.Image.UNDERLYING;
                    jpg.SetAbsolutePosition(0, 30);


                    string strLogo1 = Server.MapPath("~" + "\\Image\\2.jpg");
                    iTextSharp.text.Image bdlogo1 = iTextSharp.text.Image.GetInstance(strLogo1);
                    bdlogo1.ScaleAbsolute(120f, 55f);



                    //iTextSharp.text.Document pdfDoc = new iTextSharp.text.Document(PageSize.A4, 0, 0, 0, 0);


                    PdfWriter.GetInstance(pdfDoc, Response.OutputStream);
                    HTMLWorker htmlparser = new HTMLWorker(pdfDoc);

                    PdfWriter writer = PdfWriter.GetInstance(pdfDoc, Response.OutputStream);

                    PdfPTable table = new PdfPTable(5);// number of part 
                    table.TotalWidth = 500f;
                    //fix the absolute width of the table
                    table.LockedWidth = true;

                    float[] widths = new float[] { 100f, 100f, 130f, 120f, 100f };
                    table.SetWidths(widths);

                    table.HorizontalAlignment = 1;//0=Left, 1=Centre, 2=Right
                                                  //table.VerticalAlignment = 1;


                    table.WidthPercentage = 100;
                    table.SpacingBefore = 20f;
                    table.SpacingAfter = 20f;
                    table.PaddingTop = 30;
                    //table.DefaultCell.Border = Rectangle.ALIGN_TOP;


                    var Data = dc.Students.ToList();


                    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(85, 85, 85));
                    var boldCap = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, new BaseColor(85, 85, 85));
                    var NormalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(85, 85, 85));
                    var MainHeading = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(85, 85, 85));
                    var InnertableHeading = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10, new BaseColor(255, 255, 255));


                    PdfPCell cell_Logo1 = new PdfPCell(bdlogo1) { PaddingTop = 55, PaddingLeft = 10, HorizontalAlignment = 1, BorderWidth = 1 };
                    ///////Blank line
                    PdfPCell blank1 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingTop = 30, PaddingLeft = 20 };
                    PdfPCell blank2 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingTop = 30, PaddingLeft = 10 };
                    PdfPCell blank3 = new PdfPCell(new Phrase("DETAILS", MainHeading)) { BorderWidth = 0, PaddingTop = 30, PaddingLeft = 10 };
                    PdfPCell blank4 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingTop = 30, PaddingLeft = 10 };
                    PdfPCell blank5 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingTop = 30, PaddingLeft = 10 };

                    table.AddCell(blank1);
                    table.AddCell(blank2);
                    table.AddCell(blank3);
                    table.AddCell(blank4);
                    table.AddCell(blank5);

                    ////////////////////

                    /////////////////////////space or new line

                    PdfPCell blank6 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingBottom = 40, PaddingLeft = 20 };
                    PdfPCell blank7 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingBottom = 40, PaddingLeft = 10 };
                    PdfPCell blank8 = new PdfPCell(new Phrase("", MainHeading)) { BorderWidth = 0, PaddingBottom = 40, PaddingLeft = 10 };
                    PdfPCell blank9 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingBottom = 40, PaddingLeft = 10 };
                    PdfPCell blank10 = new PdfPCell(new Phrase("", boldFont)) { BorderWidth = 0, PaddingBottom = 40, PaddingLeft = 10 };

                    table.AddCell(blank6);
                    table.AddCell(blank7);
                    table.AddCell(blank8);
                    table.AddCell(blank9);
                    table.AddCell(blank10);

                    //////////////////////

                    PdfPCell FirstName = new PdfPCell(new Phrase("FirstName", boldFont)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 20 };
                    PdfPCell LastName = new PdfPCell(new Phrase("FirstName", boldFont)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };
                    PdfPCell EmailID = new PdfPCell(new Phrase("EmailID", boldFont)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };
                    PdfPCell DateOfBirth = new PdfPCell(new Phrase("Date Of Birth", boldFont)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };
                    PdfPCell Password = new PdfPCell(new Phrase("Password", boldFont)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };



                    table.AddCell(FirstName);
                    table.AddCell(LastName);
                    table.AddCell(EmailID);
                    table.AddCell(DateOfBirth);
                    table.AddCell(Password);

                    foreach (var item in Data)
                    {
                        PdfPCell FName = new PdfPCell(new Phrase(item.FirstName.ToUpper(), boldCap)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 20 };
                        PdfPCell LName = new PdfPCell(new Phrase(item.LastName, boldCap)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };
                        PdfPCell Email = new PdfPCell(new Phrase(item.EmailID, boldCap)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };
                        PdfPCell DOB = new PdfPCell(new Phrase(Convert.ToString(item.DateOfBirth), boldCap)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };
                        PdfPCell Pass = new PdfPCell(new Phrase(item.Password, boldCap)) { BorderWidth = 1, PaddingTop = 10, PaddingLeft = 10 };

                        table.AddCell(FName);
                        table.AddCell(LName);
                        table.AddCell(Email);
                        table.AddCell(DOB);
                        table.AddCell(Pass);

                    }
                    pdfDoc.Open();
                    pdfDoc.Add(jpg);

                    pdfDoc.Add(table);


                    pdfDoc.Close();
                    Response.ContentType = "application/pdf";
                    Response.AddHeader("Content-Disposition", "inline;filename=Certificate_" + fileName + ".pdf");// Used for show the pdf
                                                                                                                  // Response.AddHeader("Content-Disposition", "attachment;filename=Certificate_" + fileName + ".pdf");//Used for download PDF


                    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    Response.Write(pdfDoc);
                    Response.End();
                    //////////////////////////////////////////////////////////////
                    //string fileName = "Data";


                    //var Data = dc.Students.ToList();
                    //Document document = new Document(PageSize.A4, 10f, 10f, 10f, 10f);

                    //document.SetMargins(70f, 50f, 20f, 100f);
                    //PdfWriter.GetInstance(document, Response.OutputStream);
                    //HTMLWorker htmlparser = new HTMLWorker(document);
                    //PdfWriter writer = PdfWriter.GetInstance(document, Response.OutputStream);
                    //document.Open();
                    //foreach (var item in Data)
                    //{
                    //    document.NewPage();

                    //    PdfPTable table = new PdfPTable(3);
                    //    table.TotalWidth = 450f;
                    //    //fix the absolute width of the table
                    //    table.LockedWidth = true;

                    //    //relative col widths in proportions - 1/3 and 2/3
                    //    float[] widths = new float[] { 4f, 10f, 5f };
                    //    table.SetWidths(widths);
                    //    table.HorizontalAlignment = 0;
                    //    //leave a gap before and after the table
                    //    table.SpacingBefore = 20f;
                    //    table.SpacingAfter = 30f;
                    //    var MainHeading = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14, new BaseColor(85, 85, 85));
                    //    var boldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, new BaseColor(85, 85, 85));
                    //    var boldCap = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 9, new BaseColor(85, 85, 85));
                    //    var NormalFont = FontFactory.GetFont(FontFactory.HELVETICA, 9, new BaseColor(85, 85, 85));

                    //    PdfPCell cell = new PdfPCell(new Phrase(item.FirstName.ToUpper(), MainHeading)) { HorizontalAlignment = 1, BorderWidth = 0, PaddingTop = 10 };
                    //    PdfPCell cell1 = new PdfPCell(new Phrase(item.LastName.ToLower(), MainHeading)) { HorizontalAlignment = 1, BorderWidth = 0, PaddingTop = 10 };
                    //    table.AddCell(cell);
                    //    table.AddCell(cell1);

                    //    document.Add(table);
                    //    Response.ContentType = "application/pdf";
                    //    Response.AddHeader("Content-Disposition", "inline;filename=Certificate_" + fileName + ".pdf");
                    //    Response.Cache.SetCacheability(HttpCacheability.NoCache);
                    //    Response.Write(document);
                    //    Response.End();
                    //////////////////////////////////////////////////////////////////////////////////////////////
                }
            }





            ////////////////////////////////////////////////////////////



            return View();
        }


        public ActionResult HTML_TO_PDF()
        {
            return View();
        }


        public ActionResult Login()
        {
            /// Session["EmailID"] = null;
            if (Session["EmailID"] != null)
            {
                return RedirectToAction("AfterLogin", "User");

            }
            else
            {
                Session["EmailID"] = null;
                string ipaddress;
                ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (ipaddress == "" || ipaddress == null)
                    ipaddress = Request.ServerVariables["REMOTE_ADDR"];
                ViewBag.ipaddress = ipaddress;
                return View();
            }



        }

        [HttpPost]
        public ActionResult Login(FormCollection fc)
        {
            var EmailID = fc["EmailID"];
            var Password = fc["Password"];
            //string Pass= StringHelper.Encrypt(Password);



            var v = dc.Students.Where(a => a.EmailID == EmailID && a.Password == Password).FirstOrDefault();
            if (v != null)
            {


                var response = Request["g-recaptcha-response"];
                string secretKey = "6LdO8S4UAAAAAGc-qDKS0AS6SUa_6jGfryrFjHBt";
                var client = new WebClient();
                var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
                var obj = JObject.Parse(result);
                var status = (bool)obj.SelectToken("success");
                if (status == true)
                {
                    ViewBag.Message = status ? "Google reCaptcha validation success" : "Google reCaptcha validation failed";
                    Session["EmailID"] = EmailID;

                    return RedirectToAction("AfterLogin");
                }
                else
                {
                    ViewBag.captcha = "Invalid Recaptcha";
                    return View();
                }


            }
            else
            {
                ViewBag.Invalid = "You have Enter Wrong Username or pasword";

                return View();
            }


        }

        public ActionResult AfterLogin()
        {

            if (Session["EmailID"] == null)
            {

                return RedirectToAction("Login", "User");

            }
            else
            {
                string mail = Session["EmailID"].ToString();
                if (DateTime.Now.Hour < 12)
                {
                    ViewBag.wish = "Good morning";
                }
                else if (DateTime.Now.Hour < 17)
                {
                    ViewBag.wish = "Good Afternoon";

                }
                else
                {
                    ViewBag.wish = "Good Evening";

                }


                string ipaddress;
                ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (ipaddress == "" || ipaddress == null)
                    ipaddress = Request.ServerVariables["REMOTE_ADDR"];
                ViewBag.ipaddress = ipaddress;


                ViewBag.User = dc.Students.Where(x => x.EmailID.Equals(mail)).ToList();
                var UserID = dc.Students.Where(x => x.EmailID.Equals(mail)).Select(x => x.ID).FirstOrDefault();
                ViewBag.ID = UserID;
                ViewBag.info = dc.AllData(UserID).ToList();
                ViewBag.Name = dc.Students.Where(x => x.EmailID.Equals(mail)).Select(x => x.FirstName).FirstOrDefault();
                ViewBag.LastName = dc.Students.Where(x => x.EmailID.Equals(mail)).Select(x => x.LastName).FirstOrDefault();
                var ID = dc.Bios.Where(x => x.ID.Equals(UserID)).Select(x => x.ID).FirstOrDefault();
                var ProfileID = Convert.ToInt64(ID);


                ViewBag.ProfileComplete = dc.ProfileCompleted(ProfileID).FirstOrDefault();



                return View();
            }


        }


        public ActionResult DropDown()
        {
            var list = cg.Countries.ToList();
            ViewBag.Country = list;
            // ViewBag.CountryList = new SelectList(list, "CountryId", "CountryName");
            return View();
        }

        [HttpPost]
        public ActionResult DropDown(FormCollection fc)
        {


            return View();
        }

        public JsonResult GetStateList(int CountryId)
        {
            cg.Configuration.ProxyCreationEnabled = false;

            List<State> StateList = cg.States.Where(x => x.CountryId == CountryId).ToList();
            return Json(StateList, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Contactus()
        {


            return View();
        }


        [HttpPost]
        public ActionResult Contactus(FormCollection fc)
        {


            return View();
        }

        public ActionResult Edit(int id)
        {
            if (Session["EmailID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                var result = dc.Students.Where(a => a.ID == id).ToList();
                ViewBag.data = result;


                return View();
            }

        }
        [HttpPost]
        public ActionResult Edit(FormCollection fc)
        {
            var ID = Convert.ToInt32(fc["ID"]);
            var FirstName = fc["FName"];
            var LastName = fc["LName"];
            var EmailID = fc["EmailID"];
            var DateOfBirth = Convert.ToDateTime(fc["DOB"]);
            var Password = fc["Password"];

            var result = dc.SPUpdate(ID, FirstName, LastName, EmailID, DateOfBirth, Password);


            return RedirectToAction("List");
        }


        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();


            return RedirectToAction("Login");
        }

        public ActionResult ChangePassword()
        {
            if (Session["EmailID"] == null)
            {

                return RedirectToAction("Login", "User");

            }
            else
            {
                ViewBag.Email = Session["EmailID"];
                return View();
            }
        }

        [HttpPost]
        public ActionResult ChangePassword(FormCollection fc)
        {

            if (Session["EmailID"] == null)
            {

                return RedirectToAction("Login", "User");

            }
            else
            {
                string mail = Session["EmailID"].ToString();



                ViewBag.pass = dc.Students.Where(a => a.EmailID.Equals(mail)).FirstOrDefault();



                return View();

            }



        }
        
        public ActionResult Instruction()
        {

            return View();
        }


        public ActionResult Ajax()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Ajax(string FName, string LName, string EmailID, DateTime DOB, string password)
        {

            var FirstName = FName;
            var LastName = LName;
            var Emailid = EmailID;
            var DateOfBirth = DOB;
            var Password = password;
            var result = dc.SPInsert(FirstName, LastName, EmailID, DateOfBirth, Password);




            return RedirectToAction("Login");
        }

        public ActionResult Bio()
        {
            if (Session["EmailID"] == null)
            {
                return RedirectToAction("Login");
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        public ActionResult Bio(FormCollection fc)
        {
            var Address = fc["Address"];
            var PhoneNumber = Convert.ToInt64(fc["PhoneNumber"]);

            var file = Request.Files["Certificate"];

            string filename = file.FileName;

            string imgpath = "/Image/" + filename;



            file.SaveAs(Server.MapPath(imgpath));

            dc.BioInsert(Address, PhoneNumber, imgpath);
            return RedirectToAction("BioData");
        }



        public ActionResult BioData(int? id)
        {

            if (Session["EmailID"] == null)
            {
                return RedirectToAction("Login");

            }
            else
            {
                string mail = Session["EmailID"].ToString();

                var ID = dc.Students.Where(a => a.EmailID.Equals(mail)).Select(a => a.ID).FirstOrDefault();


                var AllData = dc.AllData(ID).ToList();
                ViewBag.AllData = AllData;

                return View();
            }
        }


        public ActionResult Doc()
        {
            return View();
        }

        public ActionResult search()
        {


            return View();
        }

        [HttpPost]
        public ActionResult search(FormCollection fc)
        {
            var search = fc["search"];


            Session["search"] = dc.SP_Search(search).ToList();
            return RedirectToAction("search_show");
        }

        public ActionResult search_show()
        {
            if (Session["search"] != null)
            {


                ViewBag.search = Session["search"];
                return View();
            }
            else
            {
                return Content("Sorry No records found!!");
            }


        }

        public ActionResult LogtoUser(int id)
        {
            var mail = dc.Students.Where(x => x.ID == id).Select(x => x.EmailID).FirstOrDefault();
            if (mail != null)
            {
                Session["EmailID"] = mail;
                return RedirectToAction("AfterLogin");
            }
            else
            {
                return View();
            }
        }

        public ActionResult OTP()
        {
            return View();
        }

        [HttpPost]
        public ActionResult OTP(FormCollection fc)
        {

            string num = "0123456789";
            int len = num.Length;
            string otp = string.Empty;
            int otpdigit = 5;
            string finaldigit;
            int getIndex;
            for (int i = 0; i < otpdigit; i++)
            {
                do
                {
                    getIndex = new Random().Next(0, len);
                    finaldigit = num.ToCharArray()[getIndex].ToString();
                }
                while (otp.IndexOf(finaldigit) != -1);
                {
                    otp += finaldigit;
                    ViewBag.otp = otp;


                }



            }
            ViewBag.final = ViewBag.otp;


            return View();
        }


        public ActionResult Asterik_Conversion()
        {
            string s = "Nilachal01@gmail.com";

            string Name = dc.Students.Where(x => x.ID == 5).Select(x => x.EmailID).FirstOrDefault().ToString();

            string result2 = s.Substring(s.Length - 13).PadLeft(s.Length, '*');

            string Asterik_Name = Name.Substring(Name.Length - 13).PadLeft(Name.Length, '*');


            return View();
        }





        public ActionResult ADMIN()
        {
            Session["EmailID"] = null;

            return View();
        }
        [HttpPost]
        public ActionResult ADMIN(FormCollection fc)
        {
            var UserName = fc["UserName"];
            var Password = fc["Password"];
            if (UserName == "Neel01@gmail.com" && Password == "898989")
            {
                return RedirectToAction("search");

            }
            else
            {
                ViewBag.Error = "Your UserName or password is wrong";
                return View();
            }



        }


        public ActionResult Text_To_Speech()
        {


            return View();
        }

        [HttpPost]
        public ActionResult Text_To_Speech(FormCollection fc)
        {

            var text = fc["Speech"];
            var speech = new SpeechSynthesizer();
            speech.SpeakAsync(text);
            speech.SetOutputToDefaultAudioDevice();


            using (SpeechSynthesizer s = new SpeechSynthesizer())
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    s.SetOutputToWaveStream(ms);
                    s.Speak(text);

                }
            }
            return View();
        }


        public ActionResult Chat()
        {

            return View();
        }
        [HttpPost]
        public ActionResult Chat(FormCollection fc)
        {
            var message = fc["message"];
            TempData["message"] = message;

            return Content(message);


        }


        public ActionResult AjaxChat()
        {
            return View();
        }

        public ActionResult StartChat(string Name)
        {
            Session["User"] = Name;
            return View("AjChat");
        }

        public ActionResult AjChat()
        {




            return PartialView();
        }

        //[HttpPost]
        //public ActionResult AjChat(string msg)
        //{
        //    Message message = new Message()
        //    {
        //        Name = Session["User"] as string,
        //        Time = DateTime.Now,
        //        Content = msg




        //    };

        //    return PartialView("Message");
        //}



        public ActionResult Ajaxsubmit()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Ajaxsubmit(FormCollection fc)
        {

            return PartialView("view1");
        }


        public ActionResult Address()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Address(FormCollection fc)
        {
            string mail = Session["EmailID"].ToString();
            var Presentaddress = fc["Presentaddress"];
            var Permanentaddress = fc["Permanentaddress"];
            var ID = dc.Students.Where(x => x.EmailID == mail).Select(x => x.ID).FirstOrDefault().ToString();
            var id = Convert.ToInt64(ID);

            var updateAddress = dc.AddressUpdate(id, Permanentaddress, Presentaddress);


            return RedirectToAction("AfterLogin");
        }


        public ActionResult Test()
        {
            return View();
        }



        //public ActionResult Angular()
        //{
        //    return View();
        //}


        public ActionResult DataTable()
        {

            //var Details = dc.Students.ToList();

            var Details = dc.StringAll();
            //var DateOfBirth = Convert.ToDateTime(dc.Students.Select(x=>x.DateOfBirth));
            ViewBag.model = Details;

            return Json(Details, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Table()
        {
            return View();
        }

        public ActionResult TextToSpeech()
        {
            return View();
        }

        [HttpPost]
        public ActionResult TextToSpeech(FormCollection fc)
        {
            var text = fc["Text"];


            //////////////////////////
            var mp3Stream = new MemoryStream();
            //Speech format
            var speechAudioFormatConfig = new SpeechAudioFormatInfo(samplesPerSecond: 8000, bitsPerSample: AudioBitsPerSample.Sixteen, channel: AudioChannel.Stereo);
            //Naudio's wave format used for mp3 conversion. Mirror configuration of speech config.
            var waveFormat = new WaveFormat(speechAudioFormatConfig.SamplesPerSecond, speechAudioFormatConfig.BitsPerSample, speechAudioFormatConfig.ChannelCount);
            try
            {
                //Build a voice prompt to have the voice talk slower and with an emphasis on words
                var prompt = new PromptBuilder { Culture = CultureInfo.CreateSpecificCulture("en-US") };
                prompt.StartVoice(prompt.Culture);
                prompt.StartSentence();
                prompt.StartStyle(new PromptStyle() { Emphasis = PromptEmphasis.Reduced, Rate = PromptRate.Slow });
                prompt.AppendText(text);
                prompt.EndStyle();
                prompt.EndSentence();
                prompt.EndVoice();

                //Wav stream output of converted text to speech
                using (var synthWavMs = new MemoryStream())
                {
                    //Spin off a new thread that's safe for an ASP.NET application pool.
                    var resetEvent = new ManualResetEvent(false);
                    ThreadPool.QueueUserWorkItem(arg =>
                    {
                        try
                        {
                            //initialize a voice with standard settings
                            var siteSpeechSynth = new SpeechSynthesizer();
                            //Set memory stream and audio format to speech synthesizer
                            siteSpeechSynth.SetOutputToAudioStream(synthWavMs, speechAudioFormatConfig);
                            //build a speech prompt
                            siteSpeechSynth.Speak(prompt);
                        }
                        catch (Exception ex)
                        {
                            //This is here to diagnostic any issues with the conversion process. It can be removed after testing.
                            Response.AddHeader("EXCEPTION", ex.GetBaseException().ToString());
                        }
                        finally
                        {
                            resetEvent.Set();//end of thread
                        }
                    });
                    //Wait until thread catches up with us
                    WaitHandle.WaitAll(new WaitHandle[] { resetEvent });
                    //Estimated bitrate
                    var bitRate = (speechAudioFormatConfig.AverageBytesPerSecond * 8);
                    //Set at starting position
                    synthWavMs.Position = 0;
                    //Be sure to have a bin folder with lame dll files in there.  They also need to be loaded on application start up via Global.asax file
                    using (var mp3FileWriter = new LameMP3FileWriter(outStream: mp3Stream, format: waveFormat, bitRate: bitRate))
                        synthWavMs.CopyTo(mp3FileWriter);
                }
            }
            catch (Exception ex)
            {
                Response.AddHeader("EXCEPTION", ex.GetBaseException().ToString());
            }
            finally
            {
                //Set no cache on this file
                Response.Cache.SetExpires(DateTime.UtcNow.AddMinutes(-1));
                Response.Cache.SetCacheability(HttpCacheability.NoCache);
                Response.Cache.SetNoStore();
                //required for chrome and safari
                Response.AppendHeader("Accept-Ranges", "bytes");
                //Write the byte length of mp3 to the client
                Response.AddHeader("Content-Length", mp3Stream.Length.ToString(CultureInfo.InvariantCulture));
            }
            //return the converted wav to mp3 stream to a byte array for a file download
            return View();
        }

        /////////////////////////


        public ActionResult Recaptcha()
        {
            return View();
        }



        [HttpPost]
        public ActionResult FormSubmit()
        {

            var response = Request["g-recaptcha-response"];
            string secretKey = "6LdO8S4UAAAAAGc-qDKS0AS6SUa_6jGfryrFjHBt";
            var client = new WebClient();
            var result = client.DownloadString(string.Format("https://www.google.com/recaptcha/api/siteverify?secret={0}&response={1}", secretKey, response));
            var obj = JObject.Parse(result);
            var status = (bool)obj.SelectToken("success");
            ViewBag.Message = status ? "Google reCaptcha validation success" : "Google reCaptcha validation failed";

            return View("Recaptcha");
        }

        public ActionResult Action()
        {
            return View();
        }

        public ActionResult Partialview()
        {


            return View();
        }

        public ActionResult Show()
        {
            ViewBag.Data = dc.Students.ToList();
            return View();
        }

        public ActionResult Example_Ajax()
        {
            return View();
        }
        public ActionResult All()
        {
            System.Threading.Thread.Sleep(2000);
            List<tblStudent> model = dc.tblStudents.ToList();
            ViewBag.data = model;
            return PartialView("_Student", model);
        }

        public ActionResult Top3()
        {
            System.Threading.Thread.Sleep(2000);
            List<tblStudent> model = dc.tblStudents.OrderByDescending(x => x.TotalMarks).Take(3).ToList();
            return PartialView("_Student", model);
        }

        public ActionResult Buttom3()
        {
            System.Threading.Thread.Sleep(2000);
            List<tblStudent> model = dc.tblStudents.OrderBy(x => x.TotalMarks).Take(3).ToList();
            return PartialView("_Student", model);
        }

        public ActionResult SearchAjax()
        {
            return View(dc.Students);
        }

        [HttpPost]
        public ActionResult SearchAjax(string Search)
        {
            var Details = dc.Students.Where(x => x.FirstName.StartsWith(Search) || x.LastName.StartsWith(Search)).ToList();
            return View(Details);
        }
        [HttpPost]
        public JsonResult Ajaxsearch(string Search)
        {
            var Details = dc.Students.Where(x => x.FirstName.StartsWith(Search)).Select(x => x.FirstName).ToList();

            return Json(Details, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Test1()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Testsearch(string Search)
        {
            var Details = dc.Students.Where(x => x.FirstName.StartsWith(Search)).Select(x => x.FirstName).ToList();

            return Json(Details, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Demo()
        {
            return View();
        }

        public ActionResult Accordion()
        {
            return View();
        }

        public ActionResult EXCEL()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ExcelCheck(FormCollection fc)
        {
            var EmailID = fc["EmailID"];
            int Marks_Obtained, Maximum_Marks;
            string status, uid, Emailid;

            DataSet ds = new DataSet();
            if (Request.Files["score"].ContentLength > 0)
            {
                string fileExtension = System.IO.Path.GetExtension(Request.Files["score"].FileName);
                if (fileExtension == ".xls" || fileExtension == ".xlsx")
                {
                    string fileLocation = Server.MapPath("~/Content/score/") + "Graduate Test Score_" + DateTime.Now.Ticks + fileExtension;//Ticks create a random number according to current data time
                    if (System.IO.File.Exists(fileLocation))
                    {
                        System.IO.File.Delete(fileLocation);
                    }
                    Request.Files["score"].SaveAs(fileLocation);
                    string excelConnectionString = string.Empty;

                    excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    //connection String for xls file format.
                    if (fileExtension == ".xls")
                    {
                        excelConnectionString = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + fileLocation + ";Extended Properties=\"Excel 8.0;HDR=Yes;IMEX=2\"";
                    }

                    //Create Connection to Excel work book and add oledb namespace
                    OleDbConnection excelConnection = new OleDbConnection(excelConnectionString);
                    excelConnection.Open();
                    DataTable dt = new DataTable();

                    dt = excelConnection.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    if (dt == null)
                    {
                        return null;
                    }

                    String[] excelSheets = new String[dt.Rows.Count];
                    int t = 0;

                    //excel data saves in temp file here.
                    foreach (DataRow row in dt.Rows)
                    {
                        excelSheets[t] = row["TABLE_NAME"].ToString();
                        t++;
                    }
                    OleDbConnection excelConnection1 = new OleDbConnection(excelConnectionString);

                    string query = string.Format("Select * from [{0}]", excelSheets[0]);
                    using (OleDbDataAdapter dataAdapter = new OleDbDataAdapter(query, excelConnection1))
                    {
                        dataAdapter.Fill(ds);
                    }
                    for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                    {
                        uid = ds.Tables[0].Rows[i]["USER ID"].ToString();
                        // mailuid=Convert.ToInt32(ds.Tables[0].Rows[i]["USER ID"].ToString());


                        if (!int.TryParse(ds.Tables[0].Rows[i]["Marks Obtained"].ToString(), out Marks_Obtained))
                            continue;

                        //Marks_Obtained = Convert.ToInt32(ds.Tables[0].Rows[i]["Marks Obtained"]);



                        status = ds.Tables[0].Rows[i]["Status(Y-Pass,N-Fail)"].ToString().ToLower();// Get the value of staus coloumn of excel
                        Emailid = ds.Tables[0].Rows[i]["Email ID"].ToString().ToLower();
                        if (!int.TryParse(ds.Tables[0].Rows[i]["Maximum Marks"].ToString(), out Maximum_Marks))
                            continue;

                        if (status == "Y" || status == "y")
                        {
                            var result = dc.IsActive1(Emailid);


                        }
                        else if (status == "N" || status == "n")
                        {
                            var result = dc.IsActive0(Emailid);
                            return Content("<script language='javascript' type='text/javascript'>alert('Data not Selected');</script>");
                        }
                        else
                        {
                            return Content("<script language ='javascript' type='text/javascript'> alert('Plese upload filled Certificate');</script >");
                        }
                    }


                    return Content("<script language='javascript' type='text/javascript'>alert('Data Selected');</script>");
                }
            }
            return View();
        }

        public ActionResult Certificate()
        {
            if (Session["EmailID"] != null)
            {
                string mail = Session["EmailID"].ToString();



                var ID = dc.BDskills.Where(a => a.EmailID.Equals(mail)).Select(a => a.ID).FirstOrDefault();




                ViewBag.Certificate = dc.datemonthyear(ID).ToList();
                return View();
            }

            else
            {
                return RedirectToAction("Login");
            }



        }


        public ActionResult ExtractText()
        {

            return View();
        }


        public ActionResult showlist()
        {
            List<string> CountryList = new List<string>();
            CultureInfo[] CInfoList = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
            foreach (CultureInfo CInfo in CInfoList)
            {
                RegionInfo R = new RegionInfo(CInfo.LCID);
                if (!(CountryList.Contains(R.EnglishName)))
                {
                    CountryList.Add(R.EnglishName);
                }
            }

            CountryList.Sort();
            ViewBag.CountryList = CountryList;

            return View();

        }
        [HttpPost]
        public ActionResult showlist(FormCollection fc)
        {
            var country = fc["CountryList"];
            return View();
        }

        [HttpGet]
        public ActionResult Auto()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Auto(string Prefix)
        {
            //Note : you can bind same list from database  
            //List<Employee> ObjList = new List<Employee>()
            //    {

            //        new Employee { Id=100, Name="Vithal Wadje" },
            //        new Employee { Id=200, Name="Sudir Wadje" },
            //        new Employee { Id=300, Name="Sachin W" },
            //        new Employee { Id=400, Name="Vikram N" },
            //        new Employee {Id=500, Name="Nilachal sethi" }


            //};
            ////Searching records from list using LINQ query  
            //var EmpDet = (from N in ObjList
            //              where N.Id.ToString().StartsWith(Prefix) || N.Name.ToLower().StartsWith(Prefix.ToLower())
            //              select new { N.Name, N.Id });

            var Details = dc.Students.Where(x => x.FirstName.StartsWith(Prefix)).ToList();


            return Json(Details, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Check()
        {
            ViewBag.data = 8;
            return View();
        }


        public ActionResult multiDrop()
        {
            ViewBag.Student = dc.Students.ToList();
            return View();
        }

        [HttpPost]
        public ActionResult multiDrop(FormCollection fc)
        {
            var value = fc["vegi"];
            string[] values = value.Split(',').Select(sValue => sValue.Trim()).ToArray();
            ViewBag.data = values;

            var vallength = values.Length;




            DataTable result = new DataTable();
            result.Clear();
            result.Columns.Add("ID");
            result.Columns.Add("FirstName");
            result.Columns.Add("LastName");
            result.Columns.Add("EmailID");
            result.Columns.Add("DateOfBirth");
            result.Columns.Add("PermanentAddress");
            result.Columns.Add("CurrentAddress");






            foreach (var item in values)
            {
                long ID = Convert.ToInt32(item);
                var results1 = dc.Students.Where(x => x.ID == ID).FirstOrDefault();

                DataRow dr = result.NewRow();
                dr["ID"] = item;
                dr["FirstName"] = results1.FirstName;
                dr["LastName"] = results1.LastName;
                dr["EmailID"] = results1.EmailID;
                dr["DateOfBirth"] = results1.DateOfBirth;
                dr["PermanentAddress"] = results1.PermanentAddress;
                dr["CurrentAddress"] = results1.CurrentAddress;

                result.Rows.Add(dr);
            }
            TempData["Result"] = result;






            //var result = dc.Students.Where(x => x.ID.Contai0ns(value)).ToList();
            var show = dc.Students.Where(x => x.FirstName.StartsWith(value)).ToList();
            //var data = dc.Students.Where(x => x.ID.Contains(Convert.ToInt64 (value))).Select(x => x.FirstName).ToList();
            ViewBag.value = result;

            var tt = values.FirstOrDefault();

            var First_Value = values[0];//Get the first value of array
            var Second_Value = values[1];//Get second value of array

            return RedirectToAction("Checkshow");
        }

        public ActionResult Checkshow()
        {
            ViewBag.data = TempData["Result"];
            return View();
        }


        public ActionResult Contact()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Contact(Contact contact)
        {
            if (ModelState.IsValid)
            {
                try
                {

                    MailMessage msz = new MailMessage();

                    string email = contact.EmailID;
                    msz.To.Add("Nilachal03@gmail.com");
                    msz.From = new MailAddress(email, "Convergent Technologies");

                    msz.Subject = contact.Subject;


                    msz.Body = "Problem in Admission process" + Environment.NewLine + "FROM   :" + email + Environment.NewLine + "Subject  :" + msz.Subject + Environment.NewLine
                              + "Message  :" + contact.Message;
                    SmtpClient smtp = new SmtpClient();

                    smtp.Host = "smtp.gmail.com";

                    smtp.Port = 587;

                    smtp.Credentials = new System.Net.NetworkCredential
                    ("Nilachal03@gmail.com", "9937902043");

                    smtp.EnableSsl = true;

                    smtp.Send(msz);

                    ModelState.Clear();
                    ViewBag.Message = "Thank you for Contacting us ";
                }
                catch (Exception ex)
                {
                    ModelState.Clear();
                    ViewBag.Message = $" Sorry we are facing Problem here {ex.Message}";
                }
            }



            ModelState.Clear();

            return RedirectToAction("SendMessage", "Home");
        }


        public ActionResult SendSMS()
        {
            return View();
        }
        [HttpPost]
        public ActionResult SendSMS(string phone, string message)
        {
            string AccountSid = "ACff9b909c26e33364889e2ac5fac81dc5";
            string Token = "0ca6ff2c9667f5ba658c86bd608ce4ab";
            //TwilioRestClient client = new TwilioRestClient(AccountSid, Token);
            //var twilio = new TwilioRestClient(AccountSid, Token);
            //string number = "+91" + phone;
            //var send = client.SendSmsMessage("+917787837988", number, message);


            TwilioClient.Init(AccountSid, Token);

            //var Message = MessageResource.Create(
            //    to: new PhoneNumber(phone),
            //    from: new PhoneNumber("+917787837988"),
            //    body: message);

            TempData["successful"] = "SMS send Successfully";
            return RedirectToAction("sendsms");
        }

        public ActionResult Chart()
        {
            return View();
        }

        public JsonResult Chartd()
        {
            var data = dc.Chart_tbl.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);

        }

        public ActionResult ReactFirst()
        {

            return View();
        }


        public ActionResult charr()
        {
            //long l = 0L;
            //int q = (int)-l;
            //int w = (char)1;
            //int i = -(int)+(long)-1;

            return View();
        }

        public ActionResult CheckReact()
        {
            return View();
        }

        public JsonResult reactdata()
        {
            var dt = dc.Students.ToList();
            return Json(dt, JsonRequestBehavior.AllowGet);
        }


        public ActionResult Feedback()
        {
            return View();
        }
        [HttpGet]
        public JsonResult GetFeedback()
        {
            var data = dc.FeedbackMessage();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Isactive0(string EmailID)
        {
            var IaActive_value = dc.IsActive0(EmailID);
            return RedirectToAction("Index", "Test");
        }

        public ActionResult PageTable(int?currentpage, string search)
        {
            
            if (search!=null)
            {
                var data = dc.Students.Where(x => x.FirstName.StartsWith((search).Trim()) || x.LastName.StartsWith((search).Trim()) || x.EmailID.StartsWith((search).Trim()) || x.PermanentAddress.StartsWith((search).Trim()) || x.CurrentAddress.StartsWith((search).Trim())).ToList();
                ViewBag.data = data;
                ViewBag.Totaldata = data.Count() / 10;
                ViewBag.currentpage = 1;

                return View();
            }
            if(currentpage==null)
            {
                ViewBag.currentpage = 1;
                var data = dc.Students.Count();
                ViewBag.Totaldata = data / 10;
                var result = dc.Students.Take(10);
                ViewBag.data = result;
                return View();
            }
            else
            {
                ViewBag.currentpage = currentpage;
                var data = dc.Students.Count();
                ViewBag.Totaldata = data/ 10;
                var result = dc.Students.OrderBy(x => x.ID).Skip(Convert.ToInt32(currentpage - 1)*10).Take(10).ToList();
                ViewBag.data = result;
                return View();

            }
                
            
            

            
        }
        [HttpPost]
        public ActionResult PageTable(int?currentpage, string search,FormCollection fc)
       {
            if (search != null)
            {
                var data1 = dc.Students.Where(x => x.FirstName.StartsWith(search) || x.LastName.StartsWith(search) || x.EmailID.StartsWith(search) || x.PermanentAddress.StartsWith(search) || x.CurrentAddress.StartsWith(search)).ToList();
                ViewBag.data1 = data1;
                ViewBag.Totaldata = data1.Count() / 10;
                ViewBag.currentpage = 1;
                return View();
            }
            else
            {
                ViewBag.currentpage = currentpage;
                var data = dc.Students.ToList();
                ViewBag.Totaldata = data.Count() / 10;
                var result = dc.Students.OrderBy(x => x.ID).Skip((Convert.ToInt32(currentpage) - 1) * 10).Take(10).ToList();
                ViewBag.data1 = result;
                
                return View();

            }
           
        }

        public ActionResult PageTable_IsActive0(string EmailID)
        {
            var IaActive_value = dc.IsActive0(EmailID);
            return RedirectToAction("PageTable", "User");
        }


        public JsonResult TimeAgocheck()
        {
            const int SECOND = 1;
            const int MINUTE = 60 * SECOND;
            const int HOUR = 60 * MINUTE;
            const int DAY = 24 * HOUR;
            const int MONTH = 30 * DAY;

            var ts = new TimeSpan(DateTime.UtcNow.Ticks - DateTime.Now.Ticks);
            double delta = Math.Abs(ts.TotalSeconds);

            if (delta < 1 * MINUTE)
             ViewBag.Time= "seconds ago";

            if (delta < 2 * MINUTE)
                ViewBag.Time = "a minute ago";

            if (delta < 45 * MINUTE)
                ViewBag.Time = " minutes ago";

            if (delta < 90 * MINUTE)
                ViewBag.Time = "an hour ago";

            if (delta < 24 * HOUR)
                ViewBag.Time = ts.Hours + " hours ago";

            if (delta < 48 * HOUR)
                ViewBag.Time = "yesterday";

            if (delta < 30 * DAY)
                ViewBag.Time = ts.Days + " days ago";

            if (delta < 12 * MONTH)
            {
                int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                ViewBag.Time = months <= 1 ? "one month ago" : months + " months ago";
            }
            else
            {
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                ViewBag.Time = years <= 1 ? "one year ago" : years + " years ago";
            }

            return Json(ViewBag.time,JsonRequestBehavior.AllowGet);
        }

        public ActionResult LanguageTest()
        {

            CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);

            ViewBag.culture = cultures;
            foreach (CultureInfo culture in cultures)
            {
                ViewBag.language = culture.EnglishName;
                ViewBag.ID = culture.LCID;
            }
            return View();
        }

        [HttpPost]
        public ActionResult LanguageTest(FormCollection fc)
        {
            var speak = fc["Speak"];
            var write = fc["Write"];
            var Read = fc["Read"];
            var Language = fc["Language"];
            return View();
        }

        public ActionResult JoinLinq()
        {
            var query = dc.Students.Join(dc.Bios, r => r.ID, p => p.ID, (r, p) => new { r.FirstName, r.LastName, p.Address,r.ID }).ToList();
            
        

            return View();
        }

        public ActionResult Location()
        {
            return View();
        }
        public ActionResult ReturnContent()
        {
            return View();
        }
        public HttpStatusCodeResult UnauthorizedStatusCode()
        {
            return new HttpStatusCodeResult(HttpStatusCode.Unauthorized, "You are not authorized to access this controller action.");
        }

        public HttpStatusCodeResult BadGateway()
        {
            return new HttpStatusCodeResult(HttpStatusCode.BadGateway, "I have no idea what this error means.");
        }



    }


}


