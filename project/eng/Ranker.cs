using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace eng
{
    class Ranker
    {
        public double k; //function to compute k
        public double k1; //ampiri
        public double k2;//ampiri
        public double b;//ampiri
        public double dl; //doc length
        public double avgdl;//avarge docs length
        public double ri;//relevant doc containing term
        public double R;//all relevant docs
        public double ni;//docs containing term
        public double N;//corpus size
        public double fi;//frec of term in doc 
        public double qfi; //frec term in query

        public double bm25;
        public double allBonuses;

        double bonusTitle;
        int countBonusTitle;
        double bonusTag100;
        int countBonusTag100;
        double bonusTag101;
        int countBonusTag101;
        double bonusFirstThird;
        int countBonusFirst3rd;
        double bonusLast10;
        int countBonusLast10th;
        double bonusTopsTerm;

        public double alpha = 0.56;

        public Ranker(double _N, double _avgdl)
        {
            setDefualt();
      
            bm25 = 0;
            N = _N;
            avgdl = _avgdl;

            b = 0.18;
            k1 =8.3;
            k2 = 10;
            
        }

        private void setK()
        {
            k = k1 * ( (1 - b) + b * (dl / avgdl) );
        }
        public void setTermInfo(double _ri,double _ni,double _R, double _qfi)
        {
            ri = _ri;
            ni = _ni;
            R = _R; 
            qfi = _qfi;
        }

        private void computeScore()
        {
            double ri05=ri+0.5;
            double ri05Minus = -ri + 0.5;
            double Rri05 = R +ri05Minus;
            double niri05 = ni +ri05Minus;
            double NniRri05 = N - ni - R + ri05;
            double log = Math.Log((ri05 / Rri05) / (niri05 / NniRri05));
            double oprnd1 = ((k1 + 1) * fi) / (k + fi);
            double oprnd2 = ((k2 + 1) * qfi) / (k2 + qfi);
            bm25 = log * oprnd1 * oprnd2; 
        }


        public void addBonusTitle()
        {
            countBonusTitle++;
            if (countBonusTitle == 1)
            {
                bonusTitle = 1.5;
            }
            else
            {
                bonusTitle += 3;
            }
            
        }

        public void addBonusTag100()
        {
            countBonusTag100++;
            if (countBonusTag100==1)
                bonusTag100 = 0.5;
            else
                bonusTag100 += 1.5;
            
        }

        public void addBonusTag101()
        {
            countBonusTag101++;
            if (countBonusTag101 == 1)
                bonusTag101 = 0.5;
            else
                bonusTag101 += 1.5;
        }
        public void addBonusFirstThird()
        {
            countBonusFirst3rd++;
            if (countBonusFirst3rd == 1)
            {
                bonusFirstThird = 1;
            }
            else
            {
                bonusFirstThird += 2;
            }
        }

        public void addBonusLast10()
        {
            countBonusLast10th++;
            if (countBonusLast10th == 1)
            {
                bonusLast10 = 1.5;
            }
            else
            {
                bonusLast10 += 3;
            }
        }


        public void ComputeBM25(double _dl, double _fi)
        {
            setDefualt();
            dl = _dl;
            fi = _fi;
            setK();
            computeScore();
        }

        public double getScore()
        {
            return bm25;
        }

        public double getBonusScore()
        {
            
            allBonuses= bonusTitle + bonusTag100 + bonusTag101 + bonusFirstThird + bonusLast10+bonusTopsTerm;
            return allBonuses;
        }

        public double getFinalScore()
        {
          /*  int div=7;
         /*   size--;
            while (size>0){
                div+=2;
                size--;
            }*/
           /* double allBonuses = bonusTitle + bonusTag100 + bonusTag101 + bonusFirstThird+bonusLast10;
            allBonuses = allBonuses / div;*/
          // alpha = 0;

            return alpha*bm25  +(1-alpha)*allBonuses;
        }



        public void setDefualt()
        {
            bonusTitle = 0;
            bonusTag100 = 0;
            bonusTag101 = 0;
            bonusFirstThird = 0;
            bonusLast10 = 0;
            bonusTopsTerm = 0;
            countBonusTitle = 0;
            countBonusTag100 = 0;
            countBonusTag101 = 0;
            countBonusFirst3rd = 0;
            countBonusLast10th = 0;
        }





        
    }
}
