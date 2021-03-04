using Devir.DMS.DL.Models.FileStorage;
using Devir.DMS.DL.Repositories;
using EPocalipse.IFilter;
using Lucene.Net.Documents;
using Lucene.Net.Index;
using Lucene.Net.QueryParsers;
using Lucene.Net.Search;
using Lucene.Net.Store;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Web;

namespace Devir.DMS.FulltextSearchEngine
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
  ConcurrencyMode = ConcurrencyMode.Single)]
    public class ServiceFTS
    {

        [OperationContract]
        public bool SaveDocument(Guid docId)
        {

            var LuceneDoc = DocumentForLucene.GetDataFromDocument(docId);

            System.IO.Directory.CreateDirectory(@"C:\LuceneDataStorage");
            using (Lucene.Net.Store.Directory directory = FSDirectory.Open(@"C:\LuceneDataStorage"))
            {
                var an = new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "Russian");//new Lucene.Net.Analysis.Ru.RussianAnalyzer(Lucene.Net.Util.Version.LUCENE_30);
                using (var indexer = new IndexWriter(directory, an, IndexWriter.MaxFieldLength.UNLIMITED))
                {
                    Document foundDoc = null;
                    //using (var searcher = new IndexSearcher(directory, true))
                    //{                       
                    //    var res = searcher.Search(new TermQuery(new Term("Id", docId.ToString())), 1);
                    //    if (res.ScoreDocs.Any())
                    //        foundDoc = searcher.Doc(res.ScoreDocs.FirstOrDefault().Doc);                        
                    //}

                    //if (foundDoc != null)
                    //{
                    //    //Реализация функционала по обновлению документа
                    //    //foundDoc.GetField("Id").SetValue(LuceneDoc.Id.ToString());
                    //    foundDoc.GetField("Body").SetValue(LuceneDoc.Body);
                    //    foundDoc.GetField("Number").SetValue(LuceneDoc.Number);
                    //    foundDoc.GetField("Type").SetValue(LuceneDoc.Type);
                    //    foundDoc.GetField("Viewers").SetValue(LuceneDoc.Viewers);
                    //    foundDoc.GetField("Attachments").SetValue(LuceneDoc.Attachmments);
                    //    foundDoc.GetField("SubBodies").SetValue(LuceneDoc.SubBodies);
                    //    foundDoc.GetField("Header").SetValue(LuceneDoc.Header);
                    //    //indexer.DeleteDocuments(new TermQuery(new Term("Id", docId.ToString())));
                    //    //indexer.Commit();              

                    //    indexer.UpdateDocument(, foundDoc);
                    //    //indexer.UpdateDocument(new Term("Id", docId.ToString()), foundDoc);
                    //}
                    //else
                    //{
                    //Вставка нового документа

                    //Попытка удалить
                    indexer.DeleteDocuments(new Term("Id", docId.ToString()));

                    //Вставка нового
                    foundDoc = new Document();

                    foundDoc.Add(new Field("Id", docId.ToString(), Field.Store.YES, Field.Index.NOT_ANALYZED_NO_NORMS));
                    foundDoc.Add(new Field("Viewers", LuceneDoc.Viewers, Field.Store.YES, Field.Index.ANALYZED));
                    foundDoc.Add(new Field("Number", LuceneDoc.Number, Field.Store.YES, Field.Index.ANALYZED));
                    foundDoc.Add(new Field("Type", LuceneDoc.Type, Field.Store.YES, Field.Index.ANALYZED));
                    foundDoc.Add(new Field("Header", LuceneDoc.Header, Field.Store.YES, Field.Index.ANALYZED));
                    foundDoc.Add(new Field("Body", LuceneDoc.Body, Field.Store.YES, Field.Index.ANALYZED));
                    foundDoc.Add(new Field("Attachments", LuceneDoc.Attachmments, Field.Store.YES, Field.Index.ANALYZED));
                    foundDoc.Add(new Field("SubBodies", LuceneDoc.SubBodies, Field.Store.YES, Field.Index.ANALYZED));
                    indexer.AddDocument(foundDoc);


                    indexer.Commit();
                    indexer.Close();
                }
                return true;
            }

        }

        [OperationContract]
        public List<FoundDocuments> SearchDocuments(string Text, string userId)
        {
            try
            {
                Console.WriteLine("Ищем документы по тексту: {0}", Text);
                using (Lucene.Net.Store.Directory directory = FSDirectory.Open(@"C:\LuceneDataStorage"))
                {

                    userId = userId.Replace("-", "");

                    var realText = new StringBuilder();
                    Text.Split(' ').ToList().ForEach(m =>
                    {
                        realText.AppendFormat("{0}~+", m);
                    });
                    Text = realText.ToString();
                    Text = Text.Trim('+');

                    using (var searcher = new IndexSearcher(directory, true))
                    {

                        var i = searcher.MaxDoc;

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




                        var parser1 = new QueryParser(
                                  Lucene.Net.Util.Version.LUCENE_30, "Header", new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "Russian")
                            //new Lucene.Net.Analysis.Ru.RussianAnalyzer(Lucene.Net.Util.Version.LUCENE_30)
                                  );
                        var query1 = parser1.Parse(Text);
                        mainSubQuery.Add(query1, Occur.SHOULD);


                        searchTerms.Add(new FieldWithBoost() { Name = "Number", Boost = 100 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Type", Boost = 100 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Header", Boost = 100 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Body", Boost = 10 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "Attachments", Boost = 2 }, Text);
                        searchTerms.Add(new FieldWithBoost() { Name = "SubBodies", Boost = 1 }, Text);


                        foreach (var pair in searchTerms)
                        {
                            parser1 =
                              new QueryParser(
                                  Lucene.Net.Util.Version.LUCENE_30, pair.Key.Name, new Lucene.Net.Analysis.Snowball.SnowballAnalyzer(Lucene.Net.Util.Version.LUCENE_30, "Russian")
                                  );

                            query1 = parser1.Parse(Text);
                            query1.Boost = pair.Key.Boost;
                            mainSubQuery.Add(query1, Occur.SHOULD);
                        }

                        mainQuery.Add(mainSubQuery, Occur.MUST);

                        var foundDocs = searcher.Search(mainQuery, 1000).ScoreDocs;

                        return foundDocs.Select(m => new FoundDocuments() { DocId = searcher.Doc(m.Doc).GetField("Id").StringValue, Score = m.Score }).ToList();

                        //return .Count();

                        //if (res.ScoreDocs.Any())
                        //    return null;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            return null;
        }

        [OperationContract]
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
            public string DocId { get; set; }
            public float Score { get; set; }
        }



        public class DocumentForLucene
        {
            public Guid Id { get; set; }
            public int Year { get; set; }
            public string Viewers { get; set; }
            public string Number { get; set; }
            public string Type { get; set; }
            public string Header { get; set; }
            public string Body { get; set; }
            public string Attachmments { get; set; }
            public string SubBodies { get; set; }

            public static DocumentForLucene GetDataFromDocument(Guid docId)
            {
                List<Guid> attachmentsToIndex = new List<Guid>();

                //Настройка делегатта для получения GUID пользователя
                RepositoryFactory.GetCurrentUser = () =>
                {
                    //Сюда код для получения GUID текущего пользователя
                    //Для примера я просто генерирую GUID
                    return new Guid("7c432691-5359-4fcf-b7f6-43f3f7f8bbb4");
                };

                var doc = RepositoryFactory.GetRepository<Devir.DMS.DL.Models.Document.Document>().Single(m => m.Id == docId);
                attachmentsToIndex.AddRange(doc.Attachments);

                var result = new DocumentForLucene();

                result.Id = doc.Id;
                result.Type = doc.DocumentType.Name;
                result.Number = doc.DocumentNumber;
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

                StringBuilder sbAttachments = new StringBuilder();

                attachmentsToIndex.ForEach(m =>
                {
                    var fss = RepositoryFactory.GetRepository<FileContentStrForFTS>().Single(n => n.FileStrorageId == m);
                    if (fss != null)
                        sbAttachments.AppendFormat("{0}     ", fss.Content);
                });

                result.Attachmments = sbAttachments.ToString();


                result.SubBodies = String.Empty;


                return result;
            }


        }



    }
}