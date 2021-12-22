using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SheetMusicOrganizer
{
    public class LibraryFileNotFoundException : Exception 
    {
        public LibraryFileNotFoundException(): base(){}
        public LibraryFileNotFoundException(string? message): base(message){}
    }

    public class InitLibraryException : Exception
    {
        public InitLibraryException() : base() { }
        public InitLibraryException(string? message) : base(message) { }
    }
}
