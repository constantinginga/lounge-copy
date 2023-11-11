namespace ScSoMe.RazorLibrary
{
    public class AppState
    {
        private bool _loggedIn;
        private string _token;
        private int _currentGroup;
        private bool _isFreeUser = true;
        private string _freeUserId;
        private long? _selectedPost;
        private string _previousPage;
        private bool _isMediaAttached;
        public API.MemberInfo? CurrentUser { get; set; }
        public event Action? OnNotificationsChanged;
        public event Action? OnAttachedMediaChanged;
        public event Action? OnGroupsUpdated;
        // https://chrissainty.com/3-ways-to-communicate-between-components-in-blazor/
        public void NotifyNotificationsChanged() => OnNotificationsChanged?.Invoke();
        public void NotifyGroupsUpdated() => OnGroupsUpdated?.Invoke();

        //private ICollection<API.ScGroup> _allGroups;

        public ICollection<API.ScGroup> AllGroups { get; set; }

        public bool IsLoggedIn
        {
            get { return _loggedIn; }
            set
            {
                if (_loggedIn != value)
                {
                    _loggedIn = value;
                }
            }
        }

        public string token{
            get { return _token; }
            set
            {
                if (_token != value)
                {
                    _token = value;
                }
            }
        }

        public bool IsFreeUser
        {
            get { return _isFreeUser; }
            set
            {
                if (_isFreeUser != value)
                {
                    _isFreeUser = value;
                }
            }
        }

        public string FreeUserId
        {
            get { return _freeUserId; }
            set
            {
                if (_freeUserId != value)
                {
                    _freeUserId = value;
                }
            }
        }

        //public ICollection<API.ScGroup> AllGroups
        //{
        //    get { return _allGroups; }
        //    set
        //    {
        //        if (_allGroups != value)
        //        {
        //            _allGroups = value;
        //            //NotifyGroupsUpdated();
        //        }
        //    }
        //}

        public bool IsMediaAttached
        {
            get { return _isMediaAttached; }
            set
            {
                if (_isMediaAttached != value)
                {
                    _isMediaAttached = value;
                    NotifyAttachedMediaChanged();
                }
            }
        }

        public string PreviousPage
        {
            get { return _previousPage; }
            set
            {
                if (_previousPage != value)
                {
                    _previousPage = value;
                }
            }
        }

        public int CurrentGroup
        {
            get { return _currentGroup; }
            set
            {
                if (_currentGroup != value)
                {
                    _currentGroup = value;
                }
            }
        }

        public long? SelectedPost
        {
            get { return _selectedPost; }
            set
            {
                if (_selectedPost != value)
                {
                    _selectedPost = value;
                }
            }
        }

        private void NotifyNotificationsStateChanged() => OnNotificationsChanged?.Invoke();
        private void NotifyAttachedMediaChanged() => OnAttachedMediaChanged?.Invoke();
    }
}
