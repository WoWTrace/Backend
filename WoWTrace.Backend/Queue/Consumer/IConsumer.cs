﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoWTrace.Backend.DataModels;

namespace WoWTrace.Backend.Queue.Consumer
{
    public interface IConsumer
    {
        public void Listen();
        public bool AlreadyProcessedCheck(ulong buildId);
        public void MarkAsProcessed(ulong buildId);
    }
}
