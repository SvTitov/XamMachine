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
                .WithContext(this)
                .For(vm => vm.ActionEnable, false)
                .Build();

            _stateMachine.ConfigureState(MainStates.Filled)
                .WithContext(this)
                .For(vm => vm.ActionEnable, true)
                .Build();

            _stateMachine.CombineActOn(this, MainStates.Empty,
                (model => model.Login, string.IsNullOrEmpty),
                (model => model.Password, string.IsNullOrEmpty));

            _stateMachine.CombineActOn(this, MainStates.Filled,
                (model => model.Login, value => !string.IsNullOrEmpty(value)),
                (model => model.Password, value => !string.IsNullOrEmpty(value)));

            //_stateMachine.ActOn(this, model => new Tuple<string, string>(model.Login, model.Password), tuple => string.IsNullOrEmpty(tuple.Item1) && string.IsNullOrEmpty(tuple.Item2), MainStates.Empty);

            //_stateMachine.ActOn
            //(
            //    this,
            //    vm => new Tuple<string,string>(vm.Login, vm.Password),  
            //    tuple => !string.IsNullOrEmpty(tuple.Item1) && !string.IsNullOrEmpty(tuple.Item2),
            //    MainStates.Filled
            //);

            //*****************************


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