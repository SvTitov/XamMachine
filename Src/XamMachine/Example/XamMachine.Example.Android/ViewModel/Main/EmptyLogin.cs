using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class EmptyLogin : State<MainViewModel.MainStates>
    {
        private MainViewModel _viewModel;

        public EmptyLogin(MainViewModel viewModel, MainViewModel.MainStates @enum) : base(@enum)
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
        {
        }
    }
}