using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SOM
{
    class FileNotFoundException:ApplicationException
    {
        public FileNotFoundException(string message) : base(message) { }
    }
}
