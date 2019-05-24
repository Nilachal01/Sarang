using Sarang.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Mvc;
using System.Xml.Linq;



namespace Sarang.Controllers
{

    public class TestController : Controller
    {
        IGITEntities dc = new IGITEntities();
        // GET: Test
        public ActionResult Index()
        {

            return View();
        }
        [HttpGet]
        public JsonResult StudentDetail()
        {
            var Data = dc.Student_data();
            return Json(Data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult JqueryPost()
        {
            return View();
        }

        public JsonResult SaveUser(TUser UserData)
        {
            int Userid = 0;
            try
            {
                if (UserData != null)
                {
                    using (IGITEntities db = new IGITEntities())
                    {
                        //saves the user data and returns userid from procedure.
                        var id = db.sp_InsertUser(UserData.Name, UserData.Email, UserData.PhoneNumber);
                           
                        Userid =Convert.ToInt32(id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(Userid, JsonRequestBehavior.AllowGet);
        }
        //saves Address data TAddress table
        public JsonResult SaveAddress(TAddress AddressData)
        {
            bool result = false;
            try
            {
                if (AddressData != null)
                {
                    using (IGITEntities db = new IGITEntities())
                    {
                        db.TAddresses.Add(AddressData);
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        //saves orderdata in TOrders table
        public JsonResult SaveOrders(TOrder OrdersData)
        {
            bool result = false;
            try
            {
                if (OrdersData != null)
                {
                    using (IGITEntities db = new IGITEntities())
                    {
                        db.TOrders.Add(OrdersData);
                        db.SaveChanges();
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        public ActionResult AjaxJquery()
        {
            return View();
        }
        public ActionResult Intelligence()
        {
            return View();
        }

        public ActionResult Post()
        {
            return View();
        }
        [HttpPost]
        public JsonResult Post1( string Details)
        {
            var details = dc.Students.ToList();
            return Json(details, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Calender()
        {
            return View();
        }

        public JsonResult GetEvents()
        {
          var events = dc.Events.ToList();
          return new JsonResult { Data = events, JsonRequestBehavior = JsonRequestBehavior.AllowGet };
            
        }


        public ActionResult check()
        {
            return View();
        }
        public ActionResult TimeAgo()
        {
            
            
            return View();
        }
        [HttpPost]
        public ActionResult TimeAgo(FormCollection fc)
        {
            string EmailID = Session["EmailID"].ToString();
            string Message = fc["Message"].ToString();
            var result = dc.Messageshow(EmailID, Message);
            ViewBag.Details = dc.Messages.Select(x => x.Message1).ToList();
            return View();
        }

        public ActionResult Timeline()
        {
            var Details = dc.Students.ToList();
            ViewBag.details = Details;
            return View();
        }
        //public JsonResult TimeAgocheck()
        //{
        //    const int SECOND = 1;
        //    const int MINUTE = 60 * SECOND;
        //    const int HOUR = 60 * MINUTE;
        //    const int DAY = 24 * HOUR;
        //    const int MONTH = 30 * DAY;

        //    var ts = new TimeSpan(DateTime.UtcNow.Ticks - yourDate.Ticks);
        //    double delta = Math.Abs(ts.TotalSeconds);

        //    if (delta < 1 * MINUTE)
        //        " seconds ago";

        //    if (delta < 2 * MINUTE)
        //        return "a minute ago";

        //    if (delta < 45 * MINUTE)
        //        " minutes ago";

        //    if (delta < 90 * MINUTE)
        //        return "an hour ago";

        //    if (delta < 24 * HOUR)
        //        return ts.Hours + " hours ago";

        //    if (delta < 48 * HOUR)
        //        return "yesterday";

        //    if (delta < 30 * DAY)
        //        return ts.Days + " days ago";

        //    if (delta < 12 * MONTH)
        //    {
        //        int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
        //        return months <= 1 ? "one month ago" : months + " months ago";
        //    }
        //    else
        //    {
        //        int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
        //        return years <= 1 ? "one year ago" : years + " years ago";
        //    }

        //    return View();
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult Angular()
        {
            if(Session["EmailID"]!=null)
            {
                return View();
            }
            return RedirectToAction("Login", "User");
        }

        public ActionResult ExportAngular()
        {
            if (Session["EmailID"] == null)
            {
                return RedirectToAction("Login", "User");
            }
            else
            {


                var TotalData = dc.Angulars.ToList();
                
                int c = 0;
                StringBuilder Ex = new StringBuilder();


                if (TotalData != null && TotalData.Any())
                {

                    Ex.Append("<table  border='1' style='border:4px solid black; font-size:12px;'>");
                    Ex.Append("<tr>");
                    Ex.Append("<th align='center'  style='width:40px;background-color:#fc9003;'><b>Sr. No.</b></th>");
                    Ex.Append("<th align='center'  style='width:40px;background-color:#fc9003;'><b>ID</b></th>");
                    Ex.Append("<th align='center'  style='width:50px;background-color:#fc9003;'><b>Name</b></th>");
                    Ex.Append("<th align='center'  style='width:60px;background-color:#fc9003;'><b>PhoneNumber</b></th>");
                    Ex.Append("<th align='center'  style='width:120px;background-color:#fc9003;'><b>Department</b></th>");
                    Ex.Append("<th align='center'  style='width:100px;background-color:#fc9003;'><b>Salary</b></th>");
                    Ex.Append("<th align='center'  style='width:150px;background-color:#fc9003;'><b>Email</b></th>");
                   

                    Ex.Append("</tr>");


                    foreach (var item in TotalData)
                    {
                        Ex.Append("<tr>");
                        Ex.Append("<td align='left'  style='width:40px;height:50px;'>" + (++c) + "</td>");
                        Ex.Append("<td align='left' style='width:40px;height:50px;'>" + item.ID + "</td>");
                        Ex.Append("<td align='left' style='width:50px;height:50px;'>" + item.Name + "</td>");
                        Ex.Append("<td align='left' style='width:60px;height:50px;'>" + item.Phone + "</td>");
                        Ex.Append("<td align='left' style='width:120px;height:50px;'>" + item.Department + "</td>");
                        Ex.Append("<td align='left' style='width:100px;height:50px;'>" + item.Salary + "</td>");
                        Ex.Append("<td align='left' style='width:150px;height:50px;'>" + item.EmailID + "</td>");
                        
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

        public ActionResult AngularValidation()
        {
            return View();
        }
        public JsonResult GetEmployee()
        {
            var data = dc.Angulars.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult InsertEmployee(FormCollection emp)
        {

            var Name = emp["Name"];
            var Phone = Convert.ToInt64(emp["Phone"]);
            var Department = emp["Department"];
            var Salary = Convert.ToSingle(emp["Salary"]);
            var Email = emp["Email"];
            var Insert = dc.Angular_Insert(Name, Phone, Department, Salary, Email);
           
            return new JsonResult { JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }

        [HttpPost]
        public JsonResult UpdateEmployee(FormCollection emp)
        {
            int ID =Convert.ToInt32(emp["ID"]);
            var Name = emp["Name"];
            var Phone = Convert.ToInt64(emp["Phone"]);
            var Department = emp["Department"];
            var Salary = Convert.ToSingle(emp["Salary"]);
            var Email = emp["Email"];

            var result = dc.Angular_Update(ID, Name, Phone, Department, Salary, Email);
            return Json(result, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public JsonResult DeleteEmployee(string EmailID)
        {
            
            var delete = dc.Angular_Delete(EmailID);
            return Json(delete, JsonRequestBehavior.AllowGet);
        }
        [HttpGet]
        public JsonResult getByid(int id)
        {

            var result = dc.Angulars.Where(x=>x.ID==id).FirstOrDefault();
                    
            return Json(result, JsonRequestBehavior.AllowGet);
        }

        


        public ActionResult postdata1()
        {
            return View();
        }
       [HttpPost]
        public JsonResult Postdata(FormCollection fc)
        {
            var FirstName = fc["fname"];
            var LastName = fc["lname"];
            var details = dc.Angulars.ToList();
            return Json(details,JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetIPAddress()
        {
            string ipaddress;
            ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (ipaddress == "" || ipaddress == null)
                ipaddress = Request.ServerVariables["REMOTE_ADDR"];
            ViewBag.ipaddress = ipaddress;
            return View();
        }

        public ActionResult CreateTextFile()
        {
            return View();
        }

        [HttpPost]
        public ActionResult CreateTextFile(string message)
        {
            string path = Server.MapPath("~/Neel.txt");
            using (StreamWriter sw = System.IO.File.CreateText(path))
            {
                sw.WriteLine(message);
            }
            return File(path, "text/plain", "Neel.txt");
        }


        public ActionResult Showreact()
        {
            return View();
        }

        public JsonResult getdata()
        {
            var data = dc.Students.ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SendBulkMail(string toEmailID, string fromEmailID, string FromName, string mailSubject, string mailBody)
        {

            if (fromEmailID == null)
                fromEmailID = "nilachal03@gmail.com";
            if (FromName == null)
                FromName = "Nila";
            mailSubject = "test";
            mailBody = "mail successfull";
            toEmailID = "nilachal01@gmail.com";

            
            System.Net.Mail.MailAddress from = new System.Net.Mail.MailAddress(fromEmailID, FromName);
            System.Net.Mail.MailMessage message = new System.Net.Mail.MailMessage();
            message.From = from;
            message.To.Add(toEmailID);
            //message.Bcc.Add("neel@gmail.com");

            AlternateView htmlView = AlternateView.CreateAlternateViewFromString(mailBody, null, "text/html");
            LinkedResource imagelink = new LinkedResource(System.Web.HttpContext.Current.Server.MapPath("~\\Image") + "/2.jpg", "image/jpg");
            imagelink.ContentId = "LogoImage";
            htmlView.LinkedResources.Add(imagelink);
            message.AlternateViews.Add(htmlView);

            message.Subject = mailSubject;
            message.Body = mailBody;
            message.IsBodyHtml = true;
            message.Priority = System.Net.Mail.MailPriority.High;
            SmtpClient sendmail = new SmtpClient();

            //System.Net.NetworkCredential SMTPUserInfo = new System.Net.NetworkCredential("iphone.partnerconnect@gmail.com", "iPhone@partnerconnect");
            sendmail.UseDefaultCredentials = false;
            sendmail.Credentials = new NetworkCredential("Nilachal03@gmail.com", "9937902043");
            sendmail.Port = 587; ;
            sendmail.EnableSsl = true;
            sendmail.Host = "smtp.gmail.com";
            sendmail.Send(message);
            return View();

        }

        public ActionResult Pagination()
        {
            //page = 1;
            //var pagedata = page * 10;

            

            var data = dc.Students.OrderByDescending(x => x.ID).ToList();
            ViewBag.data = data;
            //ViewBag.datacount = data.Count();
           
            return View();
        }
        public JsonResult data(int?page)
        {
            page = ++page;

            var data = dc.Students.Where(x=>x.ID>=page).OrderByDescending(x => x.ID).ToList();
            return Json(data, JsonRequestBehavior.AllowGet);
        }


        public ActionResult testMulti()
        {
            ViewBag.Student = dc.Students.ToList();
            return View();
        }


        public ActionResult DataTableCheck()
        {
            var Result = dc.Students.ToList();

            DataTable dt = new DataTable();
            DataColumn dc1 = new DataColumn("ID");
            dt.Columns.Add(dc1);
            dc1 = new DataColumn("FirstName");
            dt.Columns.Add(dc1);
            dc1 = new DataColumn("LastName");
            dt.Columns.Add(dc1);
            dc1 = new DataColumn("EmailID");
            dt.Columns.Add(dc1);
            dc1 = new DataColumn("PermanentAddress");
            dt.Columns.Add(dc1);
            dc1 = new DataColumn("CurrentAddress");
            dt.Columns.Add(dc1);

            DataRow dr;
            foreach (var item in Result)
            {
                dr = dt.NewRow();
                dr["ID"] = item.ID;
                dr["FirstName"] = item.FirstName;
                dr["LastName"] = item.LastName;
                dr["EmailID"] = item.EmailID;
                dr["PermanentAddress"] = item.PermanentAddress;
                dr["CurrentAddress"] = item.CurrentAddress;



                dt.Rows.Add(dr);
            }
            var Result1 = dt.AsEnumerable().ToList();
            ViewBag.TotalData = Result1;
            ViewBag.TotalResult = Result1.Count();
            return View();
        }

        public ActionResult ReduceImage()
        {
            return View();
        }
        [HttpPost]
        public ActionResult ReduceImage(FormCollection fc)
        {
            foreach (string item in Request.Files)
            {
                HttpPostedFileBase file = Request.Files[item] as HttpPostedFileBase;
                if (file.ContentLength == 0)
                    continue;

                if (file.ContentLength > 0)
                {
                    // width + height will force size, care for distortion
                    //Exmaple: ImageUpload imageUpload = new ImageUpload { Width = 800, Height = 700 };

                    // height will increase the width proportially 
                    //Example: ImageUpload imageUpload = new ImageUpload { Height= 600 };

                    // width will increase the height proportially 
                    ImageUpload imageUpload = new ImageUpload { Width = 600 };

                    // rename, resize, and upload 
                    //return object that contains {bool Success,string ErrorMessage,string ImageName}
                    ImageResult imageResult = imageUpload.RenameUploadFile(file);

                    if (imageResult.Success)
                    {
                        //TODO: write the filename to the db
                        Console.WriteLine(imageResult.ImageName);
                    }
                    else
                    {
                        //TODO: show view error
                        // use imageResult.ErrorMessage to show the error
                        ViewBag.Error = imageResult.ErrorMessage;
                    }
                }
            }

            return View();
        }

        public ActionResult TableDrop()
        {
            var Data = dc.Students.ToList();
            ViewBag.List = Data;
            return View();
        }



        

            public ActionResult convertor()

            {

                string sourceFile, worksheetName, targetFile;

                sourceFile = "Batch.xls"; worksheetName = "sheet1"; targetFile = "target.csv";

                convertExcelToCSV(sourceFile, worksheetName, targetFile);
            return Content("done");

            }


            static void convertExcelToCSV(string sourceFile, string worksheetName, string targetFile)

            {

                string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=" + sourceFile + ";Extended Properties=\" Excel.0;HDR=Yes;IMEX=1\"";

                OleDbConnection conn = null;

                StreamWriter wrtr = null;

                OleDbCommand cmd = null;

                OleDbDataAdapter da = null;

                try

                {

                    conn = new OleDbConnection(strConn);

                    conn.Open();



                    cmd = new OleDbCommand("SELECT * FROM [" + worksheetName + "$]", conn);

                    cmd.CommandType = CommandType.Text;

                    wrtr = new StreamWriter(targetFile);



                    da = new OleDbDataAdapter(cmd);

                    DataTable dt = new DataTable();

                    da.Fill(dt);



                    for (int x = 0; x < dt.Rows.Count; x++)

                    {

                        string rowString = "";

                        for (int y = 0; y < dt.Columns.Count; y++)

                        {

                            rowString += "\"" + dt.Rows[x][y].ToString() + "\",";

                        }

                        wrtr.WriteLine(rowString);

                    }

                    Console.WriteLine();

                    Console.WriteLine("Done! Your " + sourceFile + " has been converted into " + targetFile + ".");

                    Console.WriteLine();

                }

                catch (Exception exc)

                {

                    Console.WriteLine(exc.ToString());

                    Console.ReadLine();

                }

                finally

                {

                    if (conn.State == ConnectionState.Open)

                        conn.Close();

                    conn.Dispose();

                    cmd.Dispose();

                    da.Dispose();

                    wrtr.Close();

                    wrtr.Dispose();

                }

            }
        public ActionResult ExcelCheck()
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
                        string targetFile = "~/Content/score/neel.csv";
                        convertExcelToCSV(Request.Files["score"].FileName, excelSheets.ToString(), targetFile);

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







    }
    }

