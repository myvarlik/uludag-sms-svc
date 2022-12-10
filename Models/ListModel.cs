using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace uludag_sms_svc.Models
{
    public class ListModel
    {
        public int offset { get; set; }
        public int listPerPage { get; set; }
        public string filterData { get; set; }
    }
}
