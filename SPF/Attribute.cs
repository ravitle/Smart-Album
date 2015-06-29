using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPF
{
    class Attribute
    {
        private double Fscore_pressision;//calculation to check which subGroup r the best - 0.5*Fscore + 0.5*pressision
        private List<int> subGroup;// sub group of the attribute

        public Attribute()
        {
            this.subGroup = new List<int>();
            
        }

        public void setSubGroup(List<int> sub)
        {
            foreach (int i in sub)
                subGroup.Add(i);

        }
        public List<int> getSubGroub()
        {
            return subGroup;
        }

        public void setFscore_pressision(double Fscore, double pressision)
        {
            Fscore_pressision = 0.5 * Fscore + 0.5 * pressision;
        }

        public double getFscore_pressision()
        {
            return Fscore_pressision;
        }

        public Boolean contains(List<int> other)//check if bouth list contains the same numbers
        {
            if (other.Count != subGroup.Count)
                return false;
            int c = 0;
            foreach (int i in subGroup)
                if (other.Contains(i))
                    c++;
            if (c == subGroup.Count)
                return true;
            return false;
        }

        public string toString()
        { 
            string str = "";
            foreach(int n in subGroup)
                str += n +" , ";
            str += Fscore_pressision;
            return str;
        
        }
    }

}
