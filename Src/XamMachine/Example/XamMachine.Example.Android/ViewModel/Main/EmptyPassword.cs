using System.Collections.Generic;
using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class EmptyPassword : State<MainViewModel.MainStates>
    {
        private MainViewModel _viewModel;
        private List<MainViewModel.MainStates> _cases = new List<MainViewModel.MainStates>();

        public EmptyPassword(MainViewModel viewModel, MainViewModel.MainStates state)
            : base(state)
        {
            _viewModel = viewModel;
        }

        public override void Add(MainViewModel.MainStates stat)
        {
            
        }

        public override void Release()
        {
            _viewModel = null;
        }

        public override void EnterState()
        {
            _viewModel.ActionEnable = false;
        }

        public override void LeaveState()
        { }
    }
}