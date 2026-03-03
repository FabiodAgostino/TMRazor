using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;

namespace TMRazorImproved.UI.ViewModels
{
    /// <summary>
    /// Classe base per i ViewModel. Fornisce:
    /// - Dispatching thread-safe verso il UI thread (RunOnUIThread)
    /// - Sincronizzazione ObservableCollection cross-thread (EnableThreadSafeCollection)
    ///
    /// Per aggiornamenti ad alta frequenza (HP, Mana, Stamina) usare UiThrottler
    /// invece di RunOnUIThread diretto, per evitare di saturare il Dispatcher.
    /// I ViewModel che usano UiThrottler devono implementare IDisposable.
    /// </summary>
    public abstract class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// Esegue in modo sicuro un'azione sul Dispatcher Thread della UI.
        /// Da usare per aggiornamenti a bassa frequenza (eventi singoli, cambio stato).
        /// Per aggiornamenti ad alta frequenza preferire UiThrottler.
        /// </summary>
        protected void RunOnUIThread(Action action)
        {
            if (Application.Current != null && Application.Current.Dispatcher != null)
            {
                if (Application.Current.Dispatcher.CheckAccess())
                {
                    action();
                }
                else
                {
                    Application.Current.Dispatcher.BeginInvoke(action);
                }
            }
            else
            {
                // Fallback (utile in fase di startup o testing isolato)
                action();
            }
        }

        /// <summary>
        /// Abilita la modifica thread-safe per una ObservableCollection.
        /// Chiamare questo metodo nel costruttore del ViewModel passando la collezione e un lock object dedicato.
        /// </summary>
        protected void EnableThreadSafeCollection<T>(ObservableCollection<T> collection, object lockObject)
        {
            BindingOperations.EnableCollectionSynchronization(collection, lockObject);
        }
    }
}
