using Devir.DMS.DL.File;
using Devir.DMS.DL.Models.FileStorage;
using Devir.DMS.DL.Repositories;
using EPocalipse.IFilter;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Devir.DMS.FulltextSearchEngine
{
    [ServiceContractAttribute]
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
  ConcurrencyMode = ConcurrencyMode.Single)]
    public class ServiceFTS
    {

        [OperationContract]
        public bool SaveDocument(Guid docId)
        {
            Console.WriteLine("Начали сохранение документа");
            try
            {
                var LuceneDoc = DocumentForLucene.GetDataFromDocument(docId);
                if (LuceneDoc != null)
                {
                    System.IO.Directory.CreateDirectory(@"C:\LuceneDataStorage");
                    using (Lucene.Net.Store.Directory directory = FSDirectory.Open(@"C:\LuceneDataStorage"))
                    {

                        var an = new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "Russian");
                        using (var indexer = new IndexWriter(directory, an, IndexWriter.MaxFieldLength.UNLIMITED))
                        {
                            Document foundDoc = null;
                            //Попытка удалить
                            indexer.DeleteDocuments(new Term("Id", docId.ToString()));

                            //Вставка нового
                            foundDoc = new Document();

                            foundDoc.Add(new Field("Id", docId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
                            foundDoc.Add(new Field("Viewers", LuceneDoc.Viewers, Field.Store.YES, Field.Index.ANALYZED));
                            foundDoc.Add(new Field("Number", LuceneDoc.Number, Field.Store.YES, Field.Index.ANALYZED));
                            foundDoc.Add(new Field("Type", LuceneDoc.Type, Field.Store.YES, Field.Index.ANALYZED));
                            foundDoc.Add(new Field("TypeId", LuceneDoc.TypeId, Field.Store.YES, Field.Index.ANALYZED));
                            foundDoc.Add(new Field("Header", LuceneDoc.Header, Field.Store.YES, Field.Index.ANALYZED));
                            foundDoc.Add(new Field("Body", LuceneDoc.Body, Field.Store.YES, Field.Index.ANALYZED));
                            foundDoc.Add(new Field("Attachments", LuceneDoc.Attachmments, Field.Store.YES, Field.Index.ANALYZED));
                            foundDoc.Add(new Field("SubBodies", LuceneDoc.SubBodies, Field.Store.YES, Field.Index.ANALYZED));
                            indexer.AddDocument(foundDoc);


                            indexer.Commit();
                            indexer.Close();

                            Console.WriteLine("Закончили сохранение документа");
                        }
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine("Не нашли такой документ :(");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        [OperationContract]
        public List<FoundDocuments> SearchDocuments(string Text, string userId, string typeId)
        {
            Text = Text.Trim();
            while (Text.Contains("  "))
            {
                Text = Text.Replace("  ", " ");
            }

            Output($"Ищем документы по фразе: {Text} , Пользователь: {userId}", ConsoleColor.Green);
            try
            {
                using (Lucene.Net.Store.Directory directory = FSDirectory.Open(@"C:\LuceneDataStorage"))
                {

                    if (!String.IsNullOrEmpty(userId))
                        userId = userId.Replace("-", "");

                    using (var searcher = new IndexSearcher(directory, true))
                    {
                        var searchTerms = new Dictionary<FieldWithBoost, string>();
                        var mainQuery = new BooleanQuery();
                        var mainSubQuery = new BooleanQuery();

                        if (!String.IsNullOrEmpty(userId))
                        {
                            var parser = new QueryParser(
                                    Lucene.Net.Util.Version.LUCENE_30, "Viewers",
                                    new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "Russian")
                                    );
                            var query = parser.Parse(userId);
                            mainQuery.Add(query, Occur.MUST);
                        }

                        var parserTypeId = new QueryParser(
                               Lucene.Net.Util.Version.LUCENE_30, "TypeId",
                               new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "Russian")
                               );
                        var queryTypeId = parserTypeId.Parse(String.IsNullOrEmpty(typeId) ? "ANY" : typeId.Replace("-", ""));
                        mainQuery.Add(queryTypeId, Occur.MUST);

                        searchTerms.Add(new FieldWithBoost() { Name = "Number", Boost = 10000 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Type", Boost = 10000 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Header", Boost = 100 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Body", Boost = 10 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Attachments", Boost = 2 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "SubBodies", Boost = 1 }, Text);

                        foreach (var word in Text.Split(' '))
                        {
                            var secSubQuery = new BooleanQuery();
                            foreach (var pair in searchTerms)
                            {
                                var queryParser =
                                 new QueryParser(
                                     Lucene.Net.Util.Version.LUCENE_30, pair.Key.Name, new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "Russian")
                                     );
                                queryParser.AllowLeadingWildcard = true;

                                Query query;
                                if (Regex.IsMatch(word, "^[0-9]+$"))
                                {
                                    query = queryParser.Parse($"N*{word}");
                                }
                                else
                                {

                                    query = queryParser.Parse($"*{word}*");
                                }
                                query.Boost = pair.Key.Boost;
                                secSubQuery.Add(query, Occur.SHOULD);
                            }
                            mainSubQuery.Add(secSubQuery, Occur.MUST);
                        }

                        mainQuery.Add(mainSubQuery, Occur.MUST);
                        var foundDocs = searcher.Search(mainQuery, 1000).ScoreDocs;
                        var result = foundDocs.Select(m => new FoundDocuments() { DocId = searcher.Doc(m.Doc).GetField("Id").StringValue, Score = m.Score }).ToList();

                        Output($"Найдено: {result.Count}", ConsoleColor.DarkGreen);

                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                Output(ex.ToString(), ConsoleColor.Red);
                return null;
            }
        }


        private void Output(string message, ConsoleColor color = ConsoleColor.White)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"{DateTime.Now.ToString()}: {message}");
            Console.ForegroundColor = oldColor;
        }

        public string GetDataFromDocument(string Path)
        {
            TextReader reader = new FilterReader("E:\\1.docx");
            using (reader)
                return reader.ReadToEnd();
        }


        public class FieldWithBoost
        {
            public string Name { get; set; }
            public float Boost { get; set; }
        }

        [DataContract]
        public class FoundDocuments
        {
            [DataMember]
            public string DocId { get; set; }
            [DataMember]
            public float Score { get; set; }
        }

        public class DocumentForLucene
        {
            public Guid Id { get; set; }
            public int Year { get; set; }
            public string Viewers { get; set; }
            public string Number { get; set; }
            public string Type { get; set; }
            public string TypeId { get; set; }
            public string Header { get; set; }
            public string Body { get; set; }
            public string Attachmments { get; set; }
            public string SubBodies { get; set; }

            public static DocumentForLucene GetDataFromDocument(Guid docId)
            {
                try
                {
                    List<Guid> attachmentsToIndex = new List<Guid>();

                    //Настройка делегатта для получения GUID пользователя
                    RepositoryFactory.GetCurrentUser = () =>
                    {
                        //Сюда код для получения GUID текущего пользователя
                        //Для примера я просто генерирую GUID
                        return new Guid("7c432691-5359-4fcf-b7f6-43f3f7f8bbb4");
                    };

                    DocumentForLucene result = null;

                    Console.WriteLine("Ищем в БД документ с ID: {0}", docId);
                    var doc = RepositoryFactory.GetDocumentRepository().Single(m => m.Id == docId);

                    Console.WriteLine("Адрес сервера: {0}", DMS.DL.MongoHelpers.MongoHelper.client.Settings.Server.Host);
                    Console.WriteLine("БД сервера: {0}", DMS.DL.MongoHelpers.MongoHelper.Database.Name);

                    if (doc != null)
                    {
                        Console.WriteLine("Нашли в БД документ с ID: {0}", docId);
                        if (doc.Attachments != null)
                            attachmentsToIndex.AddRange(doc.Attachments);

                        result = new DocumentForLucene();

                        result.Id = doc.Id;
                        result.TypeId = String.Format("ANY {0}", doc.DocumentType.Id.ToString().Replace("-", ""));
                        result.Type = doc.DocumentType.Name;
                        result.Number = !String.IsNullOrEmpty(doc.DocumentNumber) ? $"{doc.DocumentNumber} N{doc.DocumentNumber}" : "б/н";
                        result.Header = doc.Header;

                        StringBuilder sb = new StringBuilder();
                        sb.AppendFormat("{0};", doc.Body);
                        doc.FieldValues.ForEach(m => sb.AppendFormat("{0};", m.ValueToDisplay));
                        doc.DocumentSignStages.ForEach(m => m.RouteUsers.ForEach(n =>
                        {
                            if (n.SignResult != null)
                            {
                                sb.AppendFormat("{0};", n.SignResult.Comment);
                                if (n.SignResult.attachment != null)
                                    attachmentsToIndex.AddRange(n.SignResult.attachment);
                            }
                        }));

                        result.Body = sb.ToString();

                        StringBuilder sbViewers = new StringBuilder();
                        doc.DocumentViewers.ToList().ForEach(m => sbViewers.AppendFormat("{0}     ", m.Key.Replace("-", "")));

                        result.Viewers = sbViewers.ToString();

                        StringBuilder sbAttachments = new StringBuilder("");

                        attachmentsToIndex.ForEach(m =>
                        {

                            var fsobj = RepositoryFactory.GetRepository<FileStorage>().Single(n => n.Id == m);
                            if (fsobj != null)
                            {

                                var _extension = Path.GetExtension(fsobj.FileName);
                                if (fsobj.OId != ObjectId.Empty)
                                {

                                    var client = DL.MongoHelpers.MongoHelper.client;
                                    var server = client.GetServer();
                                    var database = server.GetDatabase("FileStorage");

                                    System.IO.Directory.CreateDirectory(System.IO.Path.GetTempPath() + "\\DMSPDFFTS");

                                    string path = System.IO.Path.GetTempPath() + "DMSPDFFTS\\" + "tmpFile" + _extension;
                                    string pathToPDF = Path.ChangeExtension(path, "pdf");

                                    FileStream fs = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write);

                                    MongoGridFsHelper mongoGridFsHelper = new MongoGridFsHelper(database);

                                    Stream stream = mongoGridFsHelper.GetFile(fsobj.OId);

                                    int lenght = Convert.ToInt32(stream.Length);
                                    byte[] fileContents = new byte[lenght];

                                    stream.Read(fileContents, 0, lenght);
                                    stream.Close();

                                    fs.Write(fileContents, 0, fileContents.Length);

                                    fs.Close();

                                    try
                                    {
                                        TextReader reader = new FilterReader(path);
                                        using (reader)
                                            sbAttachments.AppendFormat("{0}           ", reader.ReadToEnd());
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.ToString());
                                    }

                                    File.Delete(path);
                                }
                            }


                        });

                        result.Attachmments = sbAttachments.ToString();


                        result.SubBodies = String.Empty;

                    }
                    else
                    {
                        Console.WriteLine("не нашли в БД документ с ID: {0}", docId);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return null;
                }
            }


        }



    }
}