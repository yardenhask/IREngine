using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace eng
{
    class Searcher
    {
        Dictionary<string, DocumentInfo> docs;
        Dictionary<string, TermInfo> dict;
        string postingPath;
        public Ranker ranker;
        HashSet<string> stopWords = new HashSet<string>();
       // Dictionary<string, double> finalRank;

        public Searcher(Dictionary<string, TermInfo> d, Dictionary<string, DocumentInfo> doc, string pp, int countD, int countDLengths, string stopPath)
        {
            dict = d;
            docs = doc;
            postingPath = pp;
            double avg=countDLengths/countD;
            ranker = new Ranker(countD, avg);
            readStopWords(stopPath);
        }
        /// <summary>
        /// recive a query and send the relavent posting.
        /// </summary>
        /// <param name="q"></param>
        /// <param name="stem"></param>
        /// <param name="langs"></param>
        /// <returns></returns>
        public List<string> parseQueryAndReturnRank(string q, bool stem, List<string> langs)
        {
            Parse p = new Parse(stopWords,stem);
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
                        if (dict.ContainsKey(syn))
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
                if (dict.ContainsKey(s))
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
            
            #region parse query
            foreach (string s in query.Keys)
            {
                if (dict.ContainsKey(s))
                {
                    dict[s].locations = new Dictionary<string, int>();
                    int ptr = dict[s].getPointer();
                    string postingOfTerm = File.ReadLines(postingPath).Skip(ptr).Take(1).First();
                    postingOfTerm = postingOfTerm.Substring(1);
                    string[] posting = postingOfTerm.Split(new char[]{',','[',']'},StringSplitOptions.RemoveEmptyEntries);
                    for (int i = 0; i < posting.Length; i += 3)
                    {
                        
                        if (langs != null)
                        {
                            foreach (string language in langs)
                            {
                                if (docs[posting[i]].language.Contains(language))
                                {
                                    dict[s].locations[posting[i]] = Int32.Parse(posting[i + 1]);
                                    int tmpidx = posting[i + 2].IndexOf('-');
                                    int tif3 = int.Parse(posting[i + 2].Substring(0, tmpidx));
                                    int til10 = int.Parse(posting[i + 2].Substring(tmpidx + 1));

                                    if (tif3 == 1)
                                        dict[s].inFirstThird[posting[i]] = true;
                                    else
                                        dict[s].inFirstThird[posting[i]] = false;
                                    if (til10 == 1)
                                        dict[s].inLast10[posting[i]] = true;
                                    else
                                        dict[s].inLast10[posting[i]] = false;
                                    allRelevantDocs.Add(posting[i]);
                           
                                }
                            }
                        }
                        else
                        {
                            dict[s].locations[posting[i]] = Int32.Parse(posting[i + 1]);
                     
                            int tmpidx = posting[i + 2].IndexOf('-');
                            int tif3 = int.Parse(posting[i + 2].Substring(0, tmpidx));
                            int til10 = int.Parse(posting[i + 2].Substring(tmpidx + 1));

                            if (tif3 == 1)
                                dict[s].inFirstThird[posting[i]] = true;
                            else
                                dict[s].inFirstThird[posting[i]] = false;
                            if (til10 == 1)
                                dict[s].inLast10[posting[i]] = true;
                            else
                                dict[s].inLast10[posting[i]] = false;
                            allRelevantDocs.Add(posting[i]);
                        }
                    }
                    postingsOfQueryTerm[s] = dict[s];
                    
                }
            }

            #endregion
            Dictionary<string, double> docsRank = ranking(query, queryTemp,postingsOfQueryTerm,allRelevantDocs,extandQueryIndex);

            
            List<string> sortDocs = new List<string>();
            var items = from pair in docsRank orderby pair.Value descending select pair;
            foreach (KeyValuePair<string, double> item in items.Take(50))
            {
                sortDocs.Add(item.Key);
            }
            return sortDocs;
        }

        /// <summary>
        /// read stop word file
        /// </summary>
        /// <param name="p"></param>
        private void readStopWords(string p)
        {
            if (File.Exists(p + @"\stop_words.txt"))
            {
                string[] words = File.ReadAllLines(p + @"\stop_words.txt");
                foreach (string word in words)
                {
                    stopWords.Add(word);
                }
            }
        }

        public List<string> synForQuery(string data)
        {
            List<string> synForTerm = new List<string>();
          /*  HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://api.datamuse.com/words?rel_syn=" + data);
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

        /// <summary>
        /// rank all documents of the query
        /// </summary>
        /// <param name="query"></param>
        /// <param name="postingsOfQueryTerm"></param>
        /// <param name="allRelevantDocs"></param>
        /// <returns></returns>
        private Dictionary<string, double> ranking
            (Dictionary<string, int> query,Dictionary<string, int> originQuery,
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
            double maxScore = maxS.Value ;
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
                    if (dict[qtermLow].inFirstThird.ContainsKey(docId) && dict[qtermLow].inFirstThird[docId])
                    {
                        ranker.addBonusFirstThird();
                    }
                    if (dict[qtermLow].inLast10.ContainsKey(docId) && dict[qtermLow].inLast10[docId])
                    {
                        ranker.addBonusLast10();
                    }
                    if (dict[qtermLow].locations.ContainsKey(docId))
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

        

        /// <summary>
        /// get a term and return 5 defernet imposible autocomplete
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public List<string> autoComplete(string s)
        {
            s = s.TrimEnd(new char[] { ' ' });
            List<string> ans = new List<string>();
            if (dict.ContainsKey(s))
            {
                ans.AddRange(dict[s].nextString);
            //    ans = dict[s].nextString;
            }
            return ans;
        }
    }
}
