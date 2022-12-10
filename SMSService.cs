using MesajPaneli.Business;
using MesajPaneli.Models.JsonPostModels;
using MesajPaneli.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using uludag_sms_svc.Models;
using Newtonsoft.Json;

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

        public ResponseModel GetAsync(ListModel listModel)
        {
            //var builder = Builders<SMSModel>.Filter;

            //var filterData = JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(listModel.filterData);

            //var dynamicQuery = builder.Empty;
            //foreach (var item in filterData)
            //{
            //    dynamicQuery &= builder.Or(item.Key, item.Value);
            //}

            var offset = (listModel.current - 1) * listModel.pageSize;

            List<SMSModel> data =  _smsCollection.Find(_ => true).Skip(offset).Limit(listModel.pageSize).SortByDescending(i => i.eklenmeTarih).ToList();
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

        public Task CreateAsync(SMSModel newBook)
        {
            newBook.eklenmeTarih = DateTime.Now;
            return _smsCollection.InsertOneAsync(newBook);
        }

        public async Task UpdateAsync(string id, SMSModel updatedBook) =>
            await _smsCollection.ReplaceOneAsync(x => x.id == id, updatedBook);

        public async Task RemoveAsync(string id) =>
            await _smsCollection.DeleteOneAsync(x => x.id == id);

        public async void Gonder()
        {
            FilterDefinition<SMSModel> nameFilter = Builders<SMSModel>.Filter.Eq(x => x.durum, 0);
            FilterDefinition<SMSModel> combineFilters = Builders<SMSModel>.Filter.And(nameFilter);

            List<SMSModel> SMSModels = await _smsCollection.Find(combineFilters).ToListAsync();

            foreach (var sms in SMSModels)
            {
                smsData MesajPaneli = new smsData();

                List<string> telList = new List<string>();

                foreach (var kisi in sms.kisiler)
                {
                    telList.Add(kisi.numara);
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
                    await _smsCollection.ReplaceOneAsync(x => x.id == sms.id, sms);
                }
                else
                {
                    sms.durum = 2;
                    sms.hata = ReturnData.error;
                    await _smsCollection.ReplaceOneAsync(x => x.id == sms.id, sms);
                }
            }
        }
    }
}
