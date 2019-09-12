using System;
using System.Threading.Tasks;
using TinyIoC;
using Xamarin.Forms;

namespace IUBH.TOR.Modules.Shared.Pages
{
    public abstract class ContentPageBase<TViewModel> : ContentPage where TViewModel : ViewModelBase
    {
        /// <summary>
        /// Our current View Model that serves as the
        /// Binding Context behind the scenes.
        /// </summary>
        protected TViewModel ViewModel => (TViewModel)BindingContext;

        protected ContentPageBase()
        {
            try
            {
                // Sets the Binding Context: Which is our View Model. Using the
                // IoC Container makes sure all constructor dependencies of the
                // View Model type are being resolved as well.
                BindingContext = TinyIoCContainer.Current.Resolve<TViewModel>();

                // We initialize the View Model but make sure this operation is
                // not blocking the UI.
                Task.Run(
                    async () =>
                    {
                        // Configure Await is set to true on purpose here. It's
                        // important to continue on the UI thread after the
                        // initialization has finished.
                        await ViewModel.InitializeAsync().ConfigureAwait(true);

                        OnViewModelInitialized();
                    }
                );
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                
                throw;
            }
        }

        /// <summary>
        /// Hook for all pages that is being called after the
        /// View Model has been initialized 
        /// </summary>
        protected virtual void OnViewModelInitialized()
        {
            // Hook
        }
    }
}
