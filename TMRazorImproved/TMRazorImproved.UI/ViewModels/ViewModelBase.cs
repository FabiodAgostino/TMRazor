using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Windows;
using System.Windows.Data;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Linq;

namespace TMRazorImproved.UI.ViewModels
{
    /// <summary>
    /// Classe base per i ViewModel. Fornisce:
    /// - Supporto alla validazione (ObservableValidator / INotifyDataErrorInfo)
    /// - Dispatching thread-safe verso il UI thread (RunOnUIThread)
    /// - Sincronizzazione ObservableCollection cross-thread (EnableThreadSafeCollection)
    /// </summary>
    public abstract partial class ViewModelBase : ObservableValidator
    {
        [ObservableProperty]
        private string _statusText = "Ready";

        /// <summary>
        /// Ritorna true se il ViewModel non ha errori di validazione.
        /// </summary>
        public bool IsValid => !HasErrors;

        /// <summary>
        /// Esegue in modo sicuro un'azione sul Dispatcher Thread della UI.
        /// </summary>
        protected void RunOnUIThread(Action action)
        {
            if (Application.Current?.Dispatcher != null)
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
                action();
            }
        }

        /// <summary>
        /// Abilita la modifica thread-safe per una ObservableCollection.
        /// </summary>
        protected void EnableThreadSafeCollection<T>(ObservableCollection<T> collection, object lockObject)
        {
            BindingOperations.EnableCollectionSynchronization(collection, lockObject);
        }

        /// <summary>
        /// Forza la validazione di tutte le proprietà del ViewModel decorate con attributi di validazione.
        /// </summary>
        protected void ValidateAll()
        {
            ValidateAllProperties();
        }

        /// <summary>
        /// Sincronizza una ObservableCollection con una lista sorgente in modo efficiente.
        /// </summary>
        protected void SyncCollection<T>(ObservableCollection<T> collection, IEnumerable<T> source, object lockObject)
        {
            lock (lockObject)
            {
                var sourceList = source.ToList();
                
                // Rimuovi elementi non più presenti
                for (int i = collection.Count - 1; i >= 0; i--)
                {
                    if (!sourceList.Contains(collection[i]))
                    {
                        collection.RemoveAt(i);
                    }
                }

                // Aggiungi elementi mancanti
                foreach (var item in sourceList)
                {
                    if (!collection.Contains(item))
                    {
                        collection.Add(item);
                    }
                }
            }
        }
    }
}
