using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoWTrace.Backend.Queue.Consumer
{
    public interface IConsumer
    {
        public void Listen();
    }
}
