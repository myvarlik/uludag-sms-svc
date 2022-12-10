using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace uludag_sms_svc.Models
{
    public class SMSStoreDatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;

        public string DatabaseName { get; set; } = null!;

        public string SMSCollectionName { get; set; } = null!;
    }
}