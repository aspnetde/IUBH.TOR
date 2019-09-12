using System.ComponentModel;
using System.Threading.Tasks;

namespace IUBH.TOR.Modules.Shared.Pages
{
    /// <summary>
    /// Base class for all of our View Models. Implements INotifyPropertyChanged
    /// so that Fody.PropertyChanged can do its job. See
    /// https://github.com/Fody/PropertyChanged for more information.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        
        public virtual Task InitializeAsync() => Task.CompletedTask;
        
        protected void OnPropertyChanged(PropertyChangedEventArgs eventArgs)
        {
            PropertyChanged?.Invoke(this, eventArgs);
        }
    }
}
