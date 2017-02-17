using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eng
{
    class DocumentInfo
    {
        public string docID;
        public int numTermsInDoc=0;
        public string dateOfPublish = "-";
        public string language = "-";
        public string title="-";
        public int max_tr=0; 
        public int uniqueTerms=0;

        public string tag100 = "-";
        public string tag101 = "-";
        
        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="id"></param>
        public DocumentInfo(string id)
        {
            docID = id;
        }
        /// <summary>
        /// override to string method for writing to file
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return String.Join("\n", new string[] { docID, numTermsInDoc.ToString(), dateOfPublish, language,title, max_tr.ToString(), uniqueTerms.ToString(), tag100, tag101 });
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lang"></param>

    }
}
