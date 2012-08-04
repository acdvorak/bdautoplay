using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BlurayAutoPlay
{
    interface IMediaPlayer
    {
        string GetPath();
        string GetName();
        string GetProgId();
        string GetHandlerName();
        string GetExeAutoplayArgs();
        string GetInitCmdLine();
        bool IsInstalled();
    }
}
