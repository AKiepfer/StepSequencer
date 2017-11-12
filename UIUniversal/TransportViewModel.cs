using System.ComponentModel;
using System.Linq;
using Caliburn.Micro;

namespace UIUniversal
{
    public class TransportViewModel : PropertyChangedBase
    {
        public TransportViewModel()
        {
            TransportIsAtTick = new bool[32];
        }

        public bool[] TransportIsAtTick { get; set; }

        public bool this[int index]
        {
            get { return TransportIsAtTick[index]; }
            set
            {
                TransportIsAtTick[index] = value;

                foreach (var t in TransportIsAtTick
                    .Select((b, i) => new {Value = b, Index = i})
                    .Where(arg => arg.Index != index))
                {
                    //.AsParallel()
                    TransportIsAtTick[t.Index] = false;
                }

                Execute.OnUIThreadAsync(() => this.OnPropertyChanged(new PropertyChangedEventArgs("Item[]")));
                //Deployment.Current.Dispatcher.InvokeAsync(() => raisePropertyChanged("Item[]"));
            }
        }
    }
}