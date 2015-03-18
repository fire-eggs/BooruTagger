using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace ImageTag
{
    public class CheckedListItem<T> : BaseIObservable
    {
        private bool isChecked;
        private T item;

        public CheckedListItem()
        {
        }
        public CheckedListItem(T item, bool isChecked = false)
        {
            this.item = item;
            this.isChecked = isChecked;
        }
        public T Item
        {
            get { return item; }
            set
            {
                item = value;
                OnPropertyChanged(()=>Item);
            }
        }
        public bool IsChecked
        {
            get { return isChecked; }
            set
            {
                isChecked = value;
                OnPropertyChanged(() => IsChecked);
            }
        }
    }
}
