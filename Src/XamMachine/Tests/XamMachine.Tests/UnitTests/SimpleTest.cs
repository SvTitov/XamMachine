using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using XamMachine.Core.Impl;
using XamMachine.Tests.Mock;

namespace XamMachine.Tests.UnitTests
{
    [TestClass]
    public class SimpleTest
    {
        private MockViewModel _viewModel;
        private DefaultStateMachine<MockViewModel, MockEnum> _stateMachine;

        public SimpleTest()
        {
            _viewModel = new MockViewModel();
            _stateMachine = DefaultStateMachine<MockViewModel, MockEnum>.Produce(_viewModel, MockEnum.EmptyState);

            ConfigureStates(_stateMachine);
        }

        private void ConfigureStates(DefaultStateMachine<MockViewModel, MockEnum> stateMachine)
        {
            bool IsNotNull(string val)
            {
                return !string.IsNullOrEmpty(val);
            }

            stateMachine.ConfigureState(MockEnum.EmptyState)
                .For(vm => vm.Print(nameof(MockEnum.EmptyState)))
                .Next(MockEnum.MiddleState);

            stateMachine.ConfigureState(MockEnum.MiddleState)
                .For(vm => vm.Print(nameof(MockEnum.MiddleState)))
                .Next(MockEnum.NotEmptyState);

            stateMachine.ConfigureState(MockEnum.NotEmptyState)
                .For(vm => vm.Print(nameof(MockEnum.NotEmptyState)));

            stateMachine.ForCombineAnd(MockEnum.EmptyState,
                (model => model.StringProperty1, string.IsNullOrEmpty),
                (model => model.StringProperty2, string.IsNullOrEmpty));

            stateMachine.ForCombineAnd(MockEnum.NotEmptyState,
                (model => model.StringProperty1, IsNotNull),
                (model => model.StringProperty2, IsNotNull));

            stateMachine.ForCombineOr(MockEnum.MiddleState,
                (model => model.StringProperty1, IsNotNull),
                (model => model.StringProperty2, IsNotNull));
           
            stateMachine.Build();
        }

        [TestMethod]
        public void CheckAllStatesEmptyContext()
        {
            _viewModel.StringProperty1 = string.Empty;
            _viewModel.StringProperty2 = string.Empty;

            Assert.AreEqual(_stateMachine.CurrentState().State, MockEnum.EmptyState);
            Assert.AreNotEqual(_stateMachine.CurrentState().State, MockEnum.NotEmptyState);
            Assert.AreNotEqual(_stateMachine.CurrentState().State, MockEnum.MiddleState);
        }

        [TestMethod]
        public void CheckAllStates_DynamicUpdating()
        {
            _viewModel.StringProperty1 = string.Empty;
            _viewModel.StringProperty2 = string.Empty;

            Assert.AreEqual(_stateMachine.CurrentState().State, MockEnum.EmptyState);

            _viewModel.StringProperty1 = "test_text";
            _viewModel.StringProperty2 = "test_text";

            Assert.AreEqual(_stateMachine.CurrentState().State, MockEnum.NotEmptyState);


            _viewModel.StringProperty1 = "test_text";
            _viewModel.StringProperty2 = string.Empty;

            Assert.AreEqual(_stateMachine.CurrentState().State, MockEnum.MiddleState);
        }
    }
}