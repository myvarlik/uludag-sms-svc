using Microsoft.AspNetCore.Mvc;
using System.Drawing;

namespace uludag_sms_svc.Models
{
    public class ListModel
    {
        public int current { get; set; }
        public int pageSize { get; set; }
    }

    public class ResponseModel
    {
        public long total { get; set; }
        public bool success { get; set; }
        public object? data { get; set; }
    }
}
