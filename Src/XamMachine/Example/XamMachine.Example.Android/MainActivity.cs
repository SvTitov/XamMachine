using System;
using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Text;
using Android.Widget;
using XamMachine.Example.Android.ViewModel.Main;

namespace XamMachine.Example.Android
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private MainViewModel _viewModel;
        private EditText _loginET;
        private EditText _passwordET;
        private Button _button;


        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            _loginET = FindViewById<EditText>(Resource.Id.logingET);
            _passwordET = FindViewById<EditText>(Resource.Id.passwordET);
            _button = FindViewById<Button>(Resource.Id.clickBtn);

            Init();
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

        private void Init()
        {
            _viewModel = new MainViewModel();

            _viewModel.SetOnActionCallback(OnAction);

            _viewModel.Init();
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
            _loginET.TextChanged += LoginEtOnTextChanged;
            _passwordET.TextChanged += PasswordEtOnTextChanged;
        }


        private void Unsubscribe()
        {
            _button.Click -= ButtonOnClick;
            _loginET.TextChanged -= LoginEtOnTextChanged;
            _passwordET.TextChanged -= PasswordEtOnTextChanged;
        }

        private void PasswordEtOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.Password = e.Text?.ToString();
        }

        private void LoginEtOnTextChanged(object sender, TextChangedEventArgs e)
        {
            _viewModel.Login = e.Text?.ToString();
        }

        private void ButtonOnClick(object sender, EventArgs e)
        {
            //TODO: Do smth
        }
    }
}
