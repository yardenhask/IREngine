using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace eng
{
    class Indexer
    {
        string path;
        int idx;
        public Dictionary<string, TermInfo> fullD;
        public int uniqueTermInCorpus;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="p"></param>
        public Indexer(string p)
        {
            path = p;
            idx = 0;
            fullD = new Dictionary<string, TermInfo>();
            uniqueTermInCorpus = 0;
        }
        /// <summary>
        /// build mini postings for dictionary
        /// </summary>
        /// <param name="terms"></param>
        public void buildInvertedIndex(Dictionary<string, TermInfo> terms)
        {
            List<string> sortTerms = terms.Keys.ToList();
            sortTerms.Sort();
            string fullPath = path + @"\" + idx + ".txt";
            
            using (FileStream fs = File.Create(fullPath))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    string newL = "";
                    foreach (string t in sortTerms)
                    {
                      //  newL = String.Join(",", terms[t].locations);
                        newL = terms[t].getStringOfDocs();
                        sw.WriteLine(t);
                        sw.Write(newL);
                        sw.WriteLine();

                        //Dictionary :
                        int listSize = terms[t].locations.Count;
                        int frecCurrPost = terms[t].corpusF;
                        if (fullD.ContainsKey(t))
                        {
                            fullD[t].docF += listSize;
                            fullD[t].corpusF += frecCurrPost;
                            fullD[t].addNextString(terms[t].nextStringFull);
                        }
                        else
                        {
                            TermInfo newT = new TermInfo(t, listSize, frecCurrPost, terms[t].type);
                            newT.nextStringFull = terms[t].nextStringFull;
                            fullD[t] = newT;
                        }
                    }
                    terms.Clear();
                }
            }

            idx++;

            
        }
        /// <summary>
        /// merge the mini posting
        /// </summary>
        /// <param name="stemm"></param>
        public void connectPostings(bool stemm)
        {
            string postingPath;
            if (stemm)
                postingPath = path + @"\postingStemm.txt";
            else
                postingPath = path + @"\postingNoStemm.txt";
            using (FileStream fs = File.Create(postingPath)) { }
            StreamWriter swFullPosting = new StreamWriter(postingPath);
            int lineNum = 0;
            
            List<string> termsFromDictinary=fullD.Keys.ToList();
            termsFromDictinary.Sort();
            int termIdx=0;
            string termFromDic = "";

            StreamReader[] postings = new StreamReader[idx];
            string[] tops = new string[idx];
            //init file 
            for (int i = 0; i < idx; i++)
            {
                string fullPath= path + @"\" + i + ".txt";
                postings[i] = new StreamReader(fullPath);
                tops[i] = postings[i].ReadLine();
            }

            string lineForPosting = "";

            while (termIdx < termsFromDictinary.Count)
            {
                termFromDic = termsFromDictinary[termIdx];
                termIdx++;
                for (int i = 0; i < idx; i++)
                {
                    if (tops[i] != "" && termFromDic.Equals(tops[i]))
                    {
                        string tmp=postings[i].ReadLine();
                        lineForPosting = String.Join(",", new string[] { lineForPosting, tmp });
                        if (postings[i].Peek() >= 0)
                          tops[i] = postings[i].ReadLine();
                        else
                        {
                            //close sr, delete file,  null value
                            tops[i] = "";
                            postings[i].Close();
                            File.Delete(path + @"\" + i + ".txt");
                        }
                    }
                }
                swFullPosting.WriteLine(lineForPosting);
                fullD[termFromDic].setPointer(lineNum);
                lineNum++;
                lineForPosting = "";
            }

            swFullPosting.Close();

            updateFullDTop5();
        }

        private void updateFullDTop5()
        {
            foreach (string t in fullD.Keys)
            {
                fullD[t].updateTop5();
            }
        }
        /// <summary>
        /// save dictionary for file
        /// </summary>
        /// <param name="stemm"></param>
        public void saveDict(bool stemm)
        {
            string pathForSave = "";
            if (stemm)
                pathForSave = path + @"\DictionaryStemm.txt";
            else
                pathForSave = path + @"\Dictionary.txt";
            using (FileStream fs = new FileStream(pathForSave, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {

                    foreach (string t in fullD.Keys)
                    {
                        sw.WriteLine(fullD[t]);
                        sw.WriteLine(String.Join(",", fullD[t].nextString) );
                      /*  if (fullD[t].type=='t')
                          fullD[t].synForTerm = saveSynForTerm(t);
                        if (fullD[t].synForTerm != null && fullD[t].synForTerm.Count>0)
                            sw.WriteLine(String.Join(",", fullD[t].synForTerm));
                        else
                            sw.WriteLine();*/
                        if (fullD[t].corpusF == 1)
                            uniqueTermInCorpus++;
                    }

                }
            }
        }

        
    }
}
