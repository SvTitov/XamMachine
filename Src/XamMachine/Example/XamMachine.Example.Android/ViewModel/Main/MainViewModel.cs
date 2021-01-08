using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using XamMachine.Core.Abstract;
using XamMachine.Example.Android.Annotations;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class MainViewModel : INotifyPropertyChanged, IDisposable
    {
        private bool _actionEnable;
        private Action<bool> _onActionCallback;
        private MainStateMachine<MainStates> _stateMachine;
        private string _login;
        private string _password;

        public string Login
        {
            get => _login;
            set
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                _password = value; 
                OnPropertyChanged(nameof(Password));
            }
        }

        public enum MainStates
        {
            Empty,
            Filled
        }

        public bool ActionEnable
        {
            get => _actionEnable;
            set
            {
                _actionEnable = value;
                _onActionCallback?.Invoke(_actionEnable);
            }
        }

        public void Init()
        {
            _stateMachine = new MainStateMachine<MainStates>(this, MainStates.Empty);

            _stateMachine.ConfigureState(MainStates.Empty)
                .For(vm => vm.ActionEnable, false);

            _stateMachine.ConfigureState(MainStates.Filled)
                .For(vm => vm.ActionEnable, true);

            _stateMachine.CombineOr(MainStates.Empty,
                (model => model.Login, string.IsNullOrEmpty),
                (model => model.Password, string.IsNullOrEmpty));

            _stateMachine.CombineAnd(MainStates.Filled,
                (model => model.Login, IsNotNull),
                (model => model.Password, IsNotNull));

            bool IsNotNull(string val)
            {
                return !string.IsNullOrEmpty(val);
            }

            _stateMachine.Build();
        }

        public void SetOnActionCallback(Action<bool> action)
        {
            _onActionCallback = action;
        }

        public void Dispose()
        {
            _stateMachine.Dispose();
            _stateMachine = null;
            _onActionCallback = null;
        }

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}