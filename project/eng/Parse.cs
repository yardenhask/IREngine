using SearchEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace eng
{
    class Parse
    {
        DocumentInfo d;
        HashSet<string> stopWords;
        Dictionary<string, int> termsInDoc;
        public Dictionary<string, char> termType;
        public Dictionary<string, bool> termsInFirstThirdOfText;
        public Dictionary<string, bool> termsInLast10OfText;

        public int countTerms = 0;
        bool stemm;

        string[] monthsFullName = { "january", "february", "march", "april", "may", "june", "july", "august", "september", "october", "november", "december" };
        string[] monthsPartialName = { "jan", "feb", "mar", "apr", "may", "jun", "jul", "aug", "sep", "oct", "nov", "dec" };
        Dictionary<string, string> months=new Dictionary<string,string> ();

        public Dictionary<string, List<string>> nextTerm;
        string lastString;

        double idxOfTerm;
        double lengthOfDoc;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="stemming"></param>
        public Parse(HashSet<string> sp, bool stemming)
        {
            stopWords = sp;
            string tmpM = "";
            for (int i = 0; i < 12; i++)
            {
                tmpM = Convert.ToString(i+1);
                if (i < 9)
                    tmpM = "0" + tmpM;
                months[monthsFullName[i ]] = tmpM;
                months[monthsPartialName[i ]] = tmpM ;
            }
            termType = new Dictionary<string, char>();
            stemm = stemming;

            nextTerm = new Dictionary<string, List<string>>();
            lastString = "";
            termsInFirstThirdOfText = new Dictionary<string, bool>();
            termsInLast10OfText = new Dictionary<string, bool>();
        }

        /// <summary>
        /// get document, split it into header and content, and send to parse each part, 
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        public Dictionary<string, int> parseDoc(string doc)
        {
            termsInDoc = new Dictionary<string, int>(); // term and tf
            Tuple<string, string> data = getSplitDoc(doc);
            parseHeader(data.Item1);
            parseText(data.Item2);
            updateMaxTrANDUnique(termsInDoc);
            return termsInDoc;
        }
        /// <summary>
        /// return the current document details
        /// </summary>
        /// <returns></returns>
        public DocumentInfo getDoc() { return d; }
        /// <summary>
        /// find the max freq. term in document and uniques
        /// </summary>
        /// <param name="terms"></param>
        private void updateMaxTrANDUnique(Dictionary<string, int> terms)
        {
            int maxTr = 0;
            int uniques = 0;
            foreach (KeyValuePair<string,int> term in terms)
            {
                if (term.Value > maxTr)
                {
                    maxTr = term.Value;
                }
                if (term.Value == 1)
                {
                    uniques++;
                }
            }
            d.max_tr = maxTr;
            d.uniqueTerms = uniques;
        }
        /// <summary>
        /// split recived document into header and text
        /// </summary>
        /// <param name="doc"></param>
        /// <returns></returns>
        private Tuple<string,string> getSplitDoc(string doc)
        {  
            int endOfHeader =  doc.IndexOf("<TEXT>");
            string h = doc.Substring(6, endOfHeader);
            string t = doc.Substring(endOfHeader+6,doc.Length-endOfHeader-6-6-9);//-6=</doc> -9=</text>
            Tuple<string, string> splited = new Tuple<string, string>(h,t);
            return splited;
        }
        /// <summary>
        /// parse the header section
        /// save doc id, title, and date of publish
        /// </summary>
        /// <param name="text"></param>
        private void parseHeader(string text)
        {
            int start = text.IndexOf("<DOCNO>");
            int end = text.IndexOf("</DOCNO>");
            string docNo = text.Substring(start+7, end - start-7);
            d = new DocumentInfo(docNo);
            start = text.IndexOf("<DATE1>");
            end = text.IndexOf("</DATE1>");
            if (start != -1)
            {
                string docDate = text.Substring(start+7, end - start-7);
             /*   string dd=parseDateHeader(docDate);
                d.dateOfPublish = docDate;
                if (dd!="")*/
                d.dateOfPublish = docDate; //saveas shown or parse?
            }
            start = text.IndexOf("<TI>");
            end = text.IndexOf("</TI>");
            if (start != -1)
            {
                string docTitle = text.Substring(start + 4, end - start - 4);
                d.title = docTitle.Replace("\n", "") ;
                d.title = d.title.ToLower();
            }

            string textToParse = saveTagsOfAdditionalData(text);
        }

        private string parseDateHeader(string docDate)
        {
            string ans = "";
            docDate = docDate.ToLower();

            string[] date = docDate.Split(' ');
            date = date.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            ans = parseDate(date);
            return ans;
        }
        private string parseDate(string[] splitedText)
        {
            string toSave = "";
            int splLen = splitedText.Length;
            for (int i = 0; i < splLen; i++)
            {
                if (isDate(splitedText[i]))
                {
                    string m = months[splitedText[i]];
                    string d = "";
                    string y = "";

                    string prev = "";
                    string next = "";
                    if (i - 1 >= 0)
                        prev = splitedText[i - 1];
                    if (i + 1 < splLen)
                        next = splitedText[i + 1];
                    int nextNum;
                    int prevNum;
                    bool isNextIsNum = Int32.TryParse(next, out nextNum);
                    bool isPrevIsNum = Int32.TryParse(prev, out prevNum);
                    if (isPrevIsNum && prevNum < 32 && prevNum > 0)
                    {
                        d = prev;
                        if (prevNum < 10)
                            d = "0" + d;
                        if (!isNextIsNum)//2.1
                        {
                            //tosave
                            toSave = String.Join("-", new string[] { m, d });
                            break;
                        }
                        else
                        {
                            y = next;
                            if (nextNum < 100 && nextNum >= 0)//1.3
                            {
                                y = "19" + y;
                                //tosave
                                toSave = String.Join("-", new string[] { y, m, d });
                                break;
                            }
                            else if (nextNum > 999 && nextNum < 10000)//1.2
                            {
                                //tosave
                                toSave = String.Join("-", new string[] { y, m, d });
                                break;
                            }
                        }
                    }
                    else if (!isPrevIsNum)
                    {
                        string n = processNumForDate(prev);
                        if (n != "")
                        {
                            isPrevIsNum = Int32.TryParse(n, out prevNum);
                            if (isPrevIsNum)
                            {
                                d = n;
                                if (prevNum < 10)
                                    d = "0" + d;
                                if (nextNum > 999 && nextNum < 1000)//1.1
                                {
                                    y = next;
                                    //tosave
                                    toSave = String.Join("-", new string[] { y, m, d });
                                    break;
                                }
                            }
                        }
                        string afterNext = "";
                        if (i + 2 < splLen)
                            afterNext = updateStringInCell(splitedText[i + 2]);
                        int afterNum;
                        bool isAfterNum = Int32.TryParse(afterNext, out afterNum);
                        if (isAfterNum && afterNum > 999 && afterNum < 10000)//1.4
                        {
                            d = next;
                            if (nextNum < 10)
                                d = "0" + d;
                            y = afterNext;
                            //tosave
                            toSave = String.Join("-", new string[] { y, m, d });
                            break;
                        }
                        else if (isNextIsNum)
                        {
                            if (nextNum > 0 && nextNum < 32)//2.2
                            {
                                d = next;
                                if (nextNum < 10)
                                    d = "0" + d;
                                //tosave
                                toSave = String.Join("-", new string[] { m, d });
                                break;
                            }
                            else if (nextNum > 9999 && nextNum < 10000)//3.1
                            {
                                y = next;
                                //tosave
                                toSave = String.Join("-", new string[] { y, m });
                                break;
                            }
                        }
                    }

                }
            }
            return toSave;
        }
        /// <summary>
        /// parse the text section
        /// save tags, split the text and  
        /// </summary>
        /// <param name="text"></param>
        private void parseText(string text)
        {
            string textToParse=saveTagsOfAdditionalData(text);
            int l=textToParse.Length;
            int tenPercent = l / 10;
            string tmpText = textToParse.Substring(0,tenPercent);//no need to check more then 10%
            int startParseText = findStartingPoint(tmpText); 
            textToParse = textToParse.Substring(startParseText);
            textToParse = prepareText(textToParse);
            string[] splitedText = textToParse.Split(' ');
            splitedText = splitedText.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            getTerms(splitedText);  
        }
        /// <summary>
        /// parse query like document
        /// </summary>
        /// <param name="q"></param>
        /// <returns></returns>
        public Dictionary<string, int> parseQuery(string q)
        {
            termsInDoc = new Dictionary<string, int>();
            q = prepareText(q);
            string[] splitedText = q.Split(' ');
            splitedText = splitedText.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            getTerms(splitedText);
            return termsInDoc;
        }
        /// <summary>
        /// save tags in text
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private string saveTagsOfAdditionalData(string text)
        {
            string updatedText = text.ToLower();
            string tmp;
            int start = 0;
            int end = 0;
            while (start != -1)
            {
                start = updatedText.IndexOf("<f p=10");
                end = updatedText.IndexOf("</f>");
                if (start != -1)
                {
                    tmp = updatedText.Substring(start + 7, end - start - 7);
                    if (tmp[0] == '0')
                    {
                        tmp = tmp.Substring(2);
                        tmp = tmp.Replace("\n", "");
                        tmp = tmp.Replace(",", " ");
                        d.tag100 = tmp;
                    }
                    else if (tmp[0] == '1')
                    {
                        tmp = tmp.Substring(2);
                        tmp = tmp.Replace("\n", "");
                        tmp = tmp.Replace(",", " ");
                        d.tag101 = tmp;
                    }
                   /* else if (tmp[0] == '2')
                    {
                    }
                    else if (tmp[0] == '3')
                    {
                    }
                    else if (tmp[0] == '4')
                    {
                    }*/
                    else if (tmp[0] == '5')
                    {
                        //language
                        int idx = tmp.IndexOf("Language:");
                        if (idx == -1)
                        {
                            tmp = tmp.Substring(2);
                        }
                        else
                        {
                            tmp = tmp.Substring(idx+9);
                        }
                        d.language = tmp;
                    }
                 /*   else if (tmp[0] == '6')
                    {

                    }*/
                    updatedText = updatedText.Substring(end+4);
                }
            }
            return updatedText;
        }
        /// <summary>
        /// find where to start parse 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        private int findStartingPoint(string text)
        {
            int startParseText = text.IndexOf("[Text]");
            if (startParseText != -1)
                return startParseText + 6;
            if (startParseText == -1)
                startParseText = text.IndexOf("[Editorial Report]");
            if (startParseText != -1)
                return startParseText + 18;
            if (startParseText == -1)
                startParseText = text.IndexOf("[Excerpt]");
            if (startParseText != -1)
                return startParseText + 9;
            if (startParseText == -1)
                startParseText = text.IndexOf("[Excerpts]");
            if (startParseText != -1)
                return startParseText + 10;
            if (startParseText == -1)
                startParseText = text.IndexOf("[Editorial Report]");
            if (startParseText != -1)
                return startParseText + 18;
            if (startParseText == -1)
                startParseText = text.IndexOf("[ITIM report]");
            if (startParseText != -1)
                return startParseText + 13;
            if (startParseText == -1)
                startParseText = text.IndexOf("[Summary]");
            if (startParseText != -1)
                return startParseText + 9;
            if (startParseText == -1)
                startParseText = 0;
            return startParseText;
        }
        /// <summary>
        /// prepare the doc for parsing
        /// </summary>
        /// <param name="textToParse"></param>
        /// <returns></returns>
        private string prepareText(string textToParse)
        {
            //make hyphen inside the th expressin
            textToParse = textToParse.Replace(" - ", "-");
            textToParse = textToParse.Replace("- ", "-");
            textToParse = textToParse.Replace(" -", "-");
            //will make % and $ be inside array cell alone
            textToParse = textToParse.Replace("%", " % ");
            textToParse = textToParse.Replace("$", " $ ");

            textToParse = textToParse.Replace("~", " ");
            textToParse = textToParse.Replace(@"\", " ");
            textToParse = textToParse.Replace("...", " ");
            textToParse = textToParse.Replace("--", " ");
            textToParse = textToParse.Replace("\n", " "); 
            textToParse = textToParse.Replace("�", " ");   
            textToParse = textToParse.Replace("_", " ");
            textToParse = textToParse.Replace("+", " ");
            textToParse = textToParse.Replace("=", " ");
            textToParse = textToParse.Replace('"' + "", "");
            textToParse = textToParse.Replace("<", " ");
            textToParse = textToParse.Replace(">", " ");
            textToParse = textToParse.Replace("h3", " ");
            textToParse = textToParse.Replace("h4", " ");
            textToParse = textToParse.Replace("h5", " ");
            textToParse = textToParse.Replace("h6", " ");

            textToParse = textToParse.Replace("&", " ");
            if (textToParse[0] == ' ')
                textToParse = textToParse.Substring(1);
            return textToParse;
        }
        /// <summary>
        /// delete characters from string
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string updateStringInCell(string s)
        {
            s = s.Replace(",", "");
            s = s.Replace("(", "");
            s = s.Replace(")", "");
            s = s.Replace("[", "");
            s = s.Replace("]", "");
            s = s.Replace("{", "");
            s = s.Replace("}", "");
            s = s.Replace("|", "");
            s = s.Replace("'", "");
            s = s.Replace("`", "");
            s = s.Replace(":", "");
            s = s.Replace("!", "");
            s = s.Replace("?", "");
            s = s.Replace("#", "");
            s = s.Replace("@", "");
            s = s.Replace(";", "");
            s = s.Replace("*", "");
            s = s.Trim(new char[] { '.', '/', '\\', '-' });
            s = s.ToLower();
            return s;
        }
        /// <summary>
        /// check rules of text
        /// </summary>
        /// <param name="splitedText"></param>
        public void getTerms(string[] splitedText)
        {
            lengthOfDoc = splitedText.Length;
            for (int i = 0; i < splitedText.Length; i++)
            {
                idxOfTerm = i;
                if (!splitedText[i].Equals("") && splitedText[i] != null && !splitedText[i].Equals(" "))
                {
                    if (splitedText[i].Length < 1)
                    {
                        continue;
                    }
                    if (splitedText[i].Equals("*"))
                        continue; 
                    string toSave = "";
                    int splLen = splitedText.Length;

                    //cheack name- New Rule
                    #region name
                    if (Char.IsUpper(splitedText[i][0]))
                    {
                        
                        if (i + 1 < splLen && splitedText[i + 1] != "" && Char.IsUpper(splitedText[i + 1][0]))
                        {
                            char lastChar = splitedText[i][splitedText[i].Length - 1];
                            if (Char.IsLetter(lastChar) && splitedText[i].Length>1 && Char.IsLower(splitedText[i][1]) && splitedText[i+1].Length>1 && Char.IsLower(splitedText[i+1][1]))
                            {
                                toSave = String.Join(" ", new string[] { splitedText[i], splitedText[i + 1] });
                                lastChar=splitedText[i+1][splitedText[i+1].Length-1];
                                if (i + 2 < splitedText.Length && Char.IsUpper(splitedText[i + 2][0]) && Char.IsLetter(lastChar))
                                {
                                    splitedText[i + 2] = splitedText[i + 2].Trim(new char[] { '.', '-' });
                                    toSave = String.Join(" ", new string[] {toSave, splitedText[i + 2] });
                                    string s1="";
                                    s1 = updateStringInCell(splitedText[i]);
                                    if (stopWords==null || (stopWords!=null && !stopWords.Contains(s1)) )
                                       saveTerm(s1, 't');
                                    s1 = updateStringInCell(splitedText[i+1]);
                                    if (stopWords == null || (stopWords != null && !stopWords.Contains(s1)))
                                        saveTerm(s1, 't');
                                    s1 = updateStringInCell(splitedText[i+2]);
                                    if (stopWords == null || (stopWords != null && !stopWords.Contains(s1)))
                                        saveTerm(s1, 't');
                                    splitedText[i + 2] = "*";
                                    splitedText[i+1] = "*";
                                    splitedText[i] = "*";
                                    i += 2;
                                    toSave = updateStringInCell(toSave);
                                    saveTerm(toSave, 'n');
                                    continue;
                                }
                                string s2 = "";
                                s2 = updateStringInCell(splitedText[i]);
                                if (stopWords == null || (stopWords != null && !stopWords.Contains(s2)))
                                    saveTerm(s2, 't');
                                s2 = updateStringInCell(splitedText[i+1]);
                                if (stopWords == null || (stopWords != null && !stopWords.Contains(s2)))
                                    saveTerm(s2, 't');
                                
                                splitedText[i] = "*";
                                splitedText[i + 1] = "*";
                                toSave= updateStringInCell(toSave);
                                saveTerm(toSave, 'n');
                                i++;
                                continue;
                            }
                        }
                    }// end name rule
                    #endregion

                    //clean cell from signs
                    splitedText[i]= updateStringInCell(splitedText[ i]);

                    //if  is less then one letter
                    if (splitedText[i].Length < 1)
                        continue;

                    //hyphen:
                    #region hyphen
                    int first = splitedText[i].IndexOf('-');
                    if (first != -1)
                    {
                        string s1 = splitedText[i].Substring(0, first);
                        string s2 = splitedText[i].Substring(first + 1);
                        if (s2[0] != '-')
                        {
                            int second = s2.IndexOf('-');
                            if (second == -1)
                            {
                                saveTerm(splitedText[i], '-');
                                splitedText[i] = "*";
                                double tmpNum;
                                bool isTmpNum = Double.TryParse(s1, out tmpNum);
                                if (isTmpNum)
                                {
                                    saveTerm(tmpNum.ToString(), '#');
                                }
                                else
                                {
                                    s1 = s1.Trim(new char[] { '.', '/' });
                                    if (stopWords == null || (stopWords != null && !stopWords.Contains(s1)))
                                        saveTerm(s1, 't');
                                }
                                isTmpNum = Double.TryParse(s2, out tmpNum);
                                if (isTmpNum)
                                {
                                    saveTerm(tmpNum.ToString(), '#');
                                }
                                else
                                {
                                    s2 = s2.Trim(new char[] { '.', '/' });
                                    if (stopWords == null || (stopWords != null && !stopWords.Contains(s2))) 
                                        saveTerm(s2, 't');
                                }
                                continue;
                            }
                            else
                            {
                                double firstN;
                                bool isNumS = Double.TryParse(s1, out firstN);
                                if (!isNumS)
                                {
                                    string s3 = s2.Substring(0, second);
                                    s2 = s2.Substring(second + 1);
                                    isNumS = Double.TryParse(s2, out firstN);
                                    if (!isNumS)
                                    {
                                        isNumS = Double.TryParse(s3, out firstN);
                                        if (!isNumS)
                                        {
                                            saveTerm(splitedText[i], '-');
                                            splitedText[i] = "*";
                                            continue;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            s2 = s2.Trim(new char[] { '-' });
                            // s2=s2.Substring(1);
                            double tmpNum;
                            bool isTmpNum = Double.TryParse(s1, out tmpNum);
                            if (isTmpNum)
                            {
                                saveTerm(tmpNum.ToString(), '#');
                            }
                            else
                            {
                                s1 = s1.Trim(new char[] { '.', '/' });
                                if (stopWords == null || (stopWords != null && !stopWords.Contains(s1)))
                                    saveTerm(s1, 't');
                            }
                            isTmpNum = Double.TryParse(s2, out tmpNum);
                            if (isTmpNum)
                            {
                                saveTerm(tmpNum.ToString(), '#');
                            }
                            else
                            {
                                s2 = s2.Trim(new char[] { '.', '/' });
                                if (stopWords == null || (stopWords != null && !stopWords.Contains(s2)))
                                    saveTerm(s2, 't');
                            }
                            splitedText[i] = "*";
                            continue;
                        }
                    }

                    if (splitedText[i].Equals("between"))
                    {
                        if (i + 3 < splLen && splitedText[i + 2].ToLower().Equals("and"))
                        {
                            double num1, num2;
                            bool isNum1 = Double.TryParse(splitedText[i + 1], out num1);
                            bool isNum2 = Double.TryParse(splitedText[i + 3], out num2);
                            if (isNum1 && isNum2)
                            {
                                toSave = String.Join(" ", new string[] { splitedText[i], splitedText[i + 1], splitedText[i + 2], splitedText[i + 3] });
                                if (i + 4 < splLen && isFraction(splitedText[i + 4]))
                                {
                                    toSave = String.Join(" ", toSave, splitedText[i + 4]);
                                    toSave = updateStringInCell(toSave);
                                    saveTerm(toSave, '-');
                                    splitedText[i] = "*";
                                    splitedText[i + 1] = "*";
                                    splitedText[i + 2] = "*";
                                    splitedText[i + 3] = "*";
                                    splitedText[i + 4] = "*";
                                    i += 4;
                                    continue;
                                }
                                toSave = updateStringInCell(toSave);
                                saveTerm(toSave, '-');
                                splitedText[i] = "*";
                                splitedText[i + 1] = "*";
                                splitedText[i + 2] = "*";
                                splitedText[i + 3] = "*";
                                i += 3;
                                continue;
                            }
                        }
                        else if (i + 3 < splLen && splitedText[i + 3].ToLower().Equals("and"))
                        {
                            double num1, num2;
                            bool isNum1 = Double.TryParse(splitedText[i + 1], out num1);
                            bool isNum2 = Double.TryParse(splitedText[i + 4], out num2);
                            if (isNum1 && isNum2 && isFraction(splitedText[i + 2]))
                            {
                                toSave = String.Join(" ", new string[] { splitedText[i], splitedText[i + 1], splitedText[i + 2], splitedText[i + 3], splitedText[i + 4] });
                                if (i + 5 < splLen && isFraction(splitedText[i + 5]))
                                {
                                    toSave = String.Join(" ", toSave, splitedText[i + 5]);
                                    toSave = updateStringInCell(toSave);
                                    saveTerm(toSave, '-');
                                    splitedText[i] = "*";
                                    splitedText[i + 1] = "*";
                                    splitedText[i + 2] = "*";
                                    splitedText[i + 3] = "*";
                                    splitedText[i + 4] = "*";
                                    splitedText[i + 5] = "*";
                                    i += 5;
                                    continue;
                                }
                                toSave = String.Join(" ", toSave, splitedText[i + 4]);
                                toSave = updateStringInCell(toSave);
                                saveTerm(toSave, '-');
                                splitedText[i] = "*";
                                splitedText[i + 1] = "*";
                                splitedText[i + 2] = "*";
                                splitedText[i + 3] = "*";
                                splitedText[i + 4] = "*";
                                i += 4;
                                continue;
                            }
                        }
                    }
                    #endregion hyphen
                    // end hyphen

                    //date:
                    #region date
                    if (isDate(splitedText[i]))
                    {
                        string m = months[splitedText[i]];
                        string d = "";
                        string y = "";

                        string prev = "";
                        string next = "";
                        if (i-1>=0)
                            prev=updateStringInCell( splitedText[i-1]);
                        if (i+1<splLen)
                            next = updateStringInCell(splitedText[i + 1]);
                        int nextNum;
                        int prevNum;
                        bool isNextIsNum = Int32.TryParse(next, out nextNum);
                        bool isPrevIsNum = Int32.TryParse(prev, out prevNum);
                        if (isPrevIsNum && prevNum < 32 && prevNum>0)
                        {
                            d = prev;
                            if (prevNum < 10)
                                d = "0" + d;
                            if (!isNextIsNum)//2.1
                            {
                                //tosave
                                toSave = String.Join("-", new string[] { m, d });
                                saveTerm(toSave, 'd');//ddmm
                                splitedText[i - 1] = "*";
                                splitedText[i] = "*";
                                continue;
                            }
                            else 
                            {
                                y = next;
                                if (nextNum < 100 && nextNum >= 0)//1.3
                                {
                                    y = "19" + y;
                                    //tosave
                                    toSave = String.Join("-", new string[] {y, m, d });
                                    saveTerm(toSave, 'd');//ddmm
                                    splitedText[i - 1] = "*";
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;

                                }
                                else if (nextNum > 999 && nextNum < 10000)//1.2
                                {
                                   //tosave
                                    toSave = String.Join("-", new string[] { y, m, d });
                                    saveTerm(toSave, 'd');//ddmm
                                    splitedText[i - 1] = "*";
                                    splitedText[i] = "*";
                                    splitedText[i + 1] = "*";
                                    i++;
                                    continue;
                                }
                            }
                        }
                        else if (!isPrevIsNum)
                        {
                            string n = processNumForDate(prev);
                            if (n != "")
                            {
                                isPrevIsNum = Int32.TryParse(n, out prevNum);
                                if (isPrevIsNum)
                                {
                                    d = n;
                                    if (prevNum < 10)
                                        d = "0" + d;
                                    if (nextNum > 999 && nextNum < 1000)//1.1
                                    {
                                        y = next;
                                        //tosave
                                        toSave = String.Join("-", new string[] { y, m, d });
                                        saveTerm(toSave, 'd');//ddmm
                                        splitedText[i - 1] = "*";
                                        splitedText[i] = "*";
                                        splitedText[i + 1] = "*";
                                        i++;
                                        continue;
                                    }
                                }
                            }
                            string afterNext = "";
                            if (i+2<splLen)
                                afterNext = updateStringInCell(splitedText[i + 2]);
                            int afterNum;
                            bool isAfterNum = Int32.TryParse(afterNext, out afterNum);
                            if (isAfterNum && afterNum > 999 && afterNum < 10000 )//1.4
                            {
                                d = next;
                                if(nextNum<10)
                                    d = "0" + d;
                                y = afterNext;
                                //tosave
                                toSave = String.Join("-", new string[] { y, m, d });
                                saveTerm(toSave, 'd');//ddmm
                                splitedText[i + 2] = "*";
                                splitedText[i] = "*";
                                splitedText[i + 1] = "*";
                                i+=2;
                                continue;
                            }
                            else if (isNextIsNum  )
                            {
                                if (nextNum > 0 && nextNum < 32)//2.2
                                {
                                    d = next;
                                    if (nextNum < 10)
                                        d = "0" + d;
                                    //tosave
                                    toSave = String.Join("-", new string[] { m, d });
                                    saveTerm(toSave, 'd');//ddmm
                                    splitedText[i] = "*";
                                    splitedText[i + 1] = "*";
                                    i++;
                                    continue;
                                }
                                else if (nextNum > 9999 && nextNum < 10000)//3.1
                                {
                                    y = next;
                                    //tosave
                                    toSave = String.Join("-", new string[] { y, m });
                                    saveTerm(toSave, 'd');//ddmm
                                    splitedText[i] = "*";
                                    splitedText[i + 1] = "*";
                                    i++;
                                    continue;
                                }
                            }   
                        }
                       
                    }// end date rule
                    #endregion date
                    //end date

                    //fraction
                    #region fraction
                    int t = splitedText[i].IndexOf('/');
                    if (t!=-1)
                    {//fraction after number already taking care of *
                        if (i - 1 >= 0)
                        {
                            double numBefore;
                            bool isNumBefore = Double.TryParse(splitedText[i - 1], out numBefore);
                            if (isNumBefore)
                                continue;
                        }
                        string s1 = splitedText[i].Substring(0, t);
                        string s2 = splitedText[i].Substring(t + 1);
                        double num1;
                        bool isNum1 = Double.TryParse(s1, out num1);
                        double num2;
                        bool isNum2 = Double.TryParse(s2, out num2);
                        if (isNum1 && isNum2)
                        {
                            saveTerm(splitedText[i], '#');
                            splitedText[i] = "*";
                            continue;
                        }

                        saveTerm(splitedText[i], 't');
                      //  s2 = s2.Trim('/');
                        s2 = s2.Trim(new char[] { '.', '/' });
                        if (s2.IndexOf('/') == -1)
                        {
                            if (isNum1)
                                saveTerm(s1, '#');
                            else
                                if (stopWords == null || (stopWords != null && !stopWords.Contains(s1)))
                                    saveTerm(s1, 't');
                            if (isNum2)
                                saveTerm(s2, '#');
                            else
                                if (stopWords == null || (stopWords != null && !stopWords.Contains(s2)))
                                    saveTerm(s2, 't');
                        }
                        splitedText[i] = "*";
                        
                        continue;
                    }
                    #endregion

                    double num;
                    bool isNum=Double.TryParse(splitedText[i],out num);
                    // number, prices and percet rules
                    if (isNum)
                    {
                        #region number, price, percent, kg
                        #region bigger the million
                        if (num >= 1000000)
                        {
                            num = num / 1000000;
                            
                            toSave = String.Join(" ", new String[] { num.ToString(), "m" });
                            if  (i + 1 < splLen && splitedText[i + 1].Equals("dollars"))
                            {
                                toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                saveTerm(toSave, '$');
                                splitedText[i] = "*";
                                splitedText[i+1] = "*";
                                i++;
                                continue;
                            }
                            else if (i - 1 >= 0 && splitedText[i - 1].Equals("$"))  
                            {
                                toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                saveTerm(toSave, '$');
                                splitedText[i] = "*";
                                splitedText[i-1] = "*";
                                continue;
                            }
                            else if (i + 1 < splLen && (splitedText[i + 1].Equals("%")
                                    || splitedText[i + 1].Equals("percent")
                                    || splitedText[i + 1].Equals("percentage")))
                            {
                                toSave = String.Join(" ", new String[] { toSave, "%" });
                                saveTerm(toSave, '%');
                                splitedText[i] = "*";
                                splitedText[i+1] = "*";
                                i++;
                                continue;
                            }
                            splitedText[i] = "*";
                            saveTerm(toSave, '#');
                            continue;
                        }
                        #endregion bigger million
                        #region less then million
                        else
                        {
                            toSave = splitedText[i];
                            if (i + 1 < splLen && isDate(splitedText[i + 1].ToLower()))
                                continue;
                            else if (i+1<splLen)
                            {
                                
                                string nextCell = splitedText[i + 1].ToLower();
                                #region %
                                if (i + 1 < splLen && ( splitedText[i + 1].Equals("%")
                                    || nextCell.Equals("percent")
                                    || nextCell.Equals("percentage")))
                                {
                                    toSave = String.Join(" ", new String[] { toSave, "%" });
                                    saveTerm(toSave, '%');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                #endregion %
                                #region kg new rule
                                if (i + 1 < splLen && (nextCell.Equals("gr")
                                    || nextCell.Equals("kg")
                                    || nextCell.Equals("tonne")))
                                {

                                    if (nextCell.Equals("gr"))
                                    {
                                        num = num / 1000;

                                    }
                                    else if (nextCell.Equals("tonne"))
                                    {
                                        num = num * 1000;
                                    }
                                    toSave = String.Join(" ", new String[] { num.ToString(), "kg" });
                                    saveTerm(toSave, 'w');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                #endregion
                                #region $ and miliion trillion billion
                                else if (i + 1 < splLen && nextCell.Equals("dollars"))
                                {
                                    toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                    saveTerm(toSave, '$');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                else if (i + 1 < splLen && isFraction(splitedText[i + 1]))
                                {
                                    toSave += splitedText[i + 1];
                                    if (i + 2 < splLen && splitedText[i + 2].ToLower().Equals("dollars"))
                                    {
                                        toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                        toSave = updateStringInCell(toSave);
                                        saveTerm(toSave, '$');
                                        splitedText[i] = "*";
                                        splitedText[i+1] = "*";
                                        splitedText[i+2] = "*";
                                        i++;
                                        i++;
                                        continue;
                                    }
                                    toSave = updateStringInCell(toSave);
                                    saveTerm(toSave, '#');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                else if (i - 1 >= 0 && splitedText[i - 1].Equals("$"))
                                {

                                    if (i + 1 < splLen && nextCell.Equals("million"))
                                    {
                                        toSave = String.Join(" ", new String[] { splitedText[i], "m dollars" });
                                        saveTerm(toSave, '$');
                                        splitedText[i] = "*";
                                        splitedText[i+1] = "*";
                                        i++;
                                        continue;
                                    }
                                    else if (i + 1 < splLen && nextCell.Equals("billion"))
                                    {
                                        num = num * 1000;
                                        toSave = String.Join(" ", new String[] { num.ToString(), "m dollars" });
                                        saveTerm(toSave, '$');
                                        splitedText[i] = "*";
                                        splitedText[i+1] = "*";
                                        i++;
                                        continue;
                                    }
                                    else if (i + 1 < splLen && nextCell.Equals("trillion"))
                                    {
                                        num = num * 1000000;
                                        toSave = String.Join(" ", new String[] { num.ToString(), "m dollars" });
                                        saveTerm(toSave, '$');
                                        splitedText[i] = "*";
                                        splitedText[i+1] = "*";
                                        i++;
                                        continue;
                                    }
                                    else
                                    {
                                        toSave = String.Join(" ", new String[] { splitedText[i], "m dollars" });
                                        saveTerm(toSave, '$');
                                        splitedText[i] = "*";
                                        continue;
                                    }
                                }
                                else if (i + 1 < splLen && nextCell.Equals("m"))
                                {
                                    toSave = splitedText[i] + " m";
                                    if (i + 2 < splLen && splitedText[i + 2].ToLower().Equals("dollars"))
                                    {
                                        toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                        saveTerm(toSave, '$');
                                        splitedText[i] = "*";
                                        splitedText[i+1] = "*";
                                        splitedText[i+2] = "*";
                                        i++;
                                        i++;
                                        continue;
                                    }
                                    saveTerm(toSave, '#');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                else if (i + 1 < splLen && nextCell.Equals("bn"))
                                {
                                    num = num * 1000;
                                    toSave = String.Join(" ", new String[] { num.ToString(), "m" });
                                    if (i + 2 < splLen && splitedText[i + 2].ToLower().Equals("dollars"))
                                    {
                                        toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                        saveTerm(toSave, '$');
                                        splitedText[i] = "*";
                                        splitedText[i+1] = "*";
                                        splitedText[i+2] = "*";
                                        i++;
                                        i++;
                                        continue;
                                    }
                                    saveTerm(toSave, '#');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                if (i + 1 < splLen && nextCell.Equals("million"))
                                {
                                    toSave = String.Join(" ", new String[] { splitedText[i], "m" });
                                    if (i + 2 < splLen && splitedText[i + 2].ToLower().Equals("u.s."))
                                    {
                                        if (i + 3 < splLen && splitedText[i + 3].ToLower().Equals("dollars"))
                                        {
                                            toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                            toSave = updateStringInCell(toSave);
                                            saveTerm(toSave,'$');
                                            splitedText[i] = "*";
                                            splitedText[i+1] = "*";
                                            splitedText[i+2] = "*";
                                            splitedText[i+3] = "*";
                                            i += 3;
                                            continue;
                                        }
                                    }
                                    toSave = updateStringInCell(toSave);
                                    saveTerm(toSave, '#');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                else if (i + 1 < splLen && nextCell.Equals("billion"))
                                {
                                    num = num * 1000;
                                    toSave = String.Join(" ", new String[] { num.ToString(), "m" });
                                    if (i + 2 < splLen && splitedText[i + 2].ToLower().Equals("u.s."))
                                    {
                                        if (i + 3 < splLen && splitedText[i + 3].ToLower().Equals("dollars"))
                                        {
                                            toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                            saveTerm(toSave, '$');
                                            splitedText[i] = "*";
                                            splitedText[i+1] = "*";
                                            splitedText[i+2] = "*";
                                            splitedText[i+3] = "*";
                                            i+=3;
                                            continue;
                                        }
                                    }
                                    saveTerm(toSave, '#');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                                else if (i + 1 < splLen && nextCell.Equals("trillion"))
                                {
                                    num = num * 1000000 ;
                                    toSave = String.Join(" ", new String[] { num.ToString(), "m" });
                                    if (i + 2 < splLen && splitedText[i + 2].ToLower().Equals("u.s."))
                                    {
                                        if (i + 3 < splLen && splitedText[i + 3].ToLower().Equals("dollars"))
                                        {
                                            toSave = String.Join(" ", new String[] { toSave, "dollars" });
                                            saveTerm(toSave, '$');
                                            splitedText[i] = "*";
                                            splitedText[i+1] = "*";
                                            splitedText[i+2] = "*";
                                            splitedText[i+3] = "*";
                                            i += 3;
                                            continue;
                                        }
                                    }
                                    saveTerm(toSave, '#');
                                    splitedText[i] = "*";
                                    splitedText[i+1] = "*";
                                    i++;
                                    continue;
                                }
                            }
                            
                            saveTerm(toSave, '#');
                            splitedText[i] = "*";
                            continue;
                                #endregion
                        }
                        #endregion less then million
                        #endregion
                    }//end num, price and percent rules
                    else
                    {                        
                         toSave = splitedText[i];
                         #region useual term
                         toSave = toSave.Trim(new char[] { '.', '/' });
                         if (toSave != "")
                        {
                            if (toSave[0] == '&')
                                continue;
                            
                            if (stemm)
                            {
                                Stemmer s = new Stemmer();
                                toSave = s.stemTerm(toSave);
                            }



                            if (toSave.Length > 1 && (stopWords==null || (stopWords!=null && !stopWords.Contains(toSave)) ) )
                                saveTerm(toSave, 't');
                            splitedText[i] = "*";
                            continue;
                        }
                         #endregion
                    }


                }
            }
        }
        /// <summary>
        /// proces number for date-delete th,st,nd,rd
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string processNumForDate(string s)
        {
            int l = s.Length;
            if (l > 1)
            {
                string end = s.Substring(l - 2).ToLower();
                if (end.Equals("st") || end.Equals("nd") || end.Equals("rd") || end.Equals("th"))
                {
                    return s.Substring(0, l - 2);
                }
            }
            return "";
        }
        /// <summary>
        /// check if string is fraction
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool isFraction(string s)
        {
            if (!s.Equals("") && s != null && !s.Equals(" "))
            {
                if (Char.IsDigit(s[0]))
                {
                    int t = s.IndexOf('/');
                    if (t != -1)
                    {
                        string numerator = s.Substring(0, t);
                        string denominator = s.Substring(t+1);
                        double numer, denom;
                        bool isNum = Double.TryParse(numerator, out numer);
                        bool isNum2 = Double.TryParse(denominator, out denom);
                        if (isNum && isNum2)
                             return true;
                    }
                }
            }
            return false;
        }    
        /// <summary>
        /// cheack if the string is month
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private bool isDate(string s)
        {
            bool isDate = false;
            if (monthsFullName.Contains(s) || monthsPartialName.Contains(s))
                isDate = true;
            return isDate;
        }
        /// <summary>
        /// save the term to the dictionary
        /// </summary>
        /// <param name="toSave"></param>
        /// <param name="type"></param>
        private void saveTerm(string toSave, char type)
        {

            if (toSave != "")
            {
                if (stemm)
                {
                    Stemmer s = new Stemmer();
                    toSave = s.stemTerm(toSave);
                }

                if (termsInDoc.ContainsKey(toSave))
                {
                    termsInDoc[toSave]++;
                }
                else
                {
                    termsInDoc[toSave] = 1;
                    termType[toSave] = type;
                    countTerms++;
                    double tmp=idxOfTerm/lengthOfDoc;
                    if (tmp <= 0.30)
                    {
                        termsInFirstThirdOfText[toSave] = true;
                    }
                    else if (!termsInFirstThirdOfText.ContainsKey(toSave))
                    {
                        termsInFirstThirdOfText[toSave] = false ;
                    }
                    if (tmp >= 0.9)
                    {
                        termsInLast10OfText[toSave] = true;
                    }
                    else if (!termsInLast10OfText.ContainsKey(toSave))
                    {
                        termsInLast10OfText[toSave] = false;
                    }
                }


                if (lastString != "")
                {
                    //for auto complete
                    if (!nextTerm.ContainsKey(lastString))
                    {
                        nextTerm[lastString] = new List<string>();

                    }
                    nextTerm[lastString].Add(toSave);
                    
                }
                lastString = toSave;
            }
        }

    }
}
