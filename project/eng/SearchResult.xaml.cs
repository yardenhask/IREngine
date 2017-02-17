using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace eng
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SearchResult : Window
    {
        Dictionary<int, List<string>> res;
        public SearchResult( Dictionary<int, List<string>> results)
        {
            InitializeComponent();
            int count = 0;
            

            ObservableCollection<lineFromDic> obs2 = new ObservableCollection<lineFromDic>();
            foreach (int d in results.Keys)
            {
                count += results[d].Count;
                foreach (string s in results[d])
                {
                    lineFromDic l = new lineFromDic( s,d);
                    obs2.Add(l);
                }
            }

            documentsFoundLabel.Content =count;
            docsListView.DataContext = obs2;
            res = results;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Select the directory for saving the results";
            folderBrowserDialog.ShowDialog();
            string p= folderBrowserDialog.SelectedPath;
         
          //  string p=@"c:\q";//delete later
            p = p + "\\results.txt";
            using (FileStream fs = new FileStream(p,FileMode.Create ))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    foreach (int d in res.Keys)
                    {
                        foreach (string s in res[d])
                        {
                            string toWrite = d + ", 0, " + s+", 0, 0, mt";
                            sw.WriteLine(toWrite);
                        }
                    }
                }
            }
        }

    }

    /// <summary>
    /// class for details to show 
    /// </summary>
    public class lineFromDic
    {
        public string docId { get; set; }
        public int qId { get; set; }

      //  public double rank { get; set; }


        public lineFromDic(string d, int q/*,double r*/)
        {
            docId = d;
            qId = q;
         //   rank = r;
        }
    }
}
