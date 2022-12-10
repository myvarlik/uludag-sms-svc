using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace uludag_sms_svc.Models
{
    public class SMSModel
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? id { get; set; }
        public string siteAdi { get; set; } = null!;
        public string mesaj { get; set; } = null!;
        public string mesajBasligi { get; set; } = null!;
        public List<Kisiler>? kisiler { get; set; }
        public int durum { get; set; } = 0;
        public string refNo { get; set; } = null!;
        public string kullanilanKredi { get; set; } = null!;
        public string kalanKredi { get; set; } = null!;
        public string hata { get; set; } = null!;
        public DateTime eklenmeTarih { get; set; }
        public DateTime gonderilmeTarih { get; set; }

    }

    public class Kisiler
    {
        public string adi { get; set; } = null!;
        public string numara { get; set; } = null!;
    }
}