using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using XamMachine.Tests.Annotations;

namespace XamMachine.Tests.Mock
{
    public class MockViewModel : INotifyPropertyChanged
    {
        private string _stringProperty2;
        private string _stringProperty1;


        public string StringProperty1
        {
            get => _stringProperty1;
            set
            {
                _stringProperty1 = value;
                OnPropertyChanged(nameof(StringProperty1));
            }
        }


        public string StringProperty2
        {
            get => _stringProperty2;
            set
            {
                _stringProperty2 = value;
                OnPropertyChanged(nameof(StringProperty2));
            }
        }


        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public void Print(string str)
        {
            Trace.WriteLine(str);
        }
    }
}
