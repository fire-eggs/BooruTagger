using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Windows;

// Represents a single image

// TODO tag character as an option
// TODO previewURL -> a preview thumbnail image. Need to store original path separately.

namespace ImageTag
{
    public class ImageFile : BaseIObservable
    {
        private string _previewURL;
        private bool _isSelected = false;
        private string _dimensions;
        private List<string> _tagList;

        private string _baseFile;

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

        public void AddTag(string tag)
        {
            if (!_tagList.Contains(tag))
            {
                _tagList.Add(tag);
                RenameFile();
            }
        }

        public void ChangeTag(string oldtag, string newtag)
        {
            bool anyChange = false;
            if (_tagList.Contains(oldtag))
            {
                _tagList.Remove(oldtag);
                anyChange = true;
            }
            if (!_tagList.Contains(newtag))
            {
                _tagList.Add(newtag);
                anyChange = true;
            }

            if (anyChange)
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

            File.Move(_previewURL, dest);
            _previewURL = dest;
        }

    }
}
