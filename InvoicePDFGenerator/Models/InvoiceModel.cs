using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace InvoicePDFGenerator.Models
{
    public class InvoiceModel
    {
        [Required(ErrorMessage ="Please Enter Invoice Number.")]
        public int InvoiceNumber { get; set; }
    }
}