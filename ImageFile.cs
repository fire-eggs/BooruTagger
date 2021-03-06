﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

// Represents a single image

// TODO tag/separator character as an option
// TODO on failure, tag list could be out of sync from reality?

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
            MakeTooltip();
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

        private void MakeTooltip()
        {
            StringBuilder sb = new StringBuilder(); 
            sb.Append(_baseFile);
            if (_tagList.Count > 0)
            {
                sb.AppendLine();
                foreach (var aTag in _tagList)
                {
                    sb.Append(aTag + " ");
                }
            }
            Dimensions = sb.ToString().Trim();
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

        public void RemoveTag(string tag)
        {
            _tagList.Remove(tag);
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
        public bool AddTag(string tag)
        {
            if (_tagList.Contains(tag))
                return true;

            _tagList.Add(tag);

            // NOTE: relies on exception to raise other problems besides "image has vanished"
            bool res = RenameFile();
            if (!res)
                _tagList.Remove(tag);
            return res;
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

        // Returns true/false: file successfully renamed
        private bool RenameFile()
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

            try
            {
                File.Move(_previewURL, dest);
            }
            catch (FileNotFoundException) // NOTE: documentation suggests an IOException will occur
            {
                // Image probably removed by another program
                return false;
            }
            _previewURL = dest;
            MakeTooltip();
            return true;
        }

    }
}
