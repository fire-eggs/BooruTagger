﻿using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace ImageTag
{
    public class BaseIObservable : INotifyPropertyChanged
    {

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(property));
            }
        }

        protected void OnPropertyChanged<T>(Expression<Func<T>> exp)
        {
            //the cast will always succeed
            MemberExpression memberExpression = (MemberExpression)exp.Body;
            string propertyName = memberExpression.Member.Name;

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }
        #endregion

    }
}
