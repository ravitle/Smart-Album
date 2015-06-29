using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.IO;

namespace SPF
{
    static class Program
    {
      
        [STAThread]
        static int Main(string[] args)
        {

            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new smartAlbumForm());
            //return 1;

            string path = smartAlbum.getPicturesDirectory();


            string goodPath = path + "\\Good";
            string allPath = path + "\\All";
            string userPath = path;
            string decidePath = path + "\\Decide";

            if ((args == null) || ((args.Length == 0)))
            {
                smartAlbum obj = new smartAlbum(allPath, goodPath, userPath, decidePath);
                obj.Test();
            }
            else
            {
                path = args[0];
                goodPath = path + "\\Good";
                allPath = path + "\\All";
                userPath = path;
                decidePath = path + "\\Decide";

                smartAlbum obj = new smartAlbum(allPath, goodPath, userPath, decidePath);
                obj.Learn();
                obj.Decide();

            }

            return 0;
            
        }
    }
}
