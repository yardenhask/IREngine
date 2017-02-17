using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eng
{
    class FindParam
    {
        Dictionary<string, TermInfo> d;
        Dictionary<string, DocumentInfo> docs;

        Searcher s;
        Ranker ranker;
        int countDocsLength = 0;
        int countDocs = 0;

        HashSet<string> relevants;


        public FindParam()
        {
            relevants = new HashSet<string>();
            loadDictionaryButton_Click();
            string postPath = @"C:\Users\ירדן\Desktop\לימודים\שנה ד\סמס1\אחזור מידע\data\posting";
            postPath = postPath + "\\postingNoStemm.txt";
            s = new Searcher(d, docs, postPath, countDocs, countDocsLength, @"C:\Users\ירדן\Desktop\לימודים\שנה ד\סמס1\אחזור מידע\data\posting\stopwords.txt");
            string toSave = @"C:\Users\ירדן\Desktop\לימודים\שנה ד\סמס1\אחזור מידע\data\results\";
            string filename = "";
            readRelevants();
            int countRel;

            filename = toSave + "result11.txt";
            double avg = countDocsLength / countDocs;
            ranker = new Ranker(countDocs, avg);
            string qingleQuery = "space program";
            List<string> documentsFromQuery;

           // documentsFromQuery = 
                parseQueryAndReturnRank(qingleQuery, false, null);
            //  documentsFromQuery = s.parseQueryAndReturnRank(qingleQuery, false, null);
         /*   countRel = 0;
            foreach (string tmp in documentsFromQuery)
            {
                string sub = tmp.Trim(' ');

                if (relevants.Contains(sub))
                {
                    countRel++;
                }
            }*/
            /*
            using (FileStream fs = new FileStream(filename, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    if (countRel > 30)
                        sw.WriteLine("*******************************");
                    sw.WriteLine("b: " + bi);
                    sw.WriteLine("k1: " + k1i);
                    sw.WriteLine("k2: " + 0);
                    //    sw.WriteLine("alpha: " + i);
                    sw.WriteLine("count rel: " + countRel);
                    sw.WriteLine();
                    sw.WriteLine();
                }
            }
            */
            /*
                     s.ranker.k2 = 0;

                     for (double bi = 0.2; bi < 0.35; bi += 0.05)
                     {
                         s.ranker.b = bi;
                         for (double k1i = 20; k1i < 100; k1i += 10)
                         {
                             s.ranker.k1 = k1i;
                             //    for (double i = 0.55; i <=0.65; i += 0.02)
                             {
                                 //  s.ranker.alpha = i;
                                 documentsFromQuery = s.parseQueryAndReturnRank(qingleQuery, false, null);
                                 countRel = 0;
                                 foreach (string tmp in documentsFromQuery)
                                 {
                                     string sub = tmp.Trim(' ');

                                     if (relevants.Contains(sub))
                                     {
                                         countRel++;
                                     }
                                 }
                                 using (FileStream fs = new FileStream(filename, FileMode.Append))
                                 {
                                     using (StreamWriter sw = new StreamWriter(fs))
                                     {
                                         if (countRel > 30)
                                             sw.WriteLine("*******************************");
                                         sw.WriteLine("b: " + bi);
                                         sw.WriteLine("k1: " + k1i);
                                         sw.WriteLine("k2: " + 0);
                                         //    sw.WriteLine("alpha: " + i);
                                         sw.WriteLine("count rel: " + countRel);
                                         sw.WriteLine();
                                         sw.WriteLine();
                                     }
                                 }

                             }
                         }
                     }


                        */
            /*   for (double b1i = 1; b1i < 3; b1i += 0.5)
               {
                   s.ranker.tobonusTitle = b1i;
                   for (double b2i = 1; b2i < 2; b2i += 0.5)
                   {
                       s.ranker.tobonusTag100 = b2i;
                       for (double b3i = 1; b3i < 2; b3i += 0.5)
                       {
                           s.ranker.tobonusTag101 = b3i;
                           for (double b4i = 1; b4i < 3; b4i += 0.5)
                           {
                               s.ranker.tobonusTheard = b4i;


                               documentsFromQuery = s.parseQueryAndReturnRank(qingleQuery, false, null);
                               countRel = 0;
                               foreach (string tmp in documentsFromQuery)
                               {
                                   string sub = tmp.Trim(' ');

                                   if (relevants.Contains(sub))
                                   {
                                       countRel++;
                                   }
                               }

                               using (FileStream fs = new FileStream(filename, FileMode.Append))
                               {
                                   using (StreamWriter sw = new StreamWriter(fs))
                                   {
                                       if (countRel > 30)
                                           sw.WriteLine("*******************************");
                                       sw.WriteLine("b: " + 0.3);
                                       sw.WriteLine("k1: " + 13);
                                       sw.WriteLine("k2: " + 5);
                                       sw.WriteLine("title: " + b1i);
                                       sw.WriteLine("tag100: " + b2i);
                                       sw.WriteLine("tag101: " + b3i);
                                       sw.WriteLine("3rd: " + b4i);
                                       sw.WriteLine("count rel: " + countRel);
                                       sw.WriteLine();
                                       sw.WriteLine();
                                   }
                               }
                           }
                       }
                   }
               }*/






        }

        private void readRelevants()
        {
            string[] lines = File.ReadAllLines(@"C:\q\qrel.txt");
            for (int i = 0; i < lines.Length; i++)
            {
                string[] data = lines[i].Split(' ');
                if (data[0] == "11")
                {
                    if (data[3] == "1")
                    {
                        relevants.Add(data[2]);
                    }
                }

            }
        }


        public void parseQueryAndReturnRank(string q, bool stem, List<string> langs)
        {



            /* Stopwatch stopWatch = new Stopwatch();
             stopWatch.Start();
             */
            Parse p = new Parse(null, stem);
            Dictionary<string, int> queryTemp = p.parseQuery(q);//origin query
            Dictionary<string, int> queryTemp2 = new Dictionary<string, int>(); //syno. from query
            Dictionary<string, int> extandQueryIndex = new Dictionary<string, int>();
            int idx = 1;
            List<string> synQuery;
            foreach (string s in queryTemp.Keys)
            {
                if (!extandQueryIndex.ContainsKey(s))
                {
                    extandQueryIndex[s] = idx;

                    synQuery = synForQuery(s);

                    foreach (string syn in synQuery)
                    {
                        extandQueryIndex[syn] = idx;
                        if (d.ContainsKey(syn))
                        {
                            queryTemp2[syn] = queryTemp[s];
                        }
                    }
                    idx++;
                }
            }
            Dictionary<string, int> query = new Dictionary<string, int>();//marge together
            foreach (string s in queryTemp.Keys)
            {
                if (d.ContainsKey(s))
                {
                    query[s] = queryTemp[s];
                }
            }
            foreach (string s in queryTemp2.Keys)
            {
                if (!query.ContainsKey(s))
                    query[s] = queryTemp2[s];
                else
                    query[s] += queryTemp2[s];
            }

            Dictionary<string, TermInfo> postingsOfQueryTerm = new Dictionary<string, TermInfo>();
            HashSet<string> allRelevantDocs = new HashSet<string>();
            string postingPath = @"C:\Users\ירדן\Desktop\לימודים\שנה ד\סמס1\אחזור מידע\data\posting" + "\\postingNoStemm.txt";
    
            foreach (string s in query.Keys)
            {
                if (d.ContainsKey(s))
                {
                    //   TermInfo ti = new TermInfo();
                    d[s].locations = new Dictionary<string, int>();
                    int ptr = d[s].getPointer();
                    string postingOfTerm = File.ReadLines(postingPath).Skip(ptr).Take(1).First();
                    postingOfTerm = postingOfTerm.Substring(1);
                    string[] posting = postingOfTerm.Split(new char[] { ',', '[', ']' }, StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < posting.Length; i += 3)
                    {

                        if (langs != null)
                        {
                            foreach (string language in langs)
                            {
                                if (docs[posting[i]].language.Contains(language))
                                {
                                    d[s].locations[posting[i]] = Int32.Parse(posting[i + 1]);
                                    //  ti.locations[posting[i]] = Int32.Parse(posting[i + 1]);
                                    int tmpidx = posting[i + 2].IndexOf('-');
                                    int tif3 = int.Parse(posting[i + 2].Substring(0, tmpidx));
                                    int til10 = int.Parse(posting[i + 2].Substring(tmpidx + 1));

                                    //   int trfa=Int32.Parse(posting[i + 2]);
                                    if (tif3 == 1)
                                        d[s].inFirstThird[posting[i]] = true;
                                    else
                                        d[s].inFirstThird[posting[i]] = false;
                                    if (til10 == 1)
                                        d[s].inLast10[posting[i]] = true;
                                    else
                                        d[s].inLast10[posting[i]] = false;
                                    allRelevantDocs.Add(posting[i]);
                                    // break;
                                }
                            }
                        }
                        else
                        {
                            d[s].locations[posting[i]] = Int32.Parse(posting[i + 1]);
                            //  allRelevantDocs.Add(posting[i]);
                            int tmpidx = posting[i + 2].IndexOf('-');
                            int tif3 = int.Parse(posting[i + 2].Substring(0, tmpidx));
                            int til10 = int.Parse(posting[i + 2].Substring(tmpidx + 1));

                            if (tif3 == 1)
                                d[s].inFirstThird[posting[i]] = true;
                            else
                                d[s].inFirstThird[posting[i]] = false;
                            if (til10 == 1)
                                d[s].inLast10[posting[i]] = true;
                            else
                                d[s].inLast10[posting[i]] = false;
                            allRelevantDocs.Add(posting[i]);
                        }
                    }
                    postingsOfQueryTerm[s] = d[s];

                }
            }

            int countRel = 0;



            string toSave = @"C:\Users\ירדן\Desktop\לימודים\שנה ד\סמס1\אחזור מידע\data\results\";
            string filename = "";
            readRelevants();
            filename = toSave + "result11.txt";
            Dictionary<string, double> docsRank;
            List<string> sortDocs;
            for (double bi=0.0; bi<1.05; bi+=0.05)
            {
                for (double k1i = 0; k1i < 10; k1i += 1)
                {
                    for (double ai = 0; ai <= 1.1; ai += 0.1)
                    {
                        ranker.b = bi;
                        ranker.k1 = k1i;
                        ranker.alpha = ai;
                        docsRank = ranking(query, queryTemp, postingsOfQueryTerm, allRelevantDocs, extandQueryIndex);
                        sortDocs = new List<string>();
                        var items = from pair in docsRank orderby pair.Value descending select pair;
                        foreach (KeyValuePair<string, double> item in items.Take(50))
                        {
                            sortDocs.Add(item.Key);
                        }
                        countRel = 0;
                        foreach (string tmp in sortDocs)
                        {
                            string sub = tmp.Trim(' ');

                            if (relevants.Contains(sub))
                            {
                                countRel++;
                            }
                        }
                        using (FileStream fs = new FileStream(filename, FileMode.Append))
                        {
                            using (StreamWriter sw = new StreamWriter(fs))
                            {
                                if (countRel > 30)
                                    sw.WriteLine("*******************************");
                                sw.WriteLine("b: " + bi);
                                sw.WriteLine("k1: " + k1i);
                                sw.WriteLine("k2: " + 0);
                                    sw.WriteLine("alpha: " + ai);
                                sw.WriteLine("count rel: " + countRel);
                                sw.WriteLine();
                                sw.WriteLine();
                            }
                        }
                    }
                }
            }
           

        }


        private Dictionary<string, double> ranking
            (Dictionary<string, int> query, Dictionary<string, int> originQuery,
            Dictionary<string, TermInfo> postingsOfQueryTerm, HashSet<string> allRelevantDocs,
            Dictionary<string, int> extandQuery)
        {
            Dictionary<string, double> docsRank = new Dictionary<string, double>();
            Dictionary<string, double> maxRank = new Dictionary<string, double>();




            foreach (string qTerm in query.Keys)
            {
                //rank term
                ranker.setTermInfo(0, postingsOfQueryTerm[qTerm].locations.Count, 0, query[qTerm]);
                foreach (string docId in allRelevantDocs)
                {
                    double score = 0;
                    int countTF = 1;
                    if (postingsOfQueryTerm[qTerm].locations.ContainsKey(docId))
                    {
                        countTF = postingsOfQueryTerm[qTerm].locations[docId];
                    }
                    ranker.ComputeBM25(docs[docId].numTermsInDoc, countTF);
                    score = ranker.getScore();
                    if (originQuery.ContainsKey(qTerm))
                        score = score * 2;
                    if (!docsRank.ContainsKey(docId))
                    {
                        docsRank[docId] = score;
                    }
                    else
                    {
                        docsRank[docId] += score;
                    }
                }
                //rank all other terms in query w/o syn
                foreach (string q in query.Keys)
                {
                    if (extandQuery[q] != extandQuery[qTerm])
                    {
                        ranker.setTermInfo(0, postingsOfQueryTerm[q].locations.Count, 0, query[q]);
                        foreach (string docId in allRelevantDocs)
                        {

                            double score = 0;
                            int countTF = 1;
                            if (postingsOfQueryTerm[q].locations.ContainsKey(docId))
                            {
                                countTF = postingsOfQueryTerm[q].locations[docId];
                            }
                            ranker.ComputeBM25(docs[docId].numTermsInDoc, countTF);
                            score = ranker.getScore();
                            if (originQuery.ContainsKey(q))
                                score = score * 2;
                            if (!docsRank.ContainsKey(docId))
                            {
                                docsRank[docId] = score;
                            }
                            else
                            {
                                docsRank[docId] += score;
                            }
                        }
                    }
                }
                foreach (string docid in docsRank.Keys)
                {
                    if (maxRank.ContainsKey(docid))
                    {
                        if (maxRank[docid] < docsRank[docid])
                        {
                            maxRank[docid] = docsRank[docid];
                        }
                    }
                    else
                    {
                        maxRank[docid] = docsRank[docid];
                    }
                }
                docsRank.Clear();
            }




            //nirmol
            var maxS = maxRank.FirstOrDefault(x => x.Value == maxRank.Values.Max());
            double maxScore = maxS.Value;
            Dictionary<string, double> toReturn = new Dictionary<string, double>();
            Dictionary<string, double> norm = new Dictionary<string, double>();
            foreach (string d in maxRank.Keys)
            {
                norm[d] = maxRank[d] / maxScore;
                // toReturn[d] = 0;
            }
            //now all bm25 are normaliz between 0 and 1

            docsRank.Clear();
            //nirmol bonus
            int countTerm;
            foreach (string docId in allRelevantDocs)
            {
                countTerm = 0;
                ranker.setDefualt();
                foreach (string qTerm in query.Keys)
                {
                    string qtermLow = qTerm.ToLower();
                    if (docs[docId].title.Contains(qtermLow))
                    {
                        ranker.addBonusTitle();
                    }
                    if (docs[docId].tag100.Contains(qtermLow))
                    {
                        ranker.addBonusTag100();
                    }
                    if (docs[docId].tag101.Contains(qtermLow))
                    {
                        ranker.addBonusTag101();
                    }
                    if (d[qtermLow].inFirstThird.ContainsKey(docId) && d[qtermLow].inFirstThird[docId])
                    {
                        ranker.addBonusFirstThird();
                    }
                    if (d[qtermLow].inLast10.ContainsKey(docId) && d[qtermLow].inLast10[docId])
                    {
                        ranker.addBonusLast10();
                    }
                    if (d[qtermLow].locations.ContainsKey(docId))
                    {
                        /*  int diff = docs[docId].max_tr - dict[qtermLow].locations[docId];
                          if (diff <= 20)
                              ranker.addBonusTopsDocTerm();*/
                        countTerm++;
                    }
                }

                docsRank[docId] = ranker.getBonusScore();
            }
            Dictionary<string, double> norm2 = new Dictionary<string, double>();
            maxS = docsRank.FirstOrDefault(x => x.Value == docsRank.Values.Max());
            maxScore = maxS.Value;
            foreach (string d in maxRank.Keys)
            {
                norm2[d] = docsRank[d] / maxScore;
                // toReturn[d] = 0;
            }
            //now bonus and BM25 normilized
            toReturn.Clear();
            foreach (string docId in allRelevantDocs)
            {
                ranker.bm25 = norm[docId];
                ranker.allBonuses = norm2[docId];
                toReturn[docId] = ranker.getFinalScore();
            }

            return toReturn;
        }


        public List<string> synForQuery(string data)
        {
            List<string> synForTerm = new List<string>();
           /* HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.datamuse.com/words?rel_syn=" + data);
            HttpWebResponse res = (HttpWebResponse)request.GetResponse();
            StreamReader sr = new StreamReader(res.GetResponseStream(), System.Text.Encoding.UTF8);
            string result = sr.ReadToEnd();
            sr.Close();
            res.Close();

            if (res.StatusCode == HttpStatusCode.OK)
            {
                JArray json = JArray.Parse(result);
                foreach (JToken j in json.Children())
                {
                    synForTerm.Add((string)j["word"]);
                }
            }*/
            return synForTerm;
        }

        private void loadDictionaryButton_Click()
        {
            string dictPath = @"C:\Users\ירדן\Desktop\לימודים\שנה ד\סמס1\אחזור מידע\data\posting";
            bool stemm = false;

            d = new Dictionary<string, TermInfo>();
            docs = new Dictionary<string, DocumentInfo>();
            string p = "";
            if (stemm)
            {
                p = dictPath + @"\DictionaryStemm.txt";
            }
            else
            {
                p = dictPath + @"\Dictionary.txt";
            }
            if (!File.Exists(p))
            {
                System.Windows.Forms.MessageBox.Show("Dictionary not found !");
            }
            else
            {
                Thread tDocs = new Thread(() => loadDocs(dictPath, stemm));
                tDocs.Start();
                string[] tempDic = File.ReadAllLines(p);
                for (int i = 0; i < tempDic.Length; i += 2)
                {
                    string[] t = tempDic[i].Split(',');
                    string term = t[0];
                    TermInfo ti = new TermInfo(t[0], int.Parse(t[1]), int.Parse(t[2]), t[3][0]);
                    ti.setPointer(int.Parse(t[4]));

                    t = tempDic[i + 1].Split(',');
                    foreach (string next in t)
                    {
                        ti.nextString.Add(next);
                    }
                    d[term] = ti;
                }
                tDocs.Join();

                System.Windows.Forms.MessageBox.Show("load data succeeded");
            }


        }

        private void loadDocs(string p, bool stemm)
        {
            string dictPath = "";
            if (stemm)
            {
                dictPath = p + "\\docsStemming.txt";
            }
            else
                dictPath = p + "\\docs.txt";
            if (File.Exists(dictPath))
            {
                string[] data = File.ReadAllLines(dictPath);
                for (int i = 0; i < data.Length; i += 9)
                {
                    DocumentInfo di = new DocumentInfo(data[i]);
                    di.numTermsInDoc = int.Parse(data[i + 1]);
                    di.dateOfPublish = data[i + 2];
                    di.language = data[i + 3];
                    di.title = data[i + 4];
                    di.max_tr = int.Parse(data[i + 5]);
                    di.uniqueTerms = int.Parse(data[i + 6]);
                    di.tag100 = data[i + 7];
                    di.tag101 = data[i + 8];
                    docs[data[i]] = di;

                    countDocsLength += di.numTermsInDoc;
                    countDocs++;
                }
            }
        }

    }
}
