using MesajPaneli.Business;
using MesajPaneli.Models.JsonPostModels;
using MesajPaneli.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using uludag_sms_svc.Models;
using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace uludag_sms_svc
{
    public class SMSService
    {
        private readonly IMongoCollection<SMSModel> _smsCollection;

        public SMSService(
            IOptions<SMSStoreDatabaseSettings> bookStoreDatabaseSettings)
        {
            var mongoClient = new MongoClient(
                bookStoreDatabaseSettings.Value.ConnectionString);

            var mongoDatabase = mongoClient.GetDatabase(
                bookStoreDatabaseSettings.Value.DatabaseName);

            _smsCollection = mongoDatabase.GetCollection<SMSModel>(
                bookStoreDatabaseSettings.Value.SMSCollectionName);
        }

        public ResponseModel Get(ListModel listModel)
        {
            //var builder = Builders<SMSModel>.Filter;

            //var filterData = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(listModel.filterData);

            //var dynamicQuery = builder.Empty;
            //foreach (var item in filterData)
            //{
            //    dynamicQuery &= builder.Or(item.Key, item.Value);
            //}

            var offset = (listModel.current - 1) * listModel.pageSize;

            List<SMSModel> data = _smsCollection.Find(_ => true).Skip(offset).Limit(listModel.pageSize).SortByDescending(i => i.eklenmeTarih).ToList();
            var count = _smsCollection.Find(_ => true).CountDocuments();

            ResponseModel result = new()
            {
                success = true,
                data = new
                {
                    current = listModel.current,
                    data = data,
                    pageSize = listModel.pageSize,
                    success = true,
                    total = count,
                }
            };

            return result;
        }

        public ResponseModel Create(SMSModel newSms)
        {
            ResponseModel result = new()
            {
                success = false,
                data = false
            };

            if (string.IsNullOrEmpty(newSms.mesaj))
            {
                return result;
            }

            newSms.eklenmeTarih = DateTime.Now;
            _smsCollection.InsertOne(newSms);

            result.data = true;
            result.success = true;
            return result;
        }

        public ResponseModel Remove(string id)
        {
            ResponseModel result = new()
            {
                success = false,
                data = false
            };

            _smsCollection.DeleteOne(x => x.id == id);
            result.data = true;
            result.success = true;
            return result;
        }

        public ResponseModel Gonder()
        {
            FilterDefinition<SMSModel> nameFilter = Builders<SMSModel>.Filter.Eq(x => x.durum, 0);
            FilterDefinition<SMSModel> combineFilters = Builders<SMSModel>.Filter.And(nameFilter);

            List<SMSModel> SMSModels = _smsCollection.Find(combineFilters).ToList();

            foreach (var sms in SMSModels)
            {
                if (string.IsNullOrEmpty(sms.mesaj))
                {
                    continue;
                }

                smsData MesajPaneli = new smsData();

                List<string> telList = new List<string>();

                foreach (var kisi in sms.kisiler)
                {
                    if (string.IsNullOrEmpty(kisi.numara))
                    {
                        continue;
                    }

                    Match Eslesme = Regex.Match(kisi.numara, "5[0-9]{9}", RegexOptions.IgnoreCase);

                    if (Eslesme.Success)
                    {
                        telList.Add(kisi.numara);
                    }
                }

                MesajPaneli.user = new UserInfo("5322407700", "q1qwtc");
                MesajPaneli.msgBaslik = "ULUDAGENRJI";
                MesajPaneli.msgData.Add(new msgdata(telList, sms.mesaj));
                MesajPaneli.tr = true;

                ReturnValue ReturnData = MesajPaneli.DoPost("http://api.mesajpaneli.com/json_api/", true, true);

                if (ReturnData.status)
                {
                    sms.durum = 1;
                    sms.refNo = ReturnData.Ref;
                    sms.kullanilanKredi = ReturnData.amount;
                    sms.kalanKredi = ReturnData.credits;
                    sms.gonderilmeTarih = DateTime.Now;
                    _smsCollection.ReplaceOne(x => x.id == sms.id, sms);
                }
                else
                {
                    sms.durum = 2;
                    sms.hata = ReturnData.error;
                    _smsCollection.ReplaceOne(x => x.id == sms.id, sms);
                }
            }

            ResponseModel result = new()
            {
                success = false,
                data = false
            };

            result.data = true;
            result.success = true;
            return result;
        }

        public static void TekliGonder(SMSModel sms)
        {
            if (string.IsNullOrEmpty(sms.mesaj))
            {
                Task.Yield();
                return;
            }

            smsData MesajPaneli = new smsData();

            List<string> telList = new List<string>();

            foreach (var kisi in sms.kisiler)
            {
                if (string.IsNullOrEmpty(kisi.numara))
                {
                    continue;
                }

                Match Eslesme = Regex.Match(kisi.numara, "5[0-9]{9}", RegexOptions.IgnoreCase);

                if (Eslesme.Success)
                {
                    telList.Add(kisi.numara);
                }
            }

            MesajPaneli.user = new UserInfo("5322407700", "q1qwtc");
            MesajPaneli.msgBaslik = "ULUDAGENRJI";
            MesajPaneli.msgData.Add(new msgdata(telList, sms.mesaj));
            MesajPaneli.tr = true;

            MesajPaneli.DoPost("http://api.mesajpaneli.com/json_api/", true, true);
        }        
    }
}
