using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

// Represents a single image

// TODO tag/separator character as an option
using System.Text;

namespace ImageTag
{
    public class ImageFile : BaseIObservable
    {
        private string _previewURL;
        private bool _isSelected = false;
        private bool _isVisible = true; // may be filtered by the GUI
        private string _dimensions;
        private List<string> _tagList;

        private string _baseFile;

        public ImageFile(string filePath)
        {
            PreviewURL = filePath;
            _tagList = MakeTags();
            MakeDims();
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

        public string BaseName { get { return _baseFile; } }

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

        public bool IsVisible
        {
            get { return _isVisible; }
            set
            {
                _isVisible = value; 
                OnPropertyChanged(() => IsVisible);
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

        private void MakeDims()
        {
            StringBuilder sb = new StringBuilder(); 
            sb.Append(_baseFile);
            sb.AppendLine();
            foreach (var aTag in _tagList)
            {
                sb.Append(" " + aTag);
            }
            sb.AppendLine();
            Dimensions = sb.ToString();
        }

        private static char[] tagSplit = { '+' };
        private List<string> MakeTags()
        {
            var fileN = Path.GetFileNameWithoutExtension(_previewURL);
            var bits = fileN.Split(tagSplit);

            _baseFile = bits[0];

            var tagList = new List<string>();
            for (int i = 1; i < bits.Length; i++)
                if (!tagList.Contains(bits[i]))
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
            bool anyHits = false;
            foreach (var tag in tagsToRemove)
            {
                anyHits |= _tagList.Remove(tag as string);
            }

            if (anyHits)
                RenameFile();
        }

        // Add tag 'A' to a file if it doesn't already exist.
        public void AddTag(string tag)
        {
            if (_tagList.Contains(tag))
                return;
            _tagList.Add(tag);
            RenameFile();
        }

        // Replace tag 'A' with 'B'. Do nothing if the file doesn't have tag 'A'.
        public void ChangeTag(string oldtag, string newtag)
        {
            if (!_tagList.Contains(oldtag))
                return;

            _tagList.Remove(oldtag);
            if (!_tagList.Contains(newtag))
            {
                _tagList.Add(newtag);
            }

            RenameFile();
        }

        private void RenameFile()
        {
            string filename = _baseFile;
            foreach (var aTag in _tagList)
            {
                filename += "+" + aTag;
            }

            string invalid = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            foreach (char c in invalid)
            {
                filename = filename.Replace(c.ToString(), "");
            }

            string dest = Path.Combine(Path.GetDirectoryName(_previewURL),filename) + Path.GetExtension(_previewURL);

            // TODO check if dest length > 256

            try
            {
                File.Move(_previewURL, dest);
            }
            catch
            {
                // Image probably removed by another program
                // TODO remove from files/tags
            }
            _previewURL = dest;
            MakeDims();
        }

    }
}
