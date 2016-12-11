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
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;
using System.Windows.Media;

namespace InstagramKiller.Wpf
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private readonly HttpClientWrapper _httpClient = new HttpClientWrapper("http://localhost:3968/");
        private string _fileName;
        private string _hashtags = "";
        private string _hashtagSearch;
        private ObservableCollection<PostView> _posts;
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
        public string HashtagSearch
        {
            get
            {
                return _hashtagSearch;
            }
            set
            {
                _hashtagSearch = value;
                OnPropertyChanged();
            }
        }
        public ObservableCollection<PostView> Posts
        {
            get
            {
                return _posts;
            }
            set
            {
                _posts = value;
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
        public ICommand GetLatestPosts
        {
            get
            {
                return new CommandWrapper((o) =>
                {
                    List<Post> posts = _httpClient.GetLatestPosts();
                    Posts = CreatePostViewByPost(posts);
                }, o => true);
            }
        }
        public ICommand FindPostsByHashtag
        {
            get
            {
                return new CommandWrapper((o) =>
                {
                    if (!string.IsNullOrEmpty(HashtagSearch))
                    {
                        List<Post> posts = _httpClient.FindPostsByHashtag(HashtagSearch);
                        Posts = CreatePostViewByPost(posts);
                    }
                }, o => true);
            }
        }
        private ObservableCollection<PostView> CreatePostViewByPost(List<Post> posts)
        {
            ObservableCollection<PostView> postViews = new ObservableCollection<PostView>();
            foreach (var post in posts)
            {
                if (post.Photo == null || post.Photo.Length == 0 || post.Photo.Length == 10)
                    continue;
                var image = new BitmapImage();
                using (var mem = new MemoryStream(post.Photo))
                {
                    mem.Position = 0;
                    image.BeginInit();
                    image.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.UriSource = null;
                    image.StreamSource = mem;
                    image.EndInit();
                }
                image.Freeze();
                User user = _httpClient.GetUserById(post.UserId);

                postViews.Add(new PostView() { Source = image, Date = post.Date.ToString(), UserName = user.Login, Hashtags = "hashtags: " + string.Join(", ", post.Hashtags) });
            }
            return postViews;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public struct PostView
    {
        public BitmapSource Source { get; set; }
        public string Date { get; set; }
        public string UserName { get; set; }
        public string Hashtags { get; set; }
    }
}
