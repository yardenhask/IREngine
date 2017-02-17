using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace eng
{
    class ReadFile
    {
        Parse parser;
        HashSet<string> stopWords = new HashSet<string>();
        string[] files;
        int idxFile;
        public int countDocs;
        public int countDocL;
        public HashSet<DocumentInfo> docsInDataBase;
        bool stemm;
        
        string p;//


        /// <summary>
        /// constructor 
        /// </summary>
        /// <param name="path"></param>
        /// <param name="stemming"></param>
        public ReadFile(string path,bool stemming)
        {
            readStopWords(path);
            files = Directory.GetFiles(path, "FB*");
            idxFile=0;
            docsInDataBase = new HashSet<DocumentInfo>();
            stemm = stemming;
            countDocs = 0;
            countDocL = 0;
            p = path;//
        }

        /// <summary>
        /// read amount of files, for each file split to docs, parse the document, and build mini dictionary
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public Dictionary<string, TermInfo>  readBatch(int amount)
        {
            if (files.Length > 0)
            {
                Dictionary<string, TermInfo> termsInFiles = new Dictionary<string, TermInfo>();
                for (int i = 0; i < amount; i++)
                {
                    string text = System.IO.File.ReadAllText(files[idxFile]);
                    List<string> docs = getDocs(text);
                    foreach (string doc in docs)
                    {
                        parser = new Parse(stopWords, stemm);
                        Dictionary<string, int> termsInCurrDoc = parser.parseDoc(doc);// term and his tf from the doc
                        DocumentInfo d = parser.getDoc(); // doc details
                        d.numTermsInDoc = parser.countTerms;
                        #region doc FBIS-3366
                        /*   string s = d.docID.Trim(' ');
                        if (s.Equals("FBIS3-3366"))
                        {
                            
                            using (StreamWriter sw = new StreamWriter(p+"\\Y.txt"))
                            {
                                foreach (string ter in termsInCurrDoc.Keys)
                                {
                                    sw.WriteLine(String.Join(",",new string[]{ter , termsInCurrDoc[ter].ToString() }));
                                }
                                
                            }
                        }*/
                        #endregion
                        // add Terms From Doc To dictionary
                        foreach (KeyValuePair<string, int> curr in termsInCurrDoc)
                        {
                            if (!termsInFiles.ContainsKey(curr.Key))
                            {
                                TermInfo t = new TermInfo();
                                t.term = curr.Key;
                                t.locations[d.docID] = curr.Value;
                                t.type = parser.termType[t.term];
                                t.corpusF = curr.Value;
                                t.docF = 1;
                                if (parser.nextTerm.ContainsKey(curr.Key))
                                    t.addNextString(parser.nextTerm[curr.Key]);
                                t.inFirstThird[d.docID] = parser.termsInFirstThirdOfText[curr.Key];
                                t.inLast10[d.docID] = parser.termsInLast10OfText[curr.Key];
                                termsInFiles[curr.Key] = t;
                            }
                            else
                            {
                                termsInFiles[curr.Key].locations[d.docID]= curr.Value;
                                termsInFiles[curr.Key].corpusF+=curr.Value;
                                termsInFiles[curr.Key].docF ++;
                                if (parser.nextTerm.ContainsKey(curr.Key))
                                    termsInFiles[curr.Key].addNextString(parser.nextTerm[curr.Key]);
                                termsInFiles[curr.Key].inFirstThird[d.docID] = parser.termsInFirstThirdOfText[curr.Key];
                                termsInFiles[curr.Key].inLast10[d.docID] = parser.termsInLast10OfText[curr.Key];
                            }
                        }
                        
                        docsInDataBase.Add(d);
                        countDocL += d.numTermsInDoc;
                    }
                    idxFile++;
                }
                return termsInFiles;
            }
            return null;
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
        /// <summary>
        ///split file into list of docs
        /// </summary>
        /// <param name="files"></param>
        /// <returns></returns>
        private List<string> getDocs(string files)
        {
            List<string> docs = new List<string>();
            bool finish = false;
            int start = 0;
            int end = 0;
            string currDoc="";
            while (!finish)
            {
                start = files.IndexOf("<DOC>");
                if (start == -1)
                {
                    finish = true;
                    continue;
                }
                end = files.IndexOf("</DOC>");
                currDoc = files.Substring(start, end - start + 6);
                docs.Add(currDoc);
                files = files.Substring(end+6);
            }
            countDocs += docs.Count;

            return docs;
        }
    }
}
