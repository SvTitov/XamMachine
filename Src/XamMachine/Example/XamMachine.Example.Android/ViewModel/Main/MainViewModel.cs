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
            EmptyLogin,
            EmptyPassword,
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
            _stateMachine = new MainStateMachine<MainStates>(this, new State<MainStates>[]
            {
                new EmptyLogin(this, MainStates.EmptyLogin), 
                new EmptyPassword(this, MainStates.EmptyPassword),
                new FilledState(this, MainStates.Filled),
            }, MainStates.EmptyLogin);


            _stateMachine.ActOn(this, model => model.Login, string.IsNullOrEmpty, MainStates.EmptyLogin);
            _stateMachine.ActOn(this, model => model.Password, string.IsNullOrEmpty, MainStates.EmptyLogin);

            _stateMachine.ActOn
            (
                this,
                vm => vm.Login,
                property => !string.IsNullOrEmpty(property),
                MainStates.Filled
            );
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