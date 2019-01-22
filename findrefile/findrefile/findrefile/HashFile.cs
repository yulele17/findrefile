using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace findrefile
{
   public class HashFile
    {
        private string path;
        private string hashcode;

        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value;
            }
        }

        public string Hashcode
        {
            get
            {
                return hashcode;
            }

            set
            {
                hashcode = value;
            }
        }
    }
}
