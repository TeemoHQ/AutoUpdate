using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoUpdateClient.Common
{
    public class StateCenter
    {
        private StateCenter()
        {

        }

        public static readonly StateCenter Instance = new StateCenter();

        public bool HasRepalced = true;
        public bool HasSelfUpdated = true;
        public bool HasRunDynCoded = true;
    }
}
