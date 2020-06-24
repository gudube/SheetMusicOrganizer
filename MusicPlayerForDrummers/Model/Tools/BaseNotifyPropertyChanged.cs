using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace MusicPlayerForDrummers.Model.Tools
{
    public abstract class BaseNotifyPropertyChanged : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName]string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected virtual bool SetField<T>(ref T field, T value, [CallerMemberName]string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;

            OnPropertyChanged(propertyName);
            return true;
        }
        /*protected virtual bool SetField(ref INotifyPropertyChanged field, INotifyPropertyChanged value, [CallerMemberName]string propertyName = null)
        {
            if (field == value)
                return false;

            if (_handlers.TryGetValue(propertyName, out var propertyHandlers))
            {
                if (field != null)
                    foreach (PropertyChangedEventHandler handler in propertyHandlers.GetInvocationList())
                        field.PropertyChanged -= handler;

                field = value;
                if (field != null)
                    foreach (PropertyChangedEventHandler handler in propertyHandlers.GetInvocationList())
                        field.PropertyChanged += handler;
            }
            else
                field = value;

            OnPropertyChanged(propertyName);
            return true;
        }


        private readonly Dictionary<string, PropertyChangedEventHandler> _handlers = new Dictionary<string, PropertyChangedEventHandler>();
        public void AddPropertyChanged(string property, PropertyChangedEventHandler handler)
        {
            if (_handlers.TryGetValue(property, out var existingHandler))
                existingHandler += handler;
            else
                existingHandler = handler;
            _handlers[property] = existingHandler;

            var prop = GetType().GetProperty(property).GetValue(this);
            if (prop != null)
                ((INotifyPropertyChanged)prop).PropertyChanged += handler;
        }

        public void RemovePropertyChanged(string property, PropertyChangedEventHandler handler)
        {
            if (!_handlers.TryGetValue(property, out var existingHandler))
                return;

            existingHandler -= handler;
            _handlers[property] = existingHandler;

            var prop = GetType().GetProperty(property).GetValue(this);
            if (prop != null)
                ((INotifyPropertyChanged)prop).PropertyChanged -= handler;
        }*/
    }
}
