﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WUApiLib;

namespace SecurityCenter.Business.Updates
{
    public class UpdateDownloadCompletedEventArgs : EventArgs
    {
        public IDownloadJob Job { get; set; }
    }
}