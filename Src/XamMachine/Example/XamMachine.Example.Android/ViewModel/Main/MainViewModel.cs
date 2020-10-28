using System;
using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class MainViewModel : IDisposable
    {
        private bool _actionEnable;
        private Action<bool> _onActionCallback;
        private MainStateMachine<MainStates> _stateMachine;
        private string _someText;

        public string SomeText
        {
            get => _someText;
            set
            {
                _someText = value;
                _stateMachine.Move(string.IsNullOrEmpty(_someText) ? MainStates.Empty : MainStates.Filled);
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

        public MainViewModel()
        {
            _stateMachine = new MainStateMachine<MainStates> (new State<MainStates>[] 
            {
                new EmptyState(this, MainStates.Empty),
                new FilledState(this, MainStates.Filled), 
            }, MainStates.Empty);
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
    }
}