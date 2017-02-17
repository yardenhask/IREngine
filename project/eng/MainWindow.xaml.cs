using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Data;
using System.Net;
using System.Xml;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace eng
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ReadFile rf;
        Indexer idxer;
        List<string> languages;
        Dictionary<string, TermInfo> d;
        Dictionary<string, DocumentInfo> docs;
        bool isSearechOpen;
        Searcher s;
        int countDocsLength;
        int countDocs;
        Dictionary<int, List<string>> results;

        public MainWindow()
        {
            InitializeComponent();

           // FindParam fp = new FindParam();


            d = new Dictionary<string, TermInfo>();
            docs = new Dictionary<string, DocumentInfo>();
            languages = new List<string>();
            setLanguagesList();
            isSearechOpen = false;
            countDocsLength = 0;
            countDocs = 0;
        }

        /// <summary>
        /// browse for corpus dirctory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseCorpus_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description ="Select the directory of corpus";
            folderBrowserDialog.ShowDialog();
            PathCorpus.Text = folderBrowserDialog.SelectedPath;
        }
        /// <summary>
        /// browse for posting and dictionary directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowsePostingDictionary_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the directory of corpus";
            folderBrowserDialog.ShowDialog();
            PathPostingDictionary.Text = folderBrowserDialog.SelectedPath;
        }
        /// <summary>
        /// start button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            bool send= (stemmingCheck.HasContent && stemmingCheck.IsChecked==true );
            string corpus = PathCorpus.Text;
            string posting = PathPostingDictionary.Text;
            Thread start = new Thread(() => startThread(corpus, posting, send ));
            start.Start();
            System.Windows.Forms.MessageBox.Show("Parsing has started. Please wait.");
       //     start.Join();
        }
        private void startThread(string corpus, string savePostDict, bool needToStemm)
        {
            //string corpus = PathCorpus.Text;
           // string savePostDict = PathPostingDictionary.Text;
         /*   bool needToStemm = false;
            if (stemmingCheck.HasContent && stemmingCheck.IsChecked == true)
            {
                needToStemm = true;
            }*/
            bool allGood = true;
            //check files exist !!
            if (!Directory.Exists(savePostDict))
            {
                System.Windows.Forms.MessageBox.Show("Directory for posting not found");
                allGood = false;
            }
            if (!Directory.Exists(corpus))
            {
                System.Windows.Forms.MessageBox.Show("Directory for corpus not found");
                allGood = false;
            }
            if (!File.Exists(corpus + @"\stop_words.txt"))
            {
                System.Windows.Forms.MessageBox.Show("Missing stop words file");
                allGood = false;
            }
            if (allGood)
            {
                rf = new ReadFile(corpus, needToStemm);
                idxer = new Indexer(savePostDict);
                int numOfDisk = 82;
                int numOfFilesInOneItert = 492 / numOfDisk;

                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                for (int i = 0; i < numOfDisk; i++)
                {
                    //        idxer.buildInvertedIndex(rf.readBatch(numOfFilesInOneItert));
                    Dictionary<string, TermInfo> tmp = rf.readBatch(numOfFilesInOneItert);
                    if (tmp == null)
                        System.Windows.Forms.MessageBox.Show("Missing files");
                    else
                        idxer.buildInvertedIndex(tmp);
                }

                idxer.connectPostings(needToStemm);
                idxer.saveDict(needToStemm);

                stopWatch.Stop();
                d = idxer.fullD;
                TimeSpan ts = stopWatch.Elapsed;
                string elapsedTime = String.Format("{0:00}:{1:00}.{2:00}",
                 ts.Minutes, ts.Seconds, ts.Milliseconds / 10);
                Thread saveDocs = new Thread(() => saveDocsDetails(savePostDict, needToStemm));
                saveDocs.Start();
                System.Windows.Forms.MessageBox.Show("Total time: " + elapsedTime + "\n #Documents: " + rf.countDocs + "\n #Unique terms: " + idxer.uniqueTermInCorpus);
                countDocs = rf.countDocs;
                countDocsLength = rf.countDocL;
                saveDocs.Join();
            }
        }
        /// <summary>
        /// save docs details for file
        /// </summary>
        /// <param name="p"></param>
        private void saveDocsDetails(string p,bool stemm)
        {
            if (stemm)
            {
                p = p + "\\docsStemming.txt";
            }
            else
                 p = p + "\\docs.txt";
            List<DocumentInfo> docToSave = rf.docsInDataBase.ToList<DocumentInfo>();
            using (FileStream fs = new FileStream(p, FileMode.OpenOrCreate))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(String.Join("\n", docToSave));
                }
            }
            
        }
        /// <summary>
        /// load docs details from file
        /// </summary>
        /// <param name="p"></param>
        private void loadDocs(string p,bool stemm)
        {
            string dictPath = "";
            if (stemm)
            {
                dictPath = p + "\\docsStemming.txt";
            }
            else
                dictPath = p +"\\docs.txt";
            if (File.Exists(dictPath ))
            {
                string[] data = File.ReadAllLines(dictPath);
                for (int i = 0; i < data.Length; i+=9)
                {
                    DocumentInfo di = new DocumentInfo(data[i]);
                    di.numTermsInDoc=int.Parse(data[i+1]);
                    di.dateOfPublish=data[i+2];
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
        /// <summary>
        /// reset button to posting and dictionary file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void resetButton_Click(object sender, RoutedEventArgs e)
        {
            string postDict = PathPostingDictionary.Text;
            bool fileMissing = true;
            if (File.Exists(postDict + @"\Dictionary.txt"))
            {
                File.Delete(postDict + @"\Dictionary.txt");
                fileMissing = false;
            }
            if (File.Exists(postDict + @"\DictionaryStemm.txt"))
            {
                File.Delete(postDict + @"\DictionaryStemm.txt");
                fileMissing = false;
            }
            if (File.Exists(postDict + @"\postingStemm.txt"))
            {
                File.Delete(postDict + @"\postingStemm.txt");
                fileMissing = false;
            }
            if (File.Exists(postDict + @"\postingNoStemm.txt"))
            {
                File.Delete(postDict + @"\postingNoStemm.txt");
                fileMissing = false;
            }
            if (File.Exists(postDict + @"\docs.txt"))
            {
                File.Delete(postDict + @"\docs.txt");
                fileMissing = false;
            }
            if (File.Exists(postDict + @"\docsStemming.txt"))
            {
                File.Delete(postDict + @"\docsStemming.txt");
                fileMissing = false;
            }
            if (fileMissing)
            {
                System.Windows.Forms.MessageBox.Show("Files are missing !");
            }
            else
            {
                System.Windows.Forms.MessageBox.Show("Posting files have been deleted !");
            }
            d = new Dictionary<string, TermInfo>();
            docs = new Dictionary<string, DocumentInfo>();
            
        }
        /// <summary>
        /// show dictionary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void showDictionaryButton_Click(object sender, RoutedEventArgs e)
        {
            if (d==null || d.Count==0)
            {
                System.Windows.Forms.MessageBox.Show("No Dictionary to Show !");
            }
            else
            {
                ShowDictionary sd = new ShowDictionary(d);
                sd.Show();
            }
        }
        /// <summary>
        /// load dictionary to memory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void loadDictionaryButton_Click(object sender, RoutedEventArgs e)
        {
            string dictPath = PathPostingDictionary.Text;
            bool stemm = false;
            if (stemmingCheck.HasContent && stemmingCheck.IsChecked == true)
            {
                stemm = true;
            }
            d = new Dictionary<string, TermInfo>();
            docs = new Dictionary<string, DocumentInfo>();
            string p = "";
            if (stemm)
            {
                p= dictPath + @"\DictionaryStemm.txt";
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
                Thread tDocs = new Thread(() => loadDocs(dictPath,stemm));
                tDocs.Start();
                string[] tempDic = File.ReadAllLines(p);
                for (int i = 0; i < tempDic.Length; i+=2)
                {
                    string[] t = tempDic[i].Split(',');
                    string term=t[0];
                    TermInfo ti = new TermInfo(t[0], int.Parse(t[1]), int.Parse(t[2]), t[3][0]);
                    ti.setPointer(int.Parse(t[4]));
                    
                    t = tempDic[i+1].Split(',');
                    foreach (string next in t)
                    {
                        ti.nextString.Add(next);
                    }

                 /*   t = tempDic[i + 2].Split(',');
                    foreach (string next in t)
                    {
                        ti.synForTerm.Add(next);
                    }*/


                    d[term] = ti;
                }
                    /*
                    foreach (string term in tempDic)
                    {
                        string[] t = term.Split(',');
                        TermInfo ti = new TermInfo(t[0], int.Parse(t[1]), int.Parse(t[2]), t[3][0]);
                        ti.setPointer(int.Parse(t[4]));
                        d[t[0]] = ti;
                    }*/
                    tDocs.Join();

                System.Windows.Forms.MessageBox.Show("load data succeeded");

               
            }


        }

        /// <summary>
        /// write all languages to file
        /// </summary>
        /// <param name="docsp"></param>
      /*  private void getLangs(string docsp)
        {
            HashSet<string> lang = new HashSet<string>();
            loadDocs(docsp,false);
            foreach (string s in docs.Keys)
            {
                lang.Add(docs[s].language.Replace(" ",""));
            }
            using (FileStream fs = new FileStream(docsp + "//lang.txt",FileMode.Create))
            {

            }

            File.WriteAllLines(docsp + "//lang.txt", lang);
        }*/
        /// <summary>
        /// read languages from file and add them to combo list of languages
        /// </summary>
        private void setLanguagesList()
        {
            string[] langs = File.ReadAllLines("lang.txt");
            langs = langs.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            foreach (string s in langs)
            {
             //   LangCombo.Items.Add(s);
                LangList.Items.Add(s);
            }
        }
        /// <summary>
        /// open the section of searching in the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void searchWindowButton_Click(object sender, RoutedEventArgs e)
        {
            if (!isSearechOpen)
            {
                if (d.Count == 0)
                {
                    System.Windows.Forms.MessageBox.Show("You Must Load a Dictionary Before Searching !");
                }
                else
                {
                    bool needToStemm = false;
                    if (stemmingCheck.HasContent && stemmingCheck.IsChecked == true)
                    {
                        needToStemm = true;
                    }
                    string postPath = "";
                    if (needToStemm)
                    {
                        postPath = PathPostingDictionary.Text + "\\postingStemm.txt";
                    }
                    else
                    {
                        postPath = PathPostingDictionary.Text + "\\postingNoStemm.txt";
                    }
                    mainWindow.Height = 530;
                    isSearechOpen = true;
                    s = new Searcher(d, docs, postPath, countDocs, countDocsLength, PathPostingDictionary.Text+"\\stopwords.txt");
                }
            }
            else
            {
                mainWindow.Height = 325;
                isSearechOpen = false;
            }
        }
       

        /// <summary>
        /// browse button for file of queries
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BrowseQuery_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.ShowDialog();
            FileQuery.Text = openFileDialog1.FileName;
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (SingleQuery.Text.Count() > 1 || FileQuery.Text.Count()>1)
            {
                bool needToStemm = false;
                if (stemmingCheck.HasContent && stemmingCheck.IsChecked == true)
                {
                    needToStemm = true;
                }
                List<string> selectedLanguages = new List<string>();
                foreach (var item in LangList.SelectedItems)
                {
                    selectedLanguages.Add(item.ToString());
                }

                int currTag = searchOptionTag.SelectedIndex;
                List<string> documentsFromQuery;
                results = new Dictionary<int, List<string>>();
                if (currTag == 0)//single query
                {
                    if (selectedLanguages.Count == 0)
                        documentsFromQuery = s.parseQueryAndReturnRank(SingleQuery.Text, needToStemm, null);
                    else
                        documentsFromQuery = s.parseQueryAndReturnRank(SingleQuery.Text, needToStemm, selectedLanguages);
                    results[0] = documentsFromQuery;///////////******************************
                    System.Windows.Forms.MessageBox.Show("Finish!");
                }
                else//file of queries
                {
                    if (File.Exists(FileQuery.Text))
                    {
                        string[] queries = File.ReadAllLines(FileQuery.Text);
                        foreach (string fq in queries)
                        {
                            string gueryInLine = fq.Substring(fq.IndexOf(" ") + 1);
                            string queryId = fq.Substring(0, fq.IndexOf(" "));
                            int id = int.Parse(queryId);
                            gueryInLine = gueryInLine.TrimStart(new char[] { ' ' });
                            if (selectedLanguages.Count == 0)
                                documentsFromQuery = s.parseQueryAndReturnRank(gueryInLine, needToStemm, null);
                            else
                                documentsFromQuery = s.parseQueryAndReturnRank(gueryInLine, needToStemm, selectedLanguages);
                            results[id] = documentsFromQuery;
                        }
                        System.Windows.Forms.MessageBox.Show("Finish!");
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show("file not exist !");
                    }
                }

            }
        }

        private void showSearchResultButton_Click(object sender, RoutedEventArgs e)
        {
            if (results != null && results.Count > 0)
            {
                SearchResult sr = new SearchResult(results);
                sr.Show();
            }
        }

        private void SingleQuery_TextChanged(object sender, TextChangedEventArgs e)
        {
            autoCompleList.ItemsSource = null;
            autoCompleList.Height = 0;
            string q = SingleQuery.Text;
            int spaces = q.Length - q.Replace(" ", "").Length;
            if (spaces == 1 && q[q.Length-1]==' ')
            {
                
                List<string> completes = s.autoComplete(q).ToList();
                autoCompleList.ItemsSource = completes;
                autoCompleList.Height = completes.Count * 23;
                
                //  SingleQuery = t;
            }
        }

        private void autoCompleList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (autoCompleList.ItemsSource != null)
            {
                string toChange = autoCompleList.SelectedItem.ToString();
                SingleQuery.Text =SingleQuery.Text+ toChange;
                autoCompleList.ItemsSource = null;
                autoCompleList.Height = 0;
            }
        }
    }
}
