using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Widget;
using XamMachine.Example.Android.ViewModel;
using XamMachine.Example.Android.ViewModel.Main;

namespace XamMachine.Example.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MainViewModel _viewModel;
        private EditText _editText;
        private Button _button;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _editText = FindViewById<EditText>(Resource.Id.editText);
            _button = FindViewById<Button>(Resource.Id.clickBtn);

            _viewModel = new MainViewModel();

            _viewModel.SetOnActionCallback(OnAction);
        }

        private void OnAction(bool value)
        {
            _button.Enabled = value;
        }

        protected override void OnResume()
        {
            base.OnResume();

            Subscribe();
        }

        protected override void OnPause()
        {
            base.OnPause();

            Unsubscribe();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            _viewModel.Dispose();
            _viewModel = null;
        }

        private void Subscribe()
        {
            _button.Click += ButtonOnClick;
            _editText.TextChanged += EditTextOnTextChanged;
        }

        private void Unsubscribe()
        {
            _button.Click -= ButtonOnClick;
            _editText.TextChanged -= EditTextOnTextChanged;
        }

        private void EditTextOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.SomeText = e.Text?.ToString();
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            //TODO: Do smth
        }
    }
}
