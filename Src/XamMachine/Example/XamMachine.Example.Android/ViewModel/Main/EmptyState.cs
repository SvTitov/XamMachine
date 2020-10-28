using System;
using System.Collections.Generic;
using System.Linq;
using XamMachine.Core.Abstract;

namespace XamMachine.Example.Android.ViewModel.Main
{
    public class EmptyState : State<MainViewModel.MainStates>
    {
        private MainViewModel _viewModel;
        private List<MainViewModel.MainStates> _cases = new List<MainViewModel.MainStates>();

        public EmptyState(MainViewModel viewModel, MainViewModel.MainStates state)
            : base(state)
        {
            _viewModel = viewModel;
        }

        public override void Move(MainViewModel.MainStates state)
        {
            var existedState = _cases.FirstOrDefault(x => x == state);

            if (existedState == null)
                throw new Exception("Can't move to state!");
        }

        public override void Add(MainViewModel.MainStates stat)
        {
            var existedCase = _cases.FirstOrDefault(x => x == stat);
            if (existedCase == null)
            {
                existedCase = stat;
                _cases.Add(existedCase);
            }
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