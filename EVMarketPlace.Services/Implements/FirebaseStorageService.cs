using Firebase.Storage;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{
    public class FirebaseStorageService
    {
        private const string BucketName = "ev-marketplace.appspot.com";
        private readonly string _credsPath;

        public FirebaseStorageService(IWebHostEnvironment env)
        {
            // file JSON để ở cùng cấp Program.cs (API project)
            _credsPath = Path.Combine(env.ContentRootPath, "firebase-adminsdk.json");
        }

        public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string contentType, CancellationToken ct = default)
        {
            // Lấy access token từ service account
            var credential = GoogleCredential.FromFile(_credsPath)
                .CreateScoped("https://www.googleapis.com/auth/devstorage.read_write");

            var accessToken = await credential.UnderlyingCredential.GetAccessTokenForRequestAsync();

            var task = new FirebaseStorage(
                            BucketName,
                            new FirebaseStorageOptions
                            {
                                ThrowOnCancel = true,
                                // 👇 cấp token cho FirebaseStorage.net
                                AuthTokenAsyncFactory = () => Task.FromResult(accessToken)
                            })
                        .Child("images")
                        .Child(fileName)
                        .PutAsync(fileStream, ct, contentType);

            return await task; // trả về download URL
        }
    }
}
