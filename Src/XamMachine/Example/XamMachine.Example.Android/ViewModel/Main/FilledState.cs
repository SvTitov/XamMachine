using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class FilledState : State<MainViewModel.MainStates>
    {
        private MainViewModel _viewModel;

        public FilledState(MainViewModel viewModel, MainViewModel.MainStates state)
            : base(state)
        {
            _viewModel = viewModel;
        }

        public override void Move(MainViewModel.MainStates state)
        {
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
            _viewModel.ActionEnable = true;
        }

        public override void LeaveState()
        {
            
        }
    }
}