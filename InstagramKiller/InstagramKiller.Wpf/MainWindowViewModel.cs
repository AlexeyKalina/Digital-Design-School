using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using InstagramKiller.Model;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;

namespace InstagramKiller.Wpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly HttpClientWrapper _httpClient = new HttpClientWrapper("http://localhost:3968/");
        private string _userName;
        private string _userId;
        private string _fileName;
        private string _hashtags = "";
        public string Hashtags
        {
            get
            {
                return _hashtags;
            }
            set
            {
                _hashtags = value;
                OnPropertyChanged();
            }
        }
        public string FileName
        {
            get
            {
                return _fileName;
            }
            set
            {
                _fileName = value;
                OnPropertyChanged();
            }
        }
        public string UserId
        {
            get
            {
                return _userId;
            }
            set
            {
                _userId = value;
                OnPropertyChanged();
            }
        }

        public string UserName
        {
            get { return _userName; }
            set
            {
                _userName = value;
                OnPropertyChanged();
            }
        }
        public ICommand OpenFile
        {
            get
            {
                return new CommandWrapper((o) =>
                {
                    OpenFileDialog myDialog = new OpenFileDialog();
                    myDialog.Filter = "Картинки(*.JPG;*.GIF)|*.JPG;*.GIF" + "|Все файлы (*.*)|*.* ";
                    myDialog.CheckFileExists = true;
                    if (myDialog.ShowDialog() == true)
                    {
                        FileName = myDialog.FileName;
                    }
                }, o => true);
            }
        }
        public ICommand AddPost
        {
            get
            {
                return new CommandWrapper((o) =>
                {
                    byte[] bData = File.ReadAllBytes(FileName);
                    _httpClient.AddPost(new Post() { Id = Guid.Empty, Date = DateTime.Now, Hashtags = Hashtags.Split().ToList(), Photo = bData, UserId = Guid.Empty});

                }, o => true);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
