using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FF_control.Bluetooth
{
    public class ReceivedData_EventArgs : EventArgs
    {
        private ArrayList al;

        public ArrayList ArrayL
        {
            get { return al; }
            set { al = value; }
        }
        

        public ReceivedData_EventArgs(params object[] list)
        {
            al = new ArrayList();
            foreach (var item in list)
            {
                al.Add(item);
            }
        }
    }
}
