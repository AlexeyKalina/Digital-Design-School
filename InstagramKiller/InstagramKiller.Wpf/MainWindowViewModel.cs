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
using System.Windows;

namespace InstagramKiller.Wpf
{
	public class MainWindowViewModel : INotifyPropertyChanged
	{
		private readonly HttpClientWrapper _httpClient = new HttpClientWrapper("http://localhost:3968/");
		private string _fileName;
		private string _hashtags = string.Empty;
		private string _hashtagSearch;
		private Guid _currentId = new Guid("addabb94-1fc5-47e2-91b3-17f8aa797ae3");
		private string _currentLogin = "alexey96";
		private string _currentPassword = "1234";
		private ObservableCollection<PostView> _latestPosts;
		private ObservableCollection<PostView> _foundPosts;
		public MainWindowViewModel()
		{
			_latestPosts = CreatePostViewByPost(_httpClient.GetLatestPosts());
		}
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
		public Guid CurrentId
		{
			get
			{
				return _currentId;
			}
			set
			{
				_currentId = value;
				OnPropertyChanged();
			}
		}
		public string CurrentLogin
		{
			get
			{
				return _currentLogin;
			}
			set
			{
				_currentLogin = value;
				OnPropertyChanged();
			}
		}
		public string CurrentPassword
		{
			get
			{
				return _currentPassword;
			}
			set
			{
				_currentPassword = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<PostView> LatestPosts
		{
			get
			{
				return _latestPosts;
			}
			set
			{
				_latestPosts = value;
				OnPropertyChanged();
			}
		}
		public ObservableCollection<PostView> FoundPosts
		{
			get
			{
				return _foundPosts;
			}
			set
			{
				_foundPosts = value;
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
					_httpClient.AddPost(new Post() { Id = Guid.Empty, Date = DateTime.Now, Hashtags = Hashtags.Split().ToList(), Photo = bData, UserId = CurrentId });
					Hashtags = string.Empty;
				}, o => true);
			}
		}
		public ICommand AddComment
		{
			get
			{
				return new CommandWrapper((o) =>
				{
					PostView post = (PostView)((StackPanel)o).DataContext;
					_httpClient.AddComment(new Comment() { Id = Guid.NewGuid(), Date = DateTime.Now, PostId = post.Id, Text = post.NewComment, UserId = CurrentId }, post.Id);
					post.Comments.Add(new CommentView() { UserName = CurrentLogin, Text = post.NewComment });
				}, o => true);
			}
		}
		public ICommand LogIn
		{
			get
			{
				return new CommandWrapper((o) =>
				{
					User user = _httpClient.GetUserById(CurrentId);
					if (user.Password == CurrentPassword)
					{
						CurrentLogin = user.Login;
						MessageBox.Show(string.Format("Hello, {0}!", CurrentLogin));
					}
				}, o => true);
			}
		}
		public ICommand DeletePost
		{
			get
			{
				return new CommandWrapper((o) =>
				{
					PostView post = (PostView)((StackPanel)o).DataContext;
					_httpClient.DeletePost(post.Id);
					LatestPosts.Remove(LatestPosts.First(p => p.Id == post.Id));
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
					LatestPosts = CreatePostViewByPost(posts);
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
						FoundPosts = CreatePostViewByPost(posts);
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

				ObservableCollection<CommentView> comments = GetCommentsByPost(post);

				postViews.Add(new PostView() { Source = image, Id = post.Id, Date = post.Date.ToString(), UserName = user.Login, Hashtags = "hashtags: " + string.Join(", ", post.Hashtags), Comments = comments });
			}
			return postViews;
		}

		private ObservableCollection<CommentView> GetCommentsByPost(Post post)
		{
			List<Comment> comments = _httpClient.GetPostComments(post.Id);
			ObservableCollection<CommentView> resultComments = new ObservableCollection<CommentView>();
			foreach (var comment in comments)
			{
				User user = _httpClient.GetUserById(comment.UserId);

				resultComments.Add(new CommentView() { UserName = user.Login, Text = comment.Text });
			}
			return resultComments;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
	public struct PostView
	{
		public Guid Id { get; set; }
		public BitmapSource Source { get; set; }
		public string Date { get; set; }
		public string UserName { get; set; }
		public string Hashtags { get; set; }
		public string NewComment { get; set; }
		public ObservableCollection<CommentView> Comments { get; set; }
	}
	public struct CommentView
	{
		public string UserName { get; set; }
		public string Text { get; set; }
	}
}
