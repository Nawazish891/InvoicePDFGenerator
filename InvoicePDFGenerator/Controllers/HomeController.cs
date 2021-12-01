using InvoicePDFGenerator.DB;
using InvoicePDFGenerator.Models;
using Microsoft.Reporting.WebForms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace InvoicePDFGenerator.Controllers
{
    public class HomeController : Controller
    {
        private Inv_TestingEntities m_db = new Inv_TestingEntities();
        string[] arrHEX = { "0", "01", "02", "03", "04", "05", "06", "07", "08", "09", "0a", "0b", "0c", "0d", "0e", "0f" };
        public ActionResult Index()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public FileResult Generate([Bind(Include = "InvoiceNumber")] InvoiceModel invoice)
        {
            var inv = m_db.AR_HEADER.Where(x => x.InvoiceNum == invoice.InvoiceNumber);

            if (inv != null)
            {
                //string tag = arrHEX[1];
                //string length = "";
                //if (inv.Name.Length <= 15)
                //    length = arrHEX[inv.Name.Length];
                //else
                //    length = BitConverter.ToString(Encoding.Default.GetBytes(inv.Name.Length.ToString())).Replace("-", "");

                //string value = BitConverter.ToString(Encoding.Default.GetBytes(inv.Name)).Replace("-", "");

                //string tlvHex = tag + length + value;

                LocalReport report = new LocalReport();
                report.DataSources.Clear();
                report.DataSources.Add(new ReportDataSource("DataSet1", inv));
                report.ReportPath = Server.MapPath("~/Report/Invoice.rdlc");
                byte[] bytes = report.Render("PDF");
                return File(bytes, "application/pdf", "inovice-pdf.pdf");
            }
            byte[] bytes2 = { };
            return File(bytes2, "application/pdf", "inovice-pdf.pdf");
        }

        #region ---- Dispose ----
        /// <summary>
        /// Disposing DB Context
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                m_db.Dispose();
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}