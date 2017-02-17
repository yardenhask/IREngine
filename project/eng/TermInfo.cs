using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eng
{
    public class TermInfo
    {
        //dictionary (docID,tf)
        //size of dictionay is df
        public Dictionary<string, int> locations;
        public Dictionary<string, bool> inFirstThird;
        public Dictionary<string, bool> inLast10;

        public string term;
        public int docF;
        public int corpusF;
        int pointer;
        public char type;
        //# number, % percent, $ price, e expression, - date/dist, d date, w kg, n name, t term

        public List<string> nextString;
        public Dictionary<string, int> nextStringFull;

      //  public List<string> synForTerm;

        /// <summary>
        /// constructor
        /// </summary>
        public TermInfo()
        { 
            locations = new Dictionary<string, int>(); 
            corpusF = 0;
            nextString = new List<string>();
            nextStringFull = new Dictionary<string, int>();
        //    synForTerm = new List<string>();
            inFirstThird = new Dictionary<string, bool>();
            inLast10 = new Dictionary<string, bool>();
        }

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="df"></param>
        /// <param name="cf"></param>
        /// <param name="tp"></param>
        public TermInfo(string name, int df, int cf, char tp)
        {
            term = name;
            docF = df;
            corpusF = cf;
            type = tp;
            nextString = new List<string>();
            nextStringFull = new Dictionary<string, int>();
         //   synForTerm = new List<string>();
            inFirstThird = new Dictionary<string, bool>();
            inLast10 = new Dictionary<string, bool>();
        }
        /// <summary>
        /// set pointer
        /// </summary>
        /// <param name="ptr"></param>
        public void setPointer(int ptr)
        {
            pointer = ptr;
        }
        /// <summary>
        /// getter to pointer
        /// </summary>
        /// <returns></returns>
        public int getPointer() { return pointer; }
        /// <summary>
        /// override to string method
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Join(",", new string[] { term, docF.ToString(), corpusF.ToString() ,type.ToString(), pointer.ToString()});

        }

        public string getStringOfDocs()
        {
            string ans = "";
            foreach (string s in locations.Keys)
            {
                if (inFirstThird[s] && inLast10[s])
                    ans = String.Join(",", new string[] {ans, s, locations[s].ToString(), "1-1" });
                else if (!inFirstThird[s] && !inLast10[s])
                    ans = String.Join(",", new string[] { ans, s, locations[s].ToString(), "0-0" });
                else if (inFirstThird[s] && !inLast10[s])
                    ans = String.Join(",", new string[] { ans, s, locations[s].ToString(), "1-0" });
                else if (!inFirstThird[s] && inLast10[s])
                    ans = String.Join(",", new string[] { ans, s, locations[s].ToString(), "0-1" });
                
            }
            return ans;
        }

        public void addNextString(Dictionary<string, int> dictionary)
        {
            foreach (string n in dictionary.Keys)
            {
                if (!nextStringFull.ContainsKey(n))
                {
                    nextStringFull[n] = dictionary[n];
                }
                else
                   nextStringFull[n]+=dictionary[n];
            }
        }

        public void addNextString(List<string> nexts)
        {
            foreach (string n in nexts)
            {
                if (!nextStringFull.ContainsKey(n))
                {
                    nextStringFull[n] = 0;
                }
                nextStringFull[n]++;
                /*if (nextString.Count == 5)
                {
                    break;
                }
                nextString.Add(n);*/
            }
        }

        /// <summary>
        /// get Current Frec of documents
        /// </summary>
        /// <returns></returns>
     /*   public int getNumOfCurrentFrecPostings()
        {
            List<int> frecs = locations.Values.ToList();
            int total=0;
            foreach (int x in frecs){
                total+=x;
            }
            return total;
        }*/





        public void updateTop5()
        {
            foreach (var item in nextStringFull.OrderByDescending(r => r.Value).Take(5))
            {
                nextString.Add(item.Key);
            }
        }
    }
}
