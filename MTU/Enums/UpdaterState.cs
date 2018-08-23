using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTU.Enums
{
    public enum UpdaterState
    {
        Started,
        DownloadingVerification,
        ParsingList,
        CheckingFiles,
        Downloading,
        Failed,
        Finished
    }
}
