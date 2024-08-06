using Plugin.Firebase.CloudMessaging;
using Firebase.Database;
using Firebase.Database.Query;

namespace FirebasePushNotification
{
    public partial class MainPage : ContentPage
    {
        FirebaseClient firebaseClient;
        private const int TokenValidityDays = 30;

        public MainPage()
        {
            InitializeComponent();
            firebaseClient = new FirebaseClient("https://pushnotification-51b43-default-rtdb.asia-southeast1.firebasedatabase.app/");
            //CheckAndRefreshTokenAsync();
        }

        private async void CheckAndRefreshTokenAsync()
        {
            try
            {
                var token = Preferences.Get("FCMToken", string.Empty);
                var storedKey = Preferences.Get("storedKey", string.Empty);
                var expiryTimeString = Preferences.Get("FCMTokenExpiryTime", string.Empty);
                var data = await firebaseClient.Child($"FCMToken/{storedKey}").OnceSingleAsync<GetToken>();
                if (true)
                {
                    await RefreshFcmTokenAsync();
                }
                else
                {
                    DateTime expiryTime = DateTime.Parse(expiryTimeString);
                    if (expiryTime < DateTime.UtcNow)
                    {
                        await RefreshFcmTokenAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private async Task RefreshFcmTokenAsync()
        {
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            var TechnicianName = InputField.Text;
            var result = await firebaseClient.Child("FCMToken").PostAsync(new GetToken
            {
                Token = token,
                TechnicianName = TechnicianName,
                ServiceURL = "www.google.com"
            });

            string newKey = result.Key;
            DateTime newExpiryTime = DateTime.UtcNow.AddDays(TokenValidityDays);

            Preferences.Set("FCMToken", token);
            Preferences.Set("storedKey", newKey);
            Preferences.Set("FCMTokenExpiryTime", newExpiryTime.ToString("o"));
        }

        private async void OnGetFcmTokenClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(InputField.Text))
            {
                await DisplayAlert("info", "please enter technician name", "ok");
                return;
            }
            var token = Preferences.Get("FCMToken", string.Empty);
            var storedKey = Preferences.Get("storedKey", string.Empty);
            var data = await firebaseClient.Child($"FCMToken/{storedKey}").OnceSingleAsync<GetToken>();
            if (string.IsNullOrEmpty(token) || data == null)
            {
                await RefreshFcmTokenAsync();
                token = Preferences.Get("FCMToken", string.Empty);
            }
        }

        private async void OnDeleteFcmTokenClicked(object sender, EventArgs e)
        {
            var storedKey = Preferences.Get("storedKey", string.Empty);
            if (string.IsNullOrEmpty(storedKey))
            {
                return;
            }

            try
            {
                string pathToDelete = $"FCMToken/{storedKey}";
                await firebaseClient.Child(pathToDelete).DeleteAsync();

                Preferences.Remove("FCMToken");
                Preferences.Remove("storedKey");
                Preferences.Remove("FCMTokenExpiryTime");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class GetToken
    {
        public string Token { get; set; }
        public string TechnicianName { get; set; }
        public string ServiceURL { get; set; }
    }

}
