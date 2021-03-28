using EmailSending.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web.Mvc;
using ClosedXML.Excel;
using System.Web.UI.WebControls;
using System.IO;
using System.Data;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.tool.xml;




namespace EmailSending.Controllers
{
    public class MailController : Controller
    {


        ProjectDBContext _db = new ProjectDBContext();

        public IList<Mail> GetMailList()
        {
            var mailList = _db.mails.ToList();                      
                               
            return mailList;
        }

        public ActionResult Index()
        {
            return View(this.GetMailList());
        }

      
        [HttpPost]
        public FileResult ExportToExcel()
        {
            DataTable dt = new DataTable("Grid");
            dt.Columns.AddRange(new DataColumn[5] {
                                                     new DataColumn("MailTo"),
                                                     new DataColumn("MailFrom"),
                                                     new DataColumn("MailSubject"),
                                                     new DataColumn("MailBody"),                                                  
                                                     new DataColumn("Date&Time")});

            var MailList = from email in _db.mails select email;

            foreach (var m in MailList)
            {
                dt.Rows.Add(m.MailTo, m.MailFrom, m.MailSubject, m.MailBody, m.SendingDateTime);
            }

            using (XLWorkbook wb = new XLWorkbook()) //Install ClosedXml from Nuget for XLWorkbook  
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream stream = new MemoryStream()) //using System.IO;  
                {
                    wb.SaveAs(stream);
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ExcelFile.xlsx");
                }
            }
        }

        [HttpPost]
        [ValidateInput(false)]
        public FileResult Export(string GridHtml)
        {
            using (MemoryStream stream = new System.IO.MemoryStream())
            {
                StringReader sr = new StringReader(GridHtml);
                Document pdfDoc = new Document(PageSize.A4, 10f, 10f, 100f, 0f);
                PdfWriter writer = PdfWriter.GetInstance(pdfDoc, stream);
                pdfDoc.Open();
                XMLWorkerHelper.GetInstance().ParseXHtml(writer, pdfDoc, sr);
                pdfDoc.Close();
                return File(stream.ToArray(), "application/pdf", "PdfFile.pdf");
            }
        }



        public ActionResult SendEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendEmail(string receiver, string subject, string message)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var senderEmail = new MailAddress("shafiqul.cse2017@gmail.com", "Shafiqul");
                    var receiverEmail = new MailAddress(receiver, "Receiver");
                    var password = "shafiqul&60";
                    var sub = subject;
                    var body = message;

                    var smtp = new SmtpClient()
                    {
                        Host = "smtp.gmail.com",
                        Port = 587,
                        EnableSsl = true,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        UseDefaultCredentials = false,
                        Credentials = new NetworkCredential(senderEmail.Address, password)
                    };
                    using (var mess = new MailMessage(senderEmail, receiverEmail)
                    {
                        Subject = subject,
                        Body = body

                    })
                    {
                        smtp.Send(mess);
                    }
                    ViewBag.Suc = "Message Successfully Sent";

                    Mail email = new Mail();

                    email.MailTo = receiver;
                    email.MailFrom = "shafiqul.cse2017@gmail.com";
                    email.MailSubject = subject;
                    email.MailBody = message;
                    _db.mails.Add(email);
                    _db.SaveChanges();

                    return View();
                }
            }
            catch (Exception)
            {
                ViewBag.Error = "Message Successfully Sending Failed";
            }
            return View();

        }

    }
}