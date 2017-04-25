using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MeasureUtilities
{
    public enum GraphCollectionChange { Graph , MinMax, Color, Collection , somethingelse, LabelCount, everything}
    public class GraphCollectionChanged_EventArgs : EventArgs
    {
        public GraphCollectionChange change;
        public object Data; 

        public GraphCollectionChanged_EventArgs(GraphCollectionChange gcc)
        {
            change = gcc;
        }
        public GraphCollectionChanged_EventArgs(GraphCollectionChange gcc, object dat)
        {
            change = gcc;
            Data = dat;
        }
    }
}
