using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace EcomApplication.Models
{
    public class Mobiles
    {
        public int SLNo
        {
            get;
            set;
        }
        public string MobileName
        {
            get;
            set;
        }
        public decimal Price
        {
            get;
            set;
        }
        public int Quantity
        {
            get;
            set;
        }
        public string Description
        {
            get;
            set;
        }
        [DisplayName("Upload File")]
        public string PicURL
        {
            get;
            set;
        }

        public IFormFile ImageFile { get; set; }
    }
}
