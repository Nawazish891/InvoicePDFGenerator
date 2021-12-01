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
                var inv1 = inv.FirstOrDefault();

                if (!string.IsNullOrEmpty(inv1.TIN))
                {
                    string tag = arrHEX[1];
                    string length = "";
                    if (inv1.Name.Length <= 15)
                        length = arrHEX[inv1.Name.Length];
                    else
                        length = BitConverter.ToString(Encoding.Default.GetBytes(inv1.Name.Length.ToString())).Replace("-", "");
                    string value = BitConverter.ToString(Encoding.Default.GetBytes(inv1.Name)).Replace("-", "");
                    string tlvHex = tag + length + value;

                    tag = arrHEX[2];
                    if (inv1.TIN.Length <= 15)
                        length = arrHEX[inv1.TIN.Length];
                    else
                        length = BitConverter.ToString(Encoding.Default.GetBytes(inv1.TIN.Length.ToString())).Replace("-", "");

                    value = BitConverter.ToString(Encoding.Default.GetBytes(inv1.TIN)).Replace("-", "");
                    tlvHex += tag + length + value;

                    tag = arrHEX[3];
                    inv1.InvoiceDate = new DateTime(inv1.InvoiceDate.Value.Year, inv1.InvoiceDate.Value.Month, inv1.InvoiceDate.Value.Day, 14, 0, 0);
                    string timeStamp = inv1.InvoiceDate.GetValueOrDefault().ToString("yyyy-MM-ddTHH:mm:ss");
                    if (timeStamp.Length <= 15)
                        length = arrHEX[timeStamp.Length];
                    else
                        length = BitConverter.ToString(Encoding.Default.GetBytes(timeStamp.Length.ToString())).Replace("-", "");
                    value = BitConverter.ToString(Encoding.Default.GetBytes(timeStamp)).Replace("-", "");
                    tlvHex += tag + length + value;

                    tag = arrHEX[4];
                    if (inv1.InvoiceAmt.ToString().Length <= 15)
                        length = arrHEX[inv1.InvoiceAmt.ToString().Length];
                    else
                        length = BitConverter.ToString(Encoding.Default.GetBytes(inv1.InvoiceAmt.ToString().Length.ToString())).Replace("-", "");
                    value = BitConverter.ToString(Encoding.Default.GetBytes(inv1.InvoiceAmt.ToString())).Replace("-", "");
                    tlvHex += tag + length + value;

                    tag = arrHEX[5];
                    double taxRate = 0;
                    var invdet = m_db.AR_DETAILS.Where(x => x.InvoiceNum == invoice.InvoiceNumber);
                    if(invdet != null && invdet.Count() > 0)
                        taxRate = invdet.FirstOrDefault().TaxRate.GetValueOrDefault();

                    taxRate = taxRate / 100;
                    string vatValue = Convert.ToString(Convert.ToDouble(inv1.InvoiceAmt.GetValueOrDefault()) * taxRate);
                    if (vatValue.Length <= 15)
                        length = arrHEX[vatValue.ToString().Length];
                    else
                        length = BitConverter.ToString(Encoding.Default.GetBytes(vatValue.Length.ToString())).Replace("-", "");
                    value = BitConverter.ToString(Encoding.Default.GetBytes(vatValue)).Replace("-", "");

                    tlvHex += tag + length + value;

                    var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(tlvHex);
                    var final = System.Convert.ToBase64String(plainTextBytes);
                }

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