using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPF
{
    class BFS_Algoritem
    {
        private const double P = 0.90; 
        private List<Attribute> open;
        private List<Attribute> closed;
        private Attribute _best;
        int[] arr;
        private const int MIN_ARRAY_SIZE = 4;
        private const double IMPROVE = 0.01;
        private const int ROUND_NUM = 10;
        private int round;
        private double pre;
        public BFS_Algoritem()
        {
            open = new List<Attribute>();
            closed = new List<Attribute>();
            round = 0;
            pre = 0;
        }

        public List<Attribute> BFS_Inc_Dec()
        {
            pre += 0.1;
            double improvment = 0;
            if (pre >= 1)
                pre = 0;

            if (pre <= P)
                foreach (Attribute a in open)
                {
                    if (_best.getFscore_pressision() <= a.getFscore_pressision())
                    {
                        improvment = Math.Abs(_best.getFscore_pressision() - a.getFscore_pressision());
                        _best = a;// best combination
                    }
                }
            else
            {
                Random rand = new Random();
                int num = rand.Next(0, open.Count());
                Attribute tmp = open.ElementAt(num);
                improvment = Math.Abs(_best.getFscore_pressision() - tmp.getFscore_pressision());
                if (tmp.getFscore_pressision() >= _best.getFscore_pressision())
                    _best = tmp;

            }
            
            Console.WriteLine("bestttttt:>>>>>>>> " + _best.toString());

             if (improvment < IMPROVE)
                round++;
            if(round >= ROUND_NUM)// there wasn't any imrovment in the last ROUNDS_NUM
                return null;
            Console.WriteLine("round: " + round);
            List<Attribute> temp;
            temp = increseNum();
            temp.AddRange(decreaseNum());//marge lists
            open.Remove(_best);
            closed.Add(_best);
            return temp;
        
        }
        //add num to subGroup
        public List<Attribute> increseNum()
        {
            
            List<Attribute> att = new List<Attribute>();
            for (int i = 0; i < ImageVector.NUMBER_OF_PARAMETERS; i++)
            {
                if (!(_best.getSubGroub().Contains(i)))
                {
                    
                    Attribute t = new Attribute();
                    t.setSubGroup(_best.getSubGroub());
                    t.getSubGroub().Add(i);
                    if (!contains(open, t) || !contains(closed, t))
                        att.Add(t);
     
                }
            }
            
            return att;
        }
        //decrieas num from subGroup
        private List<Attribute> decreaseNum()//Attribute att)
        {
            List<Attribute> att = new List<Attribute>();
           
            int index = 0;
            List<int> arr = _best.getSubGroub();

            if (arr.Count > MIN_ARRAY_SIZE)
            {
                for (int i = 0; i < arr.Count;i++ )
                {
                    Attribute t = new Attribute();
                    t.setSubGroup(_best.getSubGroub());
                    t.getSubGroub().RemoveAt(index);
                    if (!contains(open, t) || !contains(closed, t))
                        att.Add(t);

                }
            }
          
            return att; 
        }


        public int[] getAtt()
        {
            int[] arr = _best.getSubGroub().ToArray() ;
            return arr;
        }

        public void setBest(Attribute att)
        {
            _best = att;
        }
        public Attribute getBest()
        {
            return _best;
        }
        private void sortByFscorePrecision(List<Attribute> list)
        {
          /*  Attribute temp = list.First();
            foreach (Attribute att in list)
            { 
                if(att.getFscore_pressision() >= temp.getFscore_pressision())

            }*/
        
        }
        //add attribute to open List
        public void addOpen(Attribute att)
        {
            open.Add(att);

        }

        // cheack if atribute att is already exist in open/closed lists
        private Boolean contains(List<Attribute> list,Attribute att)
        { 
            Boolean exist;
            exist = true;
            
            foreach (Attribute t in list)
            {
                
                exist = true;
                if (t.getSubGroub().Count() != att.getSubGroub().Count())
                    exist = false;
                foreach (int n in att.getSubGroub())
                {
                  //  foreach (int m in att.getSubGroub())
                 //   {
                        if (!t.getSubGroub().Contains(n))
                            exist = false;
                        
                }
                if (exist)
                    break;
            }

            return exist;
        
        }



    }

   
}
 