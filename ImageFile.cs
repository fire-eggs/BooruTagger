using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Represents a single image
using System.Windows.Markup;

namespace ImageTag
{
    public class ImageFile : BaseIObservable
    {
        private string _previewURL;
        private bool _isSelected = false;
        private string _dimensions;
        private List<string> _tagList;

        public ImageFile(string filePath)
        {
            PreviewURL = filePath;
            Dimensions = "<TBD>";
            _tagList = MakeTags();
        }

        public string PreviewURL
        {
            get { return _previewURL; }
            set
            {
                _previewURL = value;
                RaisePropertyChanged("PreviewURL");
            }
        }
        public bool IsSelected
        {
            get
            {
                return _isSelected;
            }
            set
            {
                _isSelected = value;
                RaisePropertyChanged("IsSelected");
            }
        }
        public string Dimensions
        {
            get { return _dimensions; }
            set
            {
                _dimensions = value;
                RaisePropertyChanged("Dimensions");
            }
        }

        private static char[] tagSplit = new char[] { '+' };
        private List<string> MakeTags()
        {
            var fileN = Path.GetFileNameWithoutExtension(_previewURL);
            var bits = fileN.Split(tagSplit);
            var tagList = new List<string>();
            for (int i = 1; i < bits.Length; i++)
                tagList.Add(bits[i]);
            return tagList;
        }

        public bool HasTag(string tag)
        {
            return _tagList.Contains(tag);
        }

        public List<string> Tags()
        {
            return _tagList;
        }

        public void RemoveTags(IList tagsToRemove)
        {
            foreach (var tag in tagsToRemove)
            {
                _tagList.Remove(tag as string);
            }

            // TODO actually change the filename!
        }

        public void AddTag(string tag)
        {
            if (!_tagList.Contains(tag))
                _tagList.Add(tag);

            // TODO actually change the filename!
        }

        public void ChangeTag(string oldtag, string newtag)
        {
            if (_tagList.Contains(oldtag))
                _tagList.Remove(oldtag);
            if (!_tagList.Contains(newtag))
                _tagList.Add(newtag);

            // TODO actually change the filename!
        }

    }
}
