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
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace eng
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class ShowDictionary : Window
    {
        public ShowDictionary( Dictionary<string, TermInfo> d)
        {
            InitializeComponent();
            List<string> sortedTermsKeys = d.Keys.ToList();
            sortedTermsKeys.Sort();
            ObservableCollection<lineInDict> obs = new ObservableCollection<lineInDict>();
            foreach (string t in sortedTermsKeys)
            {
                lineInDict l = new lineInDict(t, d[t].corpusF);
                obs.Add(l);
            }
            
            //bind data
            DG1.DataContext = obs;
        }
    }
    /// <summary>
    /// class for details to show from the terms
    /// </summary>
     public class lineInDict
    {
        public string term { get; set; }
        public int frec { get; set; }


        public lineInDict(string t, int f)
        {
            term = t;
            frec = f;
        }
    }

}
